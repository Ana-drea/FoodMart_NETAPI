using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniMart.Data;
using MiniMart.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace MiniMart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> Get()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetById([FromRoute] int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null) { 
                return NotFound();
            }
            return Ok(category);
        }

        [HttpPost]
        public async Task<ActionResult<Category>> Create([FromBody] Category category)
        {
            if (category == null)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid) // 验证模型是否有效
            {
                return BadRequest(ModelState);
            }
            await _context.Categories.AddAsync(category);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = category.Id },category);
        }

        [HttpPut]
        [Route("{id}")]
        public async Task<ActionResult> Update([FromRoute]int id, [FromBody]Category category)
        {
            var cat = _context.Categories.FirstOrDefault(x=>x.Id == id);
            if (id != category.Id||cat==null)
            {
                return BadRequest();
            }
            cat.Name = category.Name;
            cat.Description = category.Description;
            // 保存更改到数据库
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete([FromRoute] int id)
        {
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
