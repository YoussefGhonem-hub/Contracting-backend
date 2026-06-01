using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Command.SendTextMessage;

public class SendTextMessageCommandHandler : IRequestHandler<SendTextMessageCommand, ErrorOr<GetChatMessageDto>>
{
    private readonly IChatService _chatService;

    public SendTextMessageCommandHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<GetChatMessageDto>> Handle(SendTextMessageCommand request, CancellationToken cancellationToken)
    {
        var result = await _chatService.SendTextMessageAsync(request.GroupId, request.Content, request.MessageType, cancellationToken);

        if (result is null)
            return Error.Forbidden("Chat.NotMember", "You are not a member of this chat group.");

        return result;
    }
}
