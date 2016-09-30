namespace WorkTracker.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class RoleFeature
    {
        public int Id { get; set; }

        public int roleId { get; set; }

        public int featureId { get; set; }

        public virtual Feature Feature { get; set; }

        public virtual Role Role { get; set; }
    }
}
