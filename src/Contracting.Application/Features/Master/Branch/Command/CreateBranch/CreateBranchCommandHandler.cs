using Azure.Core;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.MasterDtos.BranchDto;
using ErrorOr;
using Mapster;
using MapsterMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Branch.Command.CreateBranch
{
    public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, ErrorOr<GetBranchDto>>
    {
        private readonly IBranchService _service;
        private readonly IMapper _mapper;

        public CreateBranchCommandHandler(IBranchService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        public async Task<ErrorOr<GetBranchDto>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var obj= await _service.AddAsync(request.Branch);
                return obj;
            }
            catch (Exception ex)
            {
                return Error.Failure($"Could not create branch: {ex.Message}");
            }
        }
    }
}
