using System.Text.Json.Serialization;

namespace MiniMart.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int CartId { get; set; }
        [JsonIgnore] // Prevent circular references
        public Cart Cart { get; set; }
        [JsonIgnore] 
        public Product Product { get; set; }
    }
}
