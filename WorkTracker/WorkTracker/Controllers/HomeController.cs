using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WorkTracker.Models;
using WorkTracker.Services;

namespace WorkTracker.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Create()
        {
            if (!checkAuthentication())
            {
                return RedirectToAction("Index");
            }
            var model = new CreateViewModel();
            var currentUserId = (int)Session[WorkTracker.Models.User.ID];
            //Add Allowed users to your assigned list based on current User Roles
            model.AssignedToList = new UserService().GetAllowedUsers(currentUserId);
            model.CreatedBy = currentUserId;         
            return View(model);
        }

        public JsonResult CreateWorkItem(CreateViewModel input)
        {
            var result = true;

            //Validation Goes Here - Work In Progress
            List<string> validationResults = new List<string>();

            if (string.IsNullOrWhiteSpace(input.Name))
            {
                result = false;
                validationResults.Add("Work Item Name is Required.");
            }

            if (input.ItemDate == null)
            {
                result = false;
                validationResults.Add("Work Date is missing or in wrong format.");
            }

            if (input.Cost == null || input.Cost <= 0)
            {
                result = false;
                validationResults.Add("Work Cost is missing or has an invalid value.");
            }

            if (input.Hours == null || input.Hours <= 0)
            {
                result = false;
                validationResults.Add("Estimated Hours is missing or has an invalid value.");
            }

            if (result == true) //Only continue if result is still good
            {
                //Check permission to set status
                var newStatus = ItemStatus.Status.Pending;
                var myRole = new UserService().GetUserRole(input.CreatedBy);
                if (myRole == Role.RoleTypes.Admin || myRole == Role.RoleTypes.Owner)
                    newStatus = ItemStatus.Status.Approved;

                //Save to Database
                try
                {
                    using (var context = new DbModels())
                    {
                        var newItem = new Item()
                        {
                            AssignedTo = input.AssignedTo,
                            Cost = (decimal)input.Cost,
                            Hours = (int)input.Hours,
                            ItemDate = (DateTime)input.ItemDate,
                            Name = input.Name,
                            CreatedBy = input.CreatedBy,
                            CreatedOn = DateTimeOffset.Now,
                            ModifiedOn = DateTimeOffset.Now,
                            Status = (int)newStatus,
                            WorkStatus = (int)WorkItemStatus.Status.NotStarted //By default not Started
                        };
                        context.Items.Add(newItem);
                        context.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    result = false;
                    validationResults.Add(e.Message);
                }
            }
            return Json(new { Result = result, validations = validationResults }, JsonRequestBehavior.AllowGet);
        }

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
            if (!checkAuthentication())
            {
                return RedirectToAction("Index");
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
            notification1.Id = 2;
            return View(notificationModel);
        }

        public int addNumbersSam(int firstNumber, int secondNumber)
        {
            var sum = firstNumber + secondNumber;
            return sum;
        }

        private bool checkAuthentication()
        {
            if (!Request.IsAuthenticated)
            {
                return false;
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
            return true;
        }
    }
}