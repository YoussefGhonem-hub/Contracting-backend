using Contracting.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class VOStatusConverter : ValueConverter<VOStatus, string>
{
    public VOStatusConverter() : base(
        v => v.ToString(),
        s => Parse(s))
    {
    }

    private static VOStatus Parse(string s) => s switch
    {
        "Approved"                or "CLIENT_STATUS_APPROVED" or "1" => VOStatus.Approved,
        "Rejected"                or "CLIENT_STATUS_REJECTED" or "2" => VOStatus.Rejected,
        "Pending"                 or "CLIENT_STATUS_PENDING"  or "0" => VOStatus.Pending,
        _ => Enum.TryParse<VOStatus>(s, ignoreCase: true, out var v) ? v : VOStatus.Pending
    };
}
