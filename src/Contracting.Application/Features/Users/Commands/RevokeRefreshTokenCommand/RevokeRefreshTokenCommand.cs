using Contracting.Application.Common;
using Contracting.Application.Resources;
using Contracting.Infrustructure.Identity;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.RevokeRefreshTokenCommand;

public sealed record RevokeRefreshTokenCommand(string RefreshToken, string? Reason = null) : IRequest<ErrorOr<bool>>;

public sealed class RevokeRefreshTokenCommandHandler : IRequestHandler<RevokeRefreshTokenCommand, ErrorOr<bool>>
{
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IHttpContextAccessor _http;
    private readonly IStringLocalizer<SharedResources> _localizer;


    public RevokeRefreshTokenCommandHandler(IRefreshTokenService refreshTokens, IHttpContextAccessor http, IStringLocalizer<SharedResources> localizer)
    {
        _refreshTokens = refreshTokens;
        _http = http;
        _localizer = localizer;
    }

    public async Task<ErrorOr<bool>> Handle(RevokeRefreshTokenCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Error.Validation("General.Validation", "Refresh token" + _localizer[SharedResourcesKeys.Required]);

        var (user, token) = await _refreshTokens.GetActiveAsync(request.RefreshToken, ct);
        if (token is null)
            return Error.Validation("General.Validation", _localizer[SharedResourcesKeys.RefreshTokenExpire]);

        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
        await _refreshTokens.RevokeAsync(token, request.Reason ?? "User initiated revocation.", ip, ct);

        return true;
    }
}