using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestById
{
    public record GetRequestByIdQuery(Guid RequestId) : IRequest<ErrorOr<GetAllEngineerRequestDto>>;
}