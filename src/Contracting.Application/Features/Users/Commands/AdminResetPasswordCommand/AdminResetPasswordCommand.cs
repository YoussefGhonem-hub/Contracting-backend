using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Users.Commands.AdminResetPasswordCommand;

public record AdminResetPasswordRequest(Guid UserId, string NewPassword, string ConfirmPassword);

public record AdminResetPasswordCommand(AdminResetPasswordRequest Request) : IRequest<ErrorOr<string>>;
