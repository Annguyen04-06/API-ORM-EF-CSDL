using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using btap_api_orm.Data;
using btap_api_orm.DTO;
using btap_api_orm.Models;
using System.Data;

namespace btap_api_orm.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderManagementContext _context;

        public OrdersController(OrderManagementContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            var customerExists = await _context.Customers
                .AnyAsync(c => c.CustomerId == dto.CustomerId);

            if (!customerExists)
                return NotFound(new { message = $"Khach hang ID={dto.CustomerId} khong ton tai" });

            var order = new Order
            {
                CustomerId  = dto.CustomerId,
                OrderDate   = DateTime.Today,
                TotalAmount = 0
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var response = new OrderResponseDto
            {
                Id          = order.OrderId,
                CustomerId  = order.CustomerId,
                OrderDate   = order.OrderDate.ToString("yyyy-MM-dd"),
                TotalAmount = order.TotalAmount
            };

            return CreatedAtAction(nameof(GetOrderTotal), new { orderId = order.OrderId }, response);
        }
        [HttpPost("{orderId}/items")]
        public async Task<IActionResult> AddOrderItem(int orderId, [FromBody] AddOrderItemDto dto)
        {
            var paramResultCode = new SqlParameter
            {
                ParameterName = "@ResultCode",
                SqlDbType     = SqlDbType.Int,
                Direction     = ParameterDirection.Output
            };
            var paramMessage = new SqlParameter
            {
                ParameterName = "@Message",
                SqlDbType     = SqlDbType.NVarChar,
                Size          = 200,
                Direction     = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_AddOrderItem @OrderId, @ProductId, @Quantity, @ResultCode OUTPUT, @Message OUTPUT",
                new SqlParameter("@OrderId",    orderId),
                new SqlParameter("@ProductId",  dto.ProductId),
                new SqlParameter("@Quantity",   dto.Quantity),
                paramResultCode,
                paramMessage
            );

            int    resultCode = (int)paramResultCode.Value;
            string message    = paramMessage.Value?.ToString() ?? "";

            return resultCode switch
            {
                0   => Ok(new { message }),
                -1  => NotFound(new { message }),
                -2  => NotFound(new { message }),
                -3  => Conflict(new { message }),
                -4  => BadRequest(new { message }),
                _   => StatusCode(500, new { message })
            };
        }
        [HttpGet("{orderId}/total")]
        public async Task<IActionResult> GetOrderTotal(int orderId)
        {
            var paramTotalAmount = new SqlParameter
            {
                ParameterName = "@TotalAmount",
                SqlDbType     = SqlDbType.Decimal,
                Precision     = 18,
                Scale         = 2,
                Direction     = ParameterDirection.Output
            };
            var paramResultCode = new SqlParameter
            {
                ParameterName = "@ResultCode",
                SqlDbType     = SqlDbType.Int,
                Direction     = ParameterDirection.Output
            };
            var paramMessage = new SqlParameter
            {
                ParameterName = "@Message",
                SqlDbType     = SqlDbType.NVarChar,
                Size          = 200,
                Direction     = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_CalculateOrderTotal @OrderId, @TotalAmount OUTPUT, @ResultCode OUTPUT, @Message OUTPUT",
                new SqlParameter("@OrderId", orderId),
                paramTotalAmount,
                paramResultCode,
                paramMessage
            );

            int    resultCode = (int)paramResultCode.Value;
            string message    = paramMessage.Value?.ToString() ?? "";

            if (resultCode == -1)
                return NotFound(new { message });

            decimal totalAmount = (decimal)paramTotalAmount.Value;

            return Ok(new OrderTotalResponseDto
            {
                OrderId     = orderId,
                TotalAmount = totalAmount
            });
        }
    }
}

