using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Consolidated_ChatBot.Services
{
    public static class SecurityService
    {
        public static string Hash(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}