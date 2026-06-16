using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.ClientManagement.Query.GetClientList
{
    public record GetClientListQuery(BaseFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetClientDto>>>;
}
