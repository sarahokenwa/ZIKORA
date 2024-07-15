using AutoMapper;
using USSDMiddleware.Core.Entities;
using USSDMiddleware.Core.Models;
using USSDMiddleware.Core.Models.IdentityModel;
using USSDMiddleware.Infrastructure.Entities;
using UserModel = USSDMiddleware.Core.Models.UserModel;

namespace USSDMiddleware.Api.Mappers
{
    public class MappingConfig : Profile
    {
        public MappingConfig()
        {
            CreateMap<Token, TokenModel>().ReverseMap();
            CreateMap<User, UserModel>().ReverseMap();
            CreateMap<Provider, ProviderModel>().ReverseMap();
            CreateMap<AccountCreationRequest, Account>().ReverseMap();
        }

    }
}
