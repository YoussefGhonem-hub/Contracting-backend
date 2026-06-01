using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ScheduleDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Schedule.Query.GetClientSchedule;

public class GetClientScheduleQueryHandler : IRequestHandler<GetClientScheduleQuery, ErrorOr<GetClientScheduleDto>>
{
    private readonly IClientScheduleService _service;

    public GetClientScheduleQueryHandler(IClientScheduleService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<GetClientScheduleDto>> Handle(GetClientScheduleQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetScheduleAsync(request.ProjectId, cancellationToken);

        if (result is null)
            return Error.NotFound("Schedule.ProjectNotFound", "Project not found or not assigned to this client.");

        return result;
    }
}
