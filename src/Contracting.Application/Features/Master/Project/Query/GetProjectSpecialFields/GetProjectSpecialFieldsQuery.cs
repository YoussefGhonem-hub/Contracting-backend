using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Query.GetProjectSpecialFields
{
    public record GetProjectSpecialFieldsQuery(Guid ProjectId) : IRequest<ErrorOr<ProjectSpecialFieldsCheckDto>>;
}
