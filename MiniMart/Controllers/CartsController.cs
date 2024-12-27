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
    [Authorize]
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
            // 使用 User 获取当前登录用户
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { message = "User is not logged in." });
            }

            var userId = user.Id;

            // 查找该用户的购物车，并加载 CartItems 和对应的 Product
            var cart = await _context.Carts
                .Include(c => c.CartItems) // 加载购物车中的项目
                    .ThenInclude(ci => ci.Product) // 加载购物车项目对应的商品
                .FirstOrDefaultAsync(c => c.UserId == userId);


            if (cart == null)
            {
                return NotFound(new { message = "Cart not found for the user." });
            }

            // 返回包含购物车和商品详细信息的数据
            return Ok(new
            {
                cartId = cart.Id,
                totalPrice = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity), // 计算总价
                items = cart.CartItems.Select(ci => new
                {
                    productId = ci.Product.Id,
                    productName = ci.Product.Name,
                    productPrice = ci.Product.Price,
                    quantity = ci.Quantity
                })
            });

        }

        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartItemDto cartItemDto)
        {
            if (cartItemDto == null || cartItemDto.ProductId <= 0 || cartItemDto.Quantity <= 0)
            {
                return BadRequest("Invalid product ID or quantity.");
            }

            // 获取当前用户
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized("User not found or not logged in.");
            }

            var userId = user.Id;

            // 查找该用户的购物车
            var cart = await _context.Carts
                .Include(c => c.CartItems) // 加载购物车中的项目
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // 如果购物车不存在，为用户创建新购物车
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };

                _context.Carts.Add(cart);
            }

            // 查找购物车中是否已有该商品
            var existingCartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == cartItemDto.ProductId);

            if (existingCartItem != null)
            {
                // 如果商品已存在，更新数量
                existingCartItem.Quantity += cartItemDto.Quantity;
            }
            else
            {
                // 如果商品不存在，添加新商品到购物车
                var newCartItem = new CartItem
                {
                    ProductId = cartItemDto.ProductId,
                    Quantity = cartItemDto.Quantity
                };
                cart.CartItems.Add(newCartItem);
            }

            // 保存更改
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product added to cart successfully." });
        }
    }

}

