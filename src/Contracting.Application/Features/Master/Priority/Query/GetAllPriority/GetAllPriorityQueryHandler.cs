using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.PriorityDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Query.GetAllPriority
{
    public class GetAllPriorityQueryHandler : IRequestHandler<GetAllPriorityQuery, ErrorOr<PaginatedList<GetDropDownPriorityDto>>>
    {
        private readonly IPriorityService _service;

        public GetAllPriorityQueryHandler(IPriorityService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetDropDownPriorityDto>>> Handle(GetAllPriorityQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAllPrioritiesAsync(request.Filter, cancellationToken);
            return result;
        }
    }
}
