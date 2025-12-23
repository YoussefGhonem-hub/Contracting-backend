using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Query.GetProjectDropdown
{
    public class GetProjectDropdownQueryHandler : IRequestHandler<GetProjectDropdownQuery, ErrorOr<List<GetProjectDropDownDto>>>
    {
        private readonly IProjectService _service;

        public GetProjectDropdownQueryHandler(IProjectService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetProjectDropDownDto>>> Handle(GetProjectDropdownQuery request, CancellationToken cancellationToken)
        {
            // ✅ UPDATED: Pass BranchId to service
            var result = await _service.GetProjectDropdownAsync(request.BranchId);
            
            return result;
        }
    }
}