using System.Text.Json.Serialization;

namespace MiniMart.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int CartId { get; set; }
        [JsonIgnore] // 阻止循环引用
        public Cart Cart { get; set; }
        [JsonIgnore] // 阻止循环引用
        public Product Product { get; set; }
    }
}
