using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMart.Data;
using MiniMart.Models;

namespace MiniMart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {

        private readonly AppDbContext _context;

        public StoresController(AppDbContext context)
        {
            _context = context;
        }

        // **1. Get all stores**
        [HttpGet]
        public ActionResult<IEnumerable<Store>> GetStores()
        {
            var stores = _context.Stores.ToList();
            if (stores == null || !stores.Any())
            {
                return NotFound("No stores found.");
            }
            return Ok(stores);
        }

        // **2. Get store by Id**
        [HttpGet("{id}")]
        public ActionResult<Store> GetStoreById(int id)
        {
            var store = _context.Stores.FirstOrDefault(s => s.Id == id);
            if (store == null)
            {
                return NotFound($"Store with id {id} not found.");
            }
            return Ok(store);
        }

        // **3. Create a new store**
        [HttpPost]
        public ActionResult<Store> CreateStore(Store store)
        {
            if (store == null)
            {
                return BadRequest("Store data is required.");
            }
            if (!ModelState.IsValid) // check the model state for store is valid
            {
                return BadRequest(ModelState);
            }

            _context.Stores.Add(store);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetStoreById), new { id = store.Id }, store);
        }

        // **4. Update an existing store**
        [HttpPut("{id}")]
        public ActionResult<Store> UpdateStore(int id, Store updatedStore)
        {
            if (id != updatedStore.Id)
            {
                return BadRequest("Store ID mismatch.");
            }
            if (!ModelState.IsValid) // check the model state for store is valid
            {
                return BadRequest(ModelState);
            }
            var store = _context.Stores.FirstOrDefault(s => s.Id == id);
            if (store == null)
            {
                return NotFound($"Store with id {id} not found.");
            }

            // Update store properties
            store.Name = updatedStore.Name;
            store.Address = updatedStore.Address;
            store.IsActive = updatedStore.IsActive;

            _context.Entry(store).State = EntityState.Modified;
            _context.SaveChanges();

            return Ok(store);
        }

        // **5. Delete a store**
        [HttpDelete("{id}")]
        public ActionResult DeleteStore(int id)
        {
            var store = _context.Stores.FirstOrDefault(s => s.Id == id);
            if (store == null)
            {
                return NotFound($"Store with id {id} not found.");
            }

            _context.Stores.Remove(store);
            _context.SaveChanges();

            return NoContent();  // Successfully deleted
        }
    }
}
