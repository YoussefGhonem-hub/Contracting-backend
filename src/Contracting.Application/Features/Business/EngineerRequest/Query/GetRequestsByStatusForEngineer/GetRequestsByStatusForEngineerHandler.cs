using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestsByStatusForEngineer
{
    public class GetRequestsByStatusForEngineerHandler : IRequestHandler<GetRequestsByStatusForEngineerQuery, ErrorOr<PaginatedList<GetAllEngineerRequestDto>>>
    {
        private readonly IEngineerRequestService _service;

        public GetRequestsByStatusForEngineerHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetAllEngineerRequestDto>>> Handle(GetRequestsByStatusForEngineerQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetRequestsByStatusForEngineerAsync(
                request.Filter,
                cancellationToken);

            return result;
        }
    }
}
