using Contracting.Shared.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;
using System.Collections.Generic;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerDropdown
{
    public record GetEngineerDropdownQuery() : IRequest<ErrorOr<List<GetEngineerDropDownDto>>>;
}
