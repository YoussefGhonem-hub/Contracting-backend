using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;
using System.Collections.Generic;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerDropdown
{
    public record GetEngineerDropdownQuery(Guid departmentId) : IRequest<ErrorOr<List<GetEngineerDropDownDto>>>;
}
