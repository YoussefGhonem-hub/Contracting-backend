using FluentValidation;

namespace Contracting.Application.Features.Role.Command.UpdateRoleCommand
{
    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator()
        {
            RuleFor(x => x.Dto.Name).NotEmpty();
            RuleFor(x => x.Dto.DisplayName).NotEmpty();
        }
    }
}