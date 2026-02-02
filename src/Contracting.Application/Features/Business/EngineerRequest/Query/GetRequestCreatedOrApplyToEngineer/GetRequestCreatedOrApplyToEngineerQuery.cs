using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestCreatedOrApplyToEngineer
{
    public record GetRequestCreatedOrApplyToEngineerQuery(EngineerRequestParticipationFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetAllEngineerRequestDto>>>;
}