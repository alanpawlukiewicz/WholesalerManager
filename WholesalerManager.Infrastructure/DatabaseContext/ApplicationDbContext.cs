using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WholesalerManager.Core.Domain.Entities;
using WholesalerManager.Core.Domain.IdentityEntities;

namespace WholesalerManager.Infrastructure.DatabaseContext
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Supplier> Supplier { get; set; }
        public virtual DbSet<Delivery> Delivery { get; set; }
        public virtual DbSet<DeliveryItem> DeliveryItem { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<OrderItem> OrderItem { get; set; }

        public virtual DbSet<AuditLog> AuditLog { get; set; }
        public virtual DbSet<InventoryLog> InventoryLog { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Allows to use triggers in the database without EF Core trying to get the inserted/updated entity back from the database, which can cause issues if the trigger modifies the data.
            modelBuilder.Entity<Delivery>()
                .ToTable(tb => tb.UseSqlOutputClause(false));
            modelBuilder.Entity<Order>()
                .ToTable(tb => tb.UseSqlOutputClause(false));
        }
    }
}
