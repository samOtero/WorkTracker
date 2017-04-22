using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkTracker.Models
{
    public class WorkItemReportItemModel
    {
        public int userId { get; set; }
        public string userFullName { get; set; }
        public double totalOwed { get; set; }
        public List<WorkItemModel> workItems { get; set; }
    }
}