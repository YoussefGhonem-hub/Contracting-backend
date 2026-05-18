using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Query.GetAllTransferRequests
{
    public class GetAllTransferRequestsQueryHandler : IRequestHandler<GetAllTransferRequestsQuery, PaginatedList<GetTransferRequestDto>>
    {
        private readonly ITransferRequestService _service;
        public GetAllTransferRequestsQueryHandler(ITransferRequestService service) => _service = service;
        public Task<PaginatedList<GetTransferRequestDto>> Handle(GetAllTransferRequestsQuery request, CancellationToken cancellationToken)
            => _service.GetAllAsync(request.Filter);
    }
}
