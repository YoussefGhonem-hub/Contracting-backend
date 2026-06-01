namespace Contracting.Shared.Dtos.HelperDtos
{
    public class GetNotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;
        public bool IsRead { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public Guid? EngineerId { get; set; }
        public string? EngineerName { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? RequestId { get; set; }
        public string? RequestTitle { get; set; }
        public DateTimeOffset SentAt { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }

    public class NotificationFilterDto : BaseFilterDto
    {
        public bool? IsRead { get; set; }
    }

    public class UnreadNotificationCountDto
    {
        public int Count { get; set; }
    }
}
