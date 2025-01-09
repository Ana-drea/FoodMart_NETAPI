using Microsoft.AspNetCore.Identity;
using MiniMart.Models;

namespace MiniMart.Dtos
{
    public class orderRequestDto
    {
        public DateTime OrderDate { get; set; }
        public int StoreId { get; set; }
        public string PhoneNumber { get; set; }  // Phone number (10 digits)
    }
}
