namespace Contracting.Shared.Constants
{
    public static class ProjectFeatureConstants
    {
        public const string Chat = "Chat";
        public const string Invoices = "Invoices";
        public const string Tender = "Tender";
        public const string Variation = "Variation";
        public const string TwoDDrawings = "TwoDDrawings";
        public const string ThreeDDrawings = "ThreeDDrawings";
        public const string Schedule = "Schedule";
        public const string MonthlyReports = "MonthlyReports";

        public static readonly IReadOnlyList<string> All = new[]
        {
            Chat,
            Invoices,
            Tender,
            Variation,
            TwoDDrawings,
            ThreeDDrawings,
            Schedule,
            MonthlyReports
        };
    }
}
