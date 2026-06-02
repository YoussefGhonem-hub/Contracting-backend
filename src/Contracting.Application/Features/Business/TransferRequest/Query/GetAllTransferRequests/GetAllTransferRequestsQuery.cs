using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Query.GetAllTransferRequests
{
    public record GetAllTransferRequestsQuery(TransferRequestFilterDto Filter) : IRequest<PaginatedList<GetTransferRequestDto>>;
}
