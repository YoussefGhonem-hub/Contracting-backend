using Contracting.Shared.MasterDtos.BranchDto;
using ErrorOr;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Branch.Query.GetBranchById
{
    public record GetBranchByIdQuery(Guid Id) : IRequest<ErrorOr<GetBranchDto>>;

}
