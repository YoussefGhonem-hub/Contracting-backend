using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.ClientManagement.Query.GetClientById
{
    public record GetClientByIdQuery(Guid ClientId) : IRequest<ErrorOr<GetClientDto>>;
}
