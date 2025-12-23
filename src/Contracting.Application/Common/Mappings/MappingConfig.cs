using Contracting.Domain.Entities.master;
using Contracting.Shared.MasterDtos.BranchDto;
using Contracting.Shared.MasterDtos.DepartmentDtos;
using Contracting.Shared.MasterDtos.EngineerDto;
using Contracting.Shared.MasterDtos.PriorityDto;
using Contracting.Shared.MasterDtos.ProjectDtos;
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

        // Department mappings
        config.NewConfig<Department, GetDepartmentDto>();

        config.NewConfig<Department, UpdateDepartmentDto>();
        config.NewConfig<Department, CreateDepartmentDto>();
        config.NewConfig<CreateDepartmentDto, Department>();
        config.NewConfig<UpdateDepartmentDto, Department>();

        // Engineer mappings
        config.NewConfig<CreateEngineerDto, Engineer>();
        config.NewConfig<UpdateEngineerDto, Engineer>();

        config.NewConfig<Engineer, GetEngineerDto>()
        .Map(dest => dest.Department, src => src.Department);
        config.NewConfig<Engineer, GetEngineerDropDownDto>()
        .Map(dest => dest.Department, src => src.Department);

        config.NewConfig<Project, UpdateProjectDto>();
        config.NewConfig<Project, CreateProjectDto>();
        config.NewConfig<CreateProjectDto, Project>();
        config.NewConfig<UpdateProjectDto, Project>();

        config.NewConfig<CreatePriorityDto, Priority>();
        config.NewConfig<UpdatePriorityDto, Priority>();
        config.NewConfig<Priority, GetDropDownPriorityDto>();
    }
}