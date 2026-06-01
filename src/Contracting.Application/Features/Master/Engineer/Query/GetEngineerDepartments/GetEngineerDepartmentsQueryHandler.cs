using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerDepartments
{
    public class GetEngineerDepartmentsQueryHandler : IRequestHandler<GetEngineerDepartmentsQuery, ErrorOr<List<EngineerDepartmentRoleDto>>>
    {
        private readonly IEngineerService _service;

        public GetEngineerDepartmentsQueryHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<EngineerDepartmentRoleDto>>> Handle(GetEngineerDepartmentsQuery request, CancellationToken cancellationToken)
        {
            var departments = await _service.GetEngineerDepartmentsAsync(request.EngineerId);
            return departments;
        }
    }
}
