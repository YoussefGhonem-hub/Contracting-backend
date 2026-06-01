using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetChatTabMedia;

public record GetChatMediaQuery(Guid GroupId, string Tab) : IRequest<ErrorOr<List<GetChatMessageDto>>>;
// Tab: "media" | "docs" | "links"
