using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetOrCreateChatGroup;

public class GetOrCreateChatGroupQueryHandler : IRequestHandler<GetOrCreateChatGroupQuery, ErrorOr<GetChatGroupDto>>
{
    private readonly IChatService _chatService;

    public GetOrCreateChatGroupQueryHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<GetChatGroupDto>> Handle(GetOrCreateChatGroupQuery request, CancellationToken cancellationToken)
    {
        var result = await _chatService.GetOrCreateGroupAsync(request.ProjectId, cancellationToken);

        if (result is null)
            return Error.NotFound("Chat.ProjectNotFound", "Project not found.");

        return result;
    }
}
