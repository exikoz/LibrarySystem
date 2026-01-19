using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

using System.Linq;
using LibrarySystem.Data.Repositories.Interfaces;

namespace LibrarySystem.Data.Repositories
{
    public class LoanRepository : ILoanRepository
    {
        private readonly LibrarySystemDbContext _context;

        public LoanRepository(LibrarySystemDbContext context)
        {
            _context = context;
        }

        public void RegisterLoan(int memberId, int bookCopyId)
        {
             _context.Database.ExecuteSqlRaw("EXEC sp_RegisterLoan @p0, @p1", memberId, bookCopyId);
        }

        public Loan GetById(int id)
        {
            return _context.Loans.Find(id);
        }

        public void Update(Loan loan)
        {
            _context.Loans.Update(loan);
        }

        public List<ActiveLoanDto> GetActiveLoans()
        {
             return _context.Database.SqlQuery<ActiveLoanDto>($"SELECT * FROM v_ActiveLoans").ToList();
        }


    }
}
