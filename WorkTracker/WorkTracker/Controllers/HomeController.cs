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
        #region Create Work Item
        public ActionResult Create()
        {
            if (!checkAuthentication())
            {
                return RedirectToAction("Index");
            }
            var userService = new UserService();
            var model = new CreateViewModel();
            var currentUserId = (int)userService.GetMyID();
            //Add Allowed users to your assigned list based on current User Roles
            model.AssignedToList = new UserService().GetAllowedUsers(currentUserId);
            model.CreatedBy = currentUserId;         
            return View(model);
        }

        /// <summary>
        /// Create Work Item
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
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
                var userService = new UserService();
                //Check permission to set status
                var newStatus = ItemStatus.Status.Pending;
                var myRole = userService.GetUserRole(input.CreatedBy);
                if (myRole == Role.RoleTypes.Admin || myRole == Role.RoleTypes.Owner)
                    newStatus = ItemStatus.Status.Approved;

                //DateCreated
                var dateNow = DateTimeOffset.Now;
                var dateNowFormated = dateNow.ToString("MM/dd/yy hh:mm tt");
                //Save to Database
                try
                {
                    using (var context = new DbModels())
                    {
                        //Create the actual Item with tehe validated information
                        var newItem = new Item()
                        {
                            AssignedTo = input.AssignedTo,
                            Cost = (decimal)input.Cost,
                            Hours = (int)input.Hours,
                            ItemDate = (DateTime)input.ItemDate,
                            Name = input.Name,
                            CreatedBy = input.CreatedBy,
                            CreatedOn = dateNow,
                            ModifiedOn = dateNow,
                            Status = (int)newStatus,
                            WorkStatus = (int)WorkItemStatus.Status.NotStarted //By default not Started
                        };
                        context.Items.Add(newItem);
                        context.SaveChanges();

                        var creatorUser = userService.GetUser(input.CreatedBy);
                        var creatorName = creatorUser.FirstName + " " + creatorUser.LastName;

                        //Create Work Item History
                        var newHistory = new ItemHistory()
                        {
                            itemId = newItem.Id,
                            Note = "Created by " + creatorName + " on " + dateNowFormated + ".",
                            CreatedBy = input.CreatedBy,
                            CreatedOn = dateNow,
                        };

                        context.ItemHistories.Add(newHistory);
                        context.SaveChanges();

                        //Create Notifications for Item
                        var needUpdate = false;
                        Notification newNotification;
                        //Send to Assigned To user if he is not the creator
                        if (input.AssignedTo != input.CreatedBy)
                        {
                            newNotification = new Notification()
                            {
                                AssignedTo = input.AssignedTo,
                                CreatedOn = dateNow,
                                ItemId = newItem.Id,
                                Type = (int)Notification.Types.AssignedTo,
                                New = true
                            };
                            context.Notifications.Add(newNotification);
                            needUpdate = true;
                        }
                        //Send to owner(s)
                        var ownerList = userService.GetOwners();
                        foreach(var owner in ownerList)
                        {
                            if (owner.Id == input.AssignedTo || owner.Id == input.CreatedBy)
                                continue;

                            newNotification = new Notification()
                            {
                                AssignedTo = owner.Id,
                                CreatedOn = dateNow,
                                ItemId = newItem.Id,
                                Type = (int)Notification.Types.Created,
                                New = true
                            };
                            context.Notifications.Add(newNotification);
                            needUpdate = true;
                        }

                        if (needUpdate == true)
                        {
                            context.SaveChanges();
                        }
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

        #endregion Create Work Item
    

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

        public ActionResult Notifications()
        {
            if (!checkAuthentication())
            {
                return RedirectToAction("Index");
            }
            var userService = new UserService();

            //Create a list of my notifications
            var notificationModel = GetNotificationModel(userService, false);
            notificationModel.openBox = true;
            notificationModel.showViewAllBtn = false;

            return View(notificationModel);
        }

        /// <summary>
        /// Get List of Notifications for this User
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private NotificationModel GetNotificationModel(UserService service, bool showOnlyNew)
        {
            var notificationModel = new NotificationModel();
            var myID = (int)service.GetMyID();
            notificationModel.myNotifications = service.GetNotificationsForUser(myID, showOnlyNew);
            return notificationModel;
        }

        public ActionResult Dashboard()
        {
            if (!checkAuthentication())
            {
                return RedirectToAction("Index");
            }
            var userService = new UserService();
            var userID = (int)userService.GetMyID();
            var userRole = userService.GetUserRole(userID);

            //Create Notifications Model
            var notificationModel = GetNotificationModel(userService, true);
            notificationModel.openBox = false;
            notificationModel.showViewAllBtn = true;

            //Create Dashboard Model
            var dashboardModel = new DashboardModel();
            dashboardModel.NotificationModel = notificationModel;
            dashboardModel.userRole = userRole;

            return View(dashboardModel);
        }



        private bool checkAuthentication()
        {
            if (!Request.IsAuthenticated)
            {
                return false;
            }
            var userService = new UserService();
            //Set Session UserID
            if (userService.GetMyID() == null)
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
                            userService.SetMyID(userID);
                            return true;
                        }
                    }
                }
            }
            else
            {
                return true;
            }
            return false;
        }
    }
}