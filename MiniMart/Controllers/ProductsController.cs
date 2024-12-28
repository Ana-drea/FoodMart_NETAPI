using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniMart.Data;
using MiniMart.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MiniMart.Dtos;

namespace MiniMart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> Get()
        {
            var products = await _context.Products
                                         .Include(p => p.Category) // Load related Category data
                                         .ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetById([FromRoute] int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            // load the category for the product
            if (product.Category == null) {
                product.Category= await _context.Categories.FirstOrDefaultAsync(c =>c.Id == product.CategoryId);
                }
            return Ok(product);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetByCategoryId([FromRoute] int categoryId)
        {
            var products = await _context.Products
                .Where(p => p.CategoryId == categoryId)
                .Include(p => p.Category) // Load related Category data
                .ToListAsync();
            if (products == null || !products.Any())
            {
                return NotFound();
            }
            return Ok(products);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Create([FromBody] ProductDto productDto)
        {
            if (productDto == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // search for Category by CategoryId 
            var category = await _context.Categories.FindAsync(productDto.CategoryId);
            if (category == null)
            {
                return NotFound("Category not found.");
            }
            // change DTO into entity class
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                CategoryId = productDto.CategoryId,
                Price = productDto.Price,
                QuantityInStock = productDto.QuantityInStock,
                ImageUrl = productDto.ImageUrl
            };
            // Ensure the product's Category is assigned
            product.Category = category;
            await _context.Products.AddAsync(product);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] ProductDto productDto)
        {
            var prod = _context.Products.FirstOrDefault(x => x.Id == id);
            if ( prod == null)
            {
                return BadRequest();
            }
            prod.Name = productDto.Name;
            prod.Description = productDto.Description;
            prod.CategoryId = productDto.CategoryId;
            prod.Price = productDto.Price;
            prod.QuantityInStock = productDto.QuantityInStock;
            prod.ImageUrl = productDto.ImageUrl;

            // update Category
            if (prod.CategoryId != productDto.CategoryId)
            {
                var category = await _context.Categories.FindAsync(productDto.CategoryId);
                if (category == null)
                {
                    return NotFound("Category not found.");
                }

                prod.Category = category;
            }
            
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Products.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction(nameof(GetById), new { id = prod.Id }, prod);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
