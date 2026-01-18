using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerListByBranch
{
    public class GetEngineerListByBranchQueryHandler : IRequestHandler<GetEngineerListByBranchQuery, ErrorOr<PaginatedList<GetEngineerDto>>>
    {
        private readonly IEngineerService _service;

        public GetEngineerListByBranchQueryHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetEngineerDto>>> Handle(GetEngineerListByBranchQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAllEngineersAsync(request.Filter);
            return result;
        }
    }
}
