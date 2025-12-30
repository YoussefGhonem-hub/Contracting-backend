using Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestActivities
{
    public record GetRequestActivitiesQuery(Guid RequestId) : IRequest<ErrorOr<List<GetEngineerRequestActiviteDto>>>;
}
