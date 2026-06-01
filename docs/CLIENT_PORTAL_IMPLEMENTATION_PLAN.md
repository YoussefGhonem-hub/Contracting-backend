# Client Portal — Implementation Plan

---

## Overview

Two separate controller surfaces:
- **Mobile API** → `api/mobile/client/...`
- **Web API** → `api/web/client/...`

Both share the same Application/Domain/Infrastructure layers. Controllers differ only in response shaping and auth guards.

---

## Modules (from Client Management System.md)

| # | Module | Description |
|---|---|---|
| 1 | **Client Profile** | Client user entity linked to ApplicationUser + assigned Projects |
| 2 | **Chat** | One group per project; client + project team messages |
| 3 | **Monthly Reports** | Uploaded by assigned user; work progress + images |
| 4 | **Invoices & Financial** | Project value, payment status, VO-triggered updates |
| 5 | **Variation Orders (VO)** | Created by Technical Office; client Approves/Rejects |
| 6 | **Tender Documents** | One-time upload per project |
| 7 | **Schedules** | Regularly updated files per project |
| 8 | **Drawings & Renders** | 2D & 3D file uploads per project |

---

## Phase 1 — Domain Layer & Migrations ✅ START HERE

### 1.1 New Entities

All entities extend `BaseAuditableEntity` and live in `Contracting.Domain/Entities/client/`.

---

#### `Client.cs`
```
- Guid ApplicationUserId  → ApplicationUser
- string? CompanyName
- string? PhoneNumber
- string? Address
- ICollection<ClientProject> ClientProjects
```
Schema: `client` | Table: `Clients`

---

#### `ClientProject.cs` (junction)
```
- Guid ClientId  → Client
- Guid ProjectId → Project
```
Schema: `client` | Table: `ClientProjects`

---

#### `ChatGroup.cs`
```
- Guid ProjectId  → Project  (unique per project)
- string? Name
- ICollection<ChatMessage> Messages
- ICollection<ChatGroupMember> Members
```
Schema: `client` | Table: `ChatGroups`

---

#### `ChatGroupMember.cs`
```
- Guid ChatGroupId  → ChatGroup
- Guid ApplicationUserId → ApplicationUser
- string MemberType  ("Client" | "TeamMember")
```
Schema: `client` | Table: `ChatGroupMembers`

---

#### `ChatMessage.cs`
```
- Guid ChatGroupId   → ChatGroup
- Guid SenderId      → ApplicationUser
- string? Content
- bool IsRead
- ICollection<ChatMessageAttachment> Attachments
```
Schema: `client` | Table: `ChatMessages`

---

#### `ChatMessageAttachment.cs`
```
- Guid ChatMessageId → ChatMessage
- string? Key
- string? FileName
- string? Extension
- long? FileSize
- string? Url
```
Schema: `client` | Table: `ChatMessageAttachments`

---

#### `ClientMonthlyReport.cs`
```
- Guid ProjectId     → Project
- int Month
- int Year
- string? Title
- string? WorkProgress  (free text)
- Guid UploadedBy    → ApplicationUser
- ICollection<ClientMonthlyReportAttachment> Attachments
```
Schema: `client` | Table: `ClientMonthlyReports`

---

#### `ClientMonthlyReportAttachment.cs`
```
- Guid ClientMonthlyReportId → ClientMonthlyReport
- string? Key
- string? FileName
- string? Extension
- long? FileSize
- string? Url
```
Schema: `client` | Table: `ClientMonthlyReportAttachments`

---

#### `ProjectInvoice.cs`
```
- Guid ProjectId     → Project
- decimal TotalValue
- decimal PaidAmount
- PaymentStatus Status  (enum: Pending / Paid / PartiallyPaid)
- string? Notes
- Guid UpdatedBy     → ApplicationUser
- ICollection<InvoicePayment> Payments
```
Schema: `client` | Table: `ProjectInvoices`

---

#### `InvoicePayment.cs`
```
- Guid ProjectInvoiceId → ProjectInvoice
- decimal Amount
- DateTimeOffset PaymentDate
- string? Reference
- string? Notes
```
Schema: `client` | Table: `InvoicePayments`

---

#### `VariationOrder.cs`
```
- Guid ProjectId     → Project
- string? Title
- string? Description
- decimal Cost
- VOStatus Status    (enum: Pending / Approved / Rejected)
- Guid CreatedByEngineerId → Engineer
- DateTimeOffset? ClientActionDate
- string? ClientRejectionReason
- ICollection<VariationOrderAttachment> Attachments
```
Schema: `client` | Table: `VariationOrders`

---

#### `VariationOrderAttachment.cs`
```
- Guid VariationOrderId → VariationOrder
- string? Key
- string? FileName
- string? Extension
- long? FileSize
- string? Url
```
Schema: `client` | Table: `VariationOrderAttachments`

---

#### `TenderDocument.cs`
```
- Guid ProjectId  → Project  (one per project)
- string? Title
- string? Key
- string? FileName
- string? Extension
- long? FileSize
- string? Url
- Guid UploadedBy → ApplicationUser
```
Schema: `client` | Table: `TenderDocuments`

---

#### `ProjectSchedule.cs`
```
- Guid ProjectId  → Project
- string? Title
- string? Version
- string? Key
- string? FileName
- string? Extension
- long? FileSize
- string? Url
- Guid UploadedBy → ApplicationUser
```
Schema: `client` | Table: `ProjectSchedules`

---

#### `ProjectDrawing.cs`
```
- Guid ProjectId  → Project
- DrawingType Type  (enum: TwoD / ThreeD)
- string? Title
- string? Key
- string? FileName
- string? Extension
- long? FileSize
- string? Url
- Guid UploadedBy → ApplicationUser
```
Schema: `client` | Table: `ProjectDrawings`

---

### 1.2 Enums

Location: `Contracting.Domain/Common/Enums/`

```csharp
public enum PaymentStatus  { Pending, Paid, PartiallyPaid }
public enum VOStatus        { Pending, Approved, Rejected }
public enum DrawingType     { TwoD, ThreeD }
```

---

### 1.3 EF Configurations

Location: `Contracting.Infrustructure/Persistence/Configurations/Client/`

One `IEntityTypeConfiguration<T>` class per entity:

| Config File | Table | Schema |
|---|---|---|
| `ClientConfiguration.cs` | Clients | client |
| `ClientProjectConfiguration.cs` | ClientProjects | client |
| `ChatGroupConfiguration.cs` | ChatGroups | client |
| `ChatGroupMemberConfiguration.cs` | ChatGroupMembers | client |
| `ChatMessageConfiguration.cs` | ChatMessages | client |
| `ChatMessageAttachmentConfiguration.cs` | ChatMessageAttachments | client |
| `ClientMonthlyReportConfiguration.cs` | ClientMonthlyReports | client |
| `ClientMonthlyReportAttachmentConfiguration.cs` | ClientMonthlyReportAttachments | client |
| `ProjectInvoiceConfiguration.cs` | ProjectInvoices | client |
| `InvoicePaymentConfiguration.cs` | InvoicePayments | client |
| `VariationOrderConfiguration.cs` | VariationOrders | client |
| `VariationOrderAttachmentConfiguration.cs` | VariationOrderAttachments | client |
| `TenderDocumentConfiguration.cs` | TenderDocuments | client |
| `ProjectScheduleConfiguration.cs` | ProjectSchedules | client |
| `ProjectDrawingConfiguration.cs` | ProjectDrawings | client |

---

### 1.4 DbSet additions — `ApplicationDbContext.cs`

```csharp
// Client schema
public DbSet<Client> Clients => Set<Client>();
public DbSet<ClientProject> ClientProjects => Set<ClientProject>();
public DbSet<ChatGroup> ChatGroups => Set<ChatGroup>();
public DbSet<ChatGroupMember> ChatGroupMembers => Set<ChatGroupMember>();
public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
public DbSet<ChatMessageAttachment> ChatMessageAttachments => Set<ChatMessageAttachment>();
public DbSet<ClientMonthlyReport> ClientMonthlyReports => Set<ClientMonthlyReport>();
public DbSet<ClientMonthlyReportAttachment> ClientMonthlyReportAttachments => Set<ClientMonthlyReportAttachment>();
public DbSet<ProjectInvoice> ProjectInvoices => Set<ProjectInvoice>();
public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();
public DbSet<VariationOrder> VariationOrders => Set<VariationOrder>();
public DbSet<VariationOrderAttachment> VariationOrderAttachments => Set<VariationOrderAttachment>();
public DbSet<TenderDocument> TenderDocuments => Set<TenderDocument>();
public DbSet<ProjectSchedule> ProjectSchedules => Set<ProjectSchedule>();
public DbSet<ProjectDrawing> ProjectDrawings => Set<ProjectDrawing>();
```

---

### 1.5 Migration

```bash
cd src/Contracting.Infrustructure
dotnet ef migrations add AddClientPortalSchema --startup-project ../Contracting.API
dotnet ef database update --startup-project ../Contracting.API
```

---

## Phase 2 — Application Layer (CQRS)

Location: `Contracting.Application/Features/Client/`  
Location: `Contracting.Shared/Dtos/ClientDtos/`

### Per-module Commands & Queries

| Module | Commands | Queries |
|---|---|---|
| Client | CreateClient, UpdateClient, DeleteClient, AssignProjectToClient | GetClientById, GetAllClients, GetClientProjects |
| Chat | SendMessage, MarkMessageRead, AddMember | GetChatGroupByProject, GetMessages |
| Monthly Reports | CreateReport, DeleteReport | GetReportsByProject, GetReportById |
| Invoices | CreateInvoice, UpdatePaymentStatus, AddPayment | GetInvoiceByProject, GetInvoiceHistory |
| Variation Orders | CreateVO, ApproveVO, RejectVO, DeleteVO | GetVOsByProject, GetVOById |
| Tender Docs | UploadTenderDocument, DeleteTenderDocument | GetTenderDocumentByProject |
| Schedules | UploadSchedule, DeleteSchedule | GetSchedulesByProject |
| Drawings | UploadDrawing, DeleteDrawing | GetDrawingsByProject |

---

## Phase 3 — Infrastructure Layer

Location: `Contracting.Infrustructure/Features/Client/`  
Location: `Contracting.Infrustructure/Inteface/client/`

One service interface + implementation per module:
- `IClientService` / `ClientService`
- `IChatService` / `ChatService`
- `IClientMonthlyReportService` / `ClientMonthlyReportService`
- `IProjectInvoiceService` / `ProjectInvoiceService`
- `IVariationOrderService` / `VariationOrderService`
- `IProjectDocumentService` / `ProjectDocumentService` (Tender + Schedules + Drawings)

Register all in `DependencyInjection.cs`.

---

## Phase 4 — API Controllers

### Mobile Controller (read-heavy, client-facing)
`Contracting.API/Controllers/Client/Mobile/`

- `MobileClientProjectController`   → project summary + assigned modules
- `MobileChatController`             → send/receive messages
- `MobileClientReportsController`    → view/download monthly reports
- `MobileInvoiceController`          → view invoice + payment status
- `MobileVariationOrderController`   → view VOs, approve/reject
- `MobileProjectDocumentsController` → view tender docs, schedules, drawings

All routes: `api/mobile/client/[module]`  
Auth: `[Authorize(Roles = "Client")]`

### Web Controller (admin/team-facing, write-heavy)
`Contracting.API/Controllers/Client/Web/`

- `WebClientController`              → create/manage clients, assign projects
- `WebChatController`                → manage group members
- `WebClientReportsController`       → upload/delete monthly reports
- `WebInvoiceController`             → create invoice, update payment status, add payments
- `WebVariationOrderController`      → create/update/delete VOs
- `WebProjectDocumentsController`    → upload tender docs, schedules, drawings

All routes: `api/web/client/[module]`  
Auth: `[Authorize(Roles = "Admin,SuperAdmin,Teamlead-engineer,Office-engineer")]` (per endpoint)

---

## Phase 5 — Role-Based Access Per Feature

| Feature | Client (portal) | Planning | Technical | Accounts | Admin |
|---|---|---|---|---|---|
| Chat | Read + Send | Read + Send | Read + Send | Read + Send | Full |
| Monthly Reports | Read + Download | Upload | - | - | Full |
| Invoices | Read | - | - | Create + Update | Full |
| Variation Orders | Approve/Reject | - | Create + Upload | - | Full |
| Tender Documents | Read + Download | - | Upload | - | Full |
| Schedules | Read + Download | Upload | - | - | Full |
| Drawings & Renders | Read + Download | - | Upload | - | Full |

> Note: Planning = `Site-engineer`, Technical = `Office-engineer` / `Teamlead-engineer`, Accounts = will need a new `Accounts` role or mapped to `Admin`.

---

## Execution Order Summary

```
Phase 1  →  Domain entities + Enums + EF Configs + DbSets + Migration
Phase 2  →  DTOs + Commands + Queries + Handlers + Validators
Phase 3  →  Service interfaces + Implementations + DI registration
Phase 4  →  Mobile controllers + Web controllers
Phase 5  →  Role guards + permission policy
```
