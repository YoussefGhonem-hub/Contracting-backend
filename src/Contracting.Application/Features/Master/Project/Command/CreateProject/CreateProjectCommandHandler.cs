using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Command.CreateProject
{
    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ErrorOr<GetProjectDto>>
    {
        private readonly IProjectService _service;

        public CreateProjectCommandHandler(IProjectService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateProjectAsync(request.Project);
            
            return result is null
                ? Error.Failure("Could not create project.")
                : result;
        }
    }
}