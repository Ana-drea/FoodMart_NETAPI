using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Data;
using MiniMart.Dtos;
using MiniMart.Models;
using MiniMart.Services;
using Stripe;
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
        private readonly PaymentService _paymentService;
        private readonly CheckIsAdminService _checkIsAdminService;

        public OrdersController(UserManager<IdentityUser> userManager, AppDbContext context, PaymentService paymentService, CheckIsAdminService checkIsAdminService)
        {
            _userManager = userManager;
            _context = context;
            _paymentService = paymentService;
            _checkIsAdminService = checkIsAdminService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetById([FromRoute] int id)
        {
            // 2. Find the order with that id, including order items and related product information
            var order = await _context.OrderHistories
                .Where(o => o.Id ==id)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Store) // Include the Store entity to get its name
                .FirstOrDefaultAsync();

            // 3. Check if there are any orders
            if (order == null)
            {
                return NotFound("The queired order doesn't exist."); // Return 404 if no orders are found
            }

            // 4. Map the order to an OrderResponseDto
            var orderDto = new OrderResponseDto
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                IsCompleted = order.IsCompleted,
                StoreName = order.Store?.Name ?? "Unknown", // Replace StoreId with Store.Name,
                Items = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice
                }).ToList()
            };

            return Ok(orderDto); // Return the DTO
        }

        [HttpGet]
        public async Task<IActionResult> GetUserOrders()
        {
            // 1. Retrieve the current user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(); // Return an unauthorized error if the user is not logged in
            }

            var userId = user.Id;

            var isAdmin = await _checkIsAdminService.CheckIsAdminAsync(user, user.Email);

            // 2. Query all orders for the user, including order items and related product information
            IQueryable<OrderHistory> ordersQuery = _context.OrderHistories
                .AsNoTracking() // Disable entity tracking to improve query performance
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Store) // Include the Store entity to get its name
                .OrderByDescending(o => o.OrderDate); // Sort orders by order date in descending order

            if (!isAdmin)
            {
                ordersQuery = ordersQuery.Where(o => o.UserId == userId);
            }

            var orders = await ordersQuery.ToListAsync();


            // 3. Check if there are any orders
            if (orders == null || !orders.Any())
            {
                return NotFound("No orders found for the current user."); // Return 404 if no orders are found
            }

            // 4. Map orders to a list of OrderResponseDto
            var orderResponseDtos = orders.Select(o => new OrderResponseDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                IsCompleted = o.IsCompleted,
                StoreName = o.Store?.Name ?? "Unknown", // Replace StoreId with Store.Name,
            }).ToList();

            return Ok(orderResponseDtos); // Return the list of DTOs
        }


        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] orderRequestDto orderRequestDto)
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

            // search for cart of that user, load CartItems and corresponding Product
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return BadRequest("Cart is empty or does not exist."); // If the cart is empty or not found, return a bad request response
            }

            //// 3. Generate the order number
            //int seed = (userId.GetHashCode() + orderRequestDto.OrderDate.GetHashCode()) % int.MaxValue; // Combine UserId and OrderDate as seed
            //var random = new Random(seed);
            //var randomDigits = random.Next(100000, 999999); // Generate a 6-digit random number
            //var orderNumber = $"NO{orderRequestDto.OrderDate:yyyyMMdd}-{randomDigits}"; // Format the order number as NOyyyyMMdd-XXXXXX


            // 4. Create a new OrderHistory instance
            var orderHistory = new OrderHistory
            {

                // OrderNumber = orderNumber,
                UserId = userId,
                OrderDate = orderRequestDto.OrderDate,
                TotalAmount = 0,
                StoreId = orderRequestDto.StoreId,
                PhoneNumber = orderRequestDto.PhoneNumber,
                IsCompleted = false, // Default to not completed
                OrderItems = new List<OrderItem>()
            };

            // Loop through the cart items
            foreach (var cartItem in cart.CartItems)
            {
                // Check the product's stock
                // var product = _context.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                var product = cartItem.Product;
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

                // Add the amount to total amount
                orderHistory.TotalAmount += product.Price * cartItem.Quantity;

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

            // Create Stripe payment intent
            // 1. Ensure the amount is valid
            if (orderHistory.TotalAmount <= 0)
            {
                return BadRequest("Invalid amount.");
            }

            // 2. Create the payment intent with the dynamic amount
            string clientSecret;
            try
            {
                var amount = (long)(orderHistory.TotalAmount * 100);
                var paymentIntent = await _paymentService.CreatePaymentIntentAsync(amount, "cad", orderHistory.Id.ToString());
                // Save the Payment Intent ID to the order history
                orderHistory.StripePI = paymentIntent.Id; // Assuming CreatePaymentIntentAsync returns the PaymentIntent object

                // Save the updated order history to the database
                _context.OrderHistories.Update(orderHistory);
                await _context.SaveChangesAsync();
                clientSecret = paymentIntent.ClientSecret;
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Failed to create payment intent: " + ex.Message });
            }

            // Return JSON response
            return Ok(new
            {
                clientSecret,
                paymentPageUrl = "/payment.html" // Redirect page for frontend
            });
        }
    }
}
