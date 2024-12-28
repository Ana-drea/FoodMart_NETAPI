using System.ComponentModel.DataAnnotations;

namespace MiniMart.Dtos
{
    public class ProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        [Range(0, int.MaxValue)]
        [DisplayFormat(DataFormatString = "{0:F2}", ApplyFormatInEditMode = true)]
        public decimal Price { get; set; }
        [Range(0, int.MaxValue)]
        public int? QuantityInStock { get; set; }
        public string? ImageUrl { get; set; }
    }
}
