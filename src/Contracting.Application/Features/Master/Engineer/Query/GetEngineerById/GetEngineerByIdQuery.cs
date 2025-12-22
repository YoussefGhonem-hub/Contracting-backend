using Contracting.Shared.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;
using System;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerById
{
    public record GetEngineerByIdQuery(Guid EngineerId) : IRequest<ErrorOr<GetEngineerDto>>;
}
