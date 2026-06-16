namespace Contracting.Shared.BusinessDtos.LaborAttendanceDto
{
    /// <summary>Reassigns a labor attendance request to a different engineer (status unchanged).</summary>
    public class ReassignLaborAttendanceDto
    {
        public Guid AssignedToId { get; set; }
        public string? Comments { get; set; }
    }
}
