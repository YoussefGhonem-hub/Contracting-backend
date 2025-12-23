using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.PriorityDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Query.GetPriorityDropdown
{
    public class GetPriorityDropdownQueryHandler : IRequestHandler<GetPriorityDropdownQuery, ErrorOr<List<GetDropDownPriorityDto>>>
    {
        private readonly IPriorityService _service;

        public GetPriorityDropdownQueryHandler(IPriorityService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetDropDownPriorityDto>>> Handle(GetPriorityDropdownQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetPriorityDropdownAsync();
            
            return result;
        }
    }
}