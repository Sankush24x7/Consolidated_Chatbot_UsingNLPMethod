using Consolidated_ChatBot.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Consolidated_ChatBot.Controllers
{
    public class AuthController : Controller
    {
        string cs = ConfigurationManager.ConnectionStrings["DB"].ConnectionString;

        // 🔹 SIGNUP PAGE
        public ActionResult Signup()
        {
            if (Session["USER_ID"] != null)
            {
                return RedirectToAction("Index", "Chat");
            }

            return View();
        }


        [HttpPost]
        public ActionResult Signup(string username, string password, string confirmPassword, DateTime dob)
        {
            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match";
                return View();
            }

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                @"INSERT INTO Chat_Users (UserName, PasswordHash, DOB)
              VALUES (@u,@p,@d)", con))
            {
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", SecurityService.Hash(password));
                cmd.Parameters.AddWithValue("@d", dob);

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Login");
        }

        // 🔹 LOGIN PAGE
        public ActionResult Login()
        {
            if (Session["USER_ID"] != null)
            {
                return RedirectToAction("Index", "Chat");
            }

            return View();
        }


        [HttpPost]
        public ActionResult Login(string username, string password, bool rememberMe = false)
        {
            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                @"SELECT UserId, Role
          FROM Chat_Users
          WHERE UserName=@u AND PasswordHash=@p", con))
            {
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", SecurityService.Hash(password));

                con.Open();
                var r = cmd.ExecuteReader();
                //if (r.Read())
                //{
                //    string userId = r["UserId"].ToString();
                //    string role = r["Role"].ToString();

                //    Session["USER_ID"] = userId;
                //    Session["USERNAME"] = username;
                //    Session["ROLE"] = role;

                //    // 🍪 Remember Me
                //    if (rememberMe)
                //    {
                //        HttpCookie cookie = new HttpCookie("REMEMBER_ME");
                //        cookie.Values["USER_ID"] = userId;
                //        cookie.Values["USERNAME"] = username;
                //        cookie.Values["ROLE"] = role;
                //        cookie.Expires = DateTime.Now.AddDays(7);
                //        Response.Cookies.Add(cookie);
                //    }

                //    return RedirectToAction("Index", "Chat");
                //}
                if (r.Read())
                {
                    Guid userId = Guid.Parse(r["UserId"].ToString());
                    Guid sessionId = Guid.NewGuid();

                    Session["USER_ID"] = userId.ToString();
                    Session["USERNAME"] = username;
                    Session["ROLE"] = r["Role"].ToString();
                    Session["CHAT_SESSION_ID"] = sessionId.ToString();

                    // save chat session
                    using (var con2 = new SqlConnection(cs))
                    using (var cmd2 = new SqlCommand(
                        @"INSERT INTO Chat_Sessions (SessionId, UserId)
          VALUES (@sid,@uid)", con2))
                    {
                        cmd2.Parameters.AddWithValue("@sid", sessionId);
                        cmd2.Parameters.AddWithValue("@uid", userId);
                        con2.Open();
                        cmd2.ExecuteNonQuery();
                    }

                    return RedirectToAction("Index", "Chat");
                }

            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }


        // 🔹 LOGOUT
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            if (Request.Cookies["REMEMBER_ME"] != null)
            {
                var cookie = new HttpCookie("REMEMBER_ME");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("Login");
        }


        public JsonResult CheckUsername(string username)
        {
            bool exists = false;

            using (var con = new SqlConnection(cs))
            using (var cmd = new SqlCommand(
                "SELECT COUNT(*) FROM Chat_Users WHERE UserName=@u", con))
            {
                cmd.Parameters.AddWithValue("@u", username);
                con.Open();
                exists = (int)cmd.ExecuteScalar() > 0;
            }

            return Json(new { exists }, JsonRequestBehavior.AllowGet);
        }

    }
}