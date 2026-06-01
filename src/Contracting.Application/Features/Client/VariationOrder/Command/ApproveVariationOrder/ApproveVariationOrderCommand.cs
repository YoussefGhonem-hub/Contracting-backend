using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.VariationOrder.Command.ApproveVariationOrder;

public record ApproveVariationOrderCommand(Guid VOId) : IRequest<ErrorOr<GetClientVariationOrderDetailDto>>;
