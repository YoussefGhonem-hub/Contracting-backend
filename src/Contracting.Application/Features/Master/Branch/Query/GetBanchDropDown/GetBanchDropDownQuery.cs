using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.BranchDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Branch.Query.GetBanchDropDown
{
    public record GetBanchDropDownQuery() : IRequest<ErrorOr<List<BranchDropDownDto>>>;

}
