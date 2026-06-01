using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Command.RemoveChatMember;

public record RemoveChatMemberCommand(Guid GroupId, Guid UserId) : IRequest<ErrorOr<bool>>;
