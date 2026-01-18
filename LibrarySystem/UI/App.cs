using LibrarySystem.Data;
using LibrarySystem.Services;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.UI
{
    public class App
    {
        private readonly IBookService _bookService;
        private readonly IMemberService _memberService;
        private readonly ILoanService _loanService;

        public App(IBookService bookService, IMemberService memberService, ILoanService loanService)
        {
            _bookService = bookService;
            _memberService = memberService;
            _loanService = loanService;
        }

        public void Run()
        {
            while (true)
            {
                // ... (existing menu code) ...
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("      LIBRARY SYSTEM                    ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Manage Books");
                Console.WriteLine("2. Manage Members");
                Console.WriteLine("3. Loan Management");
                Console.WriteLine("4. Advanced (VG Features)");
                Console.WriteLine("0. Exit");
                Console.WriteLine("========================================");
                Console.Write("Select an option: ");

                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ManageBooks();
                        break;
                    case "2":
                        ManageMembers();
                        break;
                    case "3":
                        ManageLoans();
                        break;
                    case "4":
                        ShowAdvancedMenu();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Press any key...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void ManageBooks()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== MANAGE BOOKS ===");
                Console.WriteLine("1. Add New Book");
                Console.WriteLine("2. Search Books");
                Console.WriteLine("0. Back to Main Menu");
                Console.Write("Select: ");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        AddNewBook();
                        break;
                    case "2":
                        SearchBooks();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid selection.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void AddNewBook()
        {
            Console.WriteLine("\n--- Add New Book ---");
            Console.Write("Title: ");
            var title = Console.ReadLine();

            Console.Write("Author Name: ");
            var author = Console.ReadLine();

            Console.Write("ISBN: ");
            var isbn = Console.ReadLine();

            Console.Write("Published Year: ");
            if (int.TryParse(Console.ReadLine(), out int year))
            {
                _bookService.AddBook(title, isbn, year, author);
            }
            else
            {
                Console.WriteLine("Invalid year.");
            }
            Console.WriteLine("Press key to continue...");
            Console.ReadKey();
        }

        private void SearchBooks()
        {
            Console.WriteLine("\n--- Search Books ---");
            Console.Write("Enter search term (Title/Author/ISBN): ");
            var term = Console.ReadLine();
            
            _bookService.SearchBooks(term);
            
            Console.WriteLine("\nPress key to continue...");
            Console.ReadKey();
        }

        private void ManageMembers()
        {
            // memberService is now injected via constructor as _memberService

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== MANAGE MEMBERS ===");
                Console.WriteLine("1. Register New Member");
                Console.WriteLine("2. List All Members");
                Console.WriteLine("0. Back to Main Menu");
                Console.Write("Select: ");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        Console.WriteLine("\n--- Register Member ---");
                        Console.Write("First Name: ");
                        var fName = Console.ReadLine();
                        Console.Write("Last Name: ");
                        var lName = Console.ReadLine();
                        Console.Write("Email: ");
                        var email = Console.ReadLine();
                        Console.Write("Member Number (e.g., MEM-2024-X): ");
                        var memNum = Console.ReadLine();

                        _memberService.RegisterMember(fName, lName, email, memNum);
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case "2":
                        _memberService.ListMembers();
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid selection.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void ManageLoans()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== LOAN MANAGEMENT ===");
                Console.WriteLine("1. Register New Loan (SP)");
                Console.WriteLine("2. Return Book (Trigger)");
                Console.WriteLine("3. View Active Loans (View)");
                Console.WriteLine("0. Back to Main Menu");
                Console.Write("Select: ");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        RegisterLoan();
                        break;
                    case "2":
                        ReturnBook();
                        break;
                    case "3":
                        _loanService.ListActiveLoans();
                        Console.WriteLine("\nPress any key to continue...");
                        Console.ReadKey();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid selection.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void RegisterLoan()
        {
            Console.WriteLine("\n--- Register Loan ---");
            Console.Write("Member ID: ");
            if (!int.TryParse(Console.ReadLine(), out int memberId))
            {
                Console.WriteLine("Invalid Member ID.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("(Tip: Use 'Manage Books > Search' to find Book ID first)");
            Console.Write("Book ID to see copies: ");
            if (int.TryParse(Console.ReadLine(), out int bookId))
            {
                _bookService.ListBookCopies(bookId);
                
                Console.Write("Enter Copy ID to Loan: ");
                if (int.TryParse(Console.ReadLine(), out int copyId))
                {
                    _loanService.LoanBook(memberId, copyId);
                }
            }

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void ReturnBook()
        {
            Console.WriteLine("\n--- Return Book ---");
            Console.Write("Enter Loan ID to return: ");
            if (int.TryParse(Console.ReadLine(), out int loanId))
            {
                _loanService.ReturnBook(loanId);
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private void ShowAdvancedMenu()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== ADVANCED FEATURES (VG) ===");
                Console.WriteLine("1. Simulate Concurrency Conflict (RowVersion)");
                Console.WriteLine("0. Back");
                Console.Write("Select: ");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        SimulateConcurrency();
                        break;
                    case "0":
                        return;
                    default:
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void SimulateConcurrency()
        {
            Console.WriteLine("\n--- Concurrency Simulation ---");
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
                // ex.Entries would presumably contain the object in conflict
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }
    }
}
