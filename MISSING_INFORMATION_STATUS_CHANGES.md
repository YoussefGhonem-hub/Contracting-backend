# Missing Information Status — Implementation Changes

## Overview

Replaced the old **"On Hold"** status with a new **"Missing Information"** status.  
This enables team leads to flag requests that need more data from the Site Engineer, who must then add a note and resubmit — automatically resetting the status back to "New".

---

## Status Changes (Database)

| | Before | After |
|---|---|---|
| Status name (EN) | On Hold | Missing Information |
| Status name (AR) | — | معلومات ناقصة |
| Code | `ON_HOLD` | `missing_information` |
| Show in dropdown | ✅ | ✅ |

> The existing "On Hold" database row was **updated in-place** (same `Id`), so all historical requests previously set to "On Hold" now display as "Missing Information".

---

## Business Rules

### 1. Team Lead sets status → "Missing Information"
- Team lead uses the normal **Take Action** endpoint and passes the `missing_information` status ID.
- The backend **automatically sends a push notification** to the request creator (Site Engineer):
  - **EN:** "Action Required: Missing Information — Your request requires additional information. Please add a note with an attachment to proceed."
  - **AR:** "إجراء مطلوب: معلومات ناقصة — طلبك يحتاج إلى معلومات إضافية. يرجى إضافة ملاحظة مع مرفق للمتابعة."

### 2. Site Engineer updates a "Missing Information" request
- The Site Engineer **must add at least one note** (attachment is optional).
- If no note is provided → `400 Validation` error is returned.
- After saving, the backend **automatically resets the status back to "New"** (no extra call needed from mobile).
- The returned response already contains the updated status = "New".

### 3. Rejected status is final
- A **Rejected** request cannot be edited or actioned.
- Any attempt returns an error response.

---

## API Endpoints

### `POST /api/EngineerRequest/{requestId}/action`
**Who calls it:** Team Lead  
**Used for:** Setting status to "Missing Information" (among other status changes)

**Request** (`multipart/form-data`):
```
statusId       : guid   — ID of the "Missing Information" status
assignToId     : guid?  — optional
timeDuration   : int?
startDate      : datetime?
endDate        : datetime?
EngineerRequestNotes[0].Note : string?
EngineerRequestNotes[0].Attachments : file[]?
```

**Responses:**

| Case | HTTP | Body |
|---|---|---|
| Success | `200 OK` | `{ "message": "Request approved successfully" }` |
| Request is Rejected | `400 Bad Request` | `{ error: "Rejected requests are final and cannot be actioned or reopened." }` |
| Unauthorized | `400 Bad Request` | error message |

**Side effect:** If new status = `missing_information` → push notification sent to request creator.

---

### `PUT /api/EngineerRequest`
**Who calls it:** Site Engineer  
**Used for:** Submitting missing information (note required)

**Request** (`multipart/form-data`):
```
Id             : guid   — required
projectId      : guid?
departmentId   : guid?
priorityId     : guid?
requestTitle   : string?
descreption    : string  — required
EngineerRequestNotes[0].Note        : string  — REQUIRED when status is Missing Information
EngineerRequestNotes[0].Attachments : file[]? — optional
specialFieldValues : []?
```

**Responses:**

| Case | HTTP | Body |
|---|---|---|
| Success | `200 OK` | Full `GetAllEngineerRequestDto` with status reset to "New" |
| No note provided (Missing Info status) | `400 Validation` | `"A note is required when updating a request in Missing Information status."` |
| Request is Rejected | `403 Forbidden` | `"Rejected requests cannot be edited. Please create a new request instead."` |
| Request already actioned (not Missing Info) | `403 Forbidden` | `"Request has already been actioned."` |

**Side effect:** Status is automatically changed to "New" after successful update.

---

## Response DTO — `GetAllEngineerRequestDto`

```json
{
  "id": "guid",
  "projectId": "guid",
  "project": { ... },
  "departmentId": "guid",
  "department": { ... },
  "priorityId": "guid",
  "priority": { ... },
  "engineerId": "guid",
  "engineer": { ... },
  "statusId": "guid",
  "status": {
    "id": "guid",
    "nameEn": "Missing Information",
    "nameAr": "معلومات ناقصة",
    "code": "missing_information"
  },
  "assignToId": "guid",
  "assignTo": { ... },
  "requestTitle": "string",
  "descreption": "string",
  "timeDuration": 0,
  "startDate": "datetime",
  "endDate": "datetime",
  "engineerRequestNotes": [ ... ],
  "engineerRequestActivites": [ ... ],
  "engineerRequestAttachments": [ ... ],
  "specialFieldValues": [ ... ]
}
```

---

## Mobile UI Guidance

### Status Display
| `status.code` | Display (EN) | Display (AR) | Color suggestion |
|---|---|---|---|
| `missing_information` | Missing Information | معلومات ناقصة | Orange / Warning |
| `rejected` | Rejected | مرفوض | Red |

> Remove "On Hold" from all status filter lists and label maps.

### Request Detail Screen
| Status | Show Edit Button | Show Action |
|---|---|---|
| `missing_information` | ✅ Yes — show update form | ❌ Engineer cannot take action |
| `rejected` | ❌ No — show message: *"This request has been rejected. Create a new one."* | ❌ No |
| others (new, in-progress, etc.) | Based on existing logic | Based on existing logic |

### Update Form (when status = `missing_information`)
- **Note field is required** — show validation message if empty before submitting.
- Attachment field is optional.
- After successful submit → status in response will be "New" — navigate accordingly.

---

## Files Changed

| File | Change |
|---|---|
| `Contracting.Domain` | No changes |
| `Contracting.Shared/Resources/SharedResourcesKeys.cs` | Added: `RequestRejectedCannotEdit`, `RequestRejectedCannotAction`, `NotificationMissingInfoTitle`, `NotificationMissingInfoBody`, `MissingInfoRequiresNoteAndAttachment` |
| `Contracting.Shared/Resources/SharedResources.En.resx` | Added EN translations for above keys |
| `Contracting.Shared/Resources/SharedResources.Ar.resx` | Added AR translations for above keys |
| `Contracting.Shared/Dtos/.../EngineerRequestAnalysisDto.cs` | `OnHoldCount` → `PendingInfoCount` in `TeamLeadAnalysisDto` |
| `Contracting.Infrustructure/Features/business/EngineerRequestService.cs` | Rejected block, Missing Info allow-edit, auto-reset to New, Missing Info notification, note validation |
| `Contracting.Infrustructure/Features/business/EngineerRequestAnalysisService.cs` | `OnHold` keyword set → `PendingInfo` keyword set |
| `Contracting.Infrustructure/Inteface/business/IEngineerRequestService.cs` | `UpdateEngineerRequestAsync` return type → `ErrorOr<GetAllEngineerRequestDto>` |
| `Contracting.Infrustructure/Contracting.Infrustructure.csproj` | Added `ErrorOr` v2.0.1 package |
| `Contracting.Application/.../UpdateEngineerRequestCommandHandler.cs` | Simplified to pass through `ErrorOr` result |
| `Contracting.Infrustructure/Migrations/20260515131301_AddMissingInfoNeedsUpdateStatuses.cs` | Updates "On Hold" row → "Missing Information" in-place |
