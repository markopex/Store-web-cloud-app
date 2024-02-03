using AutoMapper;
using Common.Models.Product;
using ProductService.Dto;

namespace ProductService.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
            CreateMap<Product, CreateUpdateProductDto>().ReverseMap().ForSourceMember(x => x.ImageFile, y => y.DoNotValidate()); ;
        }
    }
}
