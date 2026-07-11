using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerProjects
{
    public class GetEngineerProjectsQueryHandler : IRequestHandler<GetEngineerProjectsQuery, ErrorOr<List<GetEngineerProjectDto>>>
    {
        private readonly IEngineerService _service;

        public GetEngineerProjectsQueryHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetEngineerProjectDto>>> Handle(GetEngineerProjectsQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetEngineerProjectsAsync(request.EngineerId, request.BranchId);

            return result is null
                ? Error.NotFound("Engineer not found.")
                : result;
        }
    }
}
