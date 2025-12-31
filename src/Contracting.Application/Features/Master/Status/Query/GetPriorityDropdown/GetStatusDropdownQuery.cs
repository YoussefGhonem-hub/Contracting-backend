using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Status.Query.GetStatusDropdown
{
    public record GetStatusDropdownQuery() : IRequest<ErrorOr<List<GetDropDownStatusDto>>>;
}