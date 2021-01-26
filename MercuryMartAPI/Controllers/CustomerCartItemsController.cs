using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MercuryMartAPI.Data;
using MercuryMartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using MercuryMartAPI.Helpers.AuthorizationMiddleware;

namespace MercuryMartAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerCartItemsController : ControllerBase
    {
        private readonly DataContext _context;

        public CustomerCartItemsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/CustomerCartItems
        [RequiredFunctionalityName("GetCustomerCartItem")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerCartItem>>> GetCustomerCartItem()
        {
            return await _context.CustomerCartItem.ToListAsync();
        }

        // GET: api/CustomerCartItems/5
        [RequiredFunctionalityName("GetCustomerCartItem")]
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerCartItem>> GetCustomerCartItem(int id)
        {
            var customerCartItem = await _context.CustomerCartItem.FindAsync(id);

            if (customerCartItem == null)
            {
                return NotFound();
            }

            return customerCartItem;
        }

        // PUT: api/CustomerCartItems/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("GetCustomerCartItem")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomerCartItem(int id, CustomerCartItem customerCartItem)
        {
            if (id != customerCartItem.CustomerCartItemId)
            {
                return BadRequest();
            }

            _context.Entry(customerCartItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerCartItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/CustomerCartItems
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("GetCustomerCartItem")]
        [HttpPost]
        public async Task<ActionResult<CustomerCartItem>> PostCustomerCartItem(CustomerCartItem customerCartItem)
        {
            _context.CustomerCartItem.Add(customerCartItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomerCartItem", new { id = customerCartItem.CustomerCartItemId }, customerCartItem);
        }

        // DELETE: api/CustomerCartItems/5
        [RequiredFunctionalityName("GetCustomerCartItem")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<CustomerCartItem>> DeleteCustomerCartItem(int id)
        {
            var customerCartItem = await _context.CustomerCartItem.FindAsync(id);
            if (customerCartItem == null)
            {
                return NotFound();
            }

            _context.CustomerCartItem.Remove(customerCartItem);
            await _context.SaveChangesAsync();

            return customerCartItem;
        }

        private bool CustomerCartItemExists(int id)
        {
            return _context.CustomerCartItem.Any(e => e.CustomerCartItemId == id);
        }
    }
}
