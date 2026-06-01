using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Command.DeleteEngineerDepartment
{
    public class DeleteEngineerDepartmentCommandHandler : IRequestHandler<DeleteEngineerDepartmentCommand, ErrorOr<GenericResponse>>
    {
        private readonly IEngineerService _service;

        public DeleteEngineerDepartmentCommandHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeleteEngineerDepartmentCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteEngineerDepartmentAsync(request.EngineerId, request.DepartmentId);
            return result.Success
                ? result
                : Error.NotFound(result.Message ?? "Department assignment not found.");
        }
    }
}
