using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniMart.Data;
using MiniMart.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MiniMart.Dtos;
using System.Linq.Expressions;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductsResponseDto>>> Get(
    [FromQuery] int? categoryId,
    [FromQuery] string? searchQuery,
    [FromQuery] string? sortBy,
    [FromQuery] string? sortDirection,
    [FromQuery] int? pageNumber,
    [FromQuery] int? pageSize)
        {
            // Basic search
            var query = _context.Products.Include(p => p.Category).AsQueryable(); // Load related Category data

            // Filtering
            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p => p.Name.Contains(searchQuery) || p.Description.Contains(searchQuery));
            }

            // Calculate total number before applying pagination
            int totalNumber = await query.CountAsync();

            // Sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(GetSortExpression(sortBy))
                    : query.OrderBy(GetSortExpression(sortBy));
            }

            // Pagination
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                int skip = (pageNumber.Value - 1) * pageSize.Value;
                query = query.Skip(skip).Take(pageSize.Value);
            }

            var products = await query.ToListAsync();

            // Return NotFound if search result is empty
            if (products == null || !products.Any())
            {
                return NotFound();
            }
            // Return products along with total number
            var response = new ProductsResponseDto
            {
                TotalNumber = totalNumber,
                Products = products
            };

            return Ok(response);
        }

        // Method to get sort expression
        private static Expression<Func<Product, object>> GetSortExpression(string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "name" => p => p.Name,
                "price" => p => p.Price,
                "quantity" => p => p.QuantityInStock,
                _ => p => p.Id // Sort by id by default
            };
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
