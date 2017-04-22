using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkTracker.Models
{
    public class SaveWorkItemModel
    {
        public int itemID { get; set; }
        public int newStatus { get; set; }
        public DateTime newDate { get; set; }
        public int newAssigned { get; set; }
        public string newCost { get; set; }
        public bool newPaid { get; set; }
        public string newDescription { get; set; }
        public bool isModal { get; set; }
    }
}