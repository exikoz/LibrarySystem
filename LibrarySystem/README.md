# Library System - Project Documentation

## 1. System Overview
This is a C# Console Application for a Library System, built using **Entity Framework Core (Database First)** and **SQL Server**. The system manages Books, Authors, Book Copies, Members, and Loans. It is designed to meet high academic standards (VG level) by implementing advanced database features and clean code principles.

## 2. Architecture & Design Patterns
The application follows a **Clean Architecture** approach with **Dependency Injection (DI)** to ensure Separation of Concerns and adherence to SOLID principles.

### 2.1 Project Structure
*   **Program.cs**: Evaluation Root. Minimal entry point. It configures the DI container (`ServiceProvider`) and launches the `App`.
*   **UI/App.cs**: The "Presentation Layer". Handles the menu loop, user input, and output. It delegates all business logic to injected services.
*   **Services/**: The "Business Logic Layer". Contains logic for managing entities (e.g., `BookService`, `LoanService`).
*   **Data/**: The "Data Access Layer". Contains the `LibrarySystemDbContext` (EF Core context).
*   **Models/**: POCO classes representing database tables and DTOs.

### 2.2 Dependency Injection (DI)
We utilize `Microsoft.Extensions.DependencyInjection` to implement the **Dependency Inversion Principle**.
*   **Interfaces**: We define `IBookService`, `IMemberService`, `ILoanService`.
*   **Implementation**: Concrete classes (`BookService` etc.) implement these interfaces.
*   **Injection**: `App` receives these interfaces in its constructor. It does not know *how* `BookService` works, only that it fulfills the contract.
*   **Benefit**: This creates loose coupling, making the system easier to test, extend, and maintain.

## 3. Database Features (VG Requirements)
This project goes beyond standard CRUD by integrating raw SQL features for performance and data integrity.

### 3.1 Advanced Objects (SQL)
*   **Stored Procedure (`sp_RegisterLoan`)**: 
    *   Handles loan registration within a **SQL Transaction**.
    *   Validates member existence and copy availability *atomically* on the server side correctly.
    *   *Usage*: Called via `_context.Database.ExecuteSqlRaw`.
*   **Trigger (`trg_UpdateAvailability`)**:
    *   Automatically updates `BookCopies.IsAvailable` (0 or 1) whenever a loan is inserted or returning (updated).
    *   Ensures data consistency between `Loans` and `BookCopies` tables without relying on application code.
*   **Views (`v_ActiveLoans`)**:
    *   Denormalizes complex joins between Loans, Books, BookCopies, and Members into a simple read-model.
    *   *Usage*: Mapped to `ActiveLoanDto` in `LoanService`.
*   **Indexes**:
    *   Added for frequently searched columns (`ISBN`, `Author.Name`, `Book.Title`) to optimize search performance.

### 3.2 Simulating Concurrency (RowVersion)
To handle the scenario where two users try to edit the same record simultaneously (Optimistic Concurrency):
*   **Mechanism**: The `BookCopies` table has a `RowVersion` (timestamp) column.
*   **Simulation**: `App.SimulateConcurrency()` creates two separate `DbContext` instances to mimic User A and User B.
    1.  Both users load the same Book Copy (same `RowVersion`).
    2.  User A modifies and saves. The DB updates the `RowVersion`.
    3.  User B tries to save using the *old* `RowVersion`.
    4.  EF Core throws `DbUpdateConcurrencyException`.
*   **Quirk**: We handle this exception specifically to inform the user that the data was modified by someone else.

### 4. Technical Quirks & Solutions
*   **EF Core & Triggers**: 
    *   *Problem*: EF Core normally uses the `OUTPUT` clause to get IDs back after an insert. SQL Server prevents this on tables with Triggers (`Loans`).
    *   *Solution*: We specifically configured `entity.ToTable(tb => tb.HasTrigger("trg_UpdateAvailability"))` in `LibrarySystemDbContext`. This forces EF Core to use a compatible, safer strategy for saving changes.

## 5. Fulfillment of Requirements

| Requirement | Implementation | Level |
| :--- | :--- | :--- |
| **Console App** | Menu-driven interface in `App.cs`. | G |
| **EF Core DB First** | Scaffolding used, `DbContext` manages data. | G |
| **Manage Books** | `BookService` (Add, Search). | G |
| **Manage Members** | `MemberService` (Register, List). | G |
| **Stored Procedures** | `sp_RegisterLoan` used for creating loans. | **VG** |
| **Transactions** | Implemented inside `sp_RegisterLoan`. | **VG** |
| **Views** | `v_ActiveLoans` used for listing active loans. | **VG** |
| **Triggers** | `trg_UpdateAvailability` updates stock automatically. | **VG** |
| **Concurrency** | `RowVersion` implemented and demonstrated in "Advanced Menu". | **VG** |
| **Architecture** | Services, DI, Interfaces, DTOs. | **VG** |

