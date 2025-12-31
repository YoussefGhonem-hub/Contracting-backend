using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerList
{
    public class GetEngineerListQueryHandler : IRequestHandler<GetEngineerListQuery, ErrorOr<PaginatedList<GetEngineerDto>>>
    {
        private readonly IEngineerService _service;

        public GetEngineerListQueryHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetEngineerDto>>> Handle(GetEngineerListQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetEngineerListAsync(request.DepartmentId, request.Filter);
            return result;
        }
    }
}
