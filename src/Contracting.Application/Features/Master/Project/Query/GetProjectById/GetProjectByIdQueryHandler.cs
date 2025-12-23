using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Query.GetProjectById
{
    public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ErrorOr<GetProjectDto>>
    {
        private readonly IProjectService _service;

        public GetProjectByIdQueryHandler(IProjectService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetProjectByIdAsync(request.ProjectId);
            
            return result is null
                ? Error.NotFound("Project not found.")
                : result;
        }
    }
}