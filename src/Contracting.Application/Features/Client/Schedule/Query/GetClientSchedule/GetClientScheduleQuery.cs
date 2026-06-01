using Contracting.Shared.Dtos.ClientDtos.ScheduleDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Schedule.Query.GetClientSchedule;

public record GetClientScheduleQuery(Guid ProjectId) : IRequest<ErrorOr<GetClientScheduleDto>>;
