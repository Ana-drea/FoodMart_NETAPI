using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MiniMart.Models;
using Microsoft.AspNetCore.Identity;

namespace MiniMart.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // define DbSet to represent tables in db
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // If additional model configuration is required, it can be done in this method
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // add constraints or relationship configurations for other models

            // relationship between Product and Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category) // Configure a many-to-one relationship via the navigation property
                .WithMany(c => c.Products) // A Category can have multiple Products
                .HasForeignKey(p => p.CategoryId) // The foreign key is CategoryId
                .OnDelete(DeleteBehavior.Restrict); // Set the delete behavior to Restrict

            // relationship between Cart and CartItem
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems) 
                .WithOne(ci => ci.Cart) 
                .HasForeignKey(ci => ci.CartId); 

            // relationship between CartItem and Product
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product) 
                .WithMany(p => p.CartItems) 
                .HasForeignKey(ci => ci.ProductId); 

        }
    }
}
