using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetUnreadCount;

public record GetUnreadCountQuery(Guid GroupId) : IRequest<ErrorOr<int>>;
