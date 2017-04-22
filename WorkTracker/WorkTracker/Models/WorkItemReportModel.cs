using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkTracker.Models
{
    public class WorkItemReportModel
    {
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }

        public double totalAmountOwed { get; set; }
        public List<WorkItemReportItemModel> reportItems { get; set; }
    }
}