using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;

namespace Contracting.Infrustructure.Inteface.client;

public interface IClientVariationOrderService
{
    /// <summary>Returns list + aggregate summary. Filter: null=All, "Approved", "Pending", "Rejected".</summary>
    Task<GetClientVariationOrdersDto?> GetVariationOrdersAsync(Guid projectId, string? status, CancellationToken cancellationToken = default);

    /// <summary>Returns full detail including attachments. Null if not found or client doesn't own the project.</summary>
    Task<GetClientVariationOrderDetailDto?> GetVariationOrderByIdAsync(Guid voId, CancellationToken cancellationToken = default);

    /// <summary>Approves a pending VO. Returns null if not found/not owned/not pending.</summary>
    Task<GetClientVariationOrderDetailDto?> ApproveVariationOrderAsync(Guid voId, CancellationToken cancellationToken = default);

    /// <summary>Rejects a pending VO. Returns null if not found/not owned/not pending.</summary>
    Task<GetClientVariationOrderDetailDto?> RejectVariationOrderAsync(Guid voId, string? rejectionReason, CancellationToken cancellationToken = default);
}
