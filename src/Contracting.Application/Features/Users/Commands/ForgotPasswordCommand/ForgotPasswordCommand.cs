using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Users.Commands.ForgotPasswordCommand;

public record ForgotPasswordRequest(string Email);

public record ForgotPasswordCommand(ForgotPasswordRequest Request) : IRequest<ErrorOr<string>>;
