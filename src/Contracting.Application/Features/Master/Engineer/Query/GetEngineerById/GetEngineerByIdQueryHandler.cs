using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerById
{
    public class GetEngineerByIdQueryHandler : IRequestHandler<GetEngineerByIdQuery, ErrorOr<GetEngineerDto>>
    {
        private readonly IEngineerService _service;

        public GetEngineerByIdQueryHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetEngineerDto>> Handle(GetEngineerByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetEngineerByIdAsync(request.EngineerId);
            return result is null
                ? Error.NotFound("Engineer not found.")
                : result;
        }
    }
}
