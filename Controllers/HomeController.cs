using Consolidated_ChatBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Consolidated_ChatBot.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            //return View();
            return RedirectToAction("Index", "Chat");
        }

        public ActionResult AdminMode()
        {
            if (UserContext.CurrentRole != "User")
            {
                return Content("You do not have rights to access Admin Mode.");
            }
            return RedirectToAction("Learning", "Admin");
        }
    }

}