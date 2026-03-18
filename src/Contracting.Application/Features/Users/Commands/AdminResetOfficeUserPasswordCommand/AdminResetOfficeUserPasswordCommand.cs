using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Users.Commands.AdminResetOfficeUserPasswordCommand;

public record AdminResetOfficeUserPasswordRequest(Guid UserId, string NewPassword, string ConfirmPassword);

public record AdminResetOfficeUserPasswordCommand(AdminResetOfficeUserPasswordRequest Request) : IRequest<ErrorOr<string>>;
