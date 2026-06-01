using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Command.SendTextMessage;

public record SendTextMessageCommand(Guid GroupId, string Content, string MessageType) : IRequest<ErrorOr<GetChatMessageDto>>;
