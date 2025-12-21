using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Branch.Command.DeleteBranch
{
    public record DeleteBranchCommand(Guid Id) : IRequest<ErrorOr<bool>>;
}
