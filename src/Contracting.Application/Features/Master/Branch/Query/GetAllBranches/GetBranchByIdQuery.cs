using Contracting.Shared.Dtos.MasterDtos.BranchDto;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Branch.Query.GetAllBranches
{
    public record GetBranchByIdQuery(Guid Id) : IRequest<ErrorOr<GetBranchDto>>;

}
