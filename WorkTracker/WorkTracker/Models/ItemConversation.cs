namespace WorkTracker.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ItemConversation")]
    public partial class ItemConversation
    {
        public int Id { get; set; }

        public int itemId { get; set; }

        [Required]
        public string Note { get; set; }

        public int CreatedBy { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public virtual Item Item { get; set; }

        public virtual User User { get; set; }
    }
}
