using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.ClientManagement.Command.CreateClient
{
    public record CreateClientCommand(CreateClientDto Client) : IRequest<ErrorOr<GetClientDto>>;
}
