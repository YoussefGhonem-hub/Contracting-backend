using Contracting.Domain.Entities;
using Contracting.Domain.Entities.business;
using Contracting.Domain.Entities.helper;
using Contracting.Domain.Entities.master;
using Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Dtos.HelperDtos;
using Contracting.Shared.Dtos.MasterDtos.BranchDto;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.PriorityDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
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
        config.NewConfig<Department, GetDepartmentDto>()
            .Map(dest => dest.hasSpecialFields, src => src.hasSpecialFields || src.DepartmentSpecialFields.Any())
            .Map(dest => dest.SpecialFields, src => src.DepartmentSpecialFields);

        config.NewConfig<Department, UpdateDepartmentDto>();
        config.NewConfig<Department, CreateDepartmentDto>();
        config.NewConfig<CreateDepartmentDto, Department>();
        config.NewConfig<UpdateDepartmentDto, Department>();

        // Engineer mappings
        config.NewConfig<CreateEngineerDto, Engineer>();
        config.NewConfig<UpdateEngineerDto, Engineer>();

        config.NewConfig<Engineer, GetEngineerDto>()
        .Map(dest => dest.Department, src => src.Department)
        .Ignore(dest => dest.Projects)
        .Ignore(dest => dest.Roles);
        config.NewConfig<Engineer, GetEngineerDropDownDto>()
        .Map(dest => dest.Department, src => src.Department);

        // EngineerDepartment mappings
        config.NewConfig<EngineerDepartment, EngineerDepartmentRoleDto>()
            .Map(dest => dest.Department, src => src.Department)
            .Map(dest => dest.Role, src => src.Role);

        config.NewConfig<Project, UpdateProjectDto>();
        config.NewConfig<Project, CreateProjectDto>();
        config.NewConfig<CreateProjectDto, Project>()
            .Ignore(dest => dest.imageUrl)
            .Ignore(dest => dest.imageKey);
        config.NewConfig<UpdateProjectDto, Project>()
            .Ignore(dest => dest.imageUrl)
            .Ignore(dest => dest.imageKey);
        config.NewConfig<Project, GetProjectDto>();
        config.NewConfig<Project, GetProjectDropDownDto>();


        config.NewConfig<CreatePriorityDto, Priority>();
        config.NewConfig<UpdatePriorityDto, Priority>();
        config.NewConfig<Priority, GetDropDownPriorityDto>();

        config.NewConfig<SpecialField, SpecialFieldDto>();
        config.NewConfig<DepartmentSpecialField, DepartmentSpecialFieldDto>()
            .Map(dest => dest.name, src => src.SpecialField != null ? src.SpecialField.name : null)
            .Map(dest => dest.fieldType, src => src.SpecialField != null ? src.SpecialField.fieldType : null);
        config.NewConfig<EngineerProject, GetProjectDropDownDto>()
            .Map(dest => dest.Id, src => src.ProjectId)
            .Map(dest => dest.Branch, src => src.Project != null ? src.Project.Branch : null)
            .Map(dest => dest.BranchId, src => src.Project != null ? src.Project.BranchId : null)
            .Map(dest => dest.nameEn, src => src.Project != null ? src.Project.nameEn : null)
            .Map(dest => dest.nameAr, src => src.Project != null ? src.Project.nameAr : null)
            .Map(dest => dest.location, src => src.Project != null ? src.Project.location : null)
            .Map(dest => dest.imageUrl, src => src.Project != null ? src.Project.imageUrl : null)
            .Map(dest => dest.Code, src => src.Project != null ? src.Project.Code : null);

        config.NewConfig<CreateStatusDto, Status>();
        config.NewConfig<UpdateStatusDto, Status>();
        config.NewConfig<Status, GetDropDownStatusDto>();

        config.NewConfig<CreateEngineerRequestDto, EngineerRequest>()
            .Ignore(dest => dest.SpecialFieldValues)
            .Ignore(dest => dest.SpecialFieldItems);
        config.NewConfig<UpdateEngineerRequestDto, EngineerRequest>()
            .Ignore(dest => dest.SpecialFieldValues)
            .Ignore(dest => dest.SpecialFieldItems);

        config.NewConfig<EngineerRequestSpecialFieldValue, EngineerRequestSpecialFieldValueDto>()
            .Map(dest => dest.fieldName, src => src.DepartmentSpecialField != null && src.DepartmentSpecialField.SpecialField != null ? src.DepartmentSpecialField.SpecialField.name : null)
            .Map(dest => dest.fieldType, src => src.DepartmentSpecialField != null && src.DepartmentSpecialField.SpecialField != null ? src.DepartmentSpecialField.SpecialField.fieldType : null);

        config.NewConfig<EngineerRequestSpecialFieldItem, GetEngineerRequestSpecialFieldItemDto>()
            .Map(dest => dest.ConstructionItem, src => src.ConstructionItem);

        config.NewConfig<CrearteEngineerRequestNotesDto, EngineerRequestNotes>();
          config.NewConfig<EngineerRequestNotes, GetEngineerRequestNotesDto>()
              .Map(dest => dest.Engineer, src => src.Engineer)
              .Map(dest => dest.Attachments, src => src.EngineerRequestAttachments);

          config.NewConfig<EngineerRequestAttachment, Contracting.Shared.Dtos.GetAttachmentDto>();

        config.NewConfig<EngineerSiteWorkLogAttachment, Contracting.Shared.Dtos.GetAttachmentDto>();

        config.NewConfig<EngineerSiteSurveyQuestionTemplate, GetEngineerSiteSurveyQuestionTemplateDto>();

        // ConstructionItem mappings
        config.NewConfig<ConstructionItemUnit, ConstructionItemUnitDto>();
        config.NewConfig<ConstructionItem, GetConstructionItemDto>()
            .Map(dest => dest.Units, src => src.Units);
        config.NewConfig<ConstructionItem, GetConstructionItemDropdownDto>()
            .Map(dest => dest.Units, src => src.Units);

        // Report worker mapping
        config.NewConfig<ReportConstructionItemWorker, GetReportWorkerDto>()
            .Map(dest => dest.ConstructionItem, src => src.ConstructionItem);

        // Report attachment mapping
        config.NewConfig<EngineerSiteReportAttachment, Contracting.Shared.Dtos.GetAttachmentDto>();

        // EngineerSiteReport mapping (new structure)
        config.NewConfig<EngineerSiteReport, GetEngineerSiteReportDto>()
            .Map(dest => dest.Project, src => src.Project)
            .Map(dest => dest.Engineer, src => src.Engineer)
            .Map(dest => dest.Workers, src => src.Workers)
            .Map(dest => dest.Attachments, src => src.Attachments);

        config.NewConfig<Status, GetDropDownStatusDto>();
        
        config.NewConfig<EngineerRequestActivite, GetEngineerRequestActiviteDto>()
            .Map(dest => dest.EngineerName, src => src.Engineer != null ? $"{src.Engineer.nameEn} / {src.Engineer.nameAr}" : null)
            .Map(dest => dest.StatusName, src => src.Status != null ? $"{src.Status.nameEn} / {src.Status.nameAr}" : null);
        
        config.NewConfig<EngineerRequest, GetAllEngineerRequestDto>()
                    .Map(dest => dest.RequestType, src => string.IsNullOrEmpty(src.RequestType) ? "EngineerRequest" : src.RequestType)
                    .Map(dest => dest.Project, src => src.Project)
                    .Map(dest => dest.Department, src => src.Department)
                    .Map(dest => dest.Priority, src => src.Priority)
                    .Map(dest => dest.Engineer, src => src.Engineer)
                    .Map(dest => dest.assignTo, src => src.assignTo)
                    .Map(dest => dest.Status, src => src.Status)
                    .Map(dest => dest.EngineerRequestNotes, src => src.EngineerRequestNotes)
                    .Map(dest => dest.EngineerRequestActivites, src => src.EngineerRequestActivites)
                    .Map(dest => dest.EngineerRequestAttachments, src => src.EngineerRequestAttachments)
                    .Map(dest => dest.SpecialFieldItems, src => src.SpecialFieldItems);


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

        // NotificationLog mappings
        config.NewConfig<NotificationLog, GetNotificationDto>()
            .Map(dest => dest.EngineerName, src => src.Engineer != null ? $"{src.Engineer.nameEn} / {src.Engineer.nameAr}" : null);


    }
}