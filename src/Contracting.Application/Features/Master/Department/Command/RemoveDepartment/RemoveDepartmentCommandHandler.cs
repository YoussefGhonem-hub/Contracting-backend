using Contracting.Infrustructure.Inteface;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Department.Command.RemoveDepartment
{
    public class RemoveDepartmentCommandHandler : IRequestHandler<RemoveDepartmentCommand, ErrorOr<bool>>
    {
        private readonly IDepartmentService _service;

        public RemoveDepartmentCommandHandler(IDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(RemoveDepartmentCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.RemoveDepartmentAsync(request.BranchId, request.DepartmentId);
            return result
                ? true
                : Error.NotFound("Department not found.");
        }
    }
}
