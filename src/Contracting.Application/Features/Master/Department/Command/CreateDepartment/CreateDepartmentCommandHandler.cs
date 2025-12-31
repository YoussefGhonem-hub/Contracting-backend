using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Department.Command.CreateDepartment
{
    public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, ErrorOr<GetDepartmentDto>>
    {
        private readonly IDepartmentService _service;

        public CreateDepartmentCommandHandler(IDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetDepartmentDto>> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateDepartmentAsync(request.BranchId, request.Department);
            return result is null
                ? Error.NotFound("Branch not found.")
                : result;
        }
    }
}
