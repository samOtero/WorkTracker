using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkTracker.Models
{
    public class WorkItemModel
    {
        public int itemID { get; set; }
        public string title { get; set; }
        public string approval { get; set; }
        public string time { get; set; }
        public string assignedTo { get; set; }
        public string cost { get; set; }
        public ItemStatus.Status approvalStatus { get; set; }
        public WorkItemStatus.Status workStatus { get; set; }
        public List<string> history { get; set; }
        public bool canApprove { get; set; }

        public bool forModal { get; set; }
    }
}