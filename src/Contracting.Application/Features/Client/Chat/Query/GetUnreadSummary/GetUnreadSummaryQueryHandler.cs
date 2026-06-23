using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetUnreadSummary;

public class GetUnreadSummaryQueryHandler : IRequestHandler<GetUnreadSummaryQuery, ErrorOr<ChatUnreadSummaryDto>>
{
    private readonly IChatService _chatService;

    public GetUnreadSummaryQueryHandler(IChatService chatService)
    {
        _chatService = chatService;
    }

    public async Task<ErrorOr<ChatUnreadSummaryDto>> Handle(GetUnreadSummaryQuery request, CancellationToken cancellationToken)
    {
        return await _chatService.GetUnreadSummaryAsync(cancellationToken);
    }
}
