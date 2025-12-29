using Microsoft.AspNetCore.Identity;

namespace Contracting.Shared.MasterDtos.UserDtos
{
    public class GetUserDto
    {
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
