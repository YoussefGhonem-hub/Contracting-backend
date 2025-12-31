using Contracting.Shared.Dtos.MasterDtos.BranchDto;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Branch.Command.CreateBranch
{
    public record CreateBranchCommand(CreateBranchDto Branch) : IRequest<ErrorOr<GetBranchDto>>;

}
