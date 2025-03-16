using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Data;
using MiniMart.Models;
using MiniMart.Dtos;

namespace MiniMart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        public CartsController(UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            // use User to get currently user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var userId = user.Id;

            // search for cart of that user, load CartItems and corresponding Product
            var cart = await _context.Carts
                .Include(c => c.CartItems) 
                    .ThenInclude(ci => ci.Product) 
                .FirstOrDefaultAsync(c => c.UserId == userId);


            if (cart == null)
            {
                return NotFound(new { message = "Cart not found for the user." });
            }

            // return data including cart, cartItems and products
            return Ok(new
            {
                cartId = cart.Id,
                totalPrice = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity), // 计算总价
                items = cart.CartItems.Select(ci => new
                {
                    productId = ci.Product.Id,
                    productName = ci.Product.Name,
                    productPrice = ci.Product.Price,
                    quantity = ci.Quantity,
                    productImage = ci.Product.ImageUrl
                })
            });

        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItemDto cartItemDto)
        {
            if (cartItemDto == null 
                || cartItemDto.ProductId <= 0 
                || (cartItemDto.Change == null && cartItemDto.Quantity == null) 
                || (cartItemDto.Change <= 0 && cartItemDto.Quantity < 0))
            {
                return BadRequest("Invalid product ID or quantity.");
            }


            // use User to get currently user
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found or not logged in.");
            }

            var userId = user.Id;

            // search for cart of that user
            var cart = await _context.Carts
                .Include(c => c.CartItems) // load cart items to cart
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // create cart if it doesn't exist
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };

                _context.Carts.Add(cart);
            }

            // search if the cartItem already exists the cart
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == cartItemDto.ProductId);

            // Get the product for the cart item
            var product = _context.Products.FirstOrDefault(p=>p.Id == cartItemDto.ProductId);

            if (existingCartItem != null)
            {
                // update quantity if it does
                // if frontend passed in Change, increase or decrease existingCartItem's quantity by Change
                if (cartItemDto.Change.HasValue&& cartItemDto.Change!=0)
                {
                    if(existingCartItem.Quantity + cartItemDto.Change.Value>product.QuantityInStock)
                    {
                        return BadRequest("Product quantity exceeds quantity in stock!");
                    }
                    existingCartItem.Quantity += cartItemDto.Change.Value;
                }
                // if frontend passed in Quantity, update existingCartItem's quantity
                else if (cartItemDto.Quantity.HasValue)
                {
                    existingCartItem.Quantity = cartItemDto.Quantity.Value;
                }
                // if the quantity goes to 0, remove from cart
                if (existingCartItem.Quantity <= 0)
                {
                    cart.CartItems.Remove(existingCartItem);
                    // remove cart if there is no cart item after update
                    if (cart.CartItems.Count == 0)
                    {
                        _context.Carts.Remove(cart);
                    }
                }
            }
            else
            {
                // add cartItem to cart if it doesn't
                var newCartItem = new CartItem
                {
                    ProductId = cartItemDto.ProductId,
                    Quantity = cartItemDto.Change.HasValue && cartItemDto.Change > 0 ? cartItemDto.Change.Value : cartItemDto.Quantity.Value
                };
                if (newCartItem.Quantity > product.QuantityInStock)
                {
                    return BadRequest("Product quantity exceeds quantity in stock!");
                }
                // only add new item if the final quantity is greater than 0
                if (newCartItem.Quantity >= 0)
                {
                    cart.CartItems.Add(newCartItem);
                }
                
            }

            // save the change
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product added to cart successfully." });
        }
    }

}

