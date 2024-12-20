using Microsoft.EntityFrameworkCore;
using MiniMart.Models;

namespace MiniMart.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // 定义 DbSet 属性以对应数据库中的表
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

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
        }
    }
}
