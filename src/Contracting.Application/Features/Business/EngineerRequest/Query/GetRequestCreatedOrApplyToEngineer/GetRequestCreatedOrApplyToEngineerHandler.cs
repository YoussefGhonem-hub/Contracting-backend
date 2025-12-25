using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestCreatedOrApplyToEngineer
{
    public class GetRequestCreatedOrApplyToEngineerHandler : IRequestHandler<GetRequestCreatedOrApplyToEngineerQuery, ErrorOr<PaginatedList<GetAllEngineerRequestDto>>>
    {
        private readonly IEngineerRequestService _service;

        public GetRequestCreatedOrApplyToEngineerHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetAllEngineerRequestDto>>> Handle(GetRequestCreatedOrApplyToEngineerQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetCreatedRequestOrapplaied(
                request.Filter, 
                cancellationToken);
            
            return result;
        }
    }
}