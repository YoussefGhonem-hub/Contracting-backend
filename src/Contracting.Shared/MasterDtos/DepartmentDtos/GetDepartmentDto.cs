using Contracting.Shared.MasterDtos.EngineerDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Shared.MasterDtos.DepartmentDtos
{
    public class GetDepartmentDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
    }
}
