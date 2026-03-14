using Microsoft.EntityFrameworkCore;
using btap_api_orm.Models;

namespace btap_api_orm.Data
{
    public class OrderManagementContext : DbContext
    {
        public OrderManagementContext(DbContextOptions<OrderManagementContext> options)
            : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);
                entity.Property(e => e.CustomerId).HasColumnName("Id");
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).HasColumnName("FullName");
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Email).HasColumnName("Email");
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Phone).HasColumnName("Phone");
            });
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.ProductId).HasColumnName("Id");
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.ProductName).HasColumnName("ProductName");
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Price).HasColumnName("Price");
                entity.Property(e => e.StockQuantity).HasColumnName("StockQuantity");
            });
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.OrderId).HasColumnName("Id");
                entity.Property(e => e.CustomerId).HasColumnName("CustomerId");
                entity.Property(e => e.OrderDate).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);

                entity.HasOne(e => e.Customer)
                      .WithMany(c => c.Orders)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.OrderDetailId);
                entity.Property(e => e.OrderDetailId).HasColumnName("Id");
                entity.Property(e => e.OrderId).HasColumnName("OrderId");
                entity.Property(e => e.ProductId).HasColumnName("ProductId");
                entity.Property(e => e.Quantity).HasColumnName("Quantity");
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.UnitPrice).HasColumnName("UnitPrice");
                entity.HasIndex(e => new { e.OrderId, e.ProductId })
                      .IsUnique()
                        .HasDatabaseName("UQ_Order_Product");

                entity.HasOne(e => e.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                      .WithMany(p => p.OrderDetails)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

