using Contracting.Shared.MasterDtos.StatusDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Status.Command.UpdateStatus
{
    public record UpdateStatusCommand(UpdateStatusDto Status) : IRequest<ErrorOr<GetDropDownStatusDto>>;
}