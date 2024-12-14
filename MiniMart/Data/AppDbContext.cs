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

        // 如果需要额外的模型配置，可以在此方法中完成
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 示例：设置表名为单数形式
            modelBuilder.Entity<Category>().ToTable("Category");

            // 添加其他模型的约束或关系配置
        }
    }
}
