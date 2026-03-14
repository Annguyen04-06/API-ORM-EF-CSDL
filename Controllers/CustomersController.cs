using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using btap_api_orm.Data;
using btap_api_orm.DTO;

namespace btap_api_orm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly OrderManagementContext _context;

        public CustomersController(OrderManagementContext context)
        {
            _context = context;
        }
        [HttpGet("{customerId}/orders")]
        public async Task<IActionResult> GetCustomerOrders(int customerId)
        {
            var customerExists = await _context.Customers
                .AnyAsync(c => c.CustomerId == customerId);

            if (!customerExists)
                return NotFound(new { message = $"Khach hang ID={customerId} khong ton tai" });
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new CustomerOrderDto
                {
                    OrderId     = o.OrderId,
                    OrderDate   = o.OrderDate.ToString("yyyy-MM-dd"),
                    TotalAmount = o.TotalAmount
                })
                .ToListAsync();
            return Ok(orders);
        }
    }
}

