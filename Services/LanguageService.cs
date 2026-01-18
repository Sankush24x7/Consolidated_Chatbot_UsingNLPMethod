using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Consolidated_ChatBot.Services
{
    public class LanguageService
    {
        public bool IsHindi(string text)
        {
            return Regex.IsMatch(text, "[\u0900-\u097F]");
        }
    }
}