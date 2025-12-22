using Contracting.Domain.Entities.master;
using Contracting.Shared.MasterDtos.BranchDto;
using Contracting.Shared.MasterDtos.DepartmentDtos;
using Mapster;

namespace Contracting.Application.Common.Mappings;

public static class MappingConfig
{
    private const string FallbackImage =
        "https://images.pexels.com/photos/90946/pexels-photo-90946.jpeg?auto=compress&cs=tinysrgb&dpr=1&w=500";

    public static void Register(TypeAdapterConfig config)
    {
        // Add global mapping configurations here if needed in the future
        config.NewConfig<Branch, GetBranchDto>();
        config.NewConfig<CreateBranchDto, Branch>();
        config.NewConfig<UpdateBranchDto, Branch>()
           .Ignore(dest => dest.Departments);
        config.NewConfig<Department, CreateDepartmentDto>();
        config.NewConfig<CreateDepartmentDto, Department>();
        config.NewConfig<UpdateDepartmentDto, Department>();
        config.NewConfig<Department, UpdateDepartmentDto>();

        config.NewConfig<UpdateBranchDto, Branch>()
    .Ignore(dest => dest.Departments);

    }
}