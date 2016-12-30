namespace WorkTracker.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class DbModels : DbContext
    {
        public DbModels()
            : base("name=DefaultConnection")
        {
            Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<Feature> Features { get; set; }
        public virtual DbSet<ItemHistory> ItemHistories { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<ItemStatus> ItemStatus { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<RoleFeature> RoleFeatures { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<WorkItemStatus> WorkItemStatus { get; set; }

        public virtual DbSet<NotificationStatus> NotificationStatus { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Feature>()
                .HasMany(e => e.RoleFeatures)
                .WithRequired(e => e.Feature)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Item>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Item>()
                .Property(e => e.Cost)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Item>()
                .HasMany(e => e.ItemHistories)
                .WithRequired(e => e.Item)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ItemStatus>()
                .HasMany(e => e.Items)
                .WithRequired(e => e.ItemStatu)
                .HasForeignKey(e => e.Status)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<WorkItemStatus>()
                .HasMany(e => e.Items)
                .WithRequired(e => e.WorkItemStatu)
                .HasForeignKey(e => e.WorkStatus)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Role>()
                .HasMany(e => e.RoleFeatures)
                .WithRequired(e => e.Role)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Role>()
                .HasMany(e => e.UserRoles)
                .WithRequired(e => e.Role)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.ItemHistories)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.CreatedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.Notifications)
                .WithRequired(e => e.User)
                .HasForeignKey(e => e.AssignedTo)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasMany(e => e.UserRoles)
                .WithRequired(e => e.User)
                .WillCascadeOnDelete(false);
        }
    }
}
