using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using WorkTracker.Controllers;
using WorkTracker.Models;
using WorkTracker.Services;

namespace WorkTracker.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (Request.IsAuthenticated && checkAuthentication())
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
        
        public ActionResult Report()
        {
            if (!checkAuthentication())
            {
                return RedirectToAction("Index");
            }
            var userService = new UserService();
            var userID = (int)userService.GetMyID();
            var userRole = userService.GetUserRole(userID);
            var model = new WorkItemReportModel();
            var workItemListModel = GetWorkItemListModel(userService, userID, userRole);

            model.reportItems = new List<WorkItemReportItemModel>();
            foreach(var workItem in workItemListModel.workItems)
            {
                var userId = workItem.assignedToId;
                var reportItem = model.reportItems.Where(m => m.userId == userId).FirstOrDefault();
                if (reportItem == null)
                {
                    reportItem = new WorkItemReportItemModel()
                    {
                        userId = workItem.assignedToId,
                        userFullName = workItem.assignedTo,
                        workItems = new List<WorkItemModel>(),
                        totalOwed = 0
                    };
                    model.reportItems.Add(reportItem);
                }
                if (workItem.paid == false && workItem.approvalStatus == ItemStatus.Status.Approved)
                {
                    reportItem.totalOwed += Convert.ToDouble(workItem.cost.Replace("$", ""));
                    reportItem.workItems.Add(workItem);
                }  
            }

            return View(model);
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
            var notificationModel = GetNotificationModel(userService, true, userID);
            notificationModel.openBox = false;
            notificationModel.showViewAllBtn = true;

            //Create WorkItem Model
            var workItemListModel = GetWorkItemListModel(userService, userID, userRole);

            //Create Dashboard Model
            var dashboardModel = new DashboardModel();
            dashboardModel.NotificationModel = notificationModel;
            dashboardModel.WorkItemListModel = workItemListModel;
            dashboardModel.userRole = userRole;

            return View(dashboardModel);
        }

        #region WorkItems

        /// <summary>
        /// Save a work item and return the update Work Item Partial to display
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public JsonResult SaveWorkItem(SaveWorkItemModel input)
        {
            var service = new UserService();
            var userID = (int)service.GetMyID();
            var userRole = service.GetUserRole(userID);
            if (userRole == Role.RoleTypes.Employee) //Employees can't edit items!
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var item = service.GetWorkItemFromID(input.itemID);
            var changedStatus = false;
            var changedDate = false;
            var changedAssignedTo = false;
            var changedCost = false;
            var changedPaidStatus = false;
            var changedDescription = false;
            var needUpdate = false;

            var originalStatus = item.Status;
            var originalDate = item.ItemDate.ToString("M/d/yy");
            var originalAssignedTo = item.AssignedTo;
            var originalCost = item.Cost;
            var originaPaidStatus = item.Paid;
            var originalDescription = item.WorkDescription;

            //DateCreated
            var dateNow = DateTimeOffset.Now;
            var dateNowFormated = dateNow.ToString("M/d/yy h:mm tt");

            //Get approver's name
            var user = service.GetUser(userID);
            var creatorName = user.FullName;

            var historyString = "";

            var postfixHistory = " by " + creatorName + " on " + dateNowFormated + ".";

            //if (originalStatus != input.newStatus)
            //{
            //    changedStatus = true;

            //    if (string.IsNullOrWhiteSpace(historyString))
            //        historyString += "<br/>";

            //    historyString += "Status changed from \"" + GetApprovalText((ItemStatus.Status)originalStatus) + "\" to \"" + GetApprovalText((ItemStatus.Status)input.newStatus) + "\"";
            //    item.Status = input.newStatus;
            //}

            //var newDate = input.newDate.ToString("M/d/yy");
            //if (originalDate != newDate)
            //{
            //    changedDate = true;

            //    if (string.IsNullOrWhiteSpace(historyString))
            //        historyString += "<br/>";

            //    historyString += "Date changed from \"" +originalDate+"\" to \""+newDate+"\"";
            //    item.ItemDate = input.newDate;
            //}

            var originallyAssignedTo = service.GetUser(originalAssignedTo);
            var newAssignedTo = originallyAssignedTo;
            if (originalAssignedTo != input.newAssigned)
            {
                needUpdate = true;
                changedAssignedTo = true;

                if (!string.IsNullOrWhiteSpace(historyString))
                    historyString += "<br/>";

                newAssignedTo = service.GetUser(input.newAssigned);
                item.UserAssignedTo = newAssignedTo;
                historyString += "Assigned from \"" +originallyAssignedTo.FullName+"\" to \""+newAssignedTo.FullName+"\"";
                item.AssignedTo = input.newAssigned;
            }

            var formattedNewCost = Convert.ToDecimal(input.newCost.Replace("$", "").Replace(",", ""));
            if (originalCost.ToString("C") != formattedNewCost.ToString("C"))
            {
                needUpdate = true;
                changedCost = true;

                if (!string.IsNullOrWhiteSpace(historyString))
                    historyString += "<br/>";

                historyString += "Cost changed from \"" + originalCost.ToString("C") + "\" to \"" + formattedNewCost.ToString("C")+"\"";
                item.Cost = formattedNewCost;
            }

            //if (originaPaidStatus != input.newPaid)
            //{
            //    changedPaidStatus = true;

            //    if (string.IsNullOrWhiteSpace(historyString))
            //        historyString += "<br/>";

            //    historyString += "Paid Status changed from \"" + GetPaidStatusText(originaPaidStatus) + "\" to \"" + GetPaidStatusText(input.newPaid)+ "\"";
            //    item.Paid = input.newPaid;
            //}

            if (originalDescription != input.newDescription)
            {
                needUpdate = true;
                changedDescription = true;

                if (!string.IsNullOrWhiteSpace(historyString))
                    historyString += "<br/>";

                historyString += "Description changed from \"" + originalDescription + "\' to \"" + input.newDescription+"\"";
                item.WorkDescription = input.newDescription;
            }

            if (!string.IsNullOrWhiteSpace(historyString))
                historyString += "<br/>" + postfixHistory;

            if (needUpdate == true)
            {
                try
                {
                    item.User = null; //need to get rid of any attachement
                    item.UserAssignedTo = null;
                    using (var context = new DbModels())
                    {
                        //context.Items.Attach(item);
                        context.Entry(item).State = EntityState.Modified;

                        //Create Work Item History item
                        var newHistory = new ItemHistory()
                        {
                            itemId = item.Id,
                            Note = historyString,
                            CreatedBy = userID,
                            CreatedOn = dateNow,
                        };
                        context.ItemHistories.Add(newHistory);


                        Notification newNotification;
                        //Send to Assigned To user if he is not the creator
                        if (newAssignedTo.Id != userID)
                        {
                            //newNotification = new Notification()
                            //{
                            //    AssignedTo = item.AssignedTo,
                            //    CreatedOn = dateNow,
                            //    ItemId = item.Id,
                            //    Type = (int)Notification.Types.ItemChanged,
                            //    New = true
                            //};
                            //context.Notifications.Add(newNotification);
                        }

                        if (changedAssignedTo == true)
                        {
                            if (newAssignedTo.Id != userID)
                            {
                                //newNotification = new Notification()
                                //{
                                //    AssignedTo = item.AssignedTo,
                                //    CreatedOn = dateNow,
                                //    ItemId = item.Id,
                                //    Type = (int)Notification.Types.AssignedTo,
                                //    New = true
                                //};
                                //context.Notifications.Add(newNotification);
                            }
                            if (originallyAssignedTo.Id != userID)
                            {
                                //newNotification = new Notification()
                                //{
                                //    AssignedTo = item.AssignedTo,
                                //    CreatedOn = dateNow,
                                //    ItemId = item.Id,
                                //    Type = (int)Notification.Types.ItemAssignedOff,
                                //    New = true
                                //};
                                //context.Notifications.Add(newNotification);
                            }
                        }

                        context.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                }
            }
            
            return GetWorkItemPartial(input.itemID,  input.isModal); //Get Html Partial for Work Item
        }

        /// <summary>
        /// Deny a work item and return the new History Item to display
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public JsonResult DenyWorkItem(int itemID)
        {
            var service = new UserService();
            var userID = (int)service.GetMyID();
            var userRole = service.GetUserRole(userID);
            if (userRole == Role.RoleTypes.Employee) //Employees can't deny item!
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var item = service.GetWorkItemFromID(itemID);
            item.Status = (int)ItemStatus.Status.Denied;

            //DateCreated
            var dateNow = DateTimeOffset.Now;
            var dateNowFormated = dateNow.ToString("M/d/yy h:mm tt");

            //Get approver's name
            var user = service.GetUser(userID);
            var creatorName = user.FullName;

            var historyText = "Denied by " + creatorName + " on " + dateNowFormated + ".";
            try
            {
                using (var context = new DbModels())
                {
                    context.Items.Attach(item);
                    context.Entry(item).State = EntityState.Modified;

                    //Create Work Item History
                    var newHistory = new ItemHistory()
                    {
                        itemId = item.Id,
                        Note = historyText,
                        CreatedBy = userID,
                        CreatedOn = dateNow,
                    };
                    context.ItemHistories.Add(newHistory);

                    //Notification newNotification;
                    ////Send to Assigned To user if he is not the creator
                    //newNotification = new Notification()
                    //{
                    //    AssignedTo = item.AssignedTo,
                    //    CreatedOn = dateNow,
                    //    ItemId = item.Id,
                    //    Type = (int)Notification.Types.Denied,
                    //    New = true
                    //};
                    //context.Notifications.Add(newNotification);

                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return GetWorkItemPartial(itemID, false); //Get Html Partial for Work Item

        }

        public JsonResult PayAllWorkItemsForUser(int userID)
        {
            var userList = new List<int>();
            userList.Add(userID);
            var service = new UserService();
            var workItems = service.GetWorkItemFromUserID(userList);

            //DateCreated
            var dateNow = DateTimeOffset.Now;
            var dateNowFormated = dateNow.ToString("M/d/yy h:mm tt");

            //Get approver's name
            var ownerID = (int)service.GetMyID();
            var user = service.GetUser(ownerID);
            var creatorName = user.FullName;
            var postfix = " by " + creatorName + " on " + dateNowFormated + ".";
            var historyText = "Paid Status changed from \"" + GetPaidStatusText(false) + "\" to \"" + GetPaidStatusText(true) + "\""+postfix;
            using (var context = new DbModels())
            {
                foreach (var workItem in workItems)
                {
                    if (workItem.Paid == true || workItem.Status != (int)ItemStatus.Status.Approved)
                        continue;

                    workItem.Paid = true;
                    try
                    {
                        context.Items.Attach(workItem);
                        context.Entry(workItem).State = EntityState.Modified;

                        //Create Work Item History
                        var newHistory = new ItemHistory()
                        {
                            itemId = workItem.Id,
                            Note = historyText,
                            CreatedBy = userID,
                            CreatedOn = dateNow,
                        };
                        context.ItemHistories.Add(newHistory);
                    }
                    catch (Exception e)
                    {
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                    }
                }
                context.SaveChanges();
            }
            
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Mark a work item as paid and return the new Work Item partial to display
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public JsonResult PaidWorkItem(int itemID, bool newStatus)
        {
            var service = new UserService();
            var userID = (int)service.GetMyID();
            var userRole = service.GetUserRole(userID);
            if (userRole == Role.RoleTypes.Employee) //Employees can't approve item!
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var item = service.GetWorkItemFromID(itemID);
            item.Paid = newStatus;
            

            //DateCreated
            var dateNow = DateTimeOffset.Now;
            var dateNowFormated = dateNow.ToString("M/d/yy h:mm tt");

            //Get approver's name
            var user = service.GetUser(userID);
            var creatorName = user.FullName;
            var postfix = " by " + creatorName + " on " + dateNowFormated + ".";
            var historyText = "Paid Status changed from \"" + GetPaidStatusText(!item.Paid) + "\" to \"" + GetPaidStatusText(item.Paid) + "\"";

            //Set to Approve if we set to Paid
            if (newStatus == true && item.Status != (int)ItemStatus.Status.Approved)
            {
                item.Status = (int)ItemStatus.Status.Approved;
                historyText += "<br/>Approved";
            }
            historyText += postfix;
            try
            {
                using (var context = new DbModels())
                {
                    context.Items.Attach(item);
                    context.Entry(item).State = EntityState.Modified;

                    //Create Work Item History
                    var newHistory = new ItemHistory()
                    {
                        itemId = item.Id,
                        Note = historyText,
                        CreatedBy = userID,
                        CreatedOn = dateNow,
                    };
                    context.ItemHistories.Add(newHistory);

                    //Notification newNotification;
                    ////Send to Assigned To user if he is not the creator
                    //newNotification = new Notification()
                    //{
                    //    AssignedTo = item.AssignedTo,
                    //    CreatedOn = dateNow,
                    //    ItemId = item.Id,
                    //    Type = (int)Notification.Types.Approved,
                    //    New = true
                    //};
                    //context.Notifications.Add(newNotification);

                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return GetWorkItemPartial(itemID, false); //Get Html Partial for Work Item

        }

        /// <summary>
        /// Approve a work item and return the new History Item to display
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public JsonResult ApproveWorkItem(int itemID)
        {
            var service = new UserService();
            var userID = (int)service.GetMyID();
            var userRole = service.GetUserRole(userID);
            if (userRole == Role.RoleTypes.Employee) //Employees can't approve item!
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var item = service.GetWorkItemFromID(itemID);
            item.Status = (int)ItemStatus.Status.Approved;

            //DateCreated
            var dateNow = DateTimeOffset.Now;
            var dateNowFormated = dateNow.ToString("M/d/yy h:mm tt");

            //Get approver's name
            var user = service.GetUser(userID);
            var creatorName = user.FullName;

            var historyText = "Approved by " + creatorName + " on " + dateNowFormated + ".";
            try
            {
                using (var context = new DbModels())
                {
                    context.Items.Attach(item);
                    context.Entry(item).State = EntityState.Modified;

                    //Create Work Item History
                    var newHistory = new ItemHistory()
                    {
                        itemId = item.Id,
                        Note = historyText,
                        CreatedBy = userID,
                        CreatedOn = dateNow,
                    };
                    context.ItemHistories.Add(newHistory);

                    //Notification newNotification;
                    ////Send to Assigned To user if he is not the creator
                    //newNotification = new Notification()
                    //{
                    //    AssignedTo = item.AssignedTo,
                    //    CreatedOn = dateNow,
                    //    ItemId = item.Id,
                    //    Type = (int)Notification.Types.Approved,
                    //    New = true
                    //};
                    //context.Notifications.Add(newNotification);

                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return GetWorkItemPartial(itemID, false); //Get Html Partial for Work Item

        }

        /// <summary>
        /// Get the Html Partial for a Work Item
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public JsonResult GetWorkItemPartial(int itemID, bool forModal)
        {
            var service = new UserService();
            var userID = (int)service.GetMyID();
            var userRole = service.GetUserRole(userID);
            var item = service.GetWorkItemFromID(itemID);
            var canApprove = CanApproveItem(userRole);
            

            var allowedUsers = service.GetAllowedUsers(userID);
            var statusOptionsList = getItemStatusOptions();
            var paidOptionsList = getPaidStatusOptions();

            //Create status options for editing work item
            var statusOptions = new SelectList(statusOptionsList, "Value", "Text", item.Status);

            //Create Paid Status options for editing work item
            var paidOptions = new SelectList(paidOptionsList, "Value", "Text", item.Paid);

            //Create Allowed Users options for editing work item
            var assignedOptions = new SelectList(allowedUsers, "Id", "FullName", item.AssignedTo);

            var itemModel = GetItemModelFromItem(item, canApprove);
            itemModel.statusOptions = statusOptions;
            itemModel.paidStatusOptions = paidOptions;
            itemModel.assignOptions = assignedOptions;
            itemModel.forModal = forModal;

            var viewString = RenderViewToString(this, "_WorkItem", itemModel);

            return Json(new { success = true, viewString = viewString }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get List of Work Items for this User
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public WorkItemListModel GetWorkItemListModel(UserService service, int userID, Role.RoleTypes role)
        {
            var model = new WorkItemListModel();
            model.workItems = new List<WorkItemModel>();
            //Get notifications tied to this user
            var allowedUsers = service.GetAllowedUsers(userID);
            var userIds = allowedUsers.Select(m => m.Id).ToList();
            //Can current user approve work items
            var canApprove = CanApproveItem(role);
            //Get Items from user ids
            List<Item> items = service.GetWorkItemFromUserID(userIds);

            var statusOptionsList = getItemStatusOptions();
            var paidOptionsList = getPaidStatusOptions();

            //Create Work Item Model from list
            WorkItemModel itemModel;
            foreach (var item in items)
            {
                //Create status options for editing work item
                var statusOptions = new SelectList(statusOptionsList, "Value", "Text", item.Status);

                //Create Paid Status options for editing work item
                var paidOptions = new SelectList(paidOptionsList, "Value", "Text", item.Paid);

                //Create Allowed Users options for editing work item
                var assignedOptions = new SelectList(allowedUsers, "Id", "FullName", item.AssignedTo);

                itemModel = GetItemModelFromItem(item, canApprove);
                itemModel.statusOptions = statusOptions;
                itemModel.paidStatusOptions = paidOptions;
                itemModel.assignOptions = assignedOptions;
                model.workItems.Add(itemModel);
            }
            return model;
        }

        /// <summary>
        /// Get Item Status options for Work Item Edit
        /// </summary>
        /// <returns></returns>
        private List<ItemStatusListModel> getItemStatusOptions()
        {
            //Status options for Work Item Edit
            var statusOptionsList = new List<ItemStatusListModel>();
            statusOptionsList.Add(new ItemStatusListModel() { Text = "Pending Approval", Value = (int)ItemStatus.Status.Pending });
            statusOptionsList.Add(new ItemStatusListModel() { Text = "Approved", Value = (int)ItemStatus.Status.Approved });
            statusOptionsList.Add(new ItemStatusListModel() { Text = "Denied", Value = (int)ItemStatus.Status.Denied });
            return statusOptionsList;
        }

        /// <summary>
        /// Get Paid Status options for Work Item Edit
        /// </summary>
        /// <returns></returns>
        private List<PaidStatusListModel> getPaidStatusOptions()
        {
            //Paid Status options for Work Item Edit
            var paidStatusOptionsList = new List<PaidStatusListModel>();
            paidStatusOptionsList.Add(new PaidStatusListModel() { Text = "Paid", Value = true });
            paidStatusOptionsList.Add(new PaidStatusListModel() { Text = "Not Paid", Value = false });
            return paidStatusOptionsList;
        }

        private bool CanApproveItem(Role.RoleTypes role)
        {
            var approve = false;

            if (role == Role.RoleTypes.Admin || role == Role.RoleTypes.Owner)
                approve = true;

            return approve;
        }

        private WorkItemModel GetItemModelFromItem(Item item, bool canApprove)
        {
            var itemModel = new WorkItemModel();
            itemModel.itemID = item.Id;
            itemModel.approvalStatus = (ItemStatus.Status)item.Status;
            itemModel.workStatus = (WorkItemStatus.Status)item.WorkStatus;
            itemModel.cost = item.Cost.ToString("C");
            itemModel.title = item.Name;
            itemModel.description = item.WorkDescription;
            itemModel.approval = GetApprovalText(itemModel.approvalStatus);
            itemModel.time = item.ItemDate.ToString("MM/dd/yy");
            itemModel.assignedTo = item.UserAssignedTo.FirstName + " " + item.UserAssignedTo.LastName;
            itemModel.assignedToId = item.AssignedTo;
            itemModel.paid = item.Paid;
            itemModel.paidString = GetPaidStatusText(item.Paid);
            itemModel.history = new List<string>();
            item.ItemHistories = item.ItemHistories.OrderByDescending(m => m.CreatedOn).ToList(); //Order Item History by descending date
            foreach(var history in item.ItemHistories)
            {
                itemModel.history.Add(history.Note);
            }
            itemModel.canApprove = canApprove;
            return itemModel;
        }

        private string GetApprovalText(ItemStatus.Status status)
        {
            var approvalText = "";
            switch (status)
            {
                case ItemStatus.Status.Approved:
                    approvalText = "Approved";
                    break;
                case ItemStatus.Status.Denied:
                    approvalText = "Denied";
                    break;
                case ItemStatus.Status.InReview:
                    approvalText = "In Review";
                    break;
                case ItemStatus.Status.Pending:
                    approvalText = "Pending Approval";
                    break;
                default:
                    approvalText = "Error";
                    break;

            }
            return approvalText;
        }

        private string GetPaidStatusText(bool paid)
        {
            var paidStatus = "Paid";
            if (paid == false)
            {
                paidStatus = "Not Paid";
            }
            return paidStatus;
        }

        #endregion WorkItems

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
                validationResults.Add("Work Report Name is Required.");
            }

            if (string.IsNullOrWhiteSpace(input.WorkDescription))
            {
                result = false;
                validationResults.Add("Work Report Description is Required.");
            }

            if (input.Cost == null || input.Cost <= 0)
            {
                result = false;
                validationResults.Add("Amount owed is missing or has an invalid value.");
            }

            if (input.Hours == null || input.Hours <= 0)
            {
                result = false;
                validationResults.Add("Hours Worked is missing or has an invalid value.");
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
                var dateNowFormated = dateNow.ToString("M/d/yy h:mm tt");
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
                            ItemDate = dateNow,
                            Name = input.Name,
                            CreatedBy = input.CreatedBy,
                            CreatedOn = dateNow,
                            ModifiedOn = dateNow,
                            Status = (int)newStatus,
                            WorkDescription = input.WorkDescription,
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
                            //newNotification = new Notification()
                            //{
                            //    AssignedTo = input.AssignedTo,
                            //    CreatedOn = dateNow,
                            //    ItemId = newItem.Id,
                            //    Type = (int)Notification.Types.AssignedTo,
                            //    New = true
                            //};
                            //context.Notifications.Add(newNotification);
                            needUpdate = true;
                        }
                        //Send to owner(s)
                        var ownerList = userService.GetOwners();
                        foreach (var owner in ownerList)
                        {
                            if (owner.Id == input.AssignedTo || owner.Id == input.CreatedBy)
                                continue;

                            //newNotification = new Notification()
                            //{
                            //    AssignedTo = owner.Id,
                            //    CreatedOn = dateNow,
                            //    ItemId = newItem.Id,
                            //    Type = (int)Notification.Types.Created,
                            //    New = true
                            //};
                            //context.Notifications.Add(newNotification);
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

        #region Notifications
        public ActionResult Notifications()
        {
            if (!checkAuthentication())
            {
                return RedirectToAction("Index");
            }
            var userService = new UserService();
            var userID = (int)userService.GetMyID();

            //Create a list of my notifications
            var notificationModel = GetNotificationModel(userService, false, userID);
            notificationModel.openBox = true;
            notificationModel.showViewAllBtn = false;

            return View(notificationModel);
        }

        /// <summary>
        /// Get List of Notifications for this User
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        private NotificationModel GetNotificationModel(UserService service, bool showOnlyNew, int myID)
        {
            var notificationModel = new NotificationModel();
            notificationModel.myNotifications = service.GetNotificationsForUser(myID, showOnlyNew);
            return notificationModel;
        }
        #endregion Notifications

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

        private string RenderViewToString(Controller controller, string viewName, object model)
        {
            using (var writer = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                controller.ViewData.Model = model;
                var viewCxt = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, writer);
                viewCxt.View.Render(viewCxt, writer);
                return writer.ToString();
            }
        }
    }
}