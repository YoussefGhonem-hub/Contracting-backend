using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetAllInternalRequests
{
    public class GetAllInternalRequestsQueryHandler
        : IRequestHandler<GetAllInternalRequestsQuery, PaginatedList<GetAllEngineerRequestDto>>
    {
        private readonly IEngineerRequestService _service;

        public GetAllInternalRequestsQueryHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public Task<PaginatedList<GetAllEngineerRequestDto>> Handle(
            GetAllInternalRequestsQuery request,
            CancellationToken cancellationToken)
            => _service.GetAllInternalRequestsAsync(request.Filter, cancellationToken);
    }
}
