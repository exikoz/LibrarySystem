USE LibrarySystemDB;
GO

----------------------------------------------------------
-- 1. INDEXES
----------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Authors_Name')
    CREATE INDEX IX_Authors_Name ON Authors(Name);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Books_Title')
    CREATE INDEX IX_Books_Title ON Books(Title);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Books_ISBN')
    CREATE UNIQUE INDEX IX_Books_ISBN ON Books(ISBN);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Members_LastName')
    CREATE INDEX IX_Members_LastName ON Members(LastName);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Loans_ReturnDate')
    CREATE INDEX IX_Loans_ReturnDate ON Loans(ReturnDate) WHERE ReturnDate IS NULL;
GO

----------------------------------------------------------
-- 2. VIEW: v_ActiveLoans
----------------------------------------------------------
CREATE OR ALTER VIEW v_ActiveLoans AS
SELECT 
    l.Id AS LoanId,
    b.Title,
    b.ISBN,
    bc.Id AS CopyId,
    m.MemberNumber,
    m.FirstName + ' ' + m.LastName AS MemberName,
    l.LoanDate,
    l.DueDate
FROM Loans l
JOIN BookCopies bc ON l.BookCopyId = bc.Id
JOIN Books b ON bc.BookId = b.Id
JOIN Members m ON l.MemberId = m.Id
WHERE l.ReturnDate IS NULL;
GO

----------------------------------------------------------
-- 3. TRIGGER: trg_UpdateAvailability
----------------------------------------------------------
-- Updates BookCopies.IsAvailable automatically when loans are created or returned.
CREATE OR ALTER TRIGGER trg_UpdateAvailability
ON Loans
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    -- New Loan: Set IsAvailable to 0
    IF EXISTS (SELECT * FROM inserted WHERE ReturnDate IS NULL)
    BEGIN
        UPDATE BookCopies
        SET IsAvailable = 0
        FROM BookCopies bc
        JOIN inserted i ON bc.Id = i.BookCopyId
        WHERE i.ReturnDate IS NULL;
    END

    -- Return Book: Set IsAvailable to 1
    IF UPDATE(ReturnDate)
    BEGIN
        UPDATE BookCopies
        SET IsAvailable = 1
        FROM BookCopies bc
        JOIN inserted i ON bc.Id = i.BookCopyId
        WHERE i.ReturnDate IS NOT NULL 
          AND (SELECT ReturnDate FROM deleted d WHERE d.Id = i.Id) IS NULL; 
    END
END;
GO

----------------------------------------------------------
-- 4. STORED PROCEDURE: sp_RegisterLoan
----------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_RegisterLoan
    @MemberId INT,
    @BookCopyId INT,
    @LoanDays INT = 15
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM BookCopies WHERE Id = @BookCopyId AND IsAvailable = 1)
        BEGIN
            THROW 50001, 'Book copy is not available.', 1;
        END

        IF NOT EXISTS (SELECT 1 FROM Members WHERE Id = @MemberId)
        BEGIN
            THROW 50002, 'Member not found.', 1;
        END

        INSERT INTO Loans (MemberId, BookCopyId, LoanDate, DueDate)
        VALUES (@MemberId, @BookCopyId, GETDATE(), DATEADD(DAY, @LoanDays, GETDATE()));

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
