namespace WorkTracker.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Notification
    {
        public enum Types
        {
            AssignedTo = 1,
            Created = 2
        }
        public int Id { get; set; }

        public int AssignedTo { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public int ItemId { get; set; }

        public int Type { get; set; }

        public bool New { get; set; }

        public virtual User User { get; set; }

        public virtual Item Item { get; set; }
    }
}
