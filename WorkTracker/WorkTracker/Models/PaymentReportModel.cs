using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WorkTracker.Models
{
    public class PaymentReportModel
    {
        public int workItemUserFilter { get; set; }
        public List<SelectListItem> userFilterOptions { get; set; }
        public List<WorkItemReportItemModel> reportItems { get; set; }
    }
}