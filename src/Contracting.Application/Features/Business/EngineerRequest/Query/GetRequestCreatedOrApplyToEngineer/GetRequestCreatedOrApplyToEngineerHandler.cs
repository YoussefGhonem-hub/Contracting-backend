using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.UnifiedRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestCreatedOrApplyToEngineer
{
    public class GetRequestCreatedOrApplyToEngineerHandler : IRequestHandler<GetRequestCreatedOrApplyToEngineerQuery, ErrorOr<PaginatedList<GetUnifiedRequestDto>>>
    {
        private readonly IEngineerRequestService _service;

        public GetRequestCreatedOrApplyToEngineerHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetUnifiedRequestDto>>> Handle(GetRequestCreatedOrApplyToEngineerQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetCreatedRequestOrapplaied(
                request.Filter, 
                cancellationToken);
            
            return result;
        }
    }
}