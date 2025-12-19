using Contracting.Application.Common;
using Contracting.Domain.Entities;
using Contracting.Infrustructure.Identity;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Contracting.Application.Features.Users.Commands.LoginUserCommand;

public record LoginUserCommand(LoginRequest Request) : IRequest<Result<TokenPairResponse>>;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<TokenPairResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IHttpContextAccessor _http;
    private readonly JwtSettings _jwt;
    private readonly ApplicationDbContext _db;

    public LoginUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IRefreshTokenService refreshTokens,
        IHttpContextAccessor http,
        IOptions<JwtSettings> jwtOptions,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _refreshTokens = refreshTokens;
        _http = http;
        _jwt = jwtOptions.Value;
        _db = db;
    }

    public async Task<Result<TokenPairResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        ApplicationUser? user = req.UserNameOrEmail.Contains("@")
            ? await _userManager.FindByEmailAsync(req.UserNameOrEmail)
            : await _userManager.FindByNameAsync(req.UserNameOrEmail);

        if (user is null)
            return Result<TokenPairResponse>.Failure("Invalid credentials");

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, req.Password, false);
        if (!signInResult.Succeeded)
            return Result<TokenPairResponse>.Failure("Invalid credentials");

        // Migrate guest data (cart + wishlist) to this user
        var guestId = CurrentUser.GuestId;
        await AttachGuestCartToUserAsync(user.Id, guestId, cancellationToken);
        await AttachGuestWishlistToUserAsync(user.Id, guestId, cancellationToken);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateToken(user, roles);
        var accessExp = DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes);

        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var (refreshToken, refreshExp) = await _refreshTokens.CreateAsync(user, ip, cancellationToken);

        var pair = new TokenPairResponse(
            accessToken,
            accessExp,
            refreshToken,
            refreshExp
        );

        return Result<TokenPairResponse>.Success(pair);
    }

    // Attach guest cart (UserId == null) to authenticated user, then clear GuestId
    private async Task AttachGuestCartToUserAsync(Guid userId, string? guestId, CancellationToken ct)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(guestId)) return;

        var cart = await _db.Carts
            .FirstOrDefaultAsync(c => c.UserId == null && c.GuestId == guestId, ct);

        if (cart is null) return;

        cart.UserId = userId;
        cart.GuestId = null;

        await _db.SaveChangesAsync(ct);
    }

    // Attach guest wishlist entries to authenticated user, then clear GuestId
    private async Task AttachGuestWishlistToUserAsync(Guid userId, string? guestId, CancellationToken ct)
    {
        if (userId == Guid.Empty || string.IsNullOrWhiteSpace(guestId)) return;

        // Load guest favorites that are not yet tied to a user
        var favorites = await _db.FavoriteProducts
            .Where(f => f.UserId == null && f.GuestId == guestId)
            .ToListAsync(ct);

        if (favorites.Count == 0) return;

        foreach (var f in favorites)
        {
            f.UserId = userId;
            f.GuestId = null;
        }

        await _db.SaveChangesAsync(ct);
    }
}
