using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTracker.Models
{
    public class NotificationModel
    {
        public List<NotificationBox> myNotifications;
        public bool openBox;
        public bool showViewAllBtn;
    }

   
}
