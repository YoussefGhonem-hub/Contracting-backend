using Contracting.Application.Features.Client.ClientProject.Query.GetClientProjects;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.ClientProject.Query.GetClientProjects;

public class GetClientProjectsQueryHandler : IRequestHandler<GetClientProjectsQuery, ErrorOr<List<GetClientProjectListItemDto>>>
{
    private readonly IClientProjectService _service;

    public GetClientProjectsQueryHandler(IClientProjectService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<List<GetClientProjectListItemDto>>> Handle(GetClientProjectsQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetClientProjectsAsync(cancellationToken);
        return result; // implicit conversion List<T> -> ErrorOr<List<T>>
    }
}
