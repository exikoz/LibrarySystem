using LibrarySystem.Data;
using LibrarySystem.UI.Helpers;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Services.Interfaces;
using LibrarySystem.Services.Validation;
using LibrarySystem.Data.Repositories.Interfaces;

namespace LibrarySystem.UI
{
    public class App
    {
        private readonly IBookService _bookService;
        private readonly IMemberService _memberService;
        private readonly ILoanService _loanService;
        private readonly IUnitOfWork _unitOfWork; // Added for WarmUp
        private readonly IValidator _validator;

        public App(IBookService bookService, IMemberService memberService, ILoanService loanService, IUnitOfWork unitOfWork, IValidator validator)
        {
            _bookService = bookService;
            _memberService = memberService;
            _loanService = loanService;
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public void Run()
        {
            // Warm-up Screen
            ConsoleHelper.ShowSpinner("Initializing System...", async () => await _unitOfWork.WarmUpAsync());

            while (true)
            {
                var menuOptions = new List<MenuOption>
                {
                    new MenuOption("Manage Books", ManageBooks),
                    new MenuOption("Manage Members", ManageMembers),
                    new MenuOption("Loan Management", ManageLoans),
                    new MenuOption("Advanced (VG Features)", ShowAdvancedMenu),
                    new MenuOption("Exit", () => Environment.Exit(0))
                };

                var selectedOption = ConsoleHelper.SelectFromList(menuOptions, m => m.Description, "LIBRARY SYSTEM");
                selectedOption?.Action();
            }
        }

        private void ManageBooks()
        {
            while (true)
            {
                var options = new List<MenuOption>
                {
                    new MenuOption("Add New Book", AddNewBook),
                    new MenuOption("Search Books", SearchBooks),
                    new MenuOption("List All Books", ListAllBooks),
                    new MenuOption("Add Copies to Existing Book", AddCopies),
                    new MenuOption("Back to Main Menu", () => { })
                };

                var selected = ConsoleHelper.SelectFromList(options, o => o.Description, "MANAGE BOOKS");
                if (selected == null || selected.Description == "Back to Main Menu") return;
                
                selected.Action();
            }
        }

        private void AddNewBook()
        {
            ConsoleHelper.WriteSimpleHeader("Add New Book");
            var title = ConsoleHelper.PromptUntilValid("Title", s => !string.IsNullOrWhiteSpace(s), "Required.");
            var author = ConsoleHelper.PromptUntilValid("Author Name", s => !string.IsNullOrWhiteSpace(s), "Required.");
            var isbn = ConsoleHelper.PromptUntilValid("ISBN", _validator.IsValidISBN, "Invalid ISBN.");

            Console.Write("Published Year: ");
            if (int.TryParse(Console.ReadLine(), out int year) && _validator.IsValidYear(year))
            {
                Console.Write("Copies (default 1): ");
                if (!int.TryParse(Console.ReadLine(), out int copies) || copies < 1) copies = 1;

                _bookService.AddBook(title, isbn, year, author, copies);
            }
            else
            {
                ConsoleHelper.WriteError("Invalid year.");
            }
            ConsoleHelper.PauseWithMessage();
        }

        private void SearchBooks()
        {
            ConsoleHelper.WriteSimpleHeader("Search Books");
            var term = ConsoleHelper.Prompt("Enter search term");
            
            var books = _bookService.SearchBooks(term);
            if (books.Any())
            {
                TableFormatter.PrintRow("ID", "Title", "Author", "Year", "ISBN");
                TableFormatter.PrintLine();
                foreach (var b in books)
                {
                    TableFormatter.PrintRow(b.Id.ToString(), b.Title, b.Author.Name, b.PublishedYear.ToString(), b.Isbn);
                }
            }
            else
            {
                ConsoleHelper.WriteInfo("No books found.");
            }
            
            ConsoleHelper.PauseWithMessage();
        }

        private void ListAllBooks()
        {
            ConsoleHelper.WriteSimpleHeader("All Books");
             var books = _bookService.GetAllBooks();
            if (books.Any())
            {
                TableFormatter.PrintRow("ID", "Title", "Author", "Total", "Loaned");
                TableFormatter.PrintLine();
                foreach (var b in books)
                {
                    var total = b.BookCopies.Count;
                    var loaned = b.BookCopies.Count(c => !c.IsAvailAble);
                    TableFormatter.PrintRow(b.Id.ToString(), b.Title, b.Author.Name, total.ToString(), loaned.ToString());
                }
            }
            else
            {
                ConsoleHelper.WriteInfo("No books found.");
            }
            ConsoleHelper.PauseWithMessage();
        }

        private void AddCopies()
        {
            var books = _bookService.GetAllBooks().ToList();
            if (!books.Any()) 
            {
                ConsoleHelper.WriteError("No books found.");
                ConsoleHelper.PauseWithMessage();
                return;
            }

            var selectedBook = ConsoleHelper.SelectFromList(books, b => $"{b.Title} (ID: {b.Id})", "Select Book to Add Copies");
            if (selectedBook == null) return;

            ConsoleHelper.WriteSimpleHeader($"Add Copies: {selectedBook.Title}");
            Console.Write("Number of copies to add: ");
            if (int.TryParse(Console.ReadLine(), out int count) && count > 0)
            {
                _bookService.AddCopies(selectedBook.Id, count);
            }
            else
            {
                ConsoleHelper.WriteError("Invalid quantity.");
            }
            ConsoleHelper.PauseWithMessage();
        }

        private void ManageMembers()
        {
            while (true)
            {
                var options = new List<MenuOption>
                {
                    new MenuOption("Register New Member", () => 
                    {
                        ConsoleHelper.WriteSimpleHeader("Register Member");
                        var fName = ConsoleHelper.PromptUntilValid("First Name", s => !string.IsNullOrWhiteSpace(s), "Required.");
                        var lName = ConsoleHelper.PromptUntilValid("Last Name", s => !string.IsNullOrWhiteSpace(s), "Required.");
                        var email = ConsoleHelper.PromptUntilValid("Email", _validator.IsValidEmail, "Invalid Email.");
                        
                        _memberService.RegisterMember(fName, lName, email);
                        ConsoleHelper.PauseWithMessage();
                    }),
                    new MenuOption("List All Members", () => 
                    {
                        var members = _memberService.ListMembers();
                        if (members.Any())
                        {
                            TableFormatter.PrintRow("ID", "Name", "Email", "Member No");
                            TableFormatter.PrintLine();
                            foreach (var m in members)
                            {
                                TableFormatter.PrintRow(m.Id.ToString(), $"{m.FirstName} {m.LastName}", m.Email, m.MemberNumber);
                            }
                        }
                        else
                        {
                            ConsoleHelper.WriteInfo("No members found.");
                        }
                        ConsoleHelper.PauseWithMessage();
                    }),
                    new MenuOption("Back to Main Menu", () => { })
                };

                var selected = ConsoleHelper.SelectFromList(options, o => o.Description, "MANAGE MEMBERS");
                if (selected == null || selected.Description == "Back to Main Menu") return;

                selected.Action();
            }
        }

        private void ManageLoans()
        {
            while (true)
            {
                var options = new List<MenuOption>
                {
                    new MenuOption("Register New Loan (SP)", RegisterLoan),
                    new MenuOption("Return Book (Trigger)", ReturnBook),
                    new MenuOption("View Active Loans (View)", () => 
                    {
                        var loans = _loanService.ListActiveLoans();
                        if (loans.Any()) 
                        {
                             TableFormatter.PrintRow("LoanID", "Title", "Copy", "Member", "Due Date");
                             TableFormatter.PrintLine();
                             foreach(var l in loans)
                             {
                                 TableFormatter.PrintRow(l.LoanId.ToString(), l.Title, l.CopyId.ToString(), $"{l.MemberName} ({l.MemberNumber})", l.DueDate.HasValue ? l.DueDate.Value.ToString("yyyy-MM-dd") : "-");
                             }
                        }
                        else
                        {
                            ConsoleHelper.WriteInfo("No active loans.");
                        }
                        ConsoleHelper.PauseWithMessage();
                    }),
                    new MenuOption("Back to Main Menu", () => { })
                };

                var selected = ConsoleHelper.SelectFromList(options, o => o.Description, "LOAN MANAGEMENT");
                if (selected == null || selected.Description == "Back to Main Menu") return;

                selected.Action();
            }
        }

        private void RegisterLoan()
        {
            // 1. Select Member
            var members = _memberService.ListMembers().ToList();
            if (!members.Any()) { ConsoleHelper.WriteError("No members found."); ConsoleHelper.PauseWithMessage(); return; }

            var selectedMember = ConsoleHelper.SelectFromList(members, m => $"{m.FirstName} {m.LastName} ({m.MemberNumber})", "Select Member");
            if (selectedMember == null) return;

            // 2. Select Book
            var books = _bookService.GetAllBooks().ToList();
            if (!books.Any()) { ConsoleHelper.WriteError("No books found."); ConsoleHelper.PauseWithMessage(); return; }

            var selectedBook = ConsoleHelper.SelectFromList(books, b => $"{b.Title} (Avail: {b.BookCopies.Count(c => c.IsAvailAble)})", "Select Book");
            if (selectedBook == null) return;

            // 3. Select Copy
            var copies = _bookService.GetCopies(selectedBook.Id).Where(c => c.IsAvailAble).ToList();
            if (!copies.Any())
            {
                ConsoleHelper.WriteError("No available copies for this book.");
                ConsoleHelper.PauseWithMessage();
                return;
            }

            var selectedCopy = ConsoleHelper.SelectFromList(copies, c => $"Copy ID: {c.Id} (Available)", "Select Copy to Loan");
            if (selectedCopy == null) return;

            _loanService.LoanBook(selectedMember.Id, selectedCopy.Id);
            ConsoleHelper.PauseWithMessage();
        }

        private void ReturnBook()
        {
            var loans = _loanService.ListActiveLoans().ToList();
            if (!loans.Any())
            {
                ConsoleHelper.WriteInfo("No active loans to return.");
                ConsoleHelper.PauseWithMessage();
                return;
            }

            var selectedLoan = ConsoleHelper.SelectFromList(loans, l => $"Loan {l.LoanId}: {l.Title} (Member: {l.MemberName}) - Due: {l.DueDate:yyyy-MM-dd}", "Select Loan into Return");
            if (selectedLoan == null) return;

            _loanService.ReturnBook(selectedLoan.LoanId);
            ConsoleHelper.PauseWithMessage();
        }

        private void ShowAdvancedMenu()
        {
            while (true)
            {
                var options = new List<MenuOption>
                {
                    new MenuOption("Simulate Concurrency Conflict (Optimistic - RowVersion)", SimulateConcurrency),
                    new MenuOption("Simulate Locking/Blocking (Pessimistic - Serializable)", SimulatePessimisticConcurrency),
                    new MenuOption("Back", () => { })
                };

                var selected = ConsoleHelper.SelectFromList(options, o => o.Description, "ADVANCED FEATURES (VG)");
                if (selected == null || selected.Description == "Back") return;

                selected.Action();
            }
        }

        // Method 1: Optimistic (RowVersion)
        private void SimulateConcurrency()
        {
            Console.WriteLine("\n--- Concurrency Simulation (Optimistic) ---");
            Console.WriteLine("This test demonstrates Optimistic Concurrency using RowVersion.");
            Console.WriteLine("We will simulate two users (User A and User B) trying to modify the SAME book copy simultaneously.");

            Console.Write("Enter Book Copy ID to test (e.g., from 'Active Loans' or 'Search'): ");
            if (!int.TryParse(Console.ReadLine(), out int copyId))
            {
                Console.WriteLine("Invalid ID.");
                Console.ReadKey();
                return;
            }

            try
            {
                // User A retrieves the record
                using var contextA = new LibrarySystemDbContext();
                var copyA = contextA.BookCopies.Find(copyId);
                if (copyA == null) { Console.WriteLine("Copy not found."); return; }
                
                Console.WriteLine($"[User A] retrieved copy {copyId}. Availability: {copyA.IsAvailAble}");

                // User B retrieves the SAME record
                using var contextB = new LibrarySystemDbContext();
                var copyB = contextB.BookCopies.Find(copyId);
                Console.WriteLine($"[User B] retrieved copy {copyId}. Availability: {copyB.IsAvailAble}");

                // User A modifies it
                copyA.IsAvailAble = !copyA.IsAvailAble; // Flip status
                Console.WriteLine("[User A] modifies availability...");
                
                Thread.Sleep(1000); // Simulate think time
                
                contextA.SaveChanges();
                Console.WriteLine("[User A] saved changes successfully!");

                // User B modifies the OLD version they have
                copyB.IsAvailAble = !copyB.IsAvailAble; // Flip status
                Console.WriteLine("[User B] tries to modify availability...");
                
                contextB.SaveChanges(); // This should FAIL
                Console.WriteLine("[User B] Saved changes (Unexpected code path).");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine("\n[SUCCESS] Concurrency Conflict Detected!");
                Console.WriteLine("System blocked User B's save because the data was modified by User A in the meantime.");
                Console.WriteLine("This proves RowVersion is working.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        // Method 2: Pessimistic (Isolation Levels)
        private void SimulatePessimisticConcurrency()
        {
            Console.WriteLine("\n--- Concurrency Simulation (Pessimistic) ---");
            Console.WriteLine("This test demonstrates Isolation Levels (Serializable).");
            Console.WriteLine("User A will lock a record. User B will try to access it and should be BLOCKED (waited).");
            
            Console.Write("Enter Book Copy ID to test: ");
            if (!int.TryParse(Console.ReadLine(), out int copyId)) return;

            // Using Tasks to run in parallel threads to simulate real timing
            var taskA = Task.Run(() => 
            {
                using var contextA = new LibrarySystemDbContext();
                using var transaction = contextA.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
                
                Console.WriteLine("[User A] Transaction Started. Reading record...");
                var copyA = contextA.BookCopies.Find(copyId); // Locks the row range
                
                Console.WriteLine("[User A] Record locked. Simulating long work (5 seconds)...");
                Thread.Sleep(5000); 
                
                // copyA.IsAvailAble = !copyA.IsAvailAble; 
                // contextA.SaveChanges();
                
                Console.WriteLine("[User A] Committing transaction...");
                transaction.Commit();
                Console.WriteLine("[User A] Done.");
            });

            // Give User A time to lock
            Thread.Sleep(1000);

            var taskB = Task.Run(() => 
            {
                Console.WriteLine("[User B] Requesting same record... (Should wait)");
                var sw = System.Diagnostics.Stopwatch.StartNew();
                
                using var contextB = new LibrarySystemDbContext();
                using var transaction = contextB.Database.BeginTransaction(System.Data.IsolationLevel.Serializable);
                
                var copyB = contextB.BookCopies.Find(copyId); // This should BLOCK until A releases
                
                sw.Stop();
                Console.WriteLine($"[User B] Finally got access! Waited for {sw.Elapsed.TotalSeconds:F1} seconds.");
            });

            Task.WaitAll(taskA, taskB);
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

    }
}
