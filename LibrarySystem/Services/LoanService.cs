using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Services.Interfaces;
using LibrarySystem.Data.Repositories.Interfaces;

namespace LibrarySystem.Services
{
    public class LoanService : ILoanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LoanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void LoanBook(int memberId, int bookCopyId)
        {
            try
            {
                // VG Requirement: Use Stored Procedure 'sp_RegisterLoan'
                // This SP handles validation and transaction internally.
                // UoW isn't strictly needed for raw SP calls unless wrapped in transaction scope,
                // but we use access via UoW for consistency.
                _unitOfWork.Loans.RegisterLoan(memberId, bookCopyId);
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
            var loan = _unitOfWork.Loans.GetById(loanId);
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

            // Normal Update
            loan.ReturnDate = DateTime.Now;
            _unitOfWork.Loans.Update(loan);
            
            try
            {
                _unitOfWork.Complete();
                // VG Requirement: The Trigger 'trg_UpdateAvailability' will automatically 
                // set BookCopies.IsAvailable = 1 in the database.
                Console.WriteLine("Book returned successfully. (Trigger should have updated stock).");
            }
            catch (DbUpdateConcurrencyException)
            {
                Console.WriteLine("Error: Concurrency conflict detected during return.");
            }
        }

        public IEnumerable<ActiveLoanDto> ListActiveLoans()
        {
            // VG Requirement: Use View 'v_ActiveLoans'
            try
            {
                return _unitOfWork.Loans.GetActiveLoans();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading view: {ex.Message}");
                return Enumerable.Empty<ActiveLoanDto>();
            }
        }
    }
}
