namespace Contracting.Shared.Constants
{
    /// <summary>
    /// Known request-type keys that can have a configured default department.
    /// Kept as string constants (not an enum) to match the existing "RequestType" string
    /// discriminator used across FinancialClearance / LaborAttendance / EngineerRequest.
    /// </summary>
    public static class RequestTypeKeys
    {
        public const string FinancialClearance = "FinancialClearance";
        public const string LaborAttendance = "LaborAttendance";
        public const string EngineerRequest = "EngineerRequest";
        public const string InternalRequest = "InternalRequest";

        public static readonly string[] All = { FinancialClearance, LaborAttendance, EngineerRequest, InternalRequest };
    }
}
