using Contracting.Shared.Dtos.ClientDtos.DrawingDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Drawing.Query.GetClientDrawings;

public record GetClientDrawingsQuery(Guid ProjectId, string? Type) : IRequest<ErrorOr<List<GetClientDrawingDto>>>;
