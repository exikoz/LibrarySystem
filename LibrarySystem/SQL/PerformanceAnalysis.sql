-- =============================================================
-- PERFORMANCE ANALYSIS & INDEX STRATEGY (VG Requirement)
-- =============================================================
-- INSTRUCTIONS:
-- 1. Open this file in SQL Server Management Studio (SSMS).
-- 2. Enable "Include Actual Execution Plan" (Ctrl + M).
-- 3. Run the two queries below together.
-- 4. Compare the "Execution Plan" tab results.

USE LibrarySystemDB;
GO

-- -------------------------------------------------------------
-- STRATEGY 1: SEARCH ON INDEXED COLUMN (Seek)
-- -------------------------------------------------------------
-- Created an index on Books(ISBN) in 'AdvancedDatabaseObjects.sql'.
-- Expected Result: "Index Seek" (Efficient, jumps directly to data).
SELECT * FROM Books 
WHERE ISBN = '978-0-123456-47-2'; -- Replace with a real ISBN from demo data

-- -------------------------------------------------------------
-- STRATEGY 2: SEARCH ON UNINDEXED COLUMN (Scan)
-- -------------------------------------------------------------
-- We do NOT have an index on PublishedYear.
-- Expected Result: "Table Scan" or "Clustered Index Scan" (Inefficient, checks every row).
SELECT * FROM Books 
WHERE PublishedYear = 2024;
