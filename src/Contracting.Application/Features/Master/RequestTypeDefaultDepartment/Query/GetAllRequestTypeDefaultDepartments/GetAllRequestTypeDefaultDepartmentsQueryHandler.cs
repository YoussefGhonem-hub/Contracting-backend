using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Query.GetAllRequestTypeDefaultDepartments
{
    public class GetAllRequestTypeDefaultDepartmentsQueryHandler : IRequestHandler<GetAllRequestTypeDefaultDepartmentsQuery, ErrorOr<List<GetRequestTypeDefaultDepartmentDto>>>
    {
        private readonly IRequestTypeDefaultDepartmentService _service;

        public GetAllRequestTypeDefaultDepartmentsQueryHandler(IRequestTypeDefaultDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetRequestTypeDefaultDepartmentDto>>> Handle(GetAllRequestTypeDefaultDepartmentsQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetAllByBranchAsync(request.BranchId);
        }
    }
}
