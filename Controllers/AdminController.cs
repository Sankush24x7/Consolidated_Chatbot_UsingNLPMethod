using Consolidated_ChatBot.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Consolidated_ChatBot.Controllers
{
    public class AdminController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["USER_ID"] == null)
            {
                filterContext.Result = RedirectToAction("Login", "Auth");
            }
        }
        public ActionResult Analytics()
        {
            return View();
        }
        public ActionResult Learning(string search = "", int page = 1)
        {
            int pageSize = 5;
            int startRow = ((page - 1) * pageSize) + 1;
            int endRow = page * pageSize;

            string sql = @"
        ;WITH CTE AS
        (
            SELECT *,
                   ROW_NUMBER() OVER (ORDER BY AskedCount DESC) AS RowNum,
                   COUNT(*) OVER() AS TotalCount
            FROM Chat_LearningQueue
            WHERE Status = 'Pending'
              AND (@search = '' OR Question LIKE '%' + @search + '%')
        )
        SELECT *
        FROM CTE
        WHERE RowNum BETWEEN @start AND @end
        ORDER BY RowNum";

            var dt = new DataTable();

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@search", search);
                cmd.Parameters.AddWithValue("@start", startRow);
                cmd.Parameters.AddWithValue("@end", endRow);

                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = dt.Rows.Count > 0
                ? Convert.ToInt32(dt.Rows[0]["TotalCount"])
                : 0;

            return View(dt);
        }


        [HttpPost]
        public ActionResult Approve(int id, string newAnswer)
        {
            using (var con = new SqlConnection(cs))
            {
                con.Open();

                // Get old data
                var getCmd = new SqlCommand(
                    "SELECT Question, UserAnswer FROM Chat_LearningQueue WHERE Id=@i", con);
                getCmd.Parameters.AddWithValue("@i", id);
                var r = getCmd.ExecuteReader();
                r.Read();
                string question = r["Question"].ToString();
                string oldAnswer = r["UserAnswer"].ToString();
                r.Close();
                newAnswer = newAnswer?.ToString() == null ? oldAnswer : newAnswer;
                // Insert KB
                var ins = new SqlCommand(
                    "INSERT INTO Chat_KnowledgeBase (Question, Answer) VALUES (@q,@a)", con);
                ins.Parameters.AddWithValue("@q", question);
                ins.Parameters.AddWithValue("@a", newAnswer);
                ins.ExecuteNonQuery();

                // Audit
                var audit = new SqlCommand(
                    @"INSERT INTO Chat_AuditLog
              (Question, ActionTaken, OldAnswer, NewAnswer, ActionBy)
              VALUES (@q,'Approved',@o,@n,@u)", con);
                audit.Parameters.AddWithValue("@q", question);
                audit.Parameters.AddWithValue("@o", oldAnswer);
                audit.Parameters.AddWithValue("@n", newAnswer);
                audit.Parameters.AddWithValue("@u", UserContext.CurrentRole);
                audit.ExecuteNonQuery();

                // Remove from queue
                new SqlCommand(
                    "DELETE FROM Chat_LearningQueue WHERE Id=@i", con)
                { Parameters = { new SqlParameter("@i", id) } }
                    .ExecuteNonQuery();
            }

            return RedirectToAction("Learning");
        }


        public JsonResult TopQuestions()
        {
            var dt = new DataTable();
            using (var da = new SqlDataAdapter(
                @"SELECT TOP 10 Question, COUNT(*) Count
              FROM Chat_Analytics
              GROUP BY Question
              ORDER BY Count DESC", cs))
            {
                da.Fill(dt);
            }
            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        public JsonResult FailureStats()
        {
            var dt = new DataTable();
            using (var da = new SqlDataAdapter(
                @"SELECT COUNT(*) Failures
              FROM Chat_Analytics
              WHERE IsAnswered = 0", cs))
            {
                da.Fill(dt);
            }
            return Json(dt, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Reject(int id)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                "DELETE FROM Chat_LearningQueue WHERE Id=@i ; DELETE FROM Chat_Analytics WHERE Id=@i", con))
            {
                cmd.Parameters.AddWithValue("@i", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Learning");
        }
        public ActionResult Sessions()
        {
            var dt = new DataTable();

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                @"SELECT cs.SessionId, cs.LoginTime, cu.UserName, cu.UserId
          FROM Chat_Sessions cs
          JOIN Chat_Users cu ON cu.UserId = cs.UserId
          ORDER BY cs.LoginTime DESC", con))
            {
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            return View(dt);
        }
        public ActionResult ViewChat(string sessionId)
        {
            var dt = new DataTable();

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                @"SELECT Sender, Message, CreatedOn
          FROM Chat_History
          WHERE SessionId = @s
          ORDER BY CreatedOn", con))
            {
                cmd.Parameters.AddWithValue("@s", sessionId);
                using (var da = new SqlDataAdapter(cmd))
                    da.Fill(dt);
            }

            return View(dt);
        }

    }
}