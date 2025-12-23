using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.ProjectDtos;
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
            // ? UPDATED: Pass BranchId to service
            var result = await _service.GetAllProjectsAsync(request.BranchId, request.Filter, cancellationToken);
            
            return result;
        }
    }
}