using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Department.Command.RemoveDepartment
{
    public class RemoveDepartmentCommandHandler : IRequestHandler<RemoveDepartmentCommand, ErrorOr<GenericResponse>>
    {
        private readonly IDepartmentService _service;

        public RemoveDepartmentCommandHandler(IDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(RemoveDepartmentCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.RemoveDepartmentAsync(request.BranchId, request.DepartmentId);
            return result.Success
                ? result
                : Error.NotFound(result.Message ?? "Department not found.");
        }
    }
}
