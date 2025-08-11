using FluentValidation;
using WellbeingHub.Controllers;

namespace WellbeingHub.Validators
{
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).MinimumLength(6);
            RuleFor(x => x.Role).NotEmpty();
            RuleFor(x => x.Location).NotEmpty();
        }
    }

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class GroupDtoValidator : AbstractValidator<GroupDto>
    {
        public GroupDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Location).NotEmpty();
        }
    }

    public class MarketplaceItemDtoValidator : AbstractValidator<MarketplaceItemDto>
    {
        public MarketplaceItemDtoValidator()
        {
            RuleFor(x => x.Title).NotEmpty();
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.CreatedBy).GreaterThan(0);
        }
    }
}
