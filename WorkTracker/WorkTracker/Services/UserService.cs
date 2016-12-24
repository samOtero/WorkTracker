using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WorkTracker.Models;

namespace WorkTracker.Services
{
    public class UserService
    {
        #region Work Items
        /// <summary>
        /// Get a Work Item by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Item GetWorkItemFromID(int id)
        {
            Item item = null;
            using (var context = new DbModels())
            {
                item = context.Items.Where(m => m.Id == id)
                    .Include(m => m.User)
                    .Include(m => m.UserAssignedTo)
                    .Include(m => m.ItemHistories)
                    .FirstOrDefault();
            }
            return item;
        }

        public List<Item> GetWorkItemFromUserID(List<int> ids)
        {
            List<Item> items = new List<Item>();
            using (var context = new DbModels())
            {
                items = context.Items.Where(m => ids.Contains(m.AssignedTo))
                    .Include(m => m.User)
                    .Include(m => m.UserAssignedTo)
                    .Include(m => m.ItemHistories)
                    .OrderByDescending(m => m.ItemDate)
                    .OrderByDescending(m => m.Status)
                    .ToList();
            }
            return items;
        }
        #endregion Work Items


        #region Notifications

        /// <summary>
        /// Set the New Notifications of a particular user to false
        /// </summary>
        /// <param name="myID"></param>
        public void SetNotificationToViewed(int myID)
        {
            using (var context = new DbModels())
            {
                context.Notifications.Where(m => m.AssignedTo == myID && m.New == true).ToList().ForEach(m => m.New = false);
                context.SaveChanges();

            }
        }
        /// <summary>
        /// Get Notification Box list for a particular user
        /// </summary>
        /// <param name="myID">User's ID</param>
        /// <param name="onlyNew">Only get list of "New" Notifications</param>
        /// <returns></returns>
        public List<NotificationBox> GetNotificationsForUser(int myID, bool onlyNew)
        {
            var notificationList = new List<NotificationBox>();
            var myRole = GetUserRole(myID);
            List<Notification> notifications;
            List<NotificationStatus> notificationStatuses;
            using (var context = new DbModels())
            {
                IQueryable<Notification> query;
                //If you are an Admin then get all Notifications (but don't set them as Seen (unless they are assigned to you!))
                if (myRole == Role.RoleTypes.Admin)
                {
                    query = context.Notifications.AsQueryable();
                }
                else //Get Notifications Assigned to you
                {
                    query = context.Notifications.Where(m => m.AssignedTo == myID);
                }
                //Only grab the new Notifications not seen
                if (onlyNew == true)
                {
                    query = query.Where(m => m.New == true);
                }

                //Order by newest first
                query = query.OrderByDescending(m => m.CreatedOn);

                notifications = query.ToList();

                //Get all the types of notifications to get the correct formatting
                notificationStatuses = context.NotificationStatus.ToList();
            }

            //Go through all the Notifications and create a usable NotificationBox object for each to return in the list
            NotificationBox currentNote;
            NotificationStatus currentNoteStatus;
            foreach(var note in notifications)
            {
                //Get the correct status type for the current Notification
                currentNoteStatus = notificationStatuses.Where(m => m.value == note.Type).FirstOrDefault();
                currentNote = new NotificationBox()
                {
                    isNew = note.New,
                    text = GetNotificationText(note, currentNoteStatus),
                    type = (Notification.Types)note.Type
                };

                notificationList.Add(currentNote);
            }
            SetNotificationToViewed(myID); //Set the ones belonging to you to false
            return notificationList;
        }

        /// <summary>
        /// Create Notification text from note and type
        /// </summary>
        /// <param name="note"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        private string GetNotificationText(Notification note, NotificationStatus status)
        {
            var noteText = "";
            var item = GetWorkItemFromID(note.ItemId);
            var noteFormatText = status.description;
            var workItemText = "<span class=\"itemLink\" data-itemid=\"" + note.ItemId + "\">Work Item</span>";
            
            if (note.Type == (int)Notification.Types.Approved || note.Type == (int)Notification.Types.Denied )
            {
                var owners = GetOwners();
                var owner = owners.First();
                noteText = string.Format(noteFormatText, workItemText, owner.FirstName + " " + owner.LastName);
            }
            else
            {
                var creatorUser = GetUser(item.CreatedBy);
                noteText = string.Format(noteFormatText, workItemText, creatorUser.FullName);
            }
            
            noteText += " ("+note.CreatedOn.ToString("MM/dd/yy")+")";
            return noteText;
        }

        #endregion Notifications

        /// <summary>
        /// Get current logged user's ID from Session variable
        /// </summary>
        /// <returns></returns>
        public int? GetMyID()
        {
            return (int?)HttpContext.Current.Session[WorkTracker.Models.User.ID];
        }

        /// <summary>
        /// Set the ID for the current logged in user in the Session variable
        /// </summary>
        /// <param name="newID"></param>
        public void SetMyID(int newID)
        {
            HttpContext.Current.Session[WorkTracker.Models.User.ID] = newID;
        }

        public List<User> GetOwners()
        {
            List<User> ownerList = new List<User>();

            using (var context = new DbModels())
            {
                ownerList = (from u in context.Users
                            join r in context.UserRoles on u.Id equals r.userId
                            where r.roleId == (int)Role.RoleTypes.Owner
                            select u).ToList();
            }

            return ownerList;
        }
        
        public User GetUser(int myId)
        {
            User user = null;
            using (var context = new DbModels())
            {
                user = context.Users.Where(m => m.Id == myId)
                    .Include(m => m.UserRoles)
                    .FirstOrDefault();
            }
            formatUser(user);
            return user;
        }

        public Role.RoleTypes GetUserRole(int myId)
        {
            Role.RoleTypes myRole = Role.RoleTypes.Employee;
            using (var context = new DbModels())
            {
                var user = context.UserRoles.Where(m => m.userId == myId)
                    .FirstOrDefault();
                if (user != null)
                {
                    var myRoleID = user.roleId;
                    myRole = (Role.RoleTypes)myRoleID;
                }

            }
            return myRole;
        }

        /// <summary>
        /// Get list of Allowed users, that this particular user can edit/assign task to
        /// </summary>
        /// <param name="myId">Id of user making the request</param>
        /// <returns></returns>
        public List<User> GetAllowedUsers(int myId)
        {
            var allowedUsers = new List<User>();
            using (var context = new DbModels())
            {
                //Get the current user's role to see who will we add to the list
                var myUserRoles = GetUserRole(myId);
                if (myUserRoles != null)
                {
                    //If the user is an employee only add themselves to the list
                    if (myUserRoles == Role.RoleTypes.Employee)
                    {
                        var oneUser = context.Users.Where(m => m.Id == myId).FirstOrDefault();
                        if (oneUser != null)
                        {
                            allowedUsers.Add(oneUser);
                        }
                    }
                    else if (myUserRoles == Role.RoleTypes.Owner) //If the user is an Owner add only himself and employees, NOT Admin Users
                    {
                        var myselfAndEmployees =
                            from u in context.Users
                            join ur in context.UserRoles on u.Id equals ur.userId
                            where ur.roleId == (int)Role.RoleTypes.Owner || ur.roleId == (int)Role.RoleTypes.Employee
                            select u;
                        allowedUsers.AddRange(myselfAndEmployees);
                    }
                    else if (myUserRoles == Role.RoleTypes.Admin) //If the user is an Adminthen add a list of everybody
                    {
                        var allUsers = context.Users.ToList();
                        allowedUsers.AddRange(allUsers);
                    }
                }
            }
            formatUser(allowedUsers);
            return allowedUsers;
        }

        /// <summary>
        /// Add any necessary format and data for users
        /// </summary>
        /// <param name="user"></param>
        public void formatUser(IEnumerable<User> userList)
        {
            if (userList == null)
                return;

            foreach (var user in userList)
            {
                formatUser(user);
            }
        }

        /// <summary>
        /// Add any necessary format and data for users
        /// </summary>
        /// <param name="user"></param>
        public void formatUser(User user)
        {
            if (user == null)
                return;

             //Create FullName field
            user.FullName = user.FirstName + " " + user.LastName;
        }
    }
}