using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkTracker.Models
{
    public class DashboardModel
    {
        public NotificationModel NotificationModel;
        public WorkItemListModel WorkItemListModel;
        public Role.RoleTypes userRole;
    }
}