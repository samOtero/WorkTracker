using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WorkTracker.Models
{
    public class CreateViewModel
    {

        [Required]
        [Display(Name = "Work Report Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Amount owed for this Work Report")]
        public decimal? Cost { get; set; }

        [Required]
        [Display(Name = "Work Date")]
        public DateTime? ItemDate { get; set; }

        [Required]
        [Display(Name = "Hours Worked")]
        public int? Hours { get; set; }

        //User ID
        [Display(Name = "Work done by")]
        public int AssignedTo { get; set; }

        [Display(Name = "Work Report Description")]
        public string WorkDescription { get; set; }

        //List of user that we can assign to this Work Item (list will differ from Employer and Employee)
        public List<User> AssignedToList { get; set; }

        public int CreatedBy { get; set; }

    }
}
