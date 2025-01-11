using MiniMart.Models;

namespace MiniMart.Dtos
{
    public class ProductsResponseDto
    {
        public int TotalNumber { get; set; } // Number of products that meet the searching conditions
        public List<Product> Products { get; set; } = new List<Product>(); // List of products for the current page
    }
}
