using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.StatusDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Status.Query.GetStatusDropdown
{
    public class GetStatusDropdownQueryHandler : IRequestHandler<GetStatusDropdownQuery, ErrorOr<List<GetDropDownStatusDto>>>
    {
        private readonly IStatueService _service;

        public GetStatusDropdownQueryHandler(IStatueService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetDropDownStatusDto>>> Handle(GetStatusDropdownQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetStatusDropdownAsync();
            
            return result;
        }
    }
}