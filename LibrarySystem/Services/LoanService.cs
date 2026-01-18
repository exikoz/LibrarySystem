using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace LibrarySystem.Services
{
    public class LoanService : ILoanService
    {
        private readonly LibrarySystemDbContext _context;

        public LoanService(LibrarySystemDbContext context)
        {
            _context = context;
        }

        public void LoanBook(int memberId, int bookCopyId)
        {
            try
            {
                // VG Requirement: Use Stored Procedure 'sp_RegisterLoan'
                // This SP handles validation and transaction internally.
                _context.Database.ExecuteSqlRaw("EXEC sp_RegisterLoan @p0, @p1", memberId, bookCopyId);
                Console.WriteLine("Loan registered successfully via Stored Procedure.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error registering loan: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Details: {ex.InnerException.Message}");
                }
            }
        }

        public void ReturnBook(int loanId)
        {
            var loan = _context.Loans.Find(loanId);
            if (loan == null)
            {
                Console.WriteLine("Error: Loan not found.");
                return;
            }

            if (loan.ReturnDate != null)
            {
                Console.WriteLine("Error: This book is already returned.");
                return;
            }

            // Normal EF Update
            loan.ReturnDate = DateTime.Now;
            _context.SaveChanges();
            
            // VG Requirement: The Trigger 'trg_UpdateAvailability' will automatically 
            // set BookCopies.IsAvailable = 1 in the database.
            // We can check it manually or trust the trigger (verified in task list).
            Console.WriteLine("Book returned successfully. (Trigger should have updated stock).");
        }

        public void ListActiveLoans()
        {
            // VG Requirement: Use View 'v_ActiveLoans'
            try
            {
                // Mapping raw SQL to DTO since the View isn't in the DbSet
                var activeLoans = _context.Database.SqlQuery<ActiveLoanDto>($"SELECT * FROM v_ActiveLoans").ToList();

                if (activeLoans.Any())
                {
                    Console.WriteLine("\n--- Active Loans (from v_ActiveLoans) ---");
                    foreach (var l in activeLoans)
                    {
                        Console.WriteLine($"LoanID: {l.LoanId} | {l.Title} (Copy: {l.CopyId})");
                        Console.WriteLine($"   Member: {l.MemberName} ({l.MemberNumber})");
                        Console.WriteLine($"   Due: {l.DueDate:yyyy-MM-dd}");
                        Console.WriteLine("-----------------------------------------");
                    }
                }
                else
                {
                    Console.WriteLine("No active loans found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading view: {ex.Message}");
            }
        }
    }
}
