-- ============================================================
-- Procurement Department – Special Fields Seed Script
-- Run once after creating the Procurement department.
-- The script looks up the department by name automatically.
-- ============================================================

BEGIN TRANSACTION;

-- ── 1. Resolve the Procurement department ID ──────────────────
DECLARE @DeptId UNIQUEIDENTIFIER;

SELECT @DeptId = Id
FROM   master.Departments
WHERE  IsDeleted = 0
  AND  (nameEn = 'Procurement' OR nameAr = N'المشتريات')
ORDER BY CreatedDate ASC;

IF @DeptId IS NULL
BEGIN
    RAISERROR('Procurement department not found. Create it first via POST /api/Department, then re-run this script.', 16, 1);
    ROLLBACK;
    RETURN;
END

PRINT 'Found Procurement department: ' + CAST(@DeptId AS NVARCHAR(50));

-- ── 2. Remove any existing special fields linked to this department ─
PRINT 'Removing existing special fields for this department...';

-- Capture the SpecialField IDs that are currently linked
DECLARE @OldSpecialFieldIds TABLE (SpecialFieldId UNIQUEIDENTIFIER);

INSERT INTO @OldSpecialFieldIds (SpecialFieldId)
SELECT SpecialFieldId
FROM   master.DepartmentSpecialFields
WHERE  DepartmentId = @DeptId;

-- Delete the department-field links first (FK child)
DELETE FROM master.DepartmentSpecialFields
WHERE  DepartmentId = @DeptId;

-- Delete the SpecialField records that were ONLY used by this department
-- (avoids orphans; safe because the FK is Cascade on the other side)
DELETE sf
FROM   master.SpecialFields sf
INNER  JOIN @OldSpecialFieldIds old ON sf.Id = old.SpecialFieldId
WHERE  NOT EXISTS (
    SELECT 1 FROM master.DepartmentSpecialFields dsf
    WHERE  dsf.SpecialFieldId = sf.Id
);

PRINT 'Existing fields removed.';

-- ── 3. Generate deterministic GUIDs for the SpecialFields ─────
--      Fields taken from the purchase request form:
--      اسم الصنف | الوحدة | الكمية | بند | الكود | ملاحظات
DECLARE @FieldItemName  UNIQUEIDENTIFIER = '11111111-0000-4000-A000-000000000001'; -- اسم الصنف
DECLARE @FieldUnit      UNIQUEIDENTIFIER = '11111111-0000-4000-A000-000000000002'; -- الوحدة
DECLARE @FieldQuantity  UNIQUEIDENTIFIER = '11111111-0000-4000-A000-000000000003'; -- الكمية
DECLARE @FieldLineItem  UNIQUEIDENTIFIER = '11111111-0000-4000-A000-000000000004'; -- بند
DECLARE @FieldCode      UNIQUEIDENTIFIER = '11111111-0000-4000-A000-000000000005'; -- الكود
DECLARE @FieldNotes     UNIQUEIDENTIFIER = '11111111-0000-4000-A000-000000000006'; -- ملاحظات

DECLARE @Now  DATETIMEOFFSET = SYSDATETIMEOFFSET();
DECLARE @Zero UNIQUEIDENTIFIER = '00000000-0000-0000-0000-000000000000';

-- ── 4. Insert into master.SpecialFields ───────────────────────
INSERT INTO master.SpecialFields
    (Id, name, fieldType, CreatedDate, ModifiedDate, CreatedBy, ModifiedBy, IsDeleted, DeletedDate, DeletedBy)
VALUES
(@FieldItemName, N'اسم الصنف / Item Name', 'ConstructionItem', @Now, NULL, @Zero, NULL, 0, NULL, NULL),
(@FieldUnit,     N'الوحدة / Unit',          'Text',   @Now, NULL, @Zero, NULL, 0, NULL, NULL),
(@FieldQuantity, N'الكمية / Quantity',      'Number', @Now, NULL, @Zero, NULL, 0, NULL, NULL),
(@FieldLineItem, N'بند / Line Item',        'Text',   @Now, NULL, @Zero, NULL, 0, NULL, NULL),
(@FieldCode,     N'الكود / Code',           'Text',   @Now, NULL, @Zero, NULL, 0, NULL, NULL),
(@FieldNotes,    N'ملاحظات / Notes',        'Text',   @Now, NULL, @Zero, NULL, 0, NULL, NULL);

-- ── 5. Link fields to the Procurement department ─────────────
INSERT INTO master.DepartmentSpecialFields
    (Id, DepartmentId, SpecialFieldId, value, CreatedDate, ModifiedDate, CreatedBy, ModifiedBy, IsDeleted, DeletedDate, DeletedBy)
VALUES
(NEWID(), @DeptId, @FieldItemName, NULL, @Now, NULL, @Zero, NULL, 0, NULL, NULL),
(NEWID(), @DeptId, @FieldUnit,     NULL, @Now, NULL, @Zero, NULL, 0, NULL, NULL),
(NEWID(), @DeptId, @FieldQuantity, NULL, @Now, NULL, @Zero, NULL, 0, NULL, NULL),
(NEWID(), @DeptId, @FieldLineItem, NULL, @Now, NULL, @Zero, NULL, 0, NULL, NULL),
(NEWID(), @DeptId, @FieldCode,     NULL, @Now, NULL, @Zero, NULL, 0, NULL, NULL),
(NEWID(), @DeptId, @FieldNotes,    NULL, @Now, NULL, @Zero, NULL, 0, NULL, NULL);

-- ── 6. Mark the department as having special fields ───────────
UPDATE master.Departments
SET    hasSpecialFields    = 1,
       RequiresGoodsReceipt = 1,
       ModifiedDate        = @Now
WHERE  Id = @DeptId;

PRINT 'Done. 6 special fields seeded for the Procurement department.';

COMMIT;
