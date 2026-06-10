using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Query.GetProjectsByBranch
{
    public class GetProjectsByBranchQueryHandler : IRequestHandler<GetProjectsByBranchQuery, ErrorOr<List<GetProjectDropDownDto>>>
    {
        private readonly IProjectService _service;

        public GetProjectsByBranchQueryHandler(IProjectService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetProjectDropDownDto>>> Handle(GetProjectsByBranchQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetProjectsByBranchAsync(request.BranchId, cancellationToken);
            return result;
        }
    }
}
