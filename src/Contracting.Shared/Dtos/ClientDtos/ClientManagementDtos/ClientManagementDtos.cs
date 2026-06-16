using Contracting.Shared.Dtos.MasterDtos.RoleDtos;

namespace Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos
{
    /// <summary>Payload for creating a new client (creates the login user + Client role + profile).</summary>
    public class CreateClientDto
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? CompanyName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        /// <summary>Optional list of project IDs to grant this client access to.</summary>
        public List<Guid> ProjectIds { get; set; } = new();
    }

    /// <summary>Payload for updating an existing client and its login user.</summary>
    public class UpdateClientDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationUserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? CompanyName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public List<Guid> ProjectIds { get; set; } = new();
    }

    public class GetClientDto
    {
        public Guid Id { get; set; }
        public Guid ApplicationUserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? CompanyName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public List<RoleDropDownDto> Roles { get; set; } = new();
        public List<GetClientProjectItemDto> Projects { get; set; } = new();
    }

    public class GetClientProjectItemDto
    {
        public Guid ProjectId { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? Code { get; set; }
    }
}
