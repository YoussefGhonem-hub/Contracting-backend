using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Status.Command.CreateStatus
{
    public record CreateStatusCommand(CreateStatusDto Status) : IRequest<ErrorOr<GetDropDownStatusDto>>;
}