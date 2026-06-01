using Contracting.Infrustructure.Inteface.client;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Command.MarkMessagesRead;

public class MarkMessagesReadCommandHandler : IRequestHandler<MarkMessagesReadCommand, ErrorOr<bool>>
{
    private readonly IChatService _chatService;

    public MarkMessagesReadCommandHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<bool>> Handle(MarkMessagesReadCommand request, CancellationToken cancellationToken)
    {
        await _chatService.MarkMessagesReadAsync(request.GroupId, cancellationToken);
        return true;
    }
}
