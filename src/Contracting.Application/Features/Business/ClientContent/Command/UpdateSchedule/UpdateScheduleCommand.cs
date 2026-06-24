using Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Contracting.Application.Features.Business.ClientContent.Command.UpdateSchedule;

public record UpdateScheduleCommand(
    Guid ScheduleId,
    string? Title,
    string? Version,
    IFormFile? File) : IRequest<ErrorOr<UpdatedScheduleDto>>;
