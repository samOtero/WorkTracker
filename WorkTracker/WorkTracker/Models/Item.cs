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
        [Display(Name = "Work Report Name")]
        public string Name { get; set; }

        [Column(TypeName = "money")]
        [Display(Name = "Amount owed for Work")]
        public decimal Cost { get; set; }

        [Display(Name = "Work Date")]
        public DateTimeOffset ItemDate { get; set; }

        [Display(Name = "Hours Worked")]
        public int Hours { get; set; }

        public bool Paid { get; set; }

        public int Status { get; set; }

        public int WorkStatus { get; set; }
        
        //User ID
        public int CreatedBy { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset ModifiedOn { get; set; }

        //User ID
        [Display(Name = "Work Assigned To")]
        public int AssignedTo { get; set; }

        public virtual ItemStatus ItemStatu { get; set; }

        public virtual WorkItemStatus WorkItemStatu { get; set; }

        public virtual List<ItemHistory> ItemHistories { get; set; }

        public virtual User User { get; set; }

        public virtual User UserAssignedTo { get; set; }
    }
}
