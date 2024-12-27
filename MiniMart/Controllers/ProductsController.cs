﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniMart.Data;
using MiniMart.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
            var products = await _context.Products.ToListAsync();
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
            return Ok(product);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetByCategoryId([FromRoute] int categoryId)
        {
            var products = await _context.Products
                .Where(x => x.CategoryId == categoryId)
                .ToListAsync();
            if (products == null || !products.Any())
            {
                return NotFound();
            }
            return Ok(products);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Create([FromBody] Product product)
        {
            if (product == null)
            {
                return BadRequest();
            }
            // search for Category by CategoryId 
            var category = await _context.Categories.FindAsync(product.CategoryId);
            if (category == null)
            {
                return NotFound("Category not found.");
            }

            // Ensure the product's Category is assigned
            product.Category = category;
            await _context.Products.AddAsync(product);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Update([FromRoute] int id, [FromBody] Product product)
        {
            var prod = _context.Products.FirstOrDefault(x => x.Id == id);
            if (id != product.Id || prod == null)
            {
                return BadRequest();
            }
            prod.Name = product.Name;
            prod.Description = product.Description;
            prod.CategoryId = product.CategoryId;
            prod.Price = product.Price;
            prod.QuantityInStock = product.QuantityInStock;
            prod.ImageUrl = product.ImageUrl;

            // update Category
            if (prod.CategoryId != product.CategoryId)
            {
                var category = await _context.Categories.FindAsync(product.CategoryId);
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
