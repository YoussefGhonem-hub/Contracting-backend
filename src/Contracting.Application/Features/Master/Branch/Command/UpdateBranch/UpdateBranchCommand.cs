using Contracting.Shared.Dtos.MasterDtos.BranchDto;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Branch.Command.UpdateBranch
{
    public record UpdateBranchCommand(UpdateBranchDto Branch) : IRequest<ErrorOr<GetBranchDto>>;

}
