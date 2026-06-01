using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetChatGroup;

public record GetChatGroupQuery(Guid GroupId) : IRequest<ErrorOr<GetChatGroupDto>>;
