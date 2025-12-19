namespace Contracting.Application.Features.Users.Commands.LoginUserCommand;

public record TokenPairResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc
);