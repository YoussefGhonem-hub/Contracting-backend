using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Users.Commands.ResetMyPasswordCommand;

public record ResetMyPasswordRequest(string NewPassword, string ConfirmPassword);

public record ResetMyPasswordCommand(ResetMyPasswordRequest Request) : IRequest<ErrorOr<string>>;
