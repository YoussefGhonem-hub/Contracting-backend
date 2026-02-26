using System.ComponentModel.DataAnnotations;
using Contracting.Shared.Common;

namespace Contracting.Domain.Common;
public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
    public DateTimeOffset CreatedDate { get; set; } = DateTimeHelper.Now;
    public BaseEntity()
    {
        Id = Guid.NewGuid();
    }
}