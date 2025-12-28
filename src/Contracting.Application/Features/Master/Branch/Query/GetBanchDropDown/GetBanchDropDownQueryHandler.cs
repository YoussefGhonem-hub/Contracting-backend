using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.BranchDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Branch.Query.GetBanchDropDown
{
    public sealed class GetBanchDropDownQueryHandler
    : IRequestHandler<GetBanchDropDownQuery, ErrorOr<List<BranchDropDownDto>>>
    {
        private readonly IBranchService _service;

        public GetBanchDropDownQueryHandler(IBranchService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<BranchDropDownDto>>> Handle(
            GetBanchDropDownQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _service.DropDownMethodAsync();
            return result;
        }
    }
}
