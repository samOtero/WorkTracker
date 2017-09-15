namespace WorkTracker.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Item
    {
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Item()
        {
            ItemHistories = new List<ItemHistory>();
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Column(TypeName = "money")]
        public decimal Cost { get; set; }

        public DateTimeOffset ItemDate { get; set; }

        public int Hours { get; set; }

        public bool Paid { get; set; }

        public int Status { get; set; }

        public int WorkStatus { get; set; }

        public string WorkDescription { get; set; }
        
        //User ID
        public int CreatedBy { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset ModifiedOn { get; set; }

        //User ID
        public int AssignedTo { get; set; }

        public bool Deleted { get; set; }

        public virtual ItemStatus ItemStatu { get; set; }

        public virtual WorkItemStatus WorkItemStatu { get; set; }

        public virtual List<ItemHistory> ItemHistories { get; set; }
        [NotMapped]
        public virtual User User { get; set; }
        [NotMapped]
        public virtual User UserAssignedTo { get; set; }
    }
}
