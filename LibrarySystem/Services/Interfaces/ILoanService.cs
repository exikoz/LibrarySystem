using System;
using System.Collections.Generic;
using LibrarySystem.Models;

namespace LibrarySystem.Services.Interfaces
{
    public interface ILoanService
    {
        void LoanBook(int memberId, int bookCopyId);
        void ReturnBook(int loanId);
        IEnumerable<ActiveLoanDto> ListActiveLoans();
    }
}
