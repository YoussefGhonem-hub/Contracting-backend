using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetEngineerRequestsByFilter
{
    public class GetEngineerRequestsByFilterHandler : IRequestHandler<GetEngineerRequestsByFilterQuery, ErrorOr<PaginatedList<GetAllEngineerRequestDto>>>
    {
        private readonly IEngineerRequestService _service;

        public GetEngineerRequestsByFilterHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetAllEngineerRequestDto>>> Handle(GetEngineerRequestsByFilterQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.FilterEngineerRequestsAsync(request.Filter, cancellationToken);
            return result;
        }
    }
}
