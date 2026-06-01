using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Command.SendAttachment;

public class SendAttachmentCommandHandler : IRequestHandler<SendAttachmentCommand, ErrorOr<GetChatMessageDto>>
{
    private readonly IChatService _chatService;

    public SendAttachmentCommandHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<GetChatMessageDto>> Handle(SendAttachmentCommand request, CancellationToken cancellationToken)
    {
        var result = await _chatService.SendAttachmentMessageAsync(request.GroupId, request.File, cancellationToken);

        if (result is null)
            return Error.Forbidden("Chat.NotMember", "You are not a member of this chat group.");

        return result;
    }
}
