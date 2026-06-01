using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetOrCreateChatGroup;

public record GetOrCreateChatGroupQuery(Guid ProjectId) : IRequest<ErrorOr<GetChatGroupDto>>;
