using AutoMapper;
using Azure.Data.Tables;
using Common.Models.Identity;
using IdentityService.Dto;

namespace IdentityService.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserDto>().ReverseMap();
            CreateMap<User, RegisterDto>().ReverseMap();
            CreateMap<User, LoginDto>().ReverseMap();
            CreateMap<User, UpdateUserDto>().ReverseMap();
            CreateMap<TableEntity, UserDto>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.PartitionKey))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.RowKey))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src[nameof(User.FirstName)]))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src[nameof(User.LastName)]))
            .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => Convert.ToInt64(src[nameof(User.Birthday)])))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src[nameof(User.Address)]));
        }
    }
}
