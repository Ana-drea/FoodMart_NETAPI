namespace MiniMart.Dtos
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsCompleted { get; set; }
        public String StoreName { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }
}
