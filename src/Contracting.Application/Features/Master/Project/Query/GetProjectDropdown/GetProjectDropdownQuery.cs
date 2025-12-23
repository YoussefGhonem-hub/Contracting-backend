using Contracting.Shared.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Query.GetProjectDropdown
{
    // ✅ UPDATED: Added BranchId parameter
    public record GetProjectDropdownQuery(Guid? BranchId) : IRequest<ErrorOr<List<GetProjectDropDownDto>>>;
}