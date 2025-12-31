using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Shared.Dtos.MasterDtos.BranchDto
{
    public class GetBranchDto
    {
        public Guid Id { get; set; } 
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? address { get; set; }
        public string? location { get; set; }
        public List<GetDepartmentDto> Departments { get; set; } = new();
    }
}
