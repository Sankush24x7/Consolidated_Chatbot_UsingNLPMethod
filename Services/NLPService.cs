using Consolidated_ChatBot.Models;
using Consolidated_ChatBot.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace Consolidated_ChatBot.Services
{
    public class NLPService
    {
        private readonly string cs =
            ConfigurationManager.ConnectionStrings["DB"].ConnectionString;

        private static readonly MemoryCache cache = MemoryCache.Default;
        private const string KB_CACHE_KEY = "CHAT_KB_VECTORS";

        // =====================================================
        // MAIN NLP PIPELINE
        // =====================================================
        public BotResponse Process(string input, string userName)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new BotResponse
                {
                    Text = "Please ask a question.",
                    Confidence = 0,
                    NeedsLearning = false
                };

            input = Normalize(SpellFix(input));

            // ① DYNAMIC RESPONSES (TIME / DATE)
            var dynamic = ChatBotDictionary.GetDynamicResponse(input);
            if (dynamic != null)
            {
                return new BotResponse
                {
                    Text = dynamic,
                    Confidence = 1,
                    NeedsLearning = false
                };
            }

            // ② STATIC RESPONSES (NO DB)
            foreach (var kv in ChatBotDictionary.AllStaticResponses)
            {
                if (input == kv.Key || input.Contains(kv.Key) || kv.Key.Contains(input))
                {
                    string response = kv.Value;

                    if (response.Contains("{USERNAME}"))
                        response = response.Replace("{USERNAME}", userName ?? "User");

                    return new BotResponse
                    {
                        Text = response,
                        Confidence = 1,
                        NeedsLearning = false
                    };
                }
            }

            bool projectRelated = IsProjectRelated(input);

            // ③ KNOWLEDGE BASE MATCHING
            var kb = GetKnowledgeVectors();

            // ③-A EXACT MATCH
            var exact = kb.FirstOrDefault(k => k.NormalizedQuestion == input);
            if (exact != null)
            {
                return new BotResponse
                {
                    Text = exact.Answer,
                    Confidence = 1,
                    NeedsLearning = false
                };
            }

            // ③-B TF COSINE SIMILARITY
            var inputVector = BuildVector(input);

            double bestScore = 0;
            string bestAnswer = null;

            foreach (var k in kb)
            {
                double score = Cosine(inputVector, k.Vector);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestAnswer = k.Answer;
                }
            }

            if (bestScore >= 0.6 && bestAnswer != null)
            {
                return new BotResponse
                {
                    Text = bestAnswer,
                    Confidence = Math.Round(bestScore, 2),
                    NeedsLearning = false
                };
            }

            // ④ LEARNING QUEUE
            if (projectRelated)
            {
                //SaveLearning(input);

                return new BotResponse
                {
                    Text = "I don’t know this yet. Admin will update me.",
                    Confidence = bestScore,
                    NeedsLearning = true
                };
            }

            // ⑤ OUT OF SCOPE
            return new BotResponse
            {
                Text = "I can help only with expense, travel, and purchase-related questions.",
                Confidence = 0,
                NeedsLearning = false
            };
        }

        // =====================================================
        // VECTOR BUILDING (SAFE)
        // =====================================================
        private Dictionary<string, double> BuildVector(string text)
        {
            var words = text.Split(' ')
                            .Where(w => w.Length > 1)
                            .Take(20) // hard limit
                            .ToList();

            if (words.Count == 0)
                return new Dictionary<string, double>();

            return words
                .GroupBy(w => w)
                .ToDictionary(
                    g => g.Key,
                    g => (double)g.Count() / words.Count
                );
        }

        private double Cosine(
            Dictionary<string, double> v1,
            Dictionary<string, double> v2)
        {
            if (v1.Count == 0 || v2.Count == 0)
                return 0;

            var keys = v1.Keys.Union(v2.Keys);

            double dot = 0, a = 0, b = 0;

            foreach (var k in keys)
            {
                double x = v1.TryGetValue(k, out var xv) ? xv : 0;
                double y = v2.TryGetValue(k, out var yv) ? yv : 0;

                dot += x * y;
                a += x * x;
                b += y * y;
            }

            return dot / (Math.Sqrt(a) * Math.Sqrt(b) + 0.0001);
        }

        // =====================================================
        // KNOWLEDGE BASE VECTOR CACHE
        // =====================================================
        private List<KbVector> GetKnowledgeVectors()
        {
            if (cache.Contains(KB_CACHE_KEY))
                return (List<KbVector>)cache.Get(KB_CACHE_KEY);

            var raw = LoadKnowledge();

            var vectors = raw.Select(k => new KbVector
            {
                NormalizedQuestion = Normalize(k.Question),
                Answer = k.Answer,
                Vector = BuildVector(Normalize(k.Question))
            }).ToList();

            cache.Set(
                KB_CACHE_KEY,
                vectors,
                new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
                });

            return vectors;
        }

        private List<(string Question, string Answer)> LoadKnowledge()
        {
            var list = new List<(string, string)>();

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                "SELECT Question, Answer FROM Chat_KnowledgeBase WHERE IsActive = 1",
                con))
            {
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add((r.GetString(0), r.GetString(1)));
                }
            }

            return list;
        }

        // =====================================================
        // LEARNING
        // =====================================================
        private void SaveLearning(string question)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                "INSERT INTO Chat_LearningQueue (Question) VALUES (@q)",
                con))
            {
                cmd.Parameters.AddWithValue("@q", question);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void SaveUserAnswer(string question, string answer)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                @"INSERT INTO Chat_LearningQueue (Question, UserAnswer)
                  VALUES (@q, @a)", con))
            {
                cmd.Parameters.AddWithValue("@q", question);
                cmd.Parameters.AddWithValue("@a", answer);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // =====================================================
        // NORMALIZATION & NLP HELPERS
        // =====================================================
        private string Normalize(string text)
        {
            text = text.ToLowerInvariant();
            text = Regex.Replace(text, @"[^\w\s]", "");

            var words = text.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries)
                            .Select(w =>
                                ChatBotDictionary.Synonyms.ContainsKey(w)
                                    ? ChatBotDictionary.Synonyms[w]
                                    : w)
                            .Where(w => !ChatBotDictionary.StopWords.Contains(w));

            return string.Join(" ", words);
        }

        private string SpellFix(string text)
        {
            return text
                .Replace("whats", "what is")
                .Replace("'r", " your")
                .Replace("'av", " have")
                .Replace("'m", " am")
                .Replace("'s", " is")
                .Replace("pls", "please");
        }

        private bool IsProjectRelated(string input)
        {
            return ChatBotDictionary.ProjectKeywords
                .Any(k => input.Contains(k));
        }
    }

    // =====================================================
    // VECTOR MODEL
    // =====================================================
    public class KbVector
    {
        public string NormalizedQuestion;
        public string Answer;
        public Dictionary<string, double> Vector;
    }
}
