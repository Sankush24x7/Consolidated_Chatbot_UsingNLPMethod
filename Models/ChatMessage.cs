using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Consolidated_ChatBot.Models
{
    public class ChatMessage
    {
        public string UserText { get; set; }
        public string BotText { get; set; }
    }
    public class BotResponse
    {
        public string Text { get; set; }
        public double Confidence { get; set; }
        public bool NeedsLearning { get; set; }
    }
}