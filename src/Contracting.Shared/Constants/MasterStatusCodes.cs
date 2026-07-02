namespace Contracting.Shared.Constants
{
    /// <summary>
    /// Code column values from master.Statuses — consistent across all environments.
    /// Never use the Id (Guid) directly; always resolve via these codes.
    /// </summary>
    public static class MasterStatusCodes
    {
        public const string New                = "NEW";
        public const string InProgress        = "IN_PROGRESS";
        public const string Completed         = "COMPLETED";
        public const string Rejected          = "REJECTED";
        public const string MissingInformation = "missing_information";
    }
}
