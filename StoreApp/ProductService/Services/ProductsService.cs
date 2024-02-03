using AutoMapper;
using Common.Models.Product;
using ProductService.Dto;
using ProductService.Infrastructure;
using ProductService.Interfaces;

namespace ProductService.Services
{
    public class ProductsService: IProductsService
    {
        private readonly IMapper _mapper;
        private readonly ProductsDbContext _dbContext;
        private readonly IConfiguration _config;

        public ProductsService(IMapper mapper, ProductsDbContext dbContext, IConfiguration configuration)
        {
            _mapper = mapper;
            _dbContext = dbContext;
            _config = configuration;
        }

        public async Task<ProductDto> AddProduct(CreateUpdateProductDto createProductDto)
        {
            if (!createProductDto.ImageFile.ContentType.Contains("image"))
            {
                throw new Exception("File is not image");
            }
            var product = _mapper.Map<Product>(createProductDto);
            _dbContext.Products.Add(product);
            _dbContext.SaveChanges();
            // save image with new product id name
            await SavePostImageAsync(createProductDto.ImageFile, product.Id);

            return _mapper.Map<ProductDto>(product);
        }

        private async Task SavePostImageAsync(IFormFile formFile, int id)
        {
            var filePath = Path.Combine(_config["StoredFilesPath"], id.ToString() + ".png");

            using (var stream = System.IO.File.Create(filePath))
            {
                await formFile.CopyToAsync(stream);
            }
            return;
        }

        public ProductDto GetProduct(int id)
        {
            return _mapper.Map<ProductDto>(_dbContext.Products.Find(id));
        }

        public List<ProductDto> GetProducts()
        {
            return _mapper.Map<List<ProductDto>>(_dbContext.Products.ToList());
        }
    }
}
