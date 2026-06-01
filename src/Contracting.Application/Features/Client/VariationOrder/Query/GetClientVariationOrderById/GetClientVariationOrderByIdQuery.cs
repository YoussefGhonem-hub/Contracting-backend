using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.VariationOrder.Query.GetClientVariationOrderById;

public record GetClientVariationOrderByIdQuery(Guid VOId) : IRequest<ErrorOr<GetClientVariationOrderDetailDto>>;
