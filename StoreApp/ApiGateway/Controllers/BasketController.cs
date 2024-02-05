using Common.Dto;
using Common.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Security.Claims;

namespace ApiGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly string basketServicePath = @"fabric:/StoreApp/BasketService";
        private IBasketsService? _basketService
        {
            get
            {
                var email = GetUserEmail();
                return ServiceProxy.Create<IBasketsService>(new Uri(basketServicePath), new ServicePartitionKey(email.GetHashCode()));
            }
        }
        public BasketController()
        {
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult> GetBasketAsync()
        {
            var email = GetUserEmail();
            try
            {
                return Ok(await _basketService.GetBasketAsync(email));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> UpdateBasketAsync(BasketDto dto)
        {
            var email = GetUserEmail();
            //IBasketsService? _basketService = ServiceProxy.Create<IBasketsService>(new Uri(basketServicePath), new ServicePartitionKey(email.GetHashCode()));
            try
            {
                return Ok(await _basketService.SetBasketAsync(email, dto));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult> AddBasketItemAsync(BasketItemDto item)
        {
            var email = GetUserEmail();
            try
            {
                return Ok(await _basketService.AddItemToBasketAsync(email, item));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("checkout")]
        [Authorize]
        public async Task<ActionResult> Checkout(CheckoutDto dto)
        {
            var email = GetUserEmail();
            try
            {
                //var basket = _basketService.GetBasket(email); 
                //var _bearer_token = Request.Headers[HeaderNames.Authorization].ToString().Replace("Bearer ", "");
                //// send message to order api

                //var orderDto = await _orderService.CreateOrder(basket, dto, _bearer_token);

                //return Ok(orderDto);
                return NotFound();
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
