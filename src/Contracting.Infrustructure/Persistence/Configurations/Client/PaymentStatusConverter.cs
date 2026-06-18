using Contracting.Domain.Common.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Contracting.Infrustructure.Persistence.Configurations.Client;

public class PaymentStatusConverter : ValueConverter<PaymentStatus, string>
{
    public PaymentStatusConverter() : base(
        v => v.ToString(),
        s => Parse(s))
    {
    }

    private static PaymentStatus Parse(string s) => s switch
    {
        "Paid"                    or "CLIENT_STATUS_PAID"          or "1" => PaymentStatus.Paid,
        "PartiallyPaid"           or "CLIENT_STATUS_PARTIALLYPAID" or "2" => PaymentStatus.PartiallyPaid,
        "Pending"                 or "CLIENT_STATUS_PENDING"        or "0" => PaymentStatus.Pending,
        _ => Enum.TryParse<PaymentStatus>(s, ignoreCase: true, out var v) ? v : PaymentStatus.Pending
    };
}
