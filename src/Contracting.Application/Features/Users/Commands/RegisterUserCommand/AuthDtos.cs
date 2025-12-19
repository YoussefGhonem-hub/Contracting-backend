namespace Contracting.Application.Features.Users.Commands.RegisterUserCommand;

public record AuthResponse(string Token, DateTime ExpiresAt, string UserId, string Email);
