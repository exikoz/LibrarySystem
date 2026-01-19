# Codebase Architecture & Walkthrough

This document fully explains how the LibrarySystem codebase is structured, the patterns used, and how the different pieces fit together.

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
