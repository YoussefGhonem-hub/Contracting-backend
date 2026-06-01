using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetChatMessages;

public record GetChatMessagesQuery(Guid GroupId, int Page = 1, int PageSize = 30) : IRequest<ErrorOr<GetChatMessagesPagedDto>>;
