# LibrarySystem - Project Documentation

<img width="1440" height="477" alt="image" src="https://github.com/user-attachments/assets/93b47ac9-ee89-4881-9484-c6bb75c653fd" />


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

## 6. Coherent Database Flow
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

# Codebase Architecture & Walkthrough

This document fully explains how the LibrarySystem codebase is structured, the patterns used, and how the different pieces fit together.

<img width="4512" height="2304" alt="erdplus(3)" src="https://github.com/user-attachments/assets/42185e06-36eb-41d7-bcc1-db6bb50a47b0" />
<img width="3411" height="6166" alt="Library Loan Lifecycle Flow-2026-01-19-174249" src="https://github.com/user-attachments/assets/fdef2985-22e9-41ea-a79e-6dd4b0712a07" />
<img width="6172" height="8192" alt="IBookService Data Flow-2026-01-19-012950" src="https://github.com/user-attachments/assets/ed37d0fc-ab86-4eef-beb1-27b8ac1aaeb3" />


## 1. High-level Concepts & Layers

The application uses a **Clean Architecture** (or N-Layer) approach, ensuring separation of concerns. This means the code is divided into layers where each layer has a specific responsibility:

1.  **UI Layer (`LibrarySystem.UI`)**: Handles user interaction (Console input/output).
2.  **Service Layer (`LibrarySystem.Services`)**: Contains the *business logic* (rules, validations, calculations).
3.  **Data Layer (`LibrarySystem.Data`)**: Handles database communication.

### Why Interfaces? (`IInterface`)
You will see interfaces everywhere (e.g., `IBookService`, `IBookRepository`).
-   **Decoupling**: The UI doesn't know *how* `BookService` works, only what methods it has.
-   **Dependency Injection (DI)**: Allows the system to "inject" the specific implementation at startup.
-   **Testing**: Makes it easy to swap real database repositories with "fake" ones for testing.

---

## 2. Where does it start? (The Flow)

### 1. The Entry Point: `Program.cs`
This is the starting line.
```csharp
// Program.cs
var serviceProvider = ApplicationSetup.ConfigureServices(); // 1. Setup DI
var app = serviceProvider.GetRequiredService<App>();        // 2. Get the Main App
app.Run();                                                  // 3. Start the Loop
```

### 2. The Setup: `ApplicationSetup.cs`
This class is the "Factory" of your application. It configures **Dependency Injection**.
-   It register all the "parts" of your car (Engine, Wheels, etc.) into a container.
-   **`services.AddScoped<Interface, Implementation>()`**: Tells the app "Whenever someone asks for `IBookRepository`, give them a `BookRepository`."

### 3. The Main Loop: `UI/App.cs`
The `App` class is the manager.
-   It has no "new" keywords. It receives all services via its constructor (Dependency Injection).
-   `Run()`: Starts an infinite `while(true)` loop that shows the main menu.

---

## 3. The Components Explained

### Data Layer (`Data`)
**Why separate?** To ensure the rest of the app doesn't care if you use SQL Server, SQLite, or a text file.

#### Repositories (`BookRepository`, etc.)
-   **Role**: A collection-like interface for accessing domain objects from the database.
-   **Why**: abstracts away the underlying EF Core complex queries (like `.Include()`, `.Where()`) into simple methods like `GetByName()`.

#### UnitOfWork (`UnitOfWork.cs`)
-   **Role**: It acts as a single transaction manager.
-   **Why**:
    -   It holds references to all Repositories.
    -   It has one `Complete()` method that calls `_context.SaveChanges()`.
    -   **Benefit**: If you add a Book AND a Member in one operation, calling `Complete()` once ensures both are saved together, or neither is (Atomic Transaction).

### Service Layer (`Services`)
**Role**: The "Brain" of the application.
-   It sits between the UI and Data.
-   It **orchestrates** actions: "Validate ISBN" -> "Check if Author exists" -> "Create Book" -> "Save".

**Example `BookService`**:
1.  Receives raw data from UI.
2.  Calls `Validator` to check inputs.
3.  Calls `BookRepository` via `UnitOfWork` to add entities.
4.  Calls `UnitOfWork.Complete()` to save.

### Validation (`Services/Validation`)
-   **`Validator.cs`**: A dedicated helper to keep "dirty checks" (Is ISBN valid? Is Email valid?) out of the main logic. Clean code principle.

---

## 4. Specific Mechanisms

### The Loader / WarmUp
**Problem**: The first time Entity Framework (EF) connects to the database, it's slow because it has to "compile" the models and open a connection.
**Solution**:
1.  **`UnitOfWork.WarmUpAsync()`**: Runs a dummy query (`CanConnectAsync` or `FirstOrDefault`) to force this initialization to happen early.
2.  **`ConsoleHelper.ShowSpinner`**: This is a UI trick. It starts a background Task (the warm-up) and while that task is running, the main thread loops and prints `/ - \ |` to show activity.

### The UI Logic
The `UI` folder is purely for display.
-   **`App.cs`**: Wireframe of menus.
-   **`Helpers/ConsoleHelper.cs`**: Wraps `Console.WriteLine` and `Console.ReadLine` to ensure consistent coloring, valid input loops, and cleaner code in `App.cs`.

## Summary of Interaction
When you "Add a Book":
1.  **UI**: `App.cs` asks user for input via `ConsoleHelper`.
2.  **UI**: Calls `_bookService.AddBook(...)`.
3.  **Service**: `BookService` validates input.
4.  **Service**: Calls `_unitOfWork.Books.AddBook(...)`.
5.  **Data**: `BookRepository` adds the entity to EF Core's "Local Tracking" (in memory).
6.  **Service**: Calls `_unitOfWork.Complete()`.
7.  **Data**: `DbContext` sends the SQL `INSERT` to the database.

## Sources used

- **EF Core – Concurrency handling**  
  https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations

- **Repository & Unit of Work Pattern (ASP.NET MVC)**  
  https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application

- **Dependency Injection in .NET**  
  https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection


