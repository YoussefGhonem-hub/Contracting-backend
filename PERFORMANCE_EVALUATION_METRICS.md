# Performance Evaluation Metrics — Implementation

## Overview

Role-based KPI system for **Site Engineers** and **Office Engineers**.  
Metrics are available via the existing `EngineerRequestAnalysis` reporting endpoints plus one new confirm-delivery-date action.

---

## 👷 Site Engineers — KPIs

### 1. Urgent Requests Ratio
**What it measures:** Percentage of requests handled by the engineer that have an "Urgent" priority.

**Endpoint:** `GET api/EngineerRequestAnalysis/site-engineer`

**New fields in response:**
```json
{
  "urgentRequestsCount": 5,
  "urgentRequestsRatio": 25.00
}
```

**Logic:** Any priority whose name contains the word `"urgent"` (case-insensitive) is counted. Ratio = `urgentCount / totalRequests * 100`.

---

### 2. Daily Report Completion Rate
**What it measures:** How many working days in a given month the engineer submitted a site report vs. expected working days (Mon–Thu only; Fri & Sat treated as weekend).

**Endpoint:** `GET api/EngineerRequestAnalysis/daily-report-completion?month=5&year=2026`

**Response:**
```json
{
  "year": 2026,
  "month": 5,
  "expectedWorkingDays": 22,
  "submittedDays": 18,
  "completionRate": 81.82,
  "missingDays": ["2026-05-06", "2026-05-13"]
}
```

**Logic:** Counts distinct `ReportDate` values in `EngineerSiteReports` for the current engineer within the target month. Expected days = Mon–Thu calendar days up to today within the month.

---

### 3. Request Quality (Rework Ratio)
**What it measures:** How many of the engineer's requests moved back out of a completed status after being marked done — a proxy for quality.

**Endpoint:** `GET api/EngineerRequestAnalysis/site-engineer`

**New fields in response:**
```json
{
  "reworkCount": 2,
  "reworkRatio": 10.00
}
```

**Logic:** Scoped to the current Site Engineer's requests. Detects requests that had a completed-status activity followed by a non-completed activity (rework detected via `EngineerRequestActivites` history).

---

## 🏢 Office Engineers — KPIs

### 1. Response Time
**What it measures:** Average time (in hours) between the first two activity log entries for requests assigned to the Office Engineer. Represents how quickly they acted after receiving a request.

**Endpoint:** `GET api/EngineerRequestAnalysis/office-engineer`

**New field in response:**
```json
{
  "averageResponseTimeHours": 3.45
}
```

**Logic:** For each assigned request with ≥ 2 activity entries, compute `activity[1].CreatedDate − activity[0].CreatedDate` in hours, then average across all requests.

---

### 2. Delivery Date Control Rule (Immutability)

**What it means:** Once a delivery date (`endDate`) is confirmed on a request, it cannot be changed by any subsequent action.

#### Confirm endpoint
```
PATCH api/EngineerRequest/{requestId}/confirm-delivery-date
```
- Sets `IsDeliveryDateConfirmed = true` on the request.
- Returns `204 No Content` on success.
- Returns `409 Conflict` if already confirmed.
- Returns `400` if no `endDate` is set.

#### Backend guard
Any call to `TakeActionOnRequest` that attempts to pass a different `endDate` after the date is confirmed will be **rejected** with:

```json
{
  "status": 400,
  "detail": "The delivery date has already been confirmed and cannot be changed."
}
```

Arabic: `"تم تأكيد تاريخ التسليم مسبقاً ولا يمكن تغييره."`

#### Database
New column `IsDeliveryDateConfirmed BIT NOT NULL DEFAULT 0` on the `EngineerRequests` table.  
Migration: `20260515141717_AddIsDeliveryDateConfirmed`

---

## ⚙️ Files Changed

| File | Change |
|---|---|
| `Contracting.Domain/Entities/business/EngineerRequest.cs` | Added `IsDeliveryDateConfirmed` property |
| `Contracting.Infrustructure/Migrations/20260515141717_AddIsDeliveryDateConfirmed.cs` | New migration |
| `Contracting.Shared/Dtos/BusinessDtos/EngineerRequestAnalysisDtos/EngineerRequestAnalysisDto.cs` | Added KPI fields to `SiteEngineerAnalysisDto` and `OfficeEngineerAnalysisDto`; added `DailyReportCompletionDto` |
| `Contracting.Shared/Resources/SharedResourcesKeys.cs` | Added `DeliveryDateAlreadyConfirmed` key |
| `Contracting.Shared/Resources/SharedResources.En.resx` | Added EN translation |
| `Contracting.Shared/Resources/SharedResources.Ar.resx` | Added AR translation |
| `Contracting.Infrustructure/Features/business/EngineerRequestAnalysisService.cs` | Added urgent ratio, rework, response time, daily report completion logic |
| `Contracting.Infrustructure/Features/business/EngineerRequestService.cs` | Added delivery date guard in `TakeActionOnRequestAsync`; added `ConfirmDeliveryDateAsync` |
| `Contracting.Infrustructure/Inteface/business/IEngineerRequestAnalysisService.cs` | Added `GetDailyReportCompletionRateAsync` |
| `Contracting.Infrustructure/Inteface/business/IEngineerRequestService.cs` | Added `ConfirmDeliveryDateAsync` |
| `Contracting.Application/.../GetDailyReportCompletion/` | New query + handler |
| `Contracting.Application/.../ConfirmDeliveryDate/` | New command + handler |
| `Contracting.API/Controllers/EngineerRequestAnalysisController.cs` | Added `GET daily-report-completion` endpoint |
| `Contracting.API/Controllers/EngineerRequestController.cs` | Added `PATCH confirm-delivery-date` endpoint |

---

## 🚫 Business Rules Enforced

- Delivery date becomes **immutable** after `confirm-delivery-date` is called.
- KPIs are **role-scoped**: Site Engineer metrics use the current engineer's own requests; Office Engineer metrics use assigned requests.
- Metrics do **not overlap** between roles — each DTO is separate.
- Daily report completion uses **working days only** (Friday & Saturday excluded).
