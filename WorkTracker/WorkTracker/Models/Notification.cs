namespace WorkTracker.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Notification
    {
        public int Id { get; set; }

        [Required]
        public string Note { get; set; }

        public int AssignedTo { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public virtual User User { get; set; }
    }
}
