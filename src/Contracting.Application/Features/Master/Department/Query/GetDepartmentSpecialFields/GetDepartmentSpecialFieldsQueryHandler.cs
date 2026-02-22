using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Department.Query.GetDepartmentSpecialFields
{
    public class GetDepartmentSpecialFieldsQueryHandler : IRequestHandler<GetDepartmentSpecialFieldsQuery, ErrorOr<DepartmentSpecialFieldsCheckDto>>
    {
        private readonly IDepartmentService _service;

        public GetDepartmentSpecialFieldsQueryHandler(IDepartmentService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<DepartmentSpecialFieldsCheckDto>> Handle(GetDepartmentSpecialFieldsQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetDepartmentSpecialFieldsAsync(request.DepartmentId);

            return result is null
                ? Error.NotFound("Department not found.")
                : result;
        }
    }
}
