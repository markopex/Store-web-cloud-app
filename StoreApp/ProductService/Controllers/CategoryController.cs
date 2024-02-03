using Microsoft.AspNetCore.Mvc;
using ProductService.Interfaces;

namespace ProductService.Controllers
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
        public ActionResult GetCategories()
        {
            return Ok(_categoryService.GetCategories());
        }
    }
}
