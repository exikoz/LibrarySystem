using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data.Repositories.Interfaces;

namespace LibrarySystem.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LibrarySystemDbContext _context;

        public IBookRepository Books { get; private set; }
        public IMemberRepository Members { get; private set; }
        public ILoanRepository Loans { get; private set; }

        public UnitOfWork(LibrarySystemDbContext context, 
            IBookRepository bookRepository, 
            IMemberRepository memberRepository, 
            ILoanRepository loanRepository)
        {
            _context = context;
            Books = bookRepository;
            Members = memberRepository;
            Loans = loanRepository;
        }

        public int Complete()
        {
            return _context.SaveChanges();
        }

        public async Task WarmUpAsync()
        {
            // Just accessing the database ensures connections are open and EF Model is compiled
            await _context.Database.CanConnectAsync();
            // Optional: Run a dummy query to warm up DbSet internal caches
            await _context.Books.FirstOrDefaultAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
