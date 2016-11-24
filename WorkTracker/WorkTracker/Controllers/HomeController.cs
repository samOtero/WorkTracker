using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WorkTracker.Models;

namespace WorkTracker.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Dashboard");
            }
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Dashboard()
        {
            if (!Request.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            //Set Session UserID
            if (Session[WorkTracker.Models.User.ID] == null)
            {
                var aspnetID = User.Identity.GetUserId();
                if (aspnetID != null)
                {
                    using (var dbcontext = new DbModels())
                    {
                        var currentUser = dbcontext.Users.Where(m => m.AspNetUserId == aspnetID).FirstOrDefault();
                        if (currentUser != null)
                        {
                            var userID = currentUser.Id;
                            var userEmail = currentUser.Email;
                            Session[WorkTracker.Models.User.ID] = userID;
                        }
                    }
                }
            }
            //Create a list of my notifications
            var notificationModel = new NotificationModel();
            notificationModel.myNotifications = new List<Notification>();
            //Create fake notifications for testing
            var notification1 = new Notification();
            notification1.Note = "Jairo created new work item needs approval";
            var notification2 = new Notification();
            notification2.Note = "Rodrigo approved a Work Item";
            //Add fake notifications to model list
            notificationModel.myNotifications.Add(notification1);
            notificationModel.myNotifications.Add(notification2);
            var result = addNumbersSam(200, 10);
            //notification1.Note = result.ToString();
            return View(notificationModel);
        }

        public int addNumbersSam(int firstNumber, int secondNumber)
        {
            var sum = firstNumber + secondNumber;
            return sum;
        }
    }
}