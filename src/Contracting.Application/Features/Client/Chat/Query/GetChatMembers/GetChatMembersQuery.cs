using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetChatMembers;

public record GetChatMembersQuery(Guid GroupId) : IRequest<ErrorOr<List<GetChatMemberDto>>>;
