using System;
using System.Threading.Tasks;
using LibrarySystem.Data.Repositories.Interfaces;

namespace LibrarySystem.Data.Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IBookRepository Books { get; }
        IMemberRepository Members { get; }
        ILoanRepository Loans { get; }
        
        int Complete();
        Task WarmUpAsync();
    }
}
