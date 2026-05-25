using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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

            modelBuilder.Entity<DeliveryItem>()
            .Property(di => di.PriceAtSale)
            .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.PriceAtSale)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasPrecision(18, 2);
        }

        public static void InitializeDatabase(ApplicationDbContext context)
        {
            if (!context.Database.CanConnect())
            {
                var databaseCreator = context.Database.GetService<IRelationalDatabaseCreator>();
                databaseCreator.Create();

                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var structureScriptPath = Path.Combine(baseDir, "DbScripts", "structure.sql");
                var seedScriptPath = Path.Combine(baseDir, "DbScripts", "seed.sql");

                if (File.Exists(structureScriptPath))
                {
                    var structureSql = File.ReadAllText(structureScriptPath);
                    ExecuteRawSqlScript(context, structureSql);
                }

                if (File.Exists(seedScriptPath))
                {
                    var seedSql = File.ReadAllText(seedScriptPath);
                    ExecuteRawSqlScript(context, seedSql);
                }
            }
        }

        private static void ExecuteRawSqlScript(ApplicationDbContext context, string script)
        {
            var commandTexts = script.Split(new[] { "\r\nGO\r\n", "\nGO\n", "\rGO\r" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var commandText in commandTexts)
            {
                if (!string.IsNullOrWhiteSpace(commandText))
                {
                    context.Database.ExecuteSqlRaw(commandText);
                }
            }
        }

        // Procedures
        public async Task<List<Product>> sp_GetProductsNeedingReorder()
        {
            return await Product.FromSqlRaw("EXECUTE [dbo].[Get_Products_Needing_Reorder]").ToListAsync();
        }
    }
}
