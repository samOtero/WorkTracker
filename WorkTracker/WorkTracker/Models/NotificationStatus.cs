namespace WorkTracker.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class NotificationStatus
    {
        public int Id { get; set; }

        public string name { get; set; }

        public int value { get; set; }

        public string description { get; set; }
    }
}
