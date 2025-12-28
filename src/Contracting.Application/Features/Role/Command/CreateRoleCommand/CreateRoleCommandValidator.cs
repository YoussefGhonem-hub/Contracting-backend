using FluentValidation;

namespace Contracting.Application.Features.Role.Command.CreateRoleCommand
{
    public class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleCommandValidator()
        {
            RuleFor(x => x.Dto.Name).NotEmpty();
            RuleFor(x => x.Dto.DisplayName).NotEmpty();
        }
    }
}