namespace Contracting.Shared.Constants;

public enum NotificationKey
{
    // ── Engineer Requests ────────────────────────────────────────────────────
    NewRequest                  = 1,   // Request created → notify engineer + team lead
    RequestUpdated              = 2,   // General update on a request
    Assigned                    = 3,   // Request assigned to an engineer
    AssignedUpdate              = 4,   // Assignment changed → notify original creator
    Reassigned                  = 5,   // Request reassigned to a different engineer
    MissingInfo                 = 6,   // Status set to "Missing Information" → notify creator
    ReceiptRequired             = 7,   // Office engineer marks completed → site engineer must confirm receipt
    PartialReceipt              = 8,   // Site engineer records a partial goods receipt
    PartialReceiptPendingReview = 9,   // Partial receipt recorded, awaiting office engineer review
    GoodsReceiptRecorded        = 10,  // Full goods receipt recorded by site engineer
    ReceiptConfirmed            = 11,  // Office engineer confirms the receipt → request closed
    ClosedOnReceipt             = 12,  // Request auto-closed after receipt confirmation
    StatusChanged               = 13,  // Engineer/manager manually changes request status
    StatusChangedAuto           = 14,  // System auto-changes status (e.g. deadline passed → Delayed)

    // ── Labor Attendance ─────────────────────────────────────────────────────
    LaborAssigned               = 20,  // Labor attendance request assigned to reviewer
    LaborValidated              = 21,  // Labor attendance request validated/approved
    LaborRejected               = 22,  // Labor attendance request rejected
    LaborReassigned             = 23,  // Labor attendance request reassigned

    // ── Financial Clearance ──────────────────────────────────────────────────
    FinancialReassigned         = 30,  // Financial clearance request reassigned

    // ── Chat ─────────────────────────────────────────────────────────────────
    ChatGroupAdded              = 40,  // User added to a chat group
    ChatNewMessage              = 41,  // New message received in a chat group
}
