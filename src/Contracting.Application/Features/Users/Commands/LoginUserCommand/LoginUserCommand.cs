using Contracting.Shared.Resources;
using Contracting.Domain.Entities;
using Contracting.Infrustructure.Identity;
using Contracting.Infrustructure.Persistence;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Contracting.Infrustructure.Inteface.business;

namespace Contracting.Application.Features.Users.Commands.LoginUserCommand;

public record LoginUserCommand(LoginRequest Request) : IRequest<ErrorOr<TokenPairResponse>>;

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, ErrorOr<TokenPairResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IHttpContextAccessor _http;
    private readonly JwtSettings _jwt;
    private readonly ApplicationDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly IEngineerRequestService _engineerRequest;

    public LoginUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ITokenService tokenService,
        IRefreshTokenService refreshTokens,
        IHttpContextAccessor http,
        IOptions<JwtSettings> jwtOptions,
        ApplicationDbContext db,
        IEngineerRequestService engineerRequest,
        IStringLocalizer<SharedResources> localizer = null
         )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _refreshTokens = refreshTokens;
        _http = http;
        _jwt = jwtOptions.Value;
        _db = db;
        _localizer = localizer;
        _engineerRequest = engineerRequest;
    }

    public async Task<ErrorOr<TokenPairResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var req = request.Request;
        ApplicationUser? user = req.UserNameOrEmail.Contains("@")
            ? await _userManager.FindByEmailAsync(req.UserNameOrEmail)
            : await _userManager.FindByNameAsync(req.UserNameOrEmail);

        if (user is null)
            return Error.NotFound("Auth.InvalidCredentials", _localizer[SharedResourcesKeys.InvalidCredentials]);

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, req.Password, false);
        if (!signInResult.Succeeded)
            return Error.Unauthorized("Auth.Unauthorized", _localizer[SharedResourcesKeys.InvalidCredentials]);

        // Get department name from Engineer entity
        var engineer = await _db.Engineers
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.ApplicationUserId == user.Id, cancellationToken);
        
        var departmentId = engineer?.Department?.Id;
        var engineerId = engineer?.Id;
        bool departmentHaveTeamLeadOrNot = false;
        if (departmentId != Guid.Empty && departmentId !=null)
        {
            departmentHaveTeamLeadOrNot = await _engineerRequest.DepartmentHasTeamLeadAsync(departmentId??Guid.Empty);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateToken(user, roles, departmentId, engineerId, departmentHaveTeamLeadOrNot);
        var accessExp = DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes);

        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var (refreshToken, refreshExp) = await _refreshTokens.CreateAsync(user, ip, cancellationToken);

        var pair = new TokenPairResponse(
            accessToken,
            accessExp,
            refreshToken,
            refreshExp
        );

        return pair;
    }
}
