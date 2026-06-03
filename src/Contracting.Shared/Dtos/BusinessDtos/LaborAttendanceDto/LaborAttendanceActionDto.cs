namespace Contracting.Shared.BusinessDtos.LaborAttendanceDto
{
    public class LaborAttendanceActionDto
    {
        /// <summary>Submit, Validate, Approve, Close, Assign</summary>
        public string ActionType { get; set; } = string.Empty;
        public Guid? AssignedToId { get; set; }
        public string? Comments { get; set; }
    }
}
