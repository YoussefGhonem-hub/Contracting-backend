using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Query.GetProjectSpecialFields
{
    public class GetProjectSpecialFieldsQueryHandler : IRequestHandler<GetProjectSpecialFieldsQuery, ErrorOr<ProjectSpecialFieldsCheckDto>>
    {
        private readonly IProjectService _service;

        public GetProjectSpecialFieldsQueryHandler(IProjectService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<ProjectSpecialFieldsCheckDto>> Handle(GetProjectSpecialFieldsQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetProjectSpecialFieldsAsync(request.ProjectId);

            return result is null
                ? Error.NotFound("Project not found.")
                : result;
        }
    }
}
