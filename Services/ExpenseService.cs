using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Consolidated_ChatBot.Services
{
    public class ExpenseService
    {
        string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;

        public string GetPettyCashBalance()
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand("SELECT SUM(Amount) FROM PettyCash", con))
            {
                con.Open();
                var val = cmd.ExecuteScalar();
                return "Current petty cash balance is ₹" + val;
            }
        }
    }
}