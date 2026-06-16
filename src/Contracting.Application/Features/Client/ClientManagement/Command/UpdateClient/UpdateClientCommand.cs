using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.ClientManagement.Command.UpdateClient
{
    public record UpdateClientCommand(UpdateClientDto Client) : IRequest<ErrorOr<GetClientDto>>;
}
