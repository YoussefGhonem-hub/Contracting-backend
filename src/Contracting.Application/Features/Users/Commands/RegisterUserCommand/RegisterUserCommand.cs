using Contracting.Application.Common;
using Contracting.Shared.Resources;
using Contracting.Domain.Entities;
using Contracting.Infrustructure.Identity;
using Contracting.Shared.CurrentUser;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace Contracting.Application.Features.Users.Commands.RegisterUserCommand;

public record RegisterUserCommand(RegisterRequest Request) : IRequest<ErrorOr<AuthResponse>>;
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, ErrorOr<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly JwtSettings _jwt;

    public RegisterUserCommandHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService, IStringLocalizer<SharedResources> localizer, IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _localizer = localizer;
        _jwt = jwtOptions.Value;
    }

    public async Task<ErrorOr<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingByEmail = await _userManager.FindByEmailAsync(request.Request.Email);
        if (existingByEmail is not null)
            return Error.Conflict("General.Conflict",_localizer[SharedResourcesKeys.DublicateEmail]);

        var user = new ApplicationUser
        {
            FullName = request.Request.FullName,
            Email = request.Request.Email,
            PhoneNumber = request.Request.PhoneNumber,
            UserName = request.Request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Request.Password);
        if (!result.Succeeded)
            return Error.Validation("General.Validation", result.Errors.Select(e => e.Description).FirstOrDefault()?? _localizer[SharedResourcesKeys.InvalidCredentials]);

        await _userManager.AddToRoleAsync(user, "Admin");

        var token = _tokenService.GenerateToken(user, new List<string> { "Admin" });
        var response = new AuthResponse(token, DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes), CurrentUser.UserId, user.Email!);

        return response;
    }
}
