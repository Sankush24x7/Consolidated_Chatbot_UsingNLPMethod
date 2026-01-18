using System;
using System.Collections.Generic;

namespace Consolidated_ChatBot.Data
{
    public static class ChatBotDictionary
    {
        /* =========================================================
           GREETINGS & GENERAL RESPONSES
        ========================================================= */
        public static readonly Dictionary<string, string> Greetings =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "hi", "Hello 👋 How can I help you today?" },
            { "hello", "Hi there! What can I do for you?" },
            { "hey", "Hey! Need any help?" },
            { "good morning", "Good morning ☀️ Wishing you a productive day!" },
            { "good afternoon", "Good afternoon 🌤️ How’s your day going?" },
            { "good evening", "Good evening 🌇 What can I assist you with?" },
            { "good night", "Good night 🌙 Sleep well!" },
            { "bye", "Goodbye 👋 Take care!" },

            { "what is your name", "I’m ChatBot 🤖 your assistant." },
            { "whats your name", "My name is ChatBot." },
            { "who are you", "I’m your virtual assistant here to help!" },
            { "what is my name", "Your name is {USERNAME}" },
            { "whats my name", "You are {USERNAME}" },
            { "who am i", "You are my user and I’m here to assist you." },
            { "who created you", "I was created by developers at ESPL." },
            { "where do you live", "I live in the cloud 🌐" },
            { "are you human", "Nope, I’m an AI 🤖" },

            { "can you help me", "Of course! Just ask your question." },
            { "what can you do", "I can answer questions about expenses, travel, purchases, and more!" },

            { "tell me a joke", "Why did the developer go broke? Because he used up all his cache 💸" },
            { "make me laugh", "Why don’t robots panic? They always keep their cool circuits 😎" },

            { "thanks", "You’re welcome! 😊" },
            { "thank you", "Happy to help!" }
        };

        /* =========================================================
           TECHNICAL FAQs
        ========================================================= */
        public static readonly Dictionary<string, string> TechnicalFaqs =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "what is c#", "C# is a modern, object-oriented programming language developed by Microsoft." },
            { "what is aspnet", "ASP.NET is a framework for building web applications using .NET." },
            { "what is aspnet webforms", "ASP.NET WebForms is used to build dynamic web pages using server controls." },
            { "what is sql", "SQL is used to store, retrieve, and manage data in databases." },
            { "what is sql server", "SQL Server is Microsoft’s relational database management system." },

            { "what is a stored procedure", "A stored procedure is a precompiled set of SQL statements stored in the database." },
            { "what is a trigger", "A trigger runs automatically in response to database events." },
            { "what is an index", "An index improves query performance by speeding up data retrieval." },
            { "what is clustered index", "A clustered index defines the physical order of data in a table." },
            { "what is non clustered index", "A non-clustered index is a separate structure used for faster lookups." },

            { "what is primary key", "A primary key uniquely identifies each record in a table." },
            { "what is foreign key", "A foreign key enforces relationships between tables." },
            { "what is normalization", "Normalization reduces data redundancy and improves integrity." },
            { "what is transaction", "A transaction ensures data consistency using commit and rollback." },

            { "what is javascript", "JavaScript is a scripting language used to create interactive web pages." },
            { "what is dom", "DOM represents a web page as a structured object tree." },
            { "what is ajax", "AJAX allows web pages to update asynchronously without reloading." },
            { "what is json", "JSON is a lightweight data format used for data exchange." },

            { "what is git", "Git is a distributed version control system." },
            { "what is github", "GitHub is a platform for hosting and collaborating on Git repositories." },
            { "what is docker", "Docker allows applications to run inside containers." },
            { "what is ci cd", "CI/CD automates build, test, and deployment pipelines." }
        };

        /* =========================================================
           TROUBLESHOOTING FAQs
        ========================================================= */
        public static readonly Dictionary<string, string> TroubleshootingFaqs =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "why is my gridview not updating", "Ensure DataBind() is called and ViewState is enabled." },
            { "why is my dropdown empty", "Bind data only on first load using !IsPostBack." },
            { "why is my session null", "Session may have expired or cookies may be disabled." },
            { "why is my viewstate lost", "Dynamic controls must have consistent IDs." },

            { "why is my sql query slow", "Check execution plan, indexes, and avoid SELECT *." },
            { "why is my stored procedure not executing", "Verify parameters and permissions." },
            { "why is my trigger not firing", "Ensure the trigger is enabled and attached to the correct table." },

            { "why is my javascript not working", "Check browser console for errors." },
            { "why is my ajax call failing", "Verify URL, HTTP method, and CORS settings." }
        };

        /* =========================================================
           SYNONYMS (NORMALIZATION)
        ========================================================= */
        public static readonly Dictionary<string, string> Synonyms =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "claim", "expense" },
            { "reimbursement", "expense" },
            { "travelling", "travel" }
        };

        /* =========================================================
           PROJECT KEYWORDS
        ========================================================= */
        public static readonly string[] ProjectKeywords =
        {
            "expense","claim","reimbursement",
            "travel","hotel","flight",
            "purchase","po","order",
            "approval","manager",
            "petty","cash","invoice"
        };

        /* =========================================================
           STOP WORDS
        ========================================================= */
        public static readonly string[] StopWords =
        {
            "is","the","a","an","to","of","what","how","when","where","why","can","do","does"
        };

        /* =========================================================
           COMBINED LOOKUP
        ========================================================= */
        public static Dictionary<string, string> AllStaticResponses
        {
            get
            {
                var combined = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                Merge(Greetings, combined);
                Merge(TechnicalFaqs, combined);
                Merge(TroubleshootingFaqs, combined);

                return combined;
            }
        }

        private static void Merge(
            Dictionary<string, string> source,
            Dictionary<string, string> target)
        {
            foreach (var kv in source)
            {
                target[kv.Key] = kv.Value; // override-safe
            }
        }

        /* =========================================================
           DYNAMIC RESPONSES (TIME / DATE)
        ========================================================= */
        public static string GetDynamicResponse(string input)
        {
            input = input.ToLower();

            if (input.Contains("time"))
                return "The current time is " + DateTime.Now.ToString("hh:mm tt");

            if (input.Contains("date"))
                return "Today’s date is " + DateTime.Now.ToString("dd MMM yyyy");

            if (input.Contains("day"))
                return "Today is " + DateTime.Now.ToString("dddd");

            return null;
        }
    }
}
