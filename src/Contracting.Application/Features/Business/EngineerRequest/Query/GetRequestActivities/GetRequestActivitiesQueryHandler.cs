using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestActivities
{
    public class GetRequestActivitiesQueryHandler : IRequestHandler<GetRequestActivitiesQuery, ErrorOr<List<GetEngineerRequestActiviteDto>>>
    {
        private readonly IEngineerRequestService _service;

        public GetRequestActivitiesQueryHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetEngineerRequestActiviteDto>>> Handle(GetRequestActivitiesQuery request, CancellationToken cancellationToken)
        {
            var activities = await _service.GetRequestActivitiesAsync(request.RequestId);
            return activities;
        }
    }
}
