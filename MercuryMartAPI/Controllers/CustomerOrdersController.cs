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
    public class CustomerOrdersController : ControllerBase
    {
        private readonly DataContext _context;

        public CustomerOrdersController(DataContext context)
        {
            _context = context;
        }

        // GET: api/CustomerOrders
        [RequiredFunctionalityName("DeleteCustomerOrder")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerOrder>>> GetCustomerOrder()
        {
            return await _context.CustomerOrder.ToListAsync();
        }

        // GET: api/CustomerOrders/5
        [RequiredFunctionalityName("DeleteCustomerOrder")]
        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerOrder>> GetCustomerOrder(int id)
        {
            var customerOrder = await _context.CustomerOrder.FindAsync(id);

            if (customerOrder == null)
            {
                return NotFound();
            }

            return customerOrder;
        }

        // PUT: api/CustomerOrders/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("DeleteCustomerOrder")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomerOrder(int id, CustomerOrder customerOrder)
        {
            if (id != customerOrder.CustomerOrderId)
            {
                return BadRequest();
            }

            _context.Entry(customerOrder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerOrderExists(id))
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

        // POST: api/CustomerOrders
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("DeleteCustomerOrder")]
        [HttpPost]
        public async Task<ActionResult<CustomerOrder>> PostCustomerOrder(CustomerOrder customerOrder)
        {
            _context.CustomerOrder.Add(customerOrder);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCustomerOrder", new { id = customerOrder.CustomerOrderId }, customerOrder);
        }

        // DELETE: api/CustomerOrders/5
        [RequiredFunctionalityName("DeleteCustomerOrder")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<CustomerOrder>> DeleteCustomerOrder(int id)
        {
            var customerOrder = await _context.CustomerOrder.FindAsync(id);
            if (customerOrder == null)
            {
                return NotFound();
            }

            _context.CustomerOrder.Remove(customerOrder);
            await _context.SaveChangesAsync();

            return customerOrder;
        }

        private bool CustomerOrderExists(int id)
        {
            return _context.CustomerOrder.Any(e => e.CustomerOrderId == id);
        }
    }
}
