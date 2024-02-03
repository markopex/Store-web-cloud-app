
namespace ProductService.Dto
{
    public class CreateUpdateProductDto
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public string Description { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
