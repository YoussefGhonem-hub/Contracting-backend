namespace Contracting.Shared.BusinessDtos.LaborAttendanceDto
{
    public class LaborAttendanceActionDto
    {
        /// <summary>Submit, Validate, Approve, Close</summary>
        public string ActionType { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }
}
