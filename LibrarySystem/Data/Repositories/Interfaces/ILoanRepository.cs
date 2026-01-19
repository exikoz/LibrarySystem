using LibrarySystem.Models;
using System.Collections.Generic;

namespace LibrarySystem.Data.Repositories.Interfaces
{
    public interface ILoanRepository
    {
        void RegisterLoan(int memberId, int bookCopyId);
        Loan GetById(int id);
        void Update(Loan loan);
        List<ActiveLoanDto> GetActiveLoans();

    }
}
