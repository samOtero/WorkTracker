using Microsoft.AspNet.Identity;
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
            return View();
        }
    }
}