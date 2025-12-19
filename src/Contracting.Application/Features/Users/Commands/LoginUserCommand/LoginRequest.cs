namespace Contracting.Application.Features.Users.Commands.LoginUserCommand;
public record LoginRequest(string UserNameOrEmail, string Password, bool RememberMe);
