using Contracting.Infrustructure.Inteface;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Command.DeleteProject
{
    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, ErrorOr<bool>>
    {
        private readonly IProjectService _service;

        public DeleteProjectCommandHandler(IProjectService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteProjectAsync(request.ProjectId);
            
            return result
                ? result
                : Error.NotFound("Project not found.");
        }
    }
}