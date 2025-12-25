using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.Dtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestCreatedOrApplyToEngineer
{
    public record GetRequestCreatedOrApplyToEngineerQuery(BaseFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetAllEngineerRequestDto>>>;
}