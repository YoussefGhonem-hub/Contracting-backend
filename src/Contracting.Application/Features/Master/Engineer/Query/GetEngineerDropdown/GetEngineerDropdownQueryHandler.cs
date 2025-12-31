using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerDropdown
{
    public class GetEngineerDropdownQueryHandler : IRequestHandler<GetEngineerDropdownQuery, ErrorOr<List<GetEngineerDropDownDto>>>
    {
        private readonly IEngineerService _service;

        public GetEngineerDropdownQueryHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetEngineerDropDownDto>>> Handle(GetEngineerDropdownQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetEngineerDropdownAsync(request.departmentId);
            return result;
        }
    }
}
