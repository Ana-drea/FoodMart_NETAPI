using System.Text.Json.Serialization;

namespace MiniMart.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [JsonIgnore] // 避免循环引用
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
