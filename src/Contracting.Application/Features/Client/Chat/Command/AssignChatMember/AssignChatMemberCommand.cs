using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Command.AssignChatMember;

public record AssignChatMemberCommand(Guid GroupId, Guid UserId) : IRequest<ErrorOr<bool>>;
