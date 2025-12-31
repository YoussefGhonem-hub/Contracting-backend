using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Command.UpdateProject
{
    public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ErrorOr<GetProjectDto>>
    {
        private readonly IProjectService _service;

        public UpdateProjectCommandHandler(IProjectService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetProjectDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateProjectAsync(request.Project);
            
            return result is null
                ? Error.NotFound("Project not found.")
                : result;
        }
    }
}