using Common.Dto;
using Common.Dto.Order;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Net;
using System.Security.Claims;

namespace ApiGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly string orderServicePath = @"fabric:/StoreApp/OrderService";
        private IOrderService? _orderService
        {
            get
            {
                var email = GetUserEmail();
                return ServiceProxy.Create<IOrderService>(new Uri(orderServicePath), new ServicePartitionKey(email.GetHashCode()));
            }
        }
        public OrderController()
        {
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetOrdersAsync()
        {
            var email = GetUserEmail();
            try
            {
                return Ok(await _orderService.GetOrdersByUser(email));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/Order/5
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetOrderAsync(string id)
        {
            var email = GetUserEmail();
            try
            {
                return Ok(await _orderService.GetOrder(id));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("capture")]
        public async Task<ActionResult> CaptureOrder([FromBody] CaptureOrderDto captureOrderDto)
        {

            try
            {
                await _orderService.CaptureOrder(captureOrderDto.OrderId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("cancel")]
        public async Task<ActionResult> CancelOrder([FromBody] CancelOrderDto cancelOrderDto)
        {

            try
            {
                await _orderService.CancelOrder(cancelOrderDto.OrderId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [NonAction]
        private string GetUserEmail()
        {
            var claimsIdentity = this.User.Identity as ClaimsIdentity;
            return claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}
