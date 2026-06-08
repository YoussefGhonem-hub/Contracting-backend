namespace Contracting.Shared.Constants
{
    /// <summary>
    /// Valid action types for the Financial Clearance TakeAction endpoint.
    /// Submit → Review → Approve → Close  (or Reject at any stage)
    /// </summary>
    public enum FinancialClearanceActionType
    {
        Submit,
        Review,
        Approve,
        Close,
        Reject
    }
}
