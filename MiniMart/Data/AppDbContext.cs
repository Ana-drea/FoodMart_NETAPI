using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using MiniMart.Models;

namespace MiniMart.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // 定义 DbSet 属性以对应数据库中的表
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // 如果需要额外的模型配置，可以在此方法中完成
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 示例：设置表名为单数形式
            //modelBuilder.Entity<Category>().ToTable("Category");

            // 添加其他模型的约束或关系配置

            // 配置 Product 和 Category 的关系
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category) // 通过导航属性配置多对一关系
                .WithMany(c => c.Products) // Category 拥有多个 Product
                .HasForeignKey(p => p.CategoryId) // 外键是 CategoryId
                .OnDelete(DeleteBehavior.Restrict); // 设置删除行为为 Restrict

            // 配置 Cart 和 CartItem 的关系
            modelBuilder.Entity<Cart>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Cart)
                .HasForeignKey(ci => ci.CartId);

            // 配置 CartItem 和 Product 的关系
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product) // 一个 CartItem 关联一个 Product
                .WithMany(p => p.CartItems) // 一个 Product 可以出现在多个 CartItem 中
                .HasForeignKey(ci => ci.ProductId); // 外键是 ProductId
        }
    }
}
