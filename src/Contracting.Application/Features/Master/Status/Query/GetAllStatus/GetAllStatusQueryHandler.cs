using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Status.Query.GetAllStatus
{
    public class GetAllStatusQueryHandler : IRequestHandler<GetAllStatusQuery, ErrorOr<PaginatedList<GetDropDownStatusDto>>>
    {
        private readonly IStatueService _service;

        public GetAllStatusQueryHandler(IStatueService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetDropDownStatusDto>>> Handle(GetAllStatusQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAllStatusesAsync(request.Filter, cancellationToken);
            return result;
        }
    }
}
