using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.EntityFrameworkCore;
using WholesalerManager.Core.Domain.Entities;

namespace WholesaleManager.Infrastructure.DatabaseContext
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<Category> Category { get; set; }
        public virtual DbSet<Supplier> Supplier { get; set; }
        public virtual DbSet<Delivery> Delivery { get; set; }
        public virtual DbSet<DeliveryItem> DeliveryItem { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeliveryItem>().HasKey(di => new { di.DeliveryID, di.ProductID });
        }
    }
}
