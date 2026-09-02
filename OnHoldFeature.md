# On Hold / Resume — Request Status Feature

Lets a user put **any request on hold** and later **resume** it. Applies to all four request
types. While a request is On Hold it is frozen from the automatic status jobs (auto In‑Progress,
auto Delayed) until it is resumed.

---

## The "On Hold" status

- A shared **master status** row in `master.Statuses`:
  - `Code = "ON_HOLD"`, `nameEn = "On Hold"`, `nameAr = "قيد الانتظار"`, `showInDropdown = 1`.
- It previously existed but was repurposed into "Missing Information" (migration `20260515131301`).
  It has now been **re‑introduced** as a fresh row.
- Constant: `MasterStatusCodes.Hold` (`"ON_HOLD"`).
- Resolved (optionally) via `StatusResolver.LoadRequestStatusIdsAsync` → `RequestStatusIds.Hold`
  (nullable — `null` when the status has not been seeded yet, so status resolution never throws).

### Seed migration

`20260831115026_SeedOnHoldStatus` — idempotent (`IF NOT EXISTS`) single‑row insert.

```bash
dotnet ef database update \
  --project src/Contracting.Infrustructure/Contracting.Infrustructure.csproj \
  --startup-project src/Contracting.API/Contracting.API.csproj
```

> Target database is whatever `ConnectionStrings:DefaultConnection` (in `appsettings.json`) points
> to. Encrypted `ENC:…` strings are decrypted at design time by `AppDbContextFactory`.

---

## API contract

Resume always returns a request to **In Progress**. Permissions for Hold/Resume are the **same as
the request type's other actions** (assigned engineer / team lead, per existing checks).

| Request type          | Endpoint                                  | Hold                                   | Resume                                  |
| --------------------- | ----------------------------------------- | -------------------------------------- | --------------------------------------- |
| **EngineerRequest**   | existing take‑action endpoint             | send `statusId` = *On Hold* status id  | send `statusId` = *In Progress* status id |
| **FinancialClearance**| `POST /api/financialclearance/{id}/action`| `ActionType: "Hold"`                   | `ActionType: "Resume"`                  |
| **TransferRequest**   | `POST` transfer request action endpoint   | `ActionType: "Hold"`                   | `ActionType: "Resume"`                  |
| **LaborAttendance**   | `POST` labor attendance action endpoint   | `ActionType: "Hold"`                   | `ActionType: "Resume"`                  |

- `EngineerRequest` needed no code change — its `TakeAction` already accepts any `statusId`, and the
  seeded status now appears in the status dropdown.
- For the three verb‑based workflows, `"Hold"` / `"on_hold"` and `"Resume"` were added to the action
  state machine. Action matching is case‑insensitive.

### Example — Financial Clearance

```http
POST /api/financialclearance/{id}/action
Content-Type: multipart/form-data

ActionType=Hold
Comments=Waiting on client confirmation
```

```http
POST /api/financialclearance/{id}/action
Content-Type: multipart/form-data

ActionType=Resume
```

---

## Rules & guards

- A **completed** or **rejected** request **cannot** be put on hold.
- Putting a request that is already On Hold on hold again is rejected (`AlreadyOnHold`).
- **Resume** is only valid from On Hold; it transitions to **In Progress**.
- If the On Hold status is not configured in the DB, Hold/Resume return a validation error
  (`HoldNotConfigured`) instead of throwing.
- Every Hold/Resume writes a status‑change **activity** record, like other actions.

### Interaction with the status‑automation job

The hourly `engineer-request-status-automation` job only:
- moves **New/Pending** requests → **In Progress**, and
- moves **In Progress** requests → **Delayed** (after the end date's day fully ends).

An **On Hold** request matches neither set, so it is **not** auto‑advanced or auto‑delayed. It stays
On Hold until a user resumes it. Resuming returns it to In Progress, where normal automation applies
again.

---

## Files changed

| File | Change |
| ---- | ------ |
| `src/Contracting.Shared/Constants/MasterStatusCodes.cs` | Added `Hold = "ON_HOLD"` |
| `src/Contracting.Infrustructure/Extensions/Helpers/StatusResolver.cs` | Added optional `Hold` id to `RequestStatusIds` |
| `src/Contracting.Infrustructure/Features/business/FinancialClearanceService.cs` | `hold` / `resume` action cases |
| `src/Contracting.Infrustructure/Features/business/TransferRequestService.cs` | `hold` / `resume` action cases |
| `src/Contracting.Infrustructure/Features/business/LaborAttendanceService.cs` | `hold` / `resume` action cases |
| `src/Contracting.Infrustructure/Migrations/20260831115026_SeedOnHoldStatus.cs` | Idempotent seed of the On Hold status |

---

## Deployment checklist

1. **Deploy the build** (the Hold/Resume `ActionType` handlers ship with the code).
2. **Apply the seed migration** to the target database (`SeedOnHoldStatus`).
3. Confirm the **On Hold** status appears in the status dropdown.
4. Frontend: expose Hold / Resume actions per the API contract above.

### Environment status (as of this change)

- **UAT** — On Hold status seeded ✅ (app redeploy still needed for the action handlers).
- **Production** — not yet applied; run the migration + deploy the build during the prod release,
  after switching `DefaultConnection` back to the production entry.
