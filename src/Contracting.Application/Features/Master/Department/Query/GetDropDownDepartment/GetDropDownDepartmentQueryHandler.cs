using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.DepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Department.Query.GetDropDownDepartment
{
    public class GetDropDownDepartmentQueryHandler : IRequestHandler<GetDropDownDepartmentQuery, ErrorOr<List<GetDepartmentDto>>>
    {
        private readonly IDepartmentService _service;

        public GetDropDownDepartmentQueryHandler(IDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetDepartmentDto>>> Handle(GetDropDownDepartmentQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.DropDownMethodAsync(request.BranchId);
            return result;
        }
    }
}
