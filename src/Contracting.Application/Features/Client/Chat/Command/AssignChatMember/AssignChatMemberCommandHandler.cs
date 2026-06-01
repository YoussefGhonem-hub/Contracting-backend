using Contracting.Infrustructure.Inteface.client;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Command.AssignChatMember;

public class AssignChatMemberCommandHandler : IRequestHandler<AssignChatMemberCommand, ErrorOr<bool>>
{
    private readonly IChatService _chatService;

    public AssignChatMemberCommandHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<bool>> Handle(AssignChatMemberCommand request, CancellationToken cancellationToken)
    {
        var result = await _chatService.AssignMemberAsync(request.GroupId, request.UserId, cancellationToken);

        if (!result)
            return Error.NotFound("Chat.GroupOrUserNotFound", "Chat group or user not found.");

        return true;
    }
}
