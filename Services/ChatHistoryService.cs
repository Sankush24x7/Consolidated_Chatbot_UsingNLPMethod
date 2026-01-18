using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Consolidated_ChatBot.Services
{
    public class ChatHistoryService
    {
        string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;

        //public void Save(string sessionId, string sender, string msg, double? confidence = null)
        //{
        //    using (var con = new SqlConnection(cs))
        //    using (var cmd = new SqlCommand(
        //        @"INSERT INTO Chat_History
        //      (SessionId, Sender, Message, Confidence)
        //      VALUES (@s,@u,@m,@c)", con))
        //    {
        //        cmd.Parameters.AddWithValue("@s", sessionId);
        //        cmd.Parameters.AddWithValue("@u", sender);
        //        cmd.Parameters.AddWithValue("@m", msg);
        //        cmd.Parameters.AddWithValue("@c", (object)confidence ?? DBNull.Value);
        //        con.Open();
        //        cmd.ExecuteNonQuery();
        //    }
        //}
        public void Save(string userId, string sessionId,
                 string sender, string msg, double? conf = null)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                @"INSERT INTO Chat_History
          (UserId, SessionId, Sender, Message, Confidence)
          VALUES (@u,@s,@sen,@m,@c)", con))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@s", sessionId);
                cmd.Parameters.AddWithValue("@sen", sender);
                cmd.Parameters.AddWithValue("@m", msg);
                cmd.Parameters.AddWithValue("@c", (object)conf ?? DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable GetHistory(string userId)
        {
            var dt = new DataTable();

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                @"SELECT Sender, Message, Confidence, CreatedOn
          FROM Chat_History
          WHERE UserId = @uid
          ORDER BY CreatedOn", con))
            {
                cmd.Parameters.AddWithValue("@uid", userId);
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            return dt;
        }
        public DataTable GetLastMessages(string userId, string sessionId, int limit)
        {
            var dt = new DataTable();

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                @"SELECT TOP (@n) *
          FROM Chat_History
          WHERE UserId=@u AND SessionId=@s
          ORDER BY CreatedOn DESC", con))
            {
                cmd.Parameters.AddWithValue("@n", limit);
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@s", sessionId);

                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            dt.DefaultView.Sort = "CreatedOn ASC";
            return dt.DefaultView.ToTable();
        }
        public void ClearSession(string userId, string sessionId)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                @"DELETE FROM Chat_History
          WHERE UserId=@u AND SessionId=@s", con))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@s", sessionId);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public DataTable GetUserSessions(string userId)
        {
            var dt = new DataTable();

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                @"SELECT SessionId, LoginTime
          FROM Chat_Sessions
          WHERE UserId = @u
          ORDER BY LoginTime DESC", con))
            {
                cmd.Parameters.AddWithValue("@u", userId);
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            return dt;
        }


    }
}