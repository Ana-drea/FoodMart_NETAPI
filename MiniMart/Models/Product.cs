using System.ComponentModel.DataAnnotations;

namespace MiniMart.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        [Range(0, int.MaxValue)]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }
        [Range(0, int.MaxValue)]
        public int? QuantityInStock { get; set; }
        public string? ImageUrl { get; set; }

        // Add navigation property to associate a product with a category
        public Category? Category { get; set; } = null!; 

        // Navigation property: A product may appear in multiple cart items
        public ICollection<CartItem> CartItems { get; set; } 

    }
}
