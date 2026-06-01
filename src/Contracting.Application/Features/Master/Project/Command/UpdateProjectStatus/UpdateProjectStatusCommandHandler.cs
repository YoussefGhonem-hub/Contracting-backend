using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common.Enums;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Command.UpdateProjectStatus;

public class UpdateProjectStatusCommandHandler : IRequestHandler<UpdateProjectStatusCommand, ErrorOr<GetProjectDto>>
{
    private readonly IProjectService _service;

    public UpdateProjectStatusCommandHandler(IProjectService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<GetProjectDto>> Handle(UpdateProjectStatusCommand request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateProjectStatusAsync(request.ProjectId, request.NewStatus);

        if (result is null)
        {
            // Service returns null either for not-found OR for invalid transition.
            // We return a validation error so the client gets a descriptive 400.
            return Error.Validation(
                code: "Project.InvalidStatusTransition",
                description: $"Cannot set status to '{request.NewStatus}'. The project was not found or this transition is not allowed from the current state.");
        }

        return result;
    }
}
