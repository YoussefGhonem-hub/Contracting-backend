using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Users.Commands.ResetAllUsersPasswordCommand;

public record ResetAllUsersPasswordRequest(string NewPassword, string ConfirmPassword);

public record ResetAllUsersPasswordCommand(ResetAllUsersPasswordRequest Request) : IRequest<ErrorOr<string>>;
