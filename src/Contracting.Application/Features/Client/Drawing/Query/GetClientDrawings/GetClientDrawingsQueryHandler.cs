using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.DrawingDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Drawing.Query.GetClientDrawings;

public class GetClientDrawingsQueryHandler : IRequestHandler<GetClientDrawingsQuery, ErrorOr<List<GetClientDrawingDto>>>
{
    private readonly IClientDrawingService _service;

    public GetClientDrawingsQueryHandler(IClientDrawingService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<List<GetClientDrawingDto>>> Handle(GetClientDrawingsQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetDrawingsAsync(request.ProjectId, request.Type, cancellationToken: cancellationToken);

        if (result is null)
            return Error.NotFound("Drawing.ProjectNotFound", "Project not found or not assigned to this client.");

        return result;
    }
}
