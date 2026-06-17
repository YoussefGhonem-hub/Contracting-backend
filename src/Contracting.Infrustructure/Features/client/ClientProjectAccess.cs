using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.client;

/// <summary>
/// Access rules for the client-facing project endpoints.
/// A user in the <see cref="RoleNames.Client"/> role is restricted to the projects
/// assigned to them; every other authenticated user (back-office staff, engineers,
/// admins) may access any project. This preserves client-to-client isolation while
/// letting staff read the same screens.
/// </summary>
internal static class ClientProjectAccess
{
    public static bool IsClient =>
        CurrentUser.Roles.Contains(RoleNames.Client, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the current user may access the given project.
    /// Staff: any existing project. Client: only their own assigned projects.
    /// </summary>
    public static async Task<bool> CanAccessProjectAsync(
        ApplicationDbContext db, Guid projectId, CancellationToken ct = default)
    {
        var projectExists = await db.Projects.AnyAsync(p => p.Id == projectId && !p.IsDeleted, ct);
        if (!projectExists)
            return false;

        // Staff (non-client roles) may access any project.
        if (!IsClient)
            return true;

        var userId = CurrentUser.Id;
        if (userId is null)
            return false;

        return await db.ClientProjects.AnyAsync(
            cp => cp.ProjectId == projectId
               && cp.Client != null
               && cp.Client.ApplicationUserId == userId.Value,
            ct);
    }
}
