using Consolidated_ChatBot.Models;
using Consolidated_ChatBot.Services;
using System.Web.Mvc;


namespace Consolidated_ChatBot.Controllers
{
    public class ChatController : Controller
    {
        NLPService nlp = new NLPService();
        ChatHistoryService history = new ChatHistoryService();

        // 🔐 AUTH CHECK
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["USER_ID"] == null)
            {
                filterContext.Result = RedirectToAction("Login", "Auth");
                return;
            }

            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index(string sessionId = null)
        {
            string userId = Session["USER_ID"].ToString();

            var sessions = history.GetUserSessions(userId);

            if (sessionId == null && sessions.Rows.Count > 0)
                sessionId = sessions.Rows[0]["SessionId"].ToString();

            Session["CHAT_SESSION_ID"] = sessionId;

            var messages = history.GetLastMessages(userId, sessionId, 50);

            ViewBag.Sessions = sessions;
            ViewBag.ActiveSession = sessionId;

            return View(messages);
        }



        // 💬 CHAT MESSAGE
        [HttpPost]
        public JsonResult Ask(string message)
        {
            string userId = Session["USER_ID"].ToString();
            string chatSessionId = Session["CHAT_SESSION_ID"].ToString();

            history.Save(userId, chatSessionId, "User", message);
            string userName = Session["USERNAME"] != null ? Session["USERNAME"].ToString() : "Unknown";

            BotResponse response = nlp.Process(message, userName);

            history.Save(userId, chatSessionId, "Bot", response.Text, response.Confidence);

            return Json(response);
        }


        // 📚 USER LEARNING
        [HttpPost]
        public JsonResult SubmitLearning(string question, string answer)
        {
            nlp.SaveUserAnswer(question, answer);
            return Json("Thanks! I will learn after admin approval.");
        }

        [HttpPost]
        public JsonResult ClearChat()
        {
            string userId = Session["USER_ID"].ToString();
            string sessionId = Session["CHAT_SESSION_ID"].ToString();

            history.ClearSession(userId, sessionId);
            return Json("Chat cleared");
        }

    }
}
