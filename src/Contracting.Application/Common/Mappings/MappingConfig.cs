using Contracting.Domain.Entities;
using Contracting.Domain.Entities.business;
using Contracting.Domain.Entities.master;
using Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;
using Contracting.Shared.Dtos.MasterDtos.BranchDto;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.PriorityDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
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
        .Map(dest => dest.Department, src => src.Department).Ignore(dest => dest.Roles);
        config.NewConfig<Engineer, GetEngineerDropDownDto>()
        .Map(dest => dest.Department, src => src.Department);

        config.NewConfig<Project, UpdateProjectDto>();
        config.NewConfig<Project, CreateProjectDto>();
        config.NewConfig<CreateProjectDto, Project>();
        config.NewConfig<UpdateProjectDto, Project>();
        config.NewConfig<Project, GetProjectDto>();


        config.NewConfig<CreatePriorityDto, Priority>();
        config.NewConfig<UpdatePriorityDto, Priority>();
        config.NewConfig<Priority, GetDropDownPriorityDto>();

        config.NewConfig<CreateStatusDto, Status>();
        config.NewConfig<UpdateStatusDto, Status>();
        config.NewConfig<Status, GetDropDownStatusDto>();

        config.NewConfig<CreateEngineerRequestDto, EngineerRequest>();
        config.NewConfig<UpdateEngineerRequestDto, EngineerRequest>();

        config.NewConfig<CrearteEngineerRequestNotesDto, EngineerRequestNotes>();
          config.NewConfig<EngineerRequestNotes, GetEngineerRequestNotesDto>()
              .Map(dest => dest.Engineer, src => src.Engineer)
              .Map(dest => dest.Attachments, src => src.EngineerRequestAttachments);

          config.NewConfig<EngineerRequestAttachment, Contracting.Shared.Dtos.GetAttachmentDto>();

        config.NewConfig<Status, GetDropDownStatusDto>();
        
        config.NewConfig<EngineerRequestActivite, GetEngineerRequestActiviteDto>()
            .Map(dest => dest.EngineerName, src => src.Engineer != null ? $"{src.Engineer.nameEn} / {src.Engineer.nameAr}" : null)
            .Map(dest => dest.StatusName, src => src.Status != null ? $"{src.Status.nameEn} / {src.Status.nameAr}" : null);
        
        config.NewConfig<EngineerRequest, GetAllEngineerRequestDto>()
                    .Map(dest => dest.Project, src => src.Project)
                    .Map(dest => dest.Department, src => src.Department)
                    .Map(dest => dest.Priority, src => src.Priority)
                    .Map(dest => dest.Engineer, src => src.Engineer)
                    .Map(dest => dest.assignTo, src => src.assignTo)
                    .Map(dest => dest.Status, src => src.Status)
                    .Map(dest => dest.EngineerRequestNotes, src => src.EngineerRequestNotes)
                    .Map(dest => dest.EngineerRequestActivites, src => src.EngineerRequestActivites)
                    .Map(dest => dest.EngineerRequestAttachments, src => src.EngineerRequestAttachments);


        config.NewConfig<CreateRoleDto, ApplicationRole>()
               .Map(dest => dest.Name, src => src.Name)
               .Map(dest => dest.DisplayName, src => src.DisplayName);

        // Map UpdateRoleDto to ApplicationRole
        config.NewConfig<UpdateRoleDto, ApplicationRole>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.DisplayName, src => src.DisplayName);

        // Map ApplicationRole to RoleDropDownDto
        config.NewConfig<ApplicationRole, RoleDropDownDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name);




    }
}