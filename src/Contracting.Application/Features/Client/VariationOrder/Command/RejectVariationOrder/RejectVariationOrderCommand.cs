using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.VariationOrder.Command.RejectVariationOrder;

public record RejectVariationOrderCommand(Guid VOId, string? RejectionReason) : IRequest<ErrorOr<GetClientVariationOrderDetailDto>>;
