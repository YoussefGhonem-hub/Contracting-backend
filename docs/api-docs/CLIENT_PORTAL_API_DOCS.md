# Client Portal — Mobile API Documentation

Base URL: `/api/mobile/client`  
Authentication: **Bearer token** (JWT) required on all endpoints.  
Authorization: All endpoints require the **`Client`** role.

---

## ProjectStatus Enum

Used in project-related responses and the admin project create/update endpoints.

| Value | Name | Description |
|-------|------|-------------|
| `0` | `Active` | Project is actively being constructed/worked on |
| `1` | `Planning` | Pre-construction planning phase |
| `2` | `OnHold` | Project work has been temporarily paused |
| `3` | `Delayed` | Project is running behind the agreed schedule |
| `4` | `Completed` | All work has been successfully delivered |
| `5` | `Cancelled` | Project was terminated before completion |

> Stored as a string in the database (e.g., `"Active"`, `"OnHold"`).

---

## 1. Get My Projects (Select Project Screen)
![alt text](image.png)
Returns all projects assigned to the currently logged-in client.

```
GET /api/mobile/client/projects
Authorization: Bearer <token>
```

### Sample Response `200 OK`

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nameEn": "Katameya Dunes Villa 83",
    "nameAr": "فيلا 83 كتامية دونز",
    "location": "5th Settlement, Cairo, Egypt",
    "imageUrl": "https://storage.example.com/projects/villa83.jpg",
    "area": 1000.00,
    "startDate": "2026-01-24T00:00:00+02:00",
    "projectStatus": "Active",
    "code": "KDV-083"
  },
  {
    "id": "4ca96a75-6828-5673-c4gd-3d074g77bgb7",
    "nameEn": "New Cairo Duplex",
    "nameAr": "دوبلكس القاهرة الجديدة",
    "location": "New Cairo, Cairo, Egypt",
    "imageUrl": "https://storage.example.com/projects/duplex.jpg",
    "area": 250.50,
    "startDate": "2025-09-01T00:00:00+02:00",
    "projectStatus": "OnHold",
    "code": "NCD-001"
  }
]
```

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |

---

## 2. Get Site Reports for a Project (Site Reports List Screen)
![alt text](image-1.png)
Returns all monthly site reports for a specific project, ordered newest first.

```
GET /api/mobile/client/projects/{projectId}/site-reports
Authorization: Bearer <token>
```

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `projectId` | `guid` | ID of the project |

### Sample Request

```
GET /api/mobile/client/projects/3fa85f64-5717-4562-b3fc-2c963f66afa6/site-reports
Authorization: Bearer eyJhbGciOiJSUzI1NiIsIn...
```

### Sample Response `200 OK`

```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "title": "Report 12",
    "month": 2,
    "year": 2026,
    "createdDate": "2026-02-05T10:30:00+02:00"
  },
  {
    "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "title": "Report 11",
    "month": 3,
    "year": 2026,
    "createdDate": "2026-03-05T09:00:00+02:00"
  },
  {
    "id": "c3d4e5f6-a7b8-9012-cdef-012345678902",
    "title": "Report 10",
    "month": 6,
    "year": 2026,
    "createdDate": "2026-06-05T08:45:00+02:00"
  }
]
```

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |
| `404 Not Found` | Project not found or this client has no access to the project |

---

## 3. Get Site Report Details (Report Detail Screen)
![alt text](image-2.png)
Returns the full details of a single site report including attachments.

```
GET /api/mobile/client/site-reports/{reportId}
Authorization: Bearer <token>
```

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `reportId` | `guid` | ID of the report |

### Sample Request

```
GET /api/mobile/client/site-reports/a1b2c3d4-e5f6-7890-abcd-ef1234567890
Authorization: Bearer eyJhbGciOiJSUzI1NiIsIn...
```

### Sample Response `200 OK`

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Report 12",
  "workProgress": "Foundation work completed. Columns on ground floor are 80% done. Brick work not yet started.",
  "month": 2,
  "year": 2026,
  "createdDate": "2026-02-05T10:30:00+02:00",
  "attachments": [
    {
      "id": "d4e5f6a7-b8c9-0123-def0-123456789012",
      "fileName": "Feb-2026-Site-Photos.zip",
      "extension": ".zip",
      "fileSize": 15728640,
      "url": "https://storage.example.com/reports/feb-2026-site-photos.zip"
    },
    {
      "id": "e5f6a7b8-c9d0-1234-ef01-234567890123",
      "fileName": "Progress-Report-Feb-2026.pdf",
      "extension": ".pdf",
      "fileSize": 204800,
      "url": "https://storage.example.com/reports/progress-feb-2026.pdf"
    }
  ]
}
```

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |
| `404 Not Found` | Report not found or this client has no access to it |

---

## Summary Table

| # | Method | Route | Description | Screen |
|---|--------|-------|-------------|--------|
| 1 | `GET` | `/api/mobile/client/projects` | Get all projects for the logged-in client | Select Project |
| 2 | `GET` | `/api/mobile/client/projects/{projectId}/site-reports` | List all site reports for a project | Site Reports List |
| 3 | `GET` | `/api/mobile/client/site-reports/{reportId}` | Full details of a single site report | Report Detail |
| 4 | `GET` | `/api/mobile/client/projects/{projectId}/invoices` | List invoices with payment summary | Invoices & Payments |
| 5 | `GET` | `/api/mobile/client/projects/{projectId}/invoices/financial-summary` | Contract financial summary | Contract Financial Summary |
| 6 | `GET` | `/api/mobile/client/projects/{projectId}/tender-documents` | List tender package documents | Tender Package |
| 7 | `GET` | `/api/mobile/client/projects/{projectId}/variation-orders` | List VOs with aggregate summary | Variation Orders List |
| 8 | `GET` | `/api/mobile/client/variation-orders/{voId}` | Full detail of a single VO | Variation Order Detail |
| 9 | `POST` | `/api/mobile/client/variation-orders/{voId}/approve` | Client approves a pending VO | Variation Order Detail |
| 10 | `POST` | `/api/mobile/client/variation-orders/{voId}/reject` | Client rejects a pending VO | Variation Order Detail |
| 11 | `GET` | `/api/mobile/client/projects/{projectId}/schedule` | Project timeline & milestones | Planning & Schedule |
| 12 | `GET` | `/api/mobile/client/projects/{projectId}/drawings` | Drawings & renders (2D / 3D) | Drawings & Renders |
| 13 | `POST` | `/api/auth/change-password` | Change authenticated user's password | Profile / Settings |

---

## 4. Get Invoices & Payment Summary (Invoices & Payments Screen)
![alt text](image-3.png)
Returns the list of invoices for a project together with an aggregate payment summary (total paid, remaining, settled %).

```
GET /api/mobile/client/projects/{projectId}/invoices
Authorization: Bearer <token>
```

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `projectId` | `guid` | ID of the project |

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `status` | `string` | No | Filter by invoice status: `Paid`, `Pending`, `PartiallyPaid`. Omit for **All**. |

### Sample Request — All invoices

```
GET /api/mobile/client/projects/3fa85f64-5717-4562-b3fc-2c963f66afa6/invoices
Authorization: Bearer eyJhbGciOiJSUzI1NiIsIn...
```

### Sample Request — Paid only

```
GET /api/mobile/client/projects/3fa85f64-5717-4562-b3fc-2c963f66afa6/invoices?status=Paid
Authorization: Bearer eyJhbGciOiJSUzI1NiIsIn...
```

### Sample Response `200 OK`

```json
{
  "totalValue": 2750000.00,
  "totalPaid": 1850000.00,
  "remainingAmount": 900000.00,
  "settledPercent": 68,
  "invoices": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "invoiceNumber": 1,
      "title": "Mobilization Fee",
      "totalValue": 250000.00,
      "paidAmount": 0.00,
      "status": "Pending",
      "issueDate": "2024-03-01T00:00:00+02:00",
      "dueDate": "2024-03-15T00:00:00+02:00"
    },
    {
      "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
      "invoiceNumber": 2,
      "title": "Mobilization Fee",
      "totalValue": 350000.00,
      "paidAmount": 350000.00,
      "status": "Paid",
      "issueDate": "2024-03-01T00:00:00+02:00",
      "dueDate": "2024-03-15T00:00:00+02:00"
    },
    {
      "id": "c3d4e5f6-a7b8-9012-cdef-012345678902",
      "invoiceNumber": 3,
      "title": "Mobilization Fee",
      "totalValue": 200000.00,
      "paidAmount": 200000.00,
      "status": "Paid",
      "issueDate": "2024-03-01T00:00:00+02:00",
      "dueDate": "2024-03-15T00:00:00+02:00"
    }
  ]
}
```

> **Note:** `totalValue`, `totalPaid`, `remainingAmount`, and `settledPercent` always reflect ALL invoices for the project, regardless of the `status` filter. Only the `invoices` array is filtered.

### PaymentStatus Values

| Value | Description |
|-------|-------------|
| `Pending` | Invoice issued, payment not yet received |
| `Paid` | Invoice fully paid |
| `PartiallyPaid` | A partial payment has been recorded |

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |
| `404 Not Found` | Project not found or this client has no access to it |

---

## 5. Get Contract Financial Summary (Contract Financial Summary Screen)
![alt text](image-4.png)
Returns the full financial breakdown for a project: initial contract value, approved variation orders, total paid, and remaining amount.

```
GET /api/mobile/client/projects/{projectId}/invoices/financial-summary
Authorization: Bearer <token>
```

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `projectId` | `guid` | ID of the project |

### Sample Request

```
GET /api/mobile/client/projects/3fa85f64-5717-4562-b3fc-2c963f66afa6/invoices/financial-summary
Authorization: Bearer eyJhbGciOiJSUzI1NiIsIn...
```

### Sample Response `200 OK`

```json
{
  "initialContractValue": 2650000.00,
  "approvedVariations": 100000.00,
  "totalContractValue": 2750000.00,
  "totalPaid": 1850000.00,
  "remainingAmount": 900000.00
}
```

### Field Descriptions

| Field | Source | Description |
|-------|--------|-------------|
| `initialContractValue` | `Project.ContractValue` | The original contract value set on the project |
| `approvedVariations` | Sum of `VariationOrder.Cost` where `Status = Approved` | Total cost of all approved variation orders |
| `totalContractValue` | `initialContractValue + approvedVariations` | Effective contract value |
| `totalPaid` | Sum of `ProjectInvoice.PaidAmount` | Total amount collected so far |
| `remainingAmount` | `totalContractValue - totalPaid` | Amount still outstanding |

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |
| `404 Not Found` | Project not found or this client has no access to it |

---

## 6. Get Tender Package Documents (Tender Package Screen)
![alt text](image-5.png)
Returns all tender documents uploaded for the project, ordered by upload date. Each item includes the file metadata and a direct download URL.

```
GET /api/mobile/client/projects/{projectId}/tender-documents
Authorization: Bearer <token>
```

### Path Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `projectId` | `guid` | ID of the project |

### Sample Request

```
GET /api/mobile/client/projects/3fa85f64-5717-4562-b3fc-2c963f66afa6/tender-documents
Authorization: Bearer eyJhbGciOiJSUzI1NiIsIn...
```

### Sample Response `200 OK`

```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "title": "Tender Invitation Letter",
    "fileName": "Tender-Invitation-Letter.xlsx",
    "extension": ".xlsx",
    "fileSize": 1258291,
    "url": "https://storage.example.com/tender/tender-invitation-letter.xlsx"
  },
  {
    "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "title": "Bill of Quantities (BOQ)",
    "fileName": "BOQ.pdf",
    "extension": ".pdf",
    "fileSize": 4718592,
    "url": "https://storage.example.com/tender/boq.pdf"
  },
  {
    "id": "c3d4e5f6-a7b8-9012-cdef-012345678902",
    "title": "Technical Specifications",
    "fileName": "Technical-Specifications.pdf",
    "extension": ".pdf",
    "fileSize": 4718592,
    "url": "https://storage.example.com/tender/technical-specs.pdf"
  }
]
```

> `fileSize` is in bytes. Display logic: `1.2 MB = 1258291 bytes`.

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |
| `404 Not Found` | Project not found or this client has no access to it |

---

## 7. Get Variation Orders (Variation Orders List Screen)
![alt text](image-6.png)
Returns the variation orders for a project with aggregate totals (total approved cost, total pending cost).

```
GET /api/mobile/client/projects/{projectId}/variation-orders
Authorization: Bearer <token>
```

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `status` | `string` | No | Filter: `Approved`, `Pending`, `Rejected`. Omit for **All**. |

### Sample Response `200 OK`

```json
{
  "totalApproved": 730000.00,
  "totalPending": 620000.00,
  "variationOrders": [
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "voNumber": 3,
      "title": "Kitchen Upgrade - Premium Appliances",
      "cost": 45000.00,
      "status": "Pending",
      "issueDate": "2024-03-01T00:00:00+02:00",
      "dueDate": "2024-03-15T00:00:00+02:00"
    },
    {
      "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
      "voNumber": 2,
      "title": "Master Bathroom Marble Change",
      "cost": 28500.00,
      "status": "Approved",
      "issueDate": "2024-03-01T00:00:00+02:00",
      "dueDate": "2024-03-15T00:00:00+02:00"
    }
  ]
}
```

> `totalApproved` and `totalPending` always reflect ALL VOs for the project, regardless of the `status` filter.

### VOStatus Values

| Value | Description |
|-------|-------------|
| `Pending` | Awaiting client decision |
| `Approved` | Client approved the variation |
| `Rejected` | Client rejected the variation |

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |
| `404 Not Found` | Project not found or this client has no access to it |

---

## 8. Get Variation Order Detail (Variation Order Detail Screen)
![alt text](image-7.png)
Returns the full detail of a single VO including attachments.

```
GET /api/mobile/client/variation-orders/{voId}
Authorization: Bearer <token>
```

### Sample Response `200 OK`

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "voNumber": 3,
  "title": "Kitchen Upgrade - Premium Appliances",
  "description": "Upgrade kitchen appliances to premium Miele brand as per client request. Includes built-in coffee machine, steam oven, and wine cooler.",
  "cost": 45000.00,
  "status": "Pending",
  "issueDate": "2024-03-01T00:00:00+02:00",
  "dueDate": "2024-03-15T00:00:00+02:00",
  "clientActionDate": null,
  "clientRejectionReason": null,
  "attachments": [
    {
      "id": "d4e5f6a7-b8c9-0123-def0-123456789012",
      "fileName": "Kitchen Layout.pdf",
      "extension": ".pdf",
      "fileSize": 4718592,
      "url": "https://storage.example.com/vo/kitchen-layout.pdf"
    },
    {
      "id": "e5f6a7b8-c9d0-1234-ef01-234567890123",
      "fileName": "Cost Breakdown.xlsx",
      "extension": ".xlsx",
      "fileSize": 4718592,
      "url": "https://storage.example.com/vo/cost-breakdown.xlsx"
    }
  ]
}
```

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |
| `404 Not Found` | VO not found or this client has no access to it |

---

## 9. Approve Variation Order

Client approves a **Pending** variation order. Returns `422` if the VO is not in `Pending` status.

```
POST /api/mobile/client/variation-orders/{voId}/approve
Authorization: Bearer <token>
```

### Sample Response `200 OK`

Returns the updated VO detail (same shape as endpoint 8) with `"status": "Approved"` and `clientActionDate` set.

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |
| `422 Unprocessable Entity` | VO not found, not accessible, or not in `Pending` status |

---

## 10. Reject Variation Order

Client rejects a **Pending** variation order with an optional reason. Returns `422` if the VO is not in `Pending` status.

```
POST /api/mobile/client/variation-orders/{voId}/reject
Authorization: Bearer <token>
Content-Type: application/json
```

### Request Body

```json
{
  "rejectionReason": "Budget constraints — cannot proceed at this cost."
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `rejectionReason` | `string` | No | Reason for rejection. Displayed to the engineering team. |

### Sample Response `200 OK`

Returns the updated VO detail with `"status": "Rejected"` and `clientRejectionReason` set.

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |
| `422 Unprocessable Entity` | VO not found, not accessible, or not in `Pending` status |

---

## 11. Get Project Schedule (Planning & Schedule Screen)

Returns the project timeline along with its milestones (tasks) and their completion status.

```
GET /api/mobile/client/projects/{projectId}/schedule
Authorization: Bearer <token>
Role: Client
```

### Response `200 OK`

```json
{
  "projectId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "projectName": "Villa Construction — Phase 1",
  "startDate": "2024-01-15T00:00:00Z",
  "endDate": "2024-12-31T00:00:00Z",
  "overallProgress": 62,
  "tasks": [
    {
      "id": "a1b2c3d4-...",
      "name": "Foundation Works",
      "startDate": "2024-01-15T00:00:00Z",
      "endDate": "2024-03-01T00:00:00Z",
      "completionPercentage": 100,
      "status": "Completed"
    },
    {
      "id": "e5f6...",
      "name": "Framing",
      "startDate": "2024-03-02T00:00:00Z",
      "endDate": "2024-06-30T00:00:00Z",
      "completionPercentage": 75,
      "status": "InProgress"
    }
  ]
}
```

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |
| `404 Not Found` | Project not found or not assigned to this client |

---

## 12. Get Drawings & Renders (Drawings Screen)
![alt text](image-8.png)
Returns the list of uploaded drawings (2D plans and 3D renders) for a project.  
Optionally filter by drawing type using the `type` query parameter.

```
GET /api/mobile/client/projects/{projectId}/drawings?type=TwoD
Authorization: Bearer <token>
Role: Client
```

### Query Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `type` | `string` | No | Filter by type. Accepted values: `TwoD`, `ThreeD`. Omit for **all** drawings. |

### Response `200 OK`

```json
[
  {
    "id": "d1e2f3...",
    "title": "Ground Floor Plan",
    "type": "TwoD",
    "fileName": "ground-floor.pdf",
    "extension": ".pdf",
    "fileSize": 2048000,
    "url": "/files/drawings/ground-floor.pdf",
    "uploadedAt": "2024-05-20T10:30:00+03:00"
  },
  {
    "id": "a7b8c9...",
    "title": "3D Exterior Render",
    "type": "ThreeD",
    "fileName": "exterior-render.png",
    "extension": ".png",
    "fileSize": 5120000,
    "url": "/files/drawings/exterior-render.png",
    "uploadedAt": "2024-06-01T14:00:00+03:00"
  }
]
```

### DrawingType Enum

| Value | Description |
|-------|-------------|
| `TwoD` | 2D floor plans, sections, elevations |
| `ThreeD` | 3D renders, perspectives, visualisations |

### Error Responses

| Status | Description |
|--------|-------------|
| `401 Unauthorized` | Missing or invalid token |
| `403 Forbidden` | Authenticated user does not have the `Client` role |
| `404 Not Found` | Project not found or not assigned to this client |

---

## 13. Change Password (Profile / Settings Screen)
![alt text](image-9.png)
Allows any authenticated user (including clients) to change their own password.  
Requires the current password to be provided. If the account has no password set (external login), the current password field may be left empty.

```
POST /api/auth/change-password
Authorization: Bearer <token>
Content-Type: application/json
```

### Request Body

```json
{
  "currentPassword": "OldPass@123",
  "newPassword": "NewPass@456",
  "confirmPassword": "NewPass@456"
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `currentPassword` | `string` | Conditional | Required if the account already has a password |
| `newPassword` | `string` | Yes | Must meet password complexity requirements |
| `confirmPassword` | `string` | Yes | Must match `newPassword` |

### Response `200 OK`

```json
{
  "message": "Password changed successfully."
}
```

### Response `400 Bad Request`

```json
{
  "errors": ["Current password is incorrect."]
}
```

### Password Requirements

- Minimum 8 characters  
- At least one uppercase letter  
- At least one lowercase letter  
- At least one number  
- At least one special character  

### Error Responses

| Status | Description |
|--------|-------------|
| `400 Bad Request` | Validation error or incorrect current password |
| `401 Unauthorized` | Missing or invalid token |

---

## Default Client Credentials (Seeded)

| Field | Value |
|-------|-------|
| Email | `client@shop.com` |
| Password | `Client@123` |
| Role | `Client` |
