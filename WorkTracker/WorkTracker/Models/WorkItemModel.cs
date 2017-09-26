using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorkTracker.Models
{
    public class WorkItemModel
    {
        public int itemID { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string approval { get; set; }
        public string time { get; set; }
        public string assignedTo { get; set; }
        public int assignedToId { get; set; }
        public string cost { get; set; }
        public ItemStatus.Status approvalStatus { get; set; }
        public WorkItemStatus.Status workStatus { get; set; }
        public List<string> history { get; set; }
        public bool canApprove { get; set; }
        public bool canEdit { get; set; }
        public bool forModal { get; set; }
        public bool paid { get; set; }
        public string paidString { get; set; }
        public string hours { get; set; }
        public SelectList paidStatusOptions { get; set; }
        public SelectList statusOptions { get; set; }
        public SelectList assignOptions { get; set; }
        public DateTimeOffset createdOn { get; set; }
    }
}