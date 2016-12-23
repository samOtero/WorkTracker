using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static WorkTracker.Models.Notification;

namespace WorkTracker.Models
{
    public class NotificationBox
    {
        public string text { get; set; }
        public bool isNew { get; set; }
        public Types type { get; set; }
    }
}