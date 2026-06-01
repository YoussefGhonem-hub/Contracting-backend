using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Query.GetAllProjects
{
    public class GetAllProjectsQueryHandler : IRequestHandler<GetAllProjectsQuery, ErrorOr<PaginatedList<GetProjectDto>>>
    {
        private readonly IProjectService _service;

        public GetAllProjectsQueryHandler(IProjectService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetProjectDto>>> Handle(GetAllProjectsQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAllProjectsAsync(request.BranchId, request.Filter, request.Status, cancellationToken);
            
            return result;
        }
    }
}