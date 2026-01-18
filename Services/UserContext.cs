using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Consolidated_ChatBot.Services
{
    public static class UserContext
    {
        public static string CurrentRole
        {
            get
            {
                return System.Web.HttpContext.Current.Session["ROLE"]?.ToString() ?? "Employee";
            }
            set
            {
                System.Web.HttpContext.Current.Session["ROLE"] = value;
            }
        }
    }
}