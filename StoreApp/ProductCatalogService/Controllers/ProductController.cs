using Microsoft.AspNetCore.Mvc;
using ProductCatalogService.Dto;
using ProductCatalogService
    
    .Interfaces;

namespace ProductCatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductsService _productService;

        public ProductController(IProductsService productService)
        {
            _productService = productService;
        }
        [HttpGet("category/{id}")]
        public ActionResult GetProductsByCategory(int id)
        {
            var products = _productService.GetProducts().Where(i => i.CategoryId == id);
            return Ok(products);
        }

        [HttpGet("{id}")]
        public ActionResult GetProduct(int id)
        {
            var product = _productService.GetProduct(id);
            if (product == null) return NotFound();
            return Ok(product);
        }
        [HttpGet]
        public ActionResult GetProducts()
        {
            return Ok(_productService.GetProducts());
        }


        [HttpGet("{id}/image")]
        //[Authorize]
        public async Task<IActionResult> GetProductImage(int id)
        {
            try
            {
                var fileStream = await _productService.GetProductImageAsync(id.ToString());
                return File(fileStream, "image/png");
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddProduct([FromForm] CreateUpdateProductDto productDto)
        {
            return Ok(await _productService.AddProduct(productDto));
        }
    }
}
