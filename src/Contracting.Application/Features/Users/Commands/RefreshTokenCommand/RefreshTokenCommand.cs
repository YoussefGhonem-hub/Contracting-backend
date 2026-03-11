using Contracting.Application.Common;
using Contracting.Application.Features.Users.Commands.LoginUserCommand;
using Contracting.Shared.Resources;
using Contracting.Domain.Entities;
using Contracting.Infrustructure.Identity;
using Contracting.Infrustructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.RefreshTokenCommand;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<ErrorOr<TokenPairResponse>>;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ErrorOr<TokenPairResponse>>
{
    private readonly IRefreshTokenService _refreshTokens;
    private readonly ITokenService _tokens;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHttpContextAccessor _http;
    private readonly ApplicationDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;


    public RefreshTokenCommandHandler(
        IRefreshTokenService refreshTokens,
        ITokenService tokens,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor http,
        ApplicationDbContext db,
        IStringLocalizer<SharedResources> localizer)
    {
        _refreshTokens = refreshTokens;
        _tokens = tokens;
        _userManager = userManager;
        _http = http;
        _db = db;
        _localizer = localizer;
    }

    public async Task<ErrorOr<TokenPairResponse>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Error.Validation("General.Validation", $"Refresh Token {_localizer[SharedResourcesKeys.Required]}");

        var (user, currentToken) = await _refreshTokens.GetActiveAsync(request.RefreshToken, ct);
        if (user is null || currentToken is null)
            return Error.Validation("General.Validation", _localizer[SharedResourcesKeys.RefreshTokenExpire]);

        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();

        // Atomic rotate (no duplicate insert)
        var (newRefreshPlain, newRefreshExp) = await _refreshTokens.RotateAsync(currentToken, user, ip, ct);

        // Get department name from Engineer entity
        var engineer = await _db.Engineers
            .Include(e => e.Department)
                .ThenInclude(d => d.Branch)
            .FirstOrDefaultAsync(e => e.ApplicationUserId == user.Id, ct);
        
        var departmentId = engineer?.Department?.Id;
        var branchId = engineer?.Department?.BranchId;
        var engineerId = engineer?.Id;

        var roles = await _userManager.GetRolesAsync(user);
        var (access, accessExp) = _tokens.GenerateAccessToken(user, roles, departmentId, engineerId, false, branchId);

        var pair = new TokenPairResponse(access, accessExp, newRefreshPlain, newRefreshExp);
        return pair;
    }
}