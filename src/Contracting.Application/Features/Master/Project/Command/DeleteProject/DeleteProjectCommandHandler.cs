using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Command.DeleteProject
{
    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, ErrorOr<GenericResponse>>
    {
        private readonly IProjectService _service;

        public DeleteProjectCommandHandler(IProjectService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteProjectAsync(request.ProjectId);
            
            return result.Success
                ? result
                : Error.NotFound(result.Message ?? "Project not found.");
        }
    }
}