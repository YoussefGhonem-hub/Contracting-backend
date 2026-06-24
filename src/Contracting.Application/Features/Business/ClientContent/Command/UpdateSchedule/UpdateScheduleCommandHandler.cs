using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.ClientContent.Command.UpdateSchedule;

public class UpdateScheduleCommandHandler : IRequestHandler<UpdateScheduleCommand, ErrorOr<UpdatedScheduleDto>>
{
    private readonly IClientContentService _service;

    public UpdateScheduleCommandHandler(IClientContentService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<UpdatedScheduleDto>> Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateScheduleAsync(request.ScheduleId, request.Title, request.Version, request.File, cancellationToken);

        if (result is null)
            return Error.NotFound("Schedule.NotFound", "Schedule not found.");

        return result;
    }
}
