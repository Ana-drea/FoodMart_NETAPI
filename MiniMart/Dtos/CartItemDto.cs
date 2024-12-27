namespace MiniMart.Dtos
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public int? Change { get; set; } // frontend can choose to pass in either change or quantity
        public int? Quantity { get; set; }  
        

    }
}
