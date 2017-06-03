using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorkTracker.Models
{
    public class DashboardModel
    {
        public NotificationModel NotificationModel;
        public WorkItemListModel WorkItemListModel;
        public Role.RoleTypes userRole;
        public int workItemUserFilter { get; set; }
        public int workItemPaidStatusFilter { get; set; }
        public List<SelectListItem> userFilterOptions { get; set; }
        public List<SelectListItem> paidStatusFilterOptions { get; set; }
    }
}