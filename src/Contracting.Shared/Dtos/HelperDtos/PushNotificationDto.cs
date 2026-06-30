namespace Contracting.Shared.Dtos.HelperDtos
{
    public class PushNotificationDto
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Token { get; set; } // FCM device token
        public Guid? UserId { get; set; } // recipient ApplicationUser id (used for logging in the background job)
        public string? EngineerId { get; set; }
        public string? DepartmentId { get; set; }
        public string? RequestId { get; set; }
        public string? ChatGroupId { get; set; }
        public string? Type { get; set; }
        public Dictionary<string, string>? Data { get; set; }
    }
}
