using Contracting.Shared.MasterDtos.DepartmentDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Shared.MasterDtos.BranchDto
{
    public class CreateBranchDto
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? address { get; set; }
        public string? location { get; set; }
        public List<CreateDepartmentDto> Departments { get; set; } = new();
    }
}
