using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Users.Commands.ResetPasswordCommand;

public record ResetPasswordRequest(string Email, string Token, string NewPassword, string ConfirmPassword);

public record ResetPasswordCommand(ResetPasswordRequest Request) : IRequest<ErrorOr<string>>;
