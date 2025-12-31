using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Department.Command.UpdateDepartment
{
    public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, ErrorOr<GetDepartmentDto>>
    {
        private readonly IDepartmentService _service;

        public UpdateDepartmentCommandHandler(IDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetDepartmentDto>> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateDepartmentAsync(request.Department);
            return result is null
                ? Error.NotFound("Department not found.")
                : result;
        }
    }
}
