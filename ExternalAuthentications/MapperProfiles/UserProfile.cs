using AutoMapper;
using ExternalAuthentications.Models;
using Microsoft.AspNetCore.Authentication;

namespace ExternalAuthentications.MapperProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<AuthenticationScheme, AuthenticationProviderModel>();
        }
    }
}