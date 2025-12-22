using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.DepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Department.Query.GetDepartmentsByBranchId
{
    public class GetDepartmentsByBranchIdQueryHandler : IRequestHandler<GetDepartmentsByBranchIdQuery, ErrorOr<PaginatedList<GetDepartmentDto>>>
    {
        private readonly IDepartmentService _service;

        public GetDepartmentsByBranchIdQueryHandler(IDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetDepartmentDto>>> Handle(GetDepartmentsByBranchIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetDepartmentsByBranchIdAsync(request.BranchId, request.Filter);
            return result;
        }
    }
}
