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

---

## Default Client Credentials (Seeded)

| Field | Value |
|-------|-------|
| Email | `client@shop.com` |
| Password | `Client@123` |
| Role | `Client` |
