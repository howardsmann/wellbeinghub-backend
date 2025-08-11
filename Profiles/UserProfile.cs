using AutoMapper;
using WellbeingHub.Controllers;
using WellbeingHub.Models;

namespace WellbeingHub.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDto, User>();
            CreateMap<GroupDto, Group>();
            CreateMap<MarketplaceItemDto, MarketplaceItem>();
        }
    }
}
