using Microsoft.AspNetCore.Identity;

namespace MiniMart.Models
{
    public class OrderHistory
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string UserId { get; set; }
        public IdentityUser? User { get; set; }  // Optional user property
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); // Collection of order items
        public int StoreId { get; set; }
        public Store Store { get; set; } // Related Store
        public string PhoneNumber { get; set; }  // Phone number (10 digits)
        public bool IsCompleted { get; set; }    // Indicates if the order is paid
    }
}
