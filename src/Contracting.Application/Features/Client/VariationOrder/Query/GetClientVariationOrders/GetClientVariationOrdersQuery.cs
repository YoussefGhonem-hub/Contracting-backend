using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.VariationOrder.Query.GetClientVariationOrders;

public record GetClientVariationOrdersQuery(Guid ProjectId, string? Status) : IRequest<ErrorOr<GetClientVariationOrdersDto>>;
