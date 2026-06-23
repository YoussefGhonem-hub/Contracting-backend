using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetUnreadSummary;

public record GetUnreadSummaryQuery() : IRequest<ErrorOr<ChatUnreadSummaryDto>>;
