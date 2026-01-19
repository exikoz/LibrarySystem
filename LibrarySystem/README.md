# LibrarySystem - Project Documentation

## 1. Introduction
This is a Library Management System built as a C# Console Application. It uses a **Clean Architecture** approach to organize code logically and **Entity Framework Core** to interact with a SQL Server database.

The goal of this architecture is to make the code flexible, easy to test, and easy to maintain.

## 2. Setup & Installation
1.  **Database**: Ensure you have SQL Server installed and the `LibrarySystemDB` database created using the provided SQL scripts.
2.  **Configuration**: Check `appsettings.json` (if present) or `LibrarySystemDbContext.cs` to ensure the connection string matches your local server.
3.  **Build**: Open the solution in Visual Studio or VS Code and run `dotnet build`.
4.  **Run**: Execute `dotnet run` or press Start in your IDE.
    *   *Note*: On startup, you will see a splash screen while the system "warms up" (connects to the database).

## 3. Architecture & Design Patterns

### 3.1 The "Why" and "How"
We separate our code into different "Layers" so that each part has a single responsibility.

#### **Repository Pattern (Data Access)**
*   **What it is**: A class that sits between our code and the database.
*   **Why we use it**: Instead of writing complex EF Core code everywhere, we hide (encapsulate) it inside a Repository.
    *   *Example*: `BookRepository` checks the database for books.
*   **Interfaces (`IBookRepository`)**: We use interfaces to define a "contract". The application says "I need a way to Get Books", it doesn't care *how* `BookRepository` does it. This makes it easy to swap implementations later.

#### **Unit of Work (Transaction Management)**
*   **What it is**: A "Manager" that oversees all Repositories.
*   **Why we use it**: To ensure **Atomic Transactions**. If we add a Member and then fail to add their Loan, we want *neither* to happen.
*   **How it works**:
    1.  We do all our work (Add Book, Register Loan, etc.).
    2.  We call `_unitOfWork.Complete()`.
    3.  This calls `SaveChanges()` *once* at the very end, saving everything as a single "Unit".

#### **Service Layer (Business Logic)**
*   **What it is**: The "Brain" of the application.
*   **Why we use it**: The UI shouldn't know about databases, and Repositories shouldn't make decisions. Services sit in the middle.
    *   *Example*: `LoanService` decides *if* a book can be loaned (validation) before asking the Repository to save it.
*   **Interfaces (`IBookService`)**: Just like repositories, we use interfaces here so the UI depends only on the *idea* of a service, not the specific class.

#### **Dependency Injection (DI)**
*   **What it is**: A technique where we give a class the tools it needs, rather than letting it create them.
*   **How `ApplicationSetup.cs` works**: This file is our "configuration center".
    *   We tell the program: "Whenever someone asks for `IBookService`, give them `BookService`."
    *   When the App starts, the system automatically creates `BookService` and gives it the `UnitOfWork` it needs.

### 3.2 User Interface (UI)
*   **`App.cs`**: The main controller. It shows menus and handles user input.
*   **Helpers**:
    *   **`ConsoleHelper`**: Handles all the fancy drawing (tables, colors, spinners).
    *   **`MenuOption`**: A simple object representing a menu item (Name + Action). This let us replace the big `switch` statements with a clean list of options.

### 3.3 Validation
*   **`IValidator`**: A central place for rules.
    *   *Why*: Only one place decides what a valid "Email" or "ISBN" looks like. If we change the rule, we change it in one place.

## 4. Advanced Features (VG Requirements)

### 4.1 Concurrency (Handling Multi-User Conflicts)
We implemented **Optimistic Locking** using a `RowVersion` column.

*   **The Problem**: Two users (A and B) try to edit the same record at the same time.
*   **The Solution**:
    1.  The database adds a binary timestamp (`RowVersion`) to every row.
    2.  When User A saves, the timestamp updates.
    3.  When User B tries to save, the system sees the timestamp has changed and rejects the save.
*   **How we tested it**:
    *   In the "Advanced Menu", we simulate this by creating two separate database connections (Context A and Context B).
    *   Context A loads a book. Context B loads the *same* book.
    *   Context A saves changes (updating the timestamp).
    *   Context B tries to save, and we catch the `DbUpdateConcurrencyException` meant to stop them.

### 4.2 Performance Improvements (SQL Lab)
We improved execution time by using **Indexes**.

*   **Scenario**: Searching for a Book.
*   **Before (Scan)**: Without an index, SQL Server has to check *every single row* in the table to find a book (like reading every page of a book to find a word).
*   **After (Seek)**: We added an Index on the `ISBN` column. Now SQL Server jumps directly to the result (like using the Index at the back of a book).
*   **How it works in the Lab**:
    *   We ran a search query with `SET STATISTICS TIME ON`.
    *   We compared the "CPU Time" and "Elapsed Time".
    *   The Indexed search was significantly faster (near instant) compared to the Table Scan.

## 5. Reflections on Data Integrity (Reflektion kring dataintegritet)
*Requirement: Reflektera kring hur din lösning säkerställer dataintegritet i praktiken.*

Our solution ensures data integrity through multiple layers of defense:

1.  **Database Constraints (The Foundation)**:
    *   **Primary Keys**: Uniquely identify every Row.
    *   **Foreign Keys**: Ensure a Loan cannot exist without a valid Member and BookCopy.
    *   **Unique Indexes**: Prevent duplicate emails (`UQ_Members_Email`) or ISBNs.

2.  **Transactional Consistency (Unit of Work)**:
    *   We use the **Unit of Work** pattern to ensure **Atomicity**. All changes in a business operation (e.g., adding a student and enrolling them) are saved together. If one fails, everything rolls back.
    *   *Code Snippet (`UnitOfWork.cs`)*:
        ```csharp
        public void Complete() {
            _context.SaveChanges(); // Commits all changes in one atomic transaction
        }
        ```

3.  **Encapsulation (Service Layer)**:
    *   The UI cannot modify data directly. It must go through Services which apply validation rules (e.g., `IsValidISBN`).

4.  **Concurrency Control (Optimistic Locking)**:
    *   We prevent "Lost Updates" (where one user overwrites another's work) using `RowVersion`.
    *   *Code Snippet (Handling Conflict)*:
        ```csharp
        try {
            _unitOfWork.Complete();
        } catch (DbUpdateConcurrencyException) {
            // "The record you attempted to edit was modified by another user..."
        }
        ```

## 6. Coherent Database Flow (Sammanhängande flöde)
*Requirement: Visa att du kan använda vyer, procedurer och triggers i ett sammanhängande flöde.*

We demonstrated this "Trinity" of database features in the **Loan Registration Process**:

1.  **Stored Procedure (`sp_RegisterLoan`)**:
    *   We use a Procedure to handle the logic of "Creating a Loan". It creates a transaction, checks if the book is available, and inserts the record.
    *   *Why*: Heavy logic lives close to the data for performance and safety.

2.  **Trigger (`trg_UpdateAvailability`)**:
    *   When the Procedure inserts a Loan, this Trigger **automatically fires**.
    *   It updates the `BookCopies` table to set `IsAvailable = 0`.
    *   *Why*: Ensured consistency. No matter how a loan is created (App or SQL), the book is *always* marked unavailable.

3.  **View (`v_ActiveLoans`)**:
    *   Finally, the App queries this View to show the user the result.
    *   It joins Loans, Books, and Members into one clean table.
    *   *Why*: Simplifies complex read queries.

**The Flow in Code:**
```sql
-- 1. App calls Procedure
EXEC sp_RegisterLoan @MemberId = 1, @BookCopyId = 5;

-- 2. Trigger fires automatically (Inside SQL Server)
--    -> UPDATE BookCopies SET IsAvailable = 0 WHERE Id = 5;

-- 3. App reads from View
SELECT * FROM v_ActiveLoans;
```
