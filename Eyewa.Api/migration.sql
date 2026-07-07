-- =========================================================================
-- Multi-Tenancy Schema Update, Data Migration, and RLS Policy Script
-- =========================================================================

-- 1. Create dedicated database login and user for API execution to enforce RLS
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = 'eyewa_api_user')
BEGIN
    CREATE LOGIN eyewa_api_user WITH PASSWORD = 'EyewaSecurePass2026!';
END

IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = 'eyewa_api_user')
BEGIN
    CREATE USER eyewa_api_user FOR LOGIN eyewa_api_user;
END

-- Grant read, write, and execute permissions
ALTER ROLE db_datareader ADD MEMBER eyewa_api_user;
ALTER ROLE db_datawriter ADD MEMBER eyewa_api_user;
GRANT EXECUTE TO eyewa_api_user;
GO

PRINT 'Dedicated user eyewa_api_user verified and configured.';
GO

-- 2. Add TenantId column to all target tables if it does not exist
DECLARE @tableName NVARCHAR(256)
DECLARE @sql NVARCHAR(MAX)

DECLARE table_cursor CURSOR FOR
SELECT name FROM sys.tables 
WHERE name IN (
    'Brand', 'Category', 'Counter', 'Designation', 'Employee', 'EmployeeSalary', 'ExpenseType', 
    'InvoicePayment', 'Logins', 'OrderLense', 'Organisation', 'PettyExpenseDetails', 'PettyExpensePayments', 
    'PettyExpenses', 'Prescription', 'ProcessSalary', 'Products', 'SalaryPayslip', 'Sale', 'SalesDetails', 
    'Stock', 'StockOpeningBalance', 'StockOpeningBalanceDetails', 'Store', 'StoreDeliveryNote', 
    'StoreDeliveryNoteDetails', 'StoreReturn', 'StoreReturnDetails', 'StoreStock', 'SupplierDeliveryNote', 
    'SupplierDeliveryNoteDetails', 'SupplierDeliveryNoteDetailsTemp', 'SupplierProducts', 'Suppliers', 
    'Users', 'WorkExperience', 'ZatcaInvoices', 'NotificationSettings', 'UserRefreshTokens', 'AspNetUsers'
)

OPEN table_cursor
FETCH NEXT FROM table_cursor INTO @tableName

WHILE @@FETCH_STATUS = 0
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.columns 
        WHERE object_id = OBJECT_ID(@tableName) AND name = 'TenantId'
    )
    BEGIN
        SET @sql = 'ALTER TABLE dbo.[' + @tableName + '] ADD TenantId NVARCHAR(50) NULL;'
        EXEC sp_executesql @sql
        PRINT 'Added TenantId column to ' + @tableName
    END
    ELSE
    BEGIN
        PRINT 'TenantId column already exists in ' + @tableName
    END
    
    FETCH NEXT FROM table_cursor INTO @tableName
END

CLOSE table_cursor
DEALLOCATE table_cursor;
GO

PRINT 'TenantId columns added.';
GO

-- 3. Generate unique TenantId for AspNetUsers
UPDATE AspNetUsers 
SET TenantId = LOWER(CAST(NEWID() AS NVARCHAR(50))) 
WHERE TenantId IS NULL OR TenantId = '';

-- Sync/Update TenantId for Logins table from AspNetUsers based on matching name
UPDATE L
SET L.TenantId = U.TenantId
FROM Logins L
INNER JOIN AspNetUsers U ON L.LoginName = U.UserName
WHERE L.TenantId IS NULL OR L.TenantId = '';

-- For any remaining users in Logins who might not be in AspNetUsers yet
UPDATE Logins 
SET TenantId = LOWER(CAST(NEWID() AS NVARCHAR(50))) 
WHERE TenantId IS NULL OR TenantId = '';
GO

PRINT 'Generated and synced unique TenantId for all existing users.';
GO

-- 4. Migrate existing records for tables with CreatedBy column
DECLARE @tableNameCB NVARCHAR(256)
DECLARE @sql NVARCHAR(MAX)

DECLARE cb_table_cursor CURSOR FOR
SELECT c.TABLE_NAME 
FROM INFORMATION_SCHEMA.COLUMNS c
INNER JOIN sys.tables t ON c.TABLE_NAME = t.name
WHERE c.COLUMN_NAME = 'CreatedBy' 
  AND t.name IN (
    'Brand', 'Category', 'Counter', 'Designation', 'Employee', 'EmployeeSalary', 'ExpenseType', 
    'InvoicePayment', 'Logins', 'OrderLense', 'Organisation', 'PettyExpenseDetails', 
    'PettyExpenses', 'ProcessSalary', 'Products', 'SalaryPayslip', 'Sale', 'SalesDetails', 
    'Stock', 'StockOpeningBalance', 'StockOpeningBalanceDetails', 'Store', 'StoreDeliveryNote', 
    'StoreDeliveryNoteDetails', 'StoreReturn', 'StoreReturnDetails', 'StoreStock', 'SupplierDeliveryNote', 
    'SupplierDeliveryNoteDetails', 'SupplierDeliveryNoteDetailsTemp', 'SupplierProducts', 'Suppliers', 
    'Users', 'WorkExperience'
  )

OPEN cb_table_cursor
FETCH NEXT FROM cb_table_cursor INTO @tableNameCB

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sql = '
        UPDATE T
        SET T.TenantId = L.TenantId
        FROM dbo.[' + @tableNameCB + '] T
        INNER JOIN Logins L ON T.CreatedBy = L.LoginID
        WHERE T.TenantId IS NULL AND L.TenantId IS NOT NULL;'
    EXEC sp_executesql @sql
    PRINT 'Migrated records in ' + @tableNameCB + ' based on CreatedBy.'
    
    FETCH NEXT FROM cb_table_cursor INTO @tableNameCB
END

CLOSE cb_table_cursor
DEALLOCATE cb_table_cursor;
GO

PRINT 'Migrated CreatedBy tables.';
GO

-- 5. Migrate other related tables
-- Migrate ZatcaInvoices (linked to Sale)
UPDATE Z
SET Z.TenantId = S.TenantId
FROM ZatcaInvoices Z
INNER JOIN Sale S ON Z.SalesId = S.SaleID
WHERE Z.TenantId IS NULL AND S.TenantId IS NOT NULL;

-- Migrate PettyExpensePayments (linked to PettyExpenseDetails)
UPDATE P
SET P.TenantId = D.TenantId
FROM PettyExpensePayments P
INNER JOIN PettyExpenseDetails D ON P.PettyExpenseDetailsID = D.PettyExpenseDetailsID
WHERE P.TenantId IS NULL AND D.TenantId IS NOT NULL;

-- Migrate Prescription (linked to Sale)
UPDATE P
SET P.TenantId = S.TenantId
FROM Prescription P
INNER JOIN Sale S ON P.SaleID = S.SaleID
WHERE P.TenantId IS NULL AND S.TenantId IS NOT NULL;

-- Migrate UserRefreshTokens (UserId to AspNetUsers.Id)
UPDATE T
SET T.TenantId = U.TenantId
FROM UserRefreshTokens T
INNER JOIN AspNetUsers U ON T.UserId = U.Id
WHERE T.TenantId IS NULL AND U.TenantId IS NOT NULL;

-- Migrate UserRefreshTokens (UserId to Logins.LoginID)
UPDATE T
SET T.TenantId = L.TenantId
FROM UserRefreshTokens T
INNER JOIN Logins L ON T.UserId = CAST(L.LoginID AS VARCHAR(50))
WHERE T.TenantId IS NULL AND L.TenantId IS NOT NULL;
GO

PRINT 'Migrated child/related tables.';
GO

-- 6. Assign system fallback tenant ID to any remaining NULL TenantIds across all tables
DECLARE @tableNameNull NVARCHAR(256)
DECLARE @sqlNull NVARCHAR(MAX)

DECLARE null_table_cursor CURSOR FOR
SELECT name FROM sys.tables 
WHERE name IN (
    'Brand', 'Category', 'Counter', 'Designation', 'Employee', 'EmployeeSalary', 'ExpenseType', 
    'InvoicePayment', 'Logins', 'OrderLense', 'Organisation', 'PettyExpenseDetails', 'PettyExpensePayments', 
    'PettyExpenses', 'Prescription', 'ProcessSalary', 'Products', 'SalaryPayslip', 'Sale', 'SalesDetails', 
    'Stock', 'StockOpeningBalance', 'StockOpeningBalanceDetails', 'Store', 'StoreDeliveryNote', 
    'StoreDeliveryNoteDetails', 'StoreReturn', 'StoreReturnDetails', 'StoreStock', 'SupplierDeliveryNote', 
    'SupplierDeliveryNoteDetails', 'SupplierDeliveryNoteDetailsTemp', 'SupplierProducts', 'Suppliers', 
    'Users', 'WorkExperience', 'ZatcaInvoices', 'NotificationSettings', 'UserRefreshTokens', 'AspNetUsers'
)

OPEN null_table_cursor
FETCH NEXT FROM null_table_cursor INTO @tableNameNull

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sqlNull = 'UPDATE dbo.[' + @tableNameNull + '] SET TenantId = ''d1111111-1111-1111-1111-111111111111'' WHERE TenantId IS NULL;'
    EXEC sp_executesql @sqlNull
    FETCH NEXT FROM null_table_cursor INTO @tableNameNull
END

CLOSE null_table_cursor
DEALLOCATE null_table_cursor;
GO

PRINT 'Assigned system fallback tenant ID to remaining NULLs.';
GO

-- 7. Apply NOT NULL constraints and DEFAULT constraints
DECLARE @tableNameNotNull NVARCHAR(256)
DECLARE @sqlNotNull NVARCHAR(MAX)

DECLARE notnull_table_cursor CURSOR FOR
SELECT name FROM sys.tables 
WHERE name IN (
    'Brand', 'Category', 'Counter', 'Designation', 'Employee', 'EmployeeSalary', 'ExpenseType', 
    'InvoicePayment', 'Logins', 'OrderLense', 'Organisation', 'PettyExpenseDetails', 'PettyExpensePayments', 
    'PettyExpenses', 'Prescription', 'ProcessSalary', 'Products', 'SalaryPayslip', 'Sale', 'SalesDetails', 
    'Stock', 'StockOpeningBalance', 'StockOpeningBalanceDetails', 'Store', 'StoreDeliveryNote', 
    'StoreDeliveryNoteDetails', 'StoreReturn', 'StoreReturnDetails', 'StoreStock', 'SupplierDeliveryNote', 
    'SupplierDeliveryNoteDetails', 'SupplierDeliveryNoteDetailsTemp', 'SupplierProducts', 'Suppliers', 
    'Users', 'WorkExperience', 'ZatcaInvoices', 'NotificationSettings', 'UserRefreshTokens', 'AspNetUsers'
)

OPEN notnull_table_cursor
FETCH NEXT FROM notnull_table_cursor INTO @tableNameNotNull

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Make column NOT NULL
    SET @sqlNotNull = 'ALTER TABLE dbo.[' + @tableNameNotNull + '] ALTER COLUMN TenantId NVARCHAR(50) NOT NULL;'
    EXEC sp_executesql @sqlNotNull
    
    -- Drop existing default constraint if any
    DECLARE @constraintName NVARCHAR(256)
    SELECT @constraintName = d.name 
    FROM sys.default_constraints d
    INNER JOIN sys.columns c ON d.parent_column_id = c.column_id AND d.parent_object_id = c.object_id
    WHERE parent_object_id = OBJECT_ID(@tableNameNotNull) AND c.name = 'TenantId'
    
    IF @constraintName IS NOT NULL
    BEGIN
        SET @sqlNotNull = 'ALTER TABLE dbo.[' + @tableNameNotNull + '] DROP CONSTRAINT [' + @constraintName + '];'
        EXEC sp_executesql @sqlNotNull
    END
    
    -- Add new DEFAULT constraint using SESSION_CONTEXT
    SET @sqlNotNull = 'ALTER TABLE dbo.[' + @tableNameNotNull + '] ADD CONSTRAINT [DF_' + @tableNameNotNull + '_TenantId] DEFAULT (COALESCE(CAST(SESSION_CONTEXT(N''TenantId'') AS NVARCHAR(50)), ''d1111111-1111-1111-1111-111111111111'')) FOR TenantId;'
    EXEC sp_executesql @sqlNotNull
    
    PRINT 'Applied NOT NULL and DEFAULT constraint to ' + @tableNameNotNull
    
    FETCH NEXT FROM notnull_table_cursor INTO @tableNameNotNull
END

CLOSE notnull_table_cursor
DEALLOCATE notnull_table_cursor;
GO

PRINT 'Table constraints successfully updated.';
GO

-- 8. Setup Row-Level Security Predicate Function
CREATE OR ALTER FUNCTION dbo.fn_SecurityPredicate(@TenantId NVARCHAR(50))
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN SELECT 1 AS result
WHERE SESSION_CONTEXT(N'TenantId') IS NULL OR @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS NVARCHAR(50));
GO

PRINT 'Row-Level Security predicate function defined.';
GO

-- 9. Setup and Enable Row-Level Security Policy
DROP SECURITY POLICY IF EXISTS dbo.TenantSecurityPolicy;
CREATE SECURITY POLICY dbo.TenantSecurityPolicy;
GO

DECLARE @tableNameRLS NVARCHAR(256)
DECLARE @sqlRLS NVARCHAR(MAX)

DECLARE rls_table_cursor CURSOR FOR
SELECT name FROM sys.tables 
WHERE name IN (
    'Brand', 'Category', 'Counter', 'Designation', 'Employee', 'EmployeeSalary', 'ExpenseType', 
    'InvoicePayment', 'Logins', 'OrderLense', 'Organisation', 'PettyExpenseDetails', 'PettyExpensePayments', 
    'PettyExpenses', 'Prescription', 'ProcessSalary', 'Products', 'SalaryPayslip', 'Sale', 'SalesDetails', 
    'Stock', 'StockOpeningBalance', 'StockOpeningBalanceDetails', 'Store', 'StoreDeliveryNote', 
    'StoreDeliveryNoteDetails', 'StoreReturn', 'StoreReturnDetails', 'StoreStock', 'SupplierDeliveryNote', 
    'SupplierDeliveryNoteDetails', 'SupplierDeliveryNoteDetailsTemp', 'SupplierProducts', 'Suppliers', 
    'Users', 'WorkExperience', 'ZatcaInvoices', 'NotificationSettings', 'UserRefreshTokens'
)

OPEN rls_table_cursor
FETCH NEXT FROM rls_table_cursor INTO @tableNameRLS

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @sqlRLS = 'ALTER SECURITY POLICY dbo.TenantSecurityPolicy 
        ADD FILTER PREDICATE dbo.fn_SecurityPredicate(TenantId) ON dbo.[' + @tableNameRLS + '],
        ADD BLOCK PREDICATE dbo.fn_SecurityPredicate(TenantId) ON dbo.[' + @tableNameRLS + '] AFTER INSERT,
        ADD BLOCK PREDICATE dbo.fn_SecurityPredicate(TenantId) ON dbo.[' + @tableNameRLS + '] AFTER UPDATE;'
    EXEC sp_executesql @sqlRLS
    PRINT 'Applied RLS predicates to ' + @tableNameRLS
    
    FETCH NEXT FROM rls_table_cursor INTO @tableNameRLS
END

CLOSE rls_table_cursor
DEALLOCATE rls_table_cursor;

-- Enable policy
ALTER SECURITY POLICY dbo.TenantSecurityPolicy WITH (STATE = ON);
PRINT 'Row-Level Security Policy activated successfully.';
GO
