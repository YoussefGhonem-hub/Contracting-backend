using Contracting.Application.Features.Users.Commands.LoginUserCommand;
using Contracting.Domain.Entities;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Identity;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Constants;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Contracting.Application.Features.Users.Commands.SwitchDepartmentCommand;

public record SwitchDepartmentCommand(Guid DepartmentId) : IRequest<ErrorOr<TokenPairResponse>>;

public class SwitchDepartmentCommandHandler : IRequestHandler<SwitchDepartmentCommand, ErrorOr<TokenPairResponse>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IHttpContextAccessor _http;
    private readonly JwtSettings _jwt;
    private readonly ApplicationDbContext _db;
    private readonly IEngineerService _engineerService;
    private readonly IEngineerRequestService _engineerRequestService;

    public SwitchDepartmentCommandHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IRefreshTokenService refreshTokens,
        IHttpContextAccessor http,
        IOptions<JwtSettings> jwtOptions,
        ApplicationDbContext db,
        IEngineerService engineerService,
        IEngineerRequestService engineerRequestService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokens = refreshTokens;
        _http = http;
        _jwt = jwtOptions.Value;
        _db = db;
        _engineerService = engineerService;
        _engineerRequestService = engineerRequestService;
    }

    public async Task<ErrorOr<TokenPairResponse>> Handle(SwitchDepartmentCommand request, CancellationToken cancellationToken)
    {
        // Get current user from token
        var userIdClaim = _http.HttpContext?.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Error.Unauthorized("Auth.Unauthorized", "User not authenticated.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
            return Error.NotFound("Auth.UserNotFound", "User not found.");

        // Get engineer
        var engineer = await _db.Engineers
            .FirstOrDefaultAsync(e => e.ApplicationUserId == userId, cancellationToken);
        if (engineer is null)
            return Error.NotFound("Engineer.NotFound", "Engineer not found.");

        // Verify engineer belongs to the requested department
        var engineerDepartment = await _db.EngineerDepartments
            .Include(ed => ed.Role)
            .FirstOrDefaultAsync(ed => ed.EngineerId == engineer.Id && ed.DepartmentId == request.DepartmentId, cancellationToken);
        if (engineerDepartment is null)
            return Error.Validation("Department.NotAssigned", "Engineer is not assigned to this department.");

        // Update active department
        engineer.DepartmentId = request.DepartmentId;
        await _db.SaveChangesAsync(cancellationToken);

        // Update UserRoles: remove engineer-type roles, add the role for the selected department
        var engineerRoleNames = new[] { RoleNames.Teamleadengineer, RoleNames.Siteengineer, RoleNames.Officeengineer };
        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToRemove = currentRoles.Where(r => engineerRoleNames.Contains(r)).ToList();
        if (rolesToRemove.Any())
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

        var newRoleName = engineerDepartment.Role?.Name;
        if (!string.IsNullOrEmpty(newRoleName))
            await _userManager.AddToRoleAsync(user, newRoleName);

        // Get branch (include Branch for currency)
        var department = await _db.Departmentes
            .Include(d => d.Branch)
            .FirstOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken);
        var branchId = department?.BranchId;
        var currency = department?.Branch?.currency;

        // Check if department has team lead
        bool departmentHaveTeamLeadOrNot = await _engineerRequestService.DepartmentHasTeamLeadAsync(request.DepartmentId);

        // Generate new tokens
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateToken(user, roles, request.DepartmentId, engineer.Id, departmentHaveTeamLeadOrNot, branchId, currency);
        var accessExp = DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes);

        var ip = _http.HttpContext?.Connection.RemoteIpAddress?.ToString();
        var (refreshToken, refreshExp) = await _refreshTokens.CreateAsync(user, ip, cancellationToken);

        return new TokenPairResponse(accessToken, accessExp, refreshToken, refreshExp, currency);
    }
}
