using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Extensions.Helpers
{
    /// <summary>
    /// Resolves master Status IDs from the database by Code column.
    /// Always reads from the DB so IDs are correct in every environment.
    /// </summary>
    public static class StatusResolver
    {
        // Hold is optional (null when the ON_HOLD status has not been seeded yet) so that resolving
        // statuses never throws on a database that predates the On Hold seed migration.
        public record RequestStatusIds(Guid New, Guid InProgress, Guid Completed, Guid Rejected, Guid MissingInformation, Guid? Hold = null);

        public static async Task<RequestStatusIds> LoadRequestStatusIdsAsync(ApplicationDbContext db)
        {
            var needed = new[] { MasterStatusCodes.New, MasterStatusCodes.InProgress, MasterStatusCodes.Completed, MasterStatusCodes.Rejected, MasterStatusCodes.MissingInformation, MasterStatusCodes.Hold };

            var rows = await db.Statuses
                .AsNoTracking()
                .Where(s => needed.Contains(s.Code))
                .Select(s => new { s.Id, s.Code })
                .ToListAsync();

            Guid Resolve(string code)
            {
                var row = rows.FirstOrDefault(r => r.Code == code);
                if (row is null) throw new InvalidOperationException($"Master status with Code='{code}' not found in the database.");
                return row.Id;
            }

            Guid? ResolveOptional(string code) => rows.FirstOrDefault(r => r.Code == code)?.Id;

            return new RequestStatusIds(
                New:                Resolve(MasterStatusCodes.New),
                InProgress:         Resolve(MasterStatusCodes.InProgress),
                Completed:          Resolve(MasterStatusCodes.Completed),
                Rejected:           Resolve(MasterStatusCodes.Rejected),
                MissingInformation: Resolve(MasterStatusCodes.MissingInformation),
                Hold:               ResolveOptional(MasterStatusCodes.Hold)
            );
        }
    }
}
