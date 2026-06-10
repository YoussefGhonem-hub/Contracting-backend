# Business Workflows & API Reference

> **Base URL:** `https://{host}/api`
> All endpoints require `Authorization: Bearer {token}` header.

---

## 1. Procurement / Engineer Request Workflow

### Description
A **Site Engineer** submits a request to a department. The **Team Lead** receives a notification, assigns it to an engineer, actions it through statuses, and optionally records a goods receipt at the end.

### Status Flow

```
[CREATED]
    │
    ▼
[New / Pending]  ──── reject ────►  [Rejected]  (terminal)
    │
    │  assign → engineer picked
    ▼
[In Progress]  ────── missing info ──►  [Missing Information]
    │                                          │
    │                                  engineer updates request
    │                                          │
    │                         auto-reset back to [New/Pending] ◄──┘
    │
    │  mark complete (office engineer)
    ▼
[Completed]  ──── NeedsReceiptConfirmation = true ──►  site engineer confirms receipt
    │
    ▼
[Closed]
```

> Statuses are **dynamic** records from the `Status` table (ordered by `orderNumber`).
> Keywords used: `new/pending`, `in progress`, `delayed`, `completed/done`, `rejected`, `missing information`.

### APIs

| Step | Method | Endpoint | Body / Notes |
|------|--------|----------|--------------|
| 1. Create request | `POST` | `/api/EngineerRequest` | `multipart/form-data` — `CreateEngineerRequestDto` |
| 2. Update request (or add info after "Missing Information") | `PUT` | `/api/EngineerRequest` | `multipart/form-data` — `UpdateEngineerRequestDto` |
| 3. Get single request | `GET` | `/api/EngineerRequest/{requestId}` | — |
| 4. List by department (manager view) | `GET` | `/api/EngineerRequest/department/{departmentId}?pageIndex=1&pageSize=10` | — |
| 5. Filter requests | `GET` | `/api/EngineerRequest/filter?...` | Query params: `EngineerRequestFilterDto` |
| 6. My requests (created or applied) | `GET` | `/api/EngineerRequest/appliedOrCreatedReqeust?...` | — |
| 7. Requests by status for current user | `GET` | `/api/EngineerRequest/byStatus?...` | — |
| 8. Take action (assign / approve / reject / close / missing-info) | `POST` | `/api/EngineerRequest/{requestId}/action` | `multipart/form-data` — `TakeActionRequestDto` |
| 9. Reassign to another engineer | `POST` | `/api/EngineerRequest/{requestId}/reassign` | `application/json` — `ReassignEngineerRequestDto` |
| 10. Confirm delivery date (lock) | `PATCH` | `/api/EngineerRequest/{requestId}/confirm-delivery-date` | — |
| 11. Create goods receipt | `POST` | `/api/EngineerRequest/{requestId}/receipts` | `application/json` — `CreateGoodsReceiptDto` |
| 12. List goods receipts | `GET` | `/api/EngineerRequest/{requestId}/receipts` | — |
| 13. View activity log | `GET` | `/api/EngineerRequest/{requestId}/activities` | — |
| 14. Delete (only if not yet assigned) | `DELETE` | `/api/EngineerRequest/{requestId}` | — |

---

## 2. Transfer Request Workflow

### Description
An engineer creates a materials transfer from one project/warehouse to another. The receiving side confirms receipt (full or partial).

### Status Flow

```
[Draft]  ──── submit ────►  [PendingReceipt]
    │                              │
 cancel                    confirmpartialreceipt
    │                              │
    ▼                              ▼
[Cancelled]             [PartiallyReceived]
                                   │
                           confirmreceipt
                                   │
                                   ▼
                               [Closed]

[PendingReceipt / PartiallyReceived] ── cancel ──► [Cancelled]
```

### Actions for `POST /api/TransferRequest/{id}/action`

| `actionType` | From Status | To Status |
|---|---|---|
| `submit` | Draft | PendingReceipt |
| `confirmreceipt` | PendingReceipt or PartiallyReceived | Closed |
| `confirmpartialreceipt` | PendingReceipt | PartiallyReceived |
| `cancel` | Any (except Closed) | Cancelled |

### APIs

| Step | Method | Endpoint | Body / Notes |
|------|--------|----------|--------------|
| 1. Create transfer request | `POST` | `/api/TransferRequest` | `multipart/form-data` — `CreateTransferRequestDto` (include `items[]`) |
| 2. Update (Draft only) | `PUT` | `/api/TransferRequest` | `multipart/form-data` — `UpdateTransferRequestDto` |
| 3. Get single request | `GET` | `/api/TransferRequest/{id}` | — |
| 4. List / filter | `GET` | `/api/TransferRequest?status=&sourceProjectId=&destinationProjectId=&requestedById=&fromDate=&toDate=&search=&pageIndex=1&pageSize=10` | — |
| 5. Submit request | `POST` | `/api/TransferRequest/{id}/action` | `{ "actionType": "submit" }` |
| 6. Confirm full receipt | `POST` | `/api/TransferRequest/{id}/action` | `{ "actionType": "confirmreceipt", "comments": "..." }` |
| 7. Confirm partial receipt | `POST` | `/api/TransferRequest/{id}/action` | `{ "actionType": "confirmpartialreceipt", "comments": "..." }` |
| 8. Cancel | `POST` | `/api/TransferRequest/{id}/action` | `{ "actionType": "cancel", "comments": "..." }` |
| 9. Delete (Draft only) | `DELETE` | `/api/TransferRequest/{id}` | — |

---

## 3. Labor Attendance Request Workflow

### Description
A **Site Supervisor** records daily worker attendance for a project site. A **Validator** then reviews it, a **Manager/Approver** approves it, and finally it is closed for payroll.

### Status Flow

```
[Draft]  ── submit ──►  [Submitted]  ── validate ──►  [Validated]  ── approve ──►  [Approved]  ── close ──►  [Closed]
```

> There is no rejection step — incorrect records should be corrected before submission (only Draft can be edited).

### Worker Attendance Statuses (per record)

| Status | Pay Calculation |
|---|---|
| `Present` | Full daily rate |
| `HalfDay` | 50% of daily rate |
| `Overtime` | Daily rate + (overtime hours × rate/8) |
| `Absent` | 0 |

### Actions for `POST /api/LaborAttendance/{id}/action`

| `actionType` | From Status | To Status |
|---|---|---|
| `submit` | Draft | Submitted |
| `validate` | Submitted | Validated |
| `approve` | Validated | Approved |
| `close` | Approved | Closed |

### APIs

| Step | Method | Endpoint | Body / Notes |
|------|--------|----------|--------------|
| 1. Create attendance sheet | `POST` | `/api/LaborAttendance` | `multipart/form-data` — `CreateLaborAttendanceRequestDto` (include `records[]`) |
| 2. Update (Draft only) | `PUT` | `/api/LaborAttendance` | `multipart/form-data` — `UpdateLaborAttendanceRequestDto` |
| 3. Get single request | `GET` | `/api/LaborAttendance/{id}` | — |
| 4. List / filter | `GET` | `/api/LaborAttendance?status=&projectId=&supervisorId=&fromDate=&toDate=&search=&pageIndex=1&pageSize=10` | — |
| 5. Submit | `POST` | `/api/LaborAttendance/{id}/action` | `{ "actionType": "submit" }` |
| 6. Validate | `POST` | `/api/LaborAttendance/{id}/action` | `{ "actionType": "validate", "comments": "..." }` |
| 7. Approve | `POST` | `/api/LaborAttendance/{id}/action` | `{ "actionType": "approve", "comments": "..." }` |
| 8. Close | `POST` | `/api/LaborAttendance/{id}/action` | `{ "actionType": "close", "comments": "..." }` |
| 9. Delete (Draft only) | `DELETE` | `/api/LaborAttendance/{id}` | — |

---

## 4. Financial Clearance Workflow

### Description
An employee submits a financial clearance to reconcile an advance payment. Finance reviews it, approves it (requires attachments like receipts), then closes it.

### Status Flow

```
[Draft]  ── submit ──►  [Submitted]  ── review ──►  [UnderReview]  ── approve ──►  [Approved]  ── close ──►  [Closed]
                │                         │                              │
             reject                    reject                          reject
                │                         │                              │
                └─────────────────────────┴──────────────────────────►  [Rejected]  (terminal)
```

> **Validation rules:**
> - `SpentAmount` cannot exceed `AdvanceAmount`
> - `RemainingAmount` is auto-calculated as `AdvanceAmount - SpentAmount`
> - At least one attachment is **required before closing**
> - Only `Draft` clearances can be edited or deleted

### Actions for `POST /api/FinancialClearance/{id}/action`

| `actionType` | From Status | To Status |
|---|---|---|
| `submit` | Draft | Submitted |
| `review` | Submitted | UnderReview |
| `approve` | UnderReview | Approved |
| `close` | Approved | Closed (requires attachments) |
| `reject` | Submitted / UnderReview / Approved | Rejected |

### APIs

| Step | Method | Endpoint | Body / Notes |
|------|--------|----------|--------------|
| 1. Create clearance | `POST` | `/api/FinancialClearance` | `multipart/form-data` — `CreateFinancialClearanceDto` |
| 2. Update (Draft only) | `PUT` | `/api/FinancialClearance` | `multipart/form-data` — `UpdateFinancialClearanceDto` |
| 3. Get single clearance | `GET` | `/api/FinancialClearance/{id}` | — |
| 4. List / filter | `GET` | `/api/FinancialClearance?status=&projectId=&departmentId=&requestedById=&fromDate=&toDate=&search=&pageIndex=1&pageSize=10` | — |
| 5. Submit | `POST` | `/api/FinancialClearance/{id}/action` | `{ "actionType": "submit" }` |
| 6. Set Under Review | `POST` | `/api/FinancialClearance/{id}/action` | `{ "actionType": "review", "comments": "..." }` |
| 7. Approve | `POST` | `/api/FinancialClearance/{id}/action` | `{ "actionType": "approve", "comments": "..." }` |
| 8. Close (must have attachments) | `POST` | `/api/FinancialClearance/{id}/action` | `{ "actionType": "close", "comments": "..." }` |
| 9. Reject | `POST` | `/api/FinancialClearance/{id}/action` | `{ "actionType": "reject", "comments": "reason..." }` |
| 10. Delete (Draft only) | `DELETE` | `/api/FinancialClearance/{id}` | — |

---

## Quick Reference — Action DTOs

### `TakeActionRequestDto` (EngineerRequest)
```json
{
  "actionType": "string",
  "assignToId": "guid (optional, for assign actions)",
  "startDate": "datetime (optional)",
  "endDate": "datetime (optional)",
  "notes": "string (optional)",
  "attachments": ["file (optional)"]
}
```

### `TransferRequestActionDto` / `LaborAttendanceActionDto` / `FinancialClearanceActionDto`
```json
{
  "actionType": "submit | confirm... | validate | approve | close | reject | cancel",
  "comments": "string (optional)"
}
```

---

## Workflow Summary Diagram

```
PROCUREMENT (EngineerRequest)
  Site Engineer  ──POST /EngineerRequest──►  [New]
  Team Lead      ──POST /{id}/action (assign)──►  [In Progress]
  Team Lead      ──POST /{id}/action (close)──►  [Completed]
  Site Engineer  ──POST /{id}/action (confirm)──►  [Closed]

TRANSFER REQUEST
  Engineer  ──POST /TransferRequest──►  [Draft]
  Engineer  ──action: submit──►  [PendingReceipt]
  Receiver  ──action: confirmreceipt──►  [Closed]
             ──action: confirmpartialreceipt──►  [PartiallyReceived]
                                              └──action: confirmreceipt──►  [Closed]

LABOR ATTENDANCE
  Supervisor  ──POST /LaborAttendance──►  [Draft]
  Supervisor  ──action: submit──►  [Submitted]
  Validator   ──action: validate──►  [Validated]
  Manager     ──action: approve──►  [Approved]
  Admin       ──action: close──►  [Closed]

FINANCIAL CLEARANCE
  Employee  ──POST /FinancialClearance──►  [Draft]
  Employee  ──action: submit──►  [Submitted]
  Finance   ──action: review──►  [UnderReview]
  Manager   ──action: approve──►  [Approved]
  Admin     ──action: close──►  [Closed]
  (any stage after Draft) ──action: reject──►  [Rejected]
```
