using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Contracting.Application.Features.Client.Chat.Command.SendAttachment;

public record SendAttachmentCommand(Guid GroupId, IFormFile File) : IRequest<ErrorOr<GetChatMessageDto>>;
