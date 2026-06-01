using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.Chat.Query.GetFirebaseToken;

public record GetFirebaseTokenQuery(Guid GroupId) : IRequest<ErrorOr<FirebaseTokenDto>>;
