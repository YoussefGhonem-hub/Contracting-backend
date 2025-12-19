namespace Contracting.Application.Features.Users.Commands.RegisterUserCommand;
public record RegisterRequest(string FullName, string Email, string PhoneNumber, string Password);
