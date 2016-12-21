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
        
        public User GetUser(int myId)
        {
            User user = null;
            using (var context = new DbModels())
            {
                user = context.Users.Where(m => m.Id == myId)
                    .Include(m => m.UserRoles)
                    .FirstOrDefault();
            }
            return user;
        }

        public Role.RoleTypes GetUserRole(int myId)
        {
            Role.RoleTypes myRole = Role.RoleTypes.Employee;
            using (var context = new DbModels())
            {
                var user = context.Users.Where(m => m.Id == myId)
                    .Include(m => m.UserRoles)
                    .FirstOrDefault();
                if (user != null)
                {
                    var myRoleID = user.UserRoles.FirstOrDefault().roleId;
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
                var myUserRoles = context.UserRoles.Where(m => m.userId == myId).FirstOrDefault();
                if (myUserRoles != null)
                {
                    //If the user is an employee only add themselves to the list
                    if (myUserRoles.roleId == (int)Role.RoleTypes.Employee)
                    {
                        var oneUser = context.Users.Where(m => m.Id == myId).FirstOrDefault();
                        if (oneUser != null)
                        {
                            allowedUsers.Add(oneUser);
                        }
                    }
                    else if (myUserRoles.roleId == (int)Role.RoleTypes.Owner) //If the user is an Owner add only himself and employees, NOT Admin Users
                    {
                        var myselfAndEmployees =
                            from u in context.Users
                            join ur in context.UserRoles on u.Id equals ur.userId
                            where ur.roleId == (int)Role.RoleTypes.Owner || ur.roleId == (int)Role.RoleTypes.Employee
                            select u;
                        allowedUsers.AddRange(myselfAndEmployees);
                    }
                    else if (myUserRoles.roleId == (int)Role.RoleTypes.Admin) //If the user is an Adminthen add a list of everybody
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