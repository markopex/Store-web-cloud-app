
using AutoMapper;
using Common.Models;
using ProductCatalogService.Dto;
using ProductCatalogService.Models;

namespace ProductCatalogService.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CategoryEntity, CategoryDto>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => int.Parse(entity.RowKey)));
            CreateMap<ProductEntity, Product>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(entity => int.Parse(entity.RowKey)));
            CreateMap<CreateUpdateProductDto, ProductEntity>()
                .ForMember(dest => dest.PartitionKey, opt => opt.MapFrom(src => src.CategoryId.ToString()));
        }
    }
}
