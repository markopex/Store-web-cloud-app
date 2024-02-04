using BasketService.Dto;
using BasketService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using System.Fabric;
using System.Security.Claims;

namespace BasketService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly IBasketsService _basketService;
        private readonly IOrderService _orderService;
        private readonly IReliableStateManager _stateManager;
        private readonly StatefulServiceContext _context;

        public BasketController(IReliableStateManager stateManager, StatefulServiceContext context, IBasketsService basketService, IOrderService orderService)
        {
            _stateManager = stateManager;
            _context = context;
            _basketService = basketService;
            _orderService = orderService;
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
            catch(Exception ex)
            {
                return BadRequest(new {message = ex.Message});
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> UpdateBasketAsync(BasketDto dto)
        {
            var email = GetUserEmail();
            try
            {
                return Ok(await _basketService.SetBasketAsync(email, dto));
            }
            catch (Exception ex)
            {
                return BadRequest(new {message = ex.Message});
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
                return BadRequest(new {message = ex.Message});
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
                return BadRequest(new {message = ex.Message});
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
