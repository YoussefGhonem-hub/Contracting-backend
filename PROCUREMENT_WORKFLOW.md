# Procurement Department – Purchase Request Workflow

## Overview

This document covers the full lifecycle of a **Purchase Request** submitted to the **Procurement Department**. It lists every API endpoint involved, the request/response shapes, who calls what, and the notification triggers at each step.

---

## Department Setup (One-Time)

Before the workflow can run, the Procurement department must be created with `requiresGoodsReceipt: true`. This flag activates the goods-receipt confirmation loop exclusively for this department. All other departments are unaffected.

### Create Procurement Department

```
POST /api/Department/{branchId}
```

**Request body**
```json
{
  "nameEn": "Procurement",
  "nameAr": "المشتريات",
  "hasSpecialFields": true,
  "requiresGoodsReceipt": true,
  "specialFields": [
    { "specialFieldId": "<fieldId>", "isRequired": true },
    { "specialFieldId": "<fieldId>", "isRequired": false }
  ]
}
```

> `requiresGoodsReceipt: true` enables the goods-receipt confirmation loop for every request sent to this department. Leave it `false` (or omit it) for all other departments.

### Update an existing department

```
PUT /api/Department
```

Same body shape as create (includes `id`). Toggle `requiresGoodsReceipt` at any time.

---

## Full Workflow

```
┌─────────────────────────────────────────────────────────────────────────┐
│  STEP 1  │  Site Engineer creates a Purchase Request                    │
│  STEP 2  │  Procurement Manager assigns to an Office Engineer           │
│  STEP 3  │  Office Engineer processes the request                       │
│  STEP 4  │  Office Engineer marks as "Confirmed/Completed"              │
│             → Site Engineer receives notification                        │
│  STEP 5  │  Site Engineer records goods receipt                         │
│             A) Full / Confirmed  → Request CLOSED                       │
│             B) Partial / Not confirmed → Back to Office Engineer (loop) │
│  STEP 6  │  (Loop) Office Engineer reviews → re-marks as Confirmed      │
│             → Repeat STEP 5 until site engineer confirms                │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Step-by-Step API Reference

---

### STEP 1 — Site Engineer: Create Purchase Request

#### 1a. Get procurement department special fields (render the form)

```
GET /api/Department/{procurementDepartmentId}/special-fields
```

**Response** – array of fields the site engineer must fill in:
```json
[
  { "id": "...", "label": "Item Description", "type": "Text", "isRequired": true },
  { "id": "...", "label": "Quantity",         "type": "Number", "isRequired": true },
  { "id": "...", "label": "Unit",             "type": "Text",   "isRequired": false }
]
```

#### 1b. Submit the request

```
POST /api/EngineerRequest
Content-Type: multipart/form-data
```

**Request body**
```json
{
  "projectId":    "<guid>",
  "departmentId": "<procurementDepartmentId>",
  "priorityId":   "<guid>",
  "requestTitle": "Purchase Request – Site Materials",
  "descreption":  "Required for Block A foundation",
  "specialFieldValues": [
    { "departmentSpecialFieldId": "<fieldId>", "value": "Cement Bags" },
    { "departmentSpecialFieldId": "<fieldId>", "value": "200" },
    { "departmentSpecialFieldId": "<fieldId>", "value": "Bags" }
  ],
  "attachments": [ /* optional files */ ]
}
```

**Who is notified:** The Procurement department manager receives a push notification about the new request.

---

### STEP 2 — Procurement Manager: Assign to Office Engineer

#### 2a. List all requests in the department

```
GET /api/EngineerRequest/department/{procurementDepartmentId}?pageIndex=1&pageSize=20
```

#### 2b. Assign request to an office engineer

```
POST /api/EngineerRequest/{requestId}/action
```

**Request body**
```json
{
  "assignToId": "<officeEngineerId>",
  "statusId":   "<inProgressStatusId>"
}
```

**Who is notified:**
- The assigned office engineer receives a push notification: *"You have been assigned a new request."*
- The site engineer (request creator) receives a push notification: *"Your request has been updated."*

> The manager can also use `POST /api/EngineerRequest/{requestId}/reassign` to move an already-assigned request to a different engineer.

---

### STEP 3 — Office Engineer: Process the Request

The office engineer works on the procurement (sources items, places orders, etc.) and updates the request status as needed.

```
POST /api/EngineerRequest/{requestId}/action
```

**Request body**
```json
{
  "statusId": "<processingStatusId>",
  "timeDuration": 5,
  "startDate": "2026-05-18T00:00:00",
  "endDate":   "2026-05-23T00:00:00",
  "engineerRequestNotes": [
    {
      "note": "Items ordered from supplier X",
      "attachments": [ /* optional files */ ]
    }
  ]
}
```

**Who is notified:** The site engineer receives a push notification about the status update.

---

### STEP 4 — Office Engineer: Mark as "Confirmed / Completed"

When items are ready for delivery, the office engineer sets the status to any **Confirmed / Completed** keyword status (e.g. `"Confirmed"`, `"Completed"`, `"Done"`).

```
POST /api/EngineerRequest/{requestId}/action
```

**Request body**
```json
{
  "statusId": "<confirmedStatusId>"
}
```

**What happens automatically (only for `requiresGoodsReceipt` departments):**
- `NeedsReceiptConfirmation` is set to `true` on the request
- The status is updated to "Confirmed" (visible to the office engineer)
- The site engineer receives a push notification:
  > **"Receipt Confirmation Required"**
  > *"The office engineer has marked this request as complete. Please confirm goods receipt to close the request."*

**Response shape** (`GET /api/EngineerRequest/{id}` after this action):
```json
{
  "id": "...",
  "status": { "nameEn": "Confirmed" },
  "needsReceiptConfirmation": true,
  "receipts": [],
  ...
}
```

> The `needsReceiptConfirmation: true` flag tells the frontend to show the **"Confirm Receipt"** button to the site engineer.

---

### STEP 5 — Site Engineer: Record Goods Receipt

#### 5a. View the request (check what was ordered + receipt history)

```
GET /api/EngineerRequest/{requestId}
```

Key fields in response:
```json
{
  "needsReceiptConfirmation": true,
  "specialFieldValues": [
    { "label": "Item Description", "value": "Cement Bags" },
    { "label": "Quantity",         "value": "200" }
  ],
  "receipts": [
    {
      "id": "...",
      "receiptDate": "2026-05-18T10:00:00",
      "isPartialReceipt": true,
      "isConfirmed": false,
      "notes": "Only 100 bags delivered",
      "receivedBy": { "name": "Ahmed Ali" }
    }
  ]
}
```

#### 5b. Submit goods receipt

```
POST /api/EngineerRequest/{requestId}/receipts
```

**Request body**
```json
{
  "receiptDate":     "2026-05-18T14:00:00",
  "isPartialReceipt": false,
  "isConfirmed":      true,
  "notes":           "All 200 bags received in good condition"
}
```

---

#### Receipt Behavior Matrix

| `isPartialReceipt` | `isConfirmed` | Result |
|---|---|---|
| `false` | `true` | ✅ Request **CLOSED**. `NeedsReceiptConfirmation = false`. Office engineer notified: *"Goods receipt confirmed."* |
| `true` | `true` | ✅ Request **CLOSED** (partial but engineer accepts it). Office engineer notified. |
| `true` | `false` | 🔄 **Loop back to STEP 3**. Status → `In Progress`. `NeedsReceiptConfirmation` stays `true`. Office engineer notified: *"Partial goods receipt recorded. Please review."* |

---

### STEP 6 — Loop: Office Engineer Re-checks → Back to Site Engineer

If `isPartialReceipt: true, isConfirmed: false`, the cycle repeats:

1. Office engineer receives notification: *"Partial goods receipt recorded."*
2. Office engineer resolves the issue → calls `POST /action` with confirmed status again (STEP 4)
3. Site engineer receives notification again → confirms or rejects again (STEP 5)
4. Loop continues until `isConfirmed: true`

---

### Audit Trail

#### Get all receipts for a request

```
GET /api/EngineerRequest/{requestId}/receipts
```

**Response**
```json
[
  {
    "id": "...",
    "receiptDate": "2026-05-18T10:00:00",
    "isPartialReceipt": true,
    "isConfirmed": false,
    "notes": "Only 100 bags delivered",
    "receivedById": "...",
    "receivedBy": { "id": "...", "name": "Ahmed Ali" }
  },
  {
    "id": "...",
    "receiptDate": "2026-05-20T09:00:00",
    "isPartialReceipt": false,
    "isConfirmed": true,
    "notes": "Remaining 100 bags received",
    "receivedById": "...",
    "receivedBy": { "id": "...", "name": "Ahmed Ali" }
  }
]
```

#### Get full activity log

```
GET /api/EngineerRequest/{requestId}/activities
```

Returns all status changes, assignments, and action types including `GoodsReceiptRecorded`, `PartialReceiptPendingReview`, `ClosedOnReceipt`.

---

### Site Engineer: Filter Requests Needing Receipt Confirmation

```
GET /api/EngineerRequest/appliedOrCreatedRequest?statusId={confirmedStatusId}
```

Returns only the site engineer's requests currently in the "Confirmed" status that have `needsReceiptConfirmation: true`.

---

## API Summary Table

| # | Who | Method | Endpoint | Purpose |
|---|---|---|---|---|
| 0 | Admin | `POST` | `/api/Department/{branchId}` | Create Procurement dept with `requiresGoodsReceipt: true` |
| 1a | Site Engineer | `GET` | `/api/Department/{deptId}/special-fields` | Load procurement form fields |
| 1b | Site Engineer | `POST` | `/api/EngineerRequest` | Submit purchase request |
| 2a | Dept Manager | `GET` | `/api/EngineerRequest/department/{deptId}` | List all dept requests |
| 2b | Dept Manager | `POST` | `/api/EngineerRequest/{id}/action` | Assign to office engineer |
| 2c | Dept Manager | `POST` | `/api/EngineerRequest/{id}/reassign` | Re-assign to different engineer |
| 3 | Office Engineer | `POST` | `/api/EngineerRequest/{id}/action` | Update status / add notes during processing |
| 4 | Office Engineer | `POST` | `/api/EngineerRequest/{id}/action` | Mark as Confirmed → triggers site engineer notification |
| 5a | Site Engineer | `GET` | `/api/EngineerRequest/{id}` | View request details + receipt history |
| 5b | Site Engineer | `POST` | `/api/EngineerRequest/{id}/receipts` | Submit goods receipt (confirm or partial) |
| — | Any | `GET` | `/api/EngineerRequest/{id}/receipts` | Get all receipts for a request |
| — | Any | `GET` | `/api/EngineerRequest/{id}/activities` | Full audit trail |
| — | Site Engineer | `GET` | `/api/EngineerRequest/appliedOrCreatedRequest?statusId=...` | Filter own requests by status |

---

## New Fields Added

### `Department`
| Field | Type | Default | Description |
|---|---|---|---|
| `requiresGoodsReceipt` | `bool` | `false` | Enables goods-receipt confirmation loop for this department |

### `EngineerRequest`
| Field | Type | Default | Description |
|---|---|---|---|
| `needsReceiptConfirmation` | `bool` | `false` | `true` when office engineer confirmed but site engineer hasn't confirmed receipt yet |

### `EngineerRequestParticipationFilterDto`
| Field | Type | Description |
|---|---|---|
| `statusId` | `Guid?` | Optional – filter `appliedOrCreatedRequest` by a specific status |

---

## Push Notifications Reference

| Trigger | Recipient | Title (EN) |
|---|---|---|
| New request created | Dept Manager | *New Request* |
| Manager assigns to office engineer | Office Engineer | *You have been assigned* |
| Office engineer takes action | Site Engineer | *Request Updated* |
| Office engineer marks as Confirmed | **Site Engineer** | **Receipt Confirmation Required** |
| Site engineer submits partial receipt (not confirmed) | **Office Engineer** | **Partial Receipt Submitted** |
| Site engineer confirms receipt (request closed) | **Office Engineer** | **Goods Receipt Confirmed** |
