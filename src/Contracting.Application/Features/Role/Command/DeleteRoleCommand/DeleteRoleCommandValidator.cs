using FluentValidation;

namespace Contracting.Application.Features.Role.Command.DeleteRoleCommand
{
    public class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
    {
        public DeleteRoleCommandValidator()
        {
            RuleFor(x => x.RoleId).NotEmpty().WithMessage("Role ID is required.");
        }
    }
}