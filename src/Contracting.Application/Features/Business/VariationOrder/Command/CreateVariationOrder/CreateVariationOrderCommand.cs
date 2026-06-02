using Contracting.Shared.Dtos.BusinessDtos.VariationOrderDtos;
using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.VariationOrder.Command.CreateVariationOrder;

public record CreateVariationOrderCommand(CreateVariationOrderDto Dto) : IRequest<ErrorOr<GetClientVariationOrderDetailDto>>;
