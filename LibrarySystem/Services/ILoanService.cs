using System;

namespace LibrarySystem.Services
{
    public interface ILoanService
    {
        void LoanBook(int memberId, int bookCopyId);
        void ReturnBook(int loanId);
        void ListActiveLoans();
    }
}
