using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTracker.Models
{
    public class CreateViewModel
    {

        [Required]
        [Display(Name = "Work Item Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Work Cost")]
        public decimal? Cost { get; set; }

        [Required]
        [Display(Name = "Work Date")]
        public DateTime? ItemDate { get; set; }

        [Required]
        [Display(Name = "Estimated Hours")]
        public int? Hours { get; set; }

        //User ID
        [Display(Name = "Work Assigned To")]
        public int AssignedTo { get; set; }

        //List of user that we can assign to this Work Item (list will differ from Employer and Employee)
        public List<User> AssignedToList { get; set; }

        public int CreatedBy { get; set; }

    }
}
