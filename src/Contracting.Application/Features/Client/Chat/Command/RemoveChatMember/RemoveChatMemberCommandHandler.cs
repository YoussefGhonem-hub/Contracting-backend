using Contracting.Infrustructure.Inteface.client;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Command.RemoveChatMember;

public class RemoveChatMemberCommandHandler : IRequestHandler<RemoveChatMemberCommand, ErrorOr<bool>>
{
    private readonly IChatService _chatService;

    public RemoveChatMemberCommandHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<bool>> Handle(RemoveChatMemberCommand request, CancellationToken cancellationToken)
    {
        var result = await _chatService.RemoveMemberAsync(request.GroupId, request.UserId, cancellationToken);

        if (!result)
            return Error.Failure("Chat.RemoveFailed", "Could not remove member. Member not found or is the project client.");

        return true;
    }
}
