namespace MiniMart.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderHistoryId { get; set; }
        public OrderHistory? OrderHistory { get; set; }  // Optional order history
        public int ProductId { get; set; }
        public Product Product { get; set; }  // Assuming Product class exists
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; } // Total price = Quantity * UnitPrice
    }
}
