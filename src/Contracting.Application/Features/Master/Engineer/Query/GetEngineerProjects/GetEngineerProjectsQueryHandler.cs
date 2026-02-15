using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerProjects
{
    public class GetEngineerProjectsQueryHandler : IRequestHandler<GetEngineerProjectsQuery, ErrorOr<List<GetProjectDropDownDto>>>
    {
        private readonly IEngineerService _service;

        public GetEngineerProjectsQueryHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetProjectDropDownDto>>> Handle(GetEngineerProjectsQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetEngineerProjectsAsync(request.EngineerId);

            return result is null
                ? Error.NotFound("Engineer not found.")
                : result;
        }
    }
}
