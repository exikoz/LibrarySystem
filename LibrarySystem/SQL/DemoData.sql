USE LibrarySystemDB;
GO

-- Rensar data om du vill köra om scriptet (OBS: Ordningen är viktig pga Foreign Keys)
-- DELETE FROM Loans;
-- DELETE FROM Members;
-- DELETE FROM BookCopies;
-- DELETE FROM Books;
-- DELETE FROM Authors;
-- DBCC CHECKIDENT ('Authors', RESEED, 0); -- Nollställer ID-räknaren (frivilligt)
-- GO

---------------------------------------------
-- 1. LÄGG TILL FÖRFATTARE
---------------------------------------------
INSERT INTO Authors (Name) VALUES 
('J.K. Rowling'),           -- Id 1
('Robert C. Martin'),       -- Id 2 (Farbror Bob!)
('Astrid Lindgren'),        -- Id 3
('Stephen King');           -- Id 4

---------------------------------------------
-- 2. LÄGG TILL BÖCKER
---------------------------------------------
INSERT INTO Books (AuthorId, Title, ISBN, PublishedYear) VALUES 
(1, 'Harry Potter och De Vises Sten', '978-91-29-655', 1997),
(1, 'Harry Potter och Hemligheternas Kammare', '978-91-29-656', 1998),
(2, 'Clean Code: A Handbook of Agile Software Craftsmanship', '978-0132350884', 2008), -- En klassiker för utvecklare
(3, 'Pippi Långstrump', '978-91-29-657', 1945),
(4, 'The Shining', '978-0385121675', 1977);

---------------------------------------------
-- 3. LÄGG TILL BOK-EXEMPLAR (Lagret)
---------------------------------------------
-- Harry Potter (Bok 1) - Vi har 3 exemplar
INSERT INTO BookCopies (BookId, IsAvailable) VALUES 
(1, 1), -- Ex 1 (Tillgänglig)
(1, 0), -- Ex 2 (Utlånad - Se lån nedan)
(1, 1); -- Ex 3 (Tillgänglig)

-- Clean Code (Bok 3) - Vi har 2 exemplar
INSERT INTO BookCopies (BookId, IsAvailable) VALUES 
(3, 1), -- Ex 4
(3, 0); -- Ex 5 (Utlånad)

-- Pippi (Bok 4) - 1 exemplar
INSERT INTO BookCopies (BookId, IsAvailable) VALUES 
(4, 1); -- Ex 6

---------------------------------------------
-- 4. LÄGG TILL MEDLEMMAR
---------------------------------------------
INSERT INTO Members (MemberNumber, FirstName, LastName, Email, CreatedAt) VALUES 
('MEM-2024-001', 'Anna', 'Andersson', 'anna.a@test.com', '2023-01-15'),
('MEM-2024-002', 'Björn', 'Borg', 'bjorn.b@test.com', '2023-02-20'),
('MEM-2024-003', 'Cecilia', 'Carlsson', 'cecilia.c@test.com', '2024-01-10');

---------------------------------------------
-- 5. LÄGG TILL LÅN
---------------------------------------------

-- Lån 1: Björn lånade Harry Potter (Ex 2) för en månad sedan. Inte tillbakalämnad än.
-- OBS: Vi satte IsAvailable = 0 på Ex 2 ovan.
INSERT INTO Loans (MemberId, BookCopyId, LoanDate, DueDate, ReturnDate) VALUES 
(2, 2, DATEADD(DAY, -30, GETDATE()), DATEADD(DAY, -1, GETDATE()), NULL); 
-- Detta lån är FÖRSENAT (DueDate var igår)

-- Lån 2: Cecilia lånade Clean Code (Ex 5) idag.
-- OBS: Vi satte IsAvailable = 0 på Ex 5 ovan.
INSERT INTO Loans (MemberId, BookCopyId, LoanDate, DueDate, ReturnDate) VALUES 
(3, 5, GETDATE(), DATEADD(DAY, 14, GETDATE()), NULL);

-- Lån 3: Anna lånade Pippi (Ex 6) förra året, och lämnade tillbaka den.
-- Boken är nu tillgänglig (IsAvailable = 1).
INSERT INTO Loans (MemberId, BookCopyId, LoanDate, DueDate, ReturnDate) VALUES 
(1, 6, '2023-05-01', '2023-05-15', '2023-05-10');

---------------------------------------------
-- CHECKA RESULTATET
---------------------------------------------
-- Visa alla böcker och deras status
SELECT 
    b.Title, 
    bc.Id AS CopyId, 
    CASE WHEN bc.IsAvailable = 1 THEN 'Inne' ELSE 'Utlånad' END AS Status
FROM BookCopies bc
JOIN Books b ON bc.BookId = b.Id;

-- Visa pågående lån
SELECT 
    m.FirstName, 
    m.LastName, 
    b.Title, 
    l.DueDate 
FROM Loans l
JOIN Members m ON l.MemberId = m.Id
JOIN BookCopies bc ON l.BookCopyId = bc.Id
JOIN Books b ON bc.BookId = b.Id
WHERE l.ReturnDate IS NULL;