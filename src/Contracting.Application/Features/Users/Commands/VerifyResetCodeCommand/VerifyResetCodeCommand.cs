using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Users.Commands.VerifyResetCodeCommand;

public record VerifyResetCodeRequest(string Email, string Code);

public record VerifyResetCodeCommand(VerifyResetCodeRequest Request) : IRequest<ErrorOr<string>>;
