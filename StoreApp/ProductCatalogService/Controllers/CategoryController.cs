using Microsoft.AspNetCore.Mvc;
using ProductCatalogService.Interfaces;

namespace ProductCatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }
        [HttpGet]
        public async Task<ActionResult> GetCategoriesAsync()
        {
            return Ok(await _categoryService.GetCategoriesAsync());
        }
    }
}
