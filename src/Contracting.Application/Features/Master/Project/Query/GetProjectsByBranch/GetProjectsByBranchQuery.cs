using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Query.GetProjectsByBranch
{
    public record GetProjectsByBranchQuery(Guid BranchId) : IRequest<ErrorOr<List<GetProjectDropDownDto>>>;
}
