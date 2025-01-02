using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MiniMart.Data;
using MiniMart.Dtos;
using MiniMart.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiniMart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        public OrdersController(UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostOrder([FromBody] OrderHistoryDto orderHistoryDto)
        {
            // 1. Validate the model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // If the model is invalid, return a bad request response
            }

            // 2. Get the current user's information
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found."); // If no user is found, return an unauthorized error
            }

            var userId = user.Id;

            // Retrieve the user's cart
            var cart = _context.Carts
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return BadRequest("Cart is empty or does not exist."); // If the cart is empty or not found, return a bad request response
            }

            // 3. Generate the order number
            int seed = (userId.GetHashCode() + orderHistoryDto.OrderDate.GetHashCode()) % int.MaxValue; // Combine UserId and OrderDate as seed
            var random = new Random(seed);
            var randomDigits = random.Next(100000, 999999); // Generate a 6-digit random number
            var orderNumber = $"NO{orderHistoryDto.OrderDate:yyyyMMdd}-{randomDigits}"; // Format the order number as NOyyyyMMdd-XXXXXX
            
            // 4. Create a new OrderHistory instance
            var orderHistory = new OrderHistory
            {
                UserId = userId,
                OrderNumber = orderNumber,
                OrderDate = orderHistoryDto.OrderDate,
                StoreId = orderHistoryDto.StoreId,
                PhoneNumber = orderHistoryDto.PhoneNumber,
                IsCompleted = false, // Default to not completed
                OrderItems = new List<OrderItem>()
            };

            // Loop through the cart items
            foreach (var cartItem in cart.CartItems)
            {
                // Check the product's stock
                var product = _context.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                if (product == null)
                {
                    return BadRequest($"Product with ID {cartItem.ProductId} does not exist."); // If the product does not exist, return an error
                }

                if (cartItem.Quantity > product.QuantityInStock)
                {
                    return BadRequest($"Insufficient stock for product: {product.Name}"); // If there is not enough stock, return an error
                }

                // Reduce the product's stock
                product.QuantityInStock -= cartItem.Quantity;

                // Convert CartItem to OrderItem
                var orderItem = new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = product.Price,
                    TotalPrice = cartItem.Quantity * product.Price
                };

                orderHistory.OrderItems.Add(orderItem); // Add the order item to the order
            }

            // Save the order
            _context.OrderHistories.Add(orderHistory);

            // Clear the cart
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.Carts.Remove(cart);

            // Commit the changes to the database
            await _context.SaveChangesAsync();

            return Ok(); 
        }
    }
}
