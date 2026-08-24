using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Query.GetDefaultDepartment
{
    public class GetDefaultDepartmentQueryHandler : IRequestHandler<GetDefaultDepartmentQuery, ErrorOr<GetDefaultDepartmentDto>>
    {
        private readonly IRequestTypeDefaultDepartmentService _service;

        public GetDefaultDepartmentQueryHandler(IRequestTypeDefaultDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetDefaultDepartmentDto>> Handle(GetDefaultDepartmentQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetDefaultAsync(request.BranchId, request.RequestType);
        }
    }
}
