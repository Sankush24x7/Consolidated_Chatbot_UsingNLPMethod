using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;

namespace Consolidated_ChatBot.Services
{
    public class NLPService_bkp
    {
        string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;

        public string Process(string userInput)
        {
            var kb = GetKnowledgeBase();
            var best = kb
                .Select(k => new
                {
                    k.Answer,
                    Score = Similarity(userInput, k.Question)
                })
                .OrderByDescending(x => x.Score)
                .FirstOrDefault();

            if (best != null && best.Score > 0.25)
            {
                SaveAnalytics(userInput, true);
                return best.Answer;
            }

            SaveLearning(userInput);
            SaveAnalytics(userInput, false);
            return "I don't know this yet. Admin will update me soon.";
        }

        double Similarity(string a, string b)
        {
            var sa = a.ToLower().Split(' ');
            var sb = b.ToLower().Split(' ');
            return sa.Intersect(sb).Count() / (double)Math.Max(sa.Length, sb.Length);
        }

        List<(string Question, string Answer)> GetKnowledgeBase()
        {
            var list = new List<(string, string)>();
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                "SELECT Question, Answer FROM Chat_KnowledgeBase WHERE IsActive = 1", con))
            {
                con.Open();
                var r = cmd.ExecuteReader();
                while (r.Read())
                    list.Add((r[0].ToString(), r[1].ToString()));
            }
            return list;
        }

        void SaveLearning(string q)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(@"
            IF EXISTS (SELECT 1 FROM Chat_LearningQueue WHERE Question=@q)
                UPDATE Chat_LearningQueue SET Frequency = Frequency + 1 WHERE Question=@q
            ELSE
                INSERT INTO Chat_LearningQueue (Question) VALUES (@q)", con))
            {
                cmd.Parameters.AddWithValue("@q", q);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        void SaveAnalytics(string q, bool answered)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                "INSERT INTO Chat_Analytics (Question, IsAnswered) VALUES (@q,@a)", con))
            {
                cmd.Parameters.AddWithValue("@q", q);
                cmd.Parameters.AddWithValue("@a", answered);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}



//using Consolidated_ChatBot.Models;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text.RegularExpressions;

//namespace Consolidated_ChatBot.Services
//{
//    public class NLPService
//    {
//        string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;

//        // ⚠️ IMPORTANT: Do NOT remove intent words like "what", "how"
//        readonly string[] stopWords = { "is", "the", "a", "an", "to", "of" };

//        Dictionary<string, string> synonyms = new Dictionary<string, string>
//        {
//            { "claim", "expense" },
//            { "reimbursement", "expense" },
//            { "traveling", "travel" }
//        };

//        // =========================
//        // MAIN NLP PIPELINE
//        // =========================
//        public BotResponse Process(string input)
//        {
//            string lang = DetectLanguage(input);

//            if (lang == "hi")
//                input = TranslateHindiToEnglish(input); // stub

//            input = Normalize(input);

//            // 🔹 Load KB
//            var kb = GetKnowledge();

//            // 🔹 FAST PATH: EXACT MATCH (CRITICAL FIX)
//            foreach (var k in kb)
//            {
//                if (Normalize(k.Question) == input)
//                {
//                    SaveAnalytics(input, 1.0, true);

//                    return new BotResponse
//                    {
//                        Text = k.Answer,
//                        Confidence = 1.0,
//                        NeedsLearning = false
//                    };
//                }
//            }

//            // 🔹 Normalize KB questions
//            var normalizedKb = kb
//                .Select(k => new
//                {
//                    Question = Normalize(k.Question),
//                    Answer = k.Answer
//                })
//                .ToList();

//            var corpus = normalizedKb.Select(k => k.Question).ToList();

//            var inputVec = TfIdf(input, corpus);

//            double maxScore = 0;
//            string bestAnswer = null;

//            foreach (var k in normalizedKb)
//            {
//                var vec = TfIdf(k.Question, corpus);
//                var score = Cosine(inputVec, vec);

//                if (score > maxScore)
//                {
//                    maxScore = score;
//                    bestAnswer = k.Answer;
//                }
//            }

//            SaveAnalytics(input, maxScore, maxScore >= 0.4);

//            // 🔹 STRONG MATCH
//            if (maxScore >= 0.6)
//            {
//                return new BotResponse
//                {
//                    Text = bestAnswer,
//                    Confidence = maxScore,
//                    NeedsLearning = false
//                };
//            }

//            // 🔹 WEAK MATCH
//            if (maxScore >= 0.4)
//            {
//                return new BotResponse
//                {
//                    Text = bestAnswer + " (Please verify)",
//                    Confidence = maxScore,
//                    NeedsLearning = false
//                };
//            }

//            // 🔹 NO MATCH → LEARNING
//            SaveLearning(input);

//            return new BotResponse
//            {
//                Text = lang == "hi"
//                    ? "मुझे इसका उत्तर नहीं पता। एडमिन इसे अपडेट करेंगे।"
//                    : "I don’t know this yet. Admin will update me.",
//                Confidence = maxScore,
//                NeedsLearning = true
//            };
//        }

//        // =========================
//        // NLP HELPERS
//        // =========================

//        string Normalize(string text)
//        {
//            if (string.IsNullOrWhiteSpace(text))
//                return "";

//            text = text.ToLower();
//            text = Regex.Replace(text, @"[^\w\s]", ""); // remove punctuation

//            foreach (var s in synonyms)
//                text = text.Replace(s.Key, s.Value);

//            var words = text.Split(' ')
//                            .Where(w => !stopWords.Contains(w))
//                            .ToArray();

//            return string.Join(" ", words);
//        }

//        Dictionary<string, double> TfIdf(string doc, List<string> corpus)
//        {
//            var words = doc.Split(' ').Where(w => w.Length > 0).ToArray();

//            var tf = words
//                .GroupBy(w => w)
//                .ToDictionary(g => g.Key, g => (double)g.Count() / words.Length);

//            var tfidf = new Dictionary<string, double>();

//            foreach (var w in tf.Keys)
//            {
//                double df = corpus.Count(d => d.Contains(w));
//                double idf = Math.Log((double)corpus.Count / (df + 1));
//                tfidf[w] = tf[w] * idf;
//            }

//            return tfidf;
//        }

//        double Cosine(Dictionary<string, double> v1, Dictionary<string, double> v2)
//        {
//            var keys = v1.Keys.Union(v2.Keys);

//            double dot = 0, a = 0, b = 0;

//            foreach (var k in keys)
//            {
//                double x = v1.ContainsKey(k) ? v1[k] : 0;
//                double y = v2.ContainsKey(k) ? v2[k] : 0;

//                dot += x * y;
//                a += x * x;
//                b += y * y;
//            }

//            return dot / (Math.Sqrt(a) * Math.Sqrt(b) + 0.0001);
//        }

//        // =========================
//        // DB ACCESS
//        // =========================

//        List<(string Question, string Answer)> GetKnowledge()
//        {
//            var list = new List<(string, string)>();

//            using (var con = new SqlConnection(cs))
//            using (var cmd = new SqlCommand(
//                @"SELECT Question, Answer
//                  FROM Chat_KnowledgeBase
//                  WHERE IsActive = 1", con))
//            {
//                con.Open();
//                var r = cmd.ExecuteReader();

//                while (r.Read())
//                    list.Add((r["Question"].ToString(), r["Answer"].ToString()));
//            }

//            return list;
//        }

//        void SaveLearning(string q)
//        {
//            using (var con = new SqlConnection(cs))
//            using (var cmd = new SqlCommand(
//                @"INSERT INTO Chat_LearningQueue (Question)
//                  VALUES (@q)", con))
//            {
//                cmd.Parameters.AddWithValue("@q", q);
//                con.Open();
//                cmd.ExecuteNonQuery();
//            }
//        }

//        void SaveAnalytics(string q, double score, bool answered)
//        {
//            using (var con = new SqlConnection(cs))
//            using (var cmd = new SqlCommand(
//                @"INSERT INTO Chat_Analytics
//                  (Question, IsAnswered, ConfidenceScore)
//                  VALUES (@q,@a,@s)", con))
//            {
//                cmd.Parameters.AddWithValue("@q", q);
//                cmd.Parameters.AddWithValue("@a", answered);
//                cmd.Parameters.AddWithValue("@s", score);
//                con.Open();
//                cmd.ExecuteNonQuery();
//            }
//        }

//        // =========================
//        // LANGUAGE (STUBS)
//        // =========================

//        string DetectLanguage(string text)
//        {
//            return Regex.IsMatch(text, "[\u0900-\u097F]") ? "hi" : "en";
//        }

//        string TranslateHindiToEnglish(string hi) => hi;
//        string TranslateEnglishToHindi(string en) => en;

//        // =========================
//        // USER LEARNING
//        // =========================

//        public void SaveUserAnswer(string question, string answer)
//        {
//            using (var con = new SqlConnection(cs))
//            using (var cmd = new SqlCommand(
//                @"INSERT INTO Chat_LearningQueue (Question, UserAnswer)
//                  VALUES (@q,@a)", con))
//            {
//                cmd.Parameters.AddWithValue("@q", question);
//                cmd.Parameters.AddWithValue("@a", answer);
//                con.Open();
//                cmd.ExecuteNonQuery();
//            }
//        }
//    }
//}