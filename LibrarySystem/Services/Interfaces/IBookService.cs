using LibrarySystem.Models;

namespace LibrarySystem.Services.Interfaces
{
    public interface IBookService
    {
        void AddBook(string title, string isbn, int year, string authorName, int copies = 1);
        // Returns data for UI to display
        IEnumerable<Book> SearchBooks(string searchTerm);
        IEnumerable<BookCopy> GetCopies(int bookId);
        IEnumerable<Book> GetAllBooks();

        // Actions

        void AddCopies(int bookId, int count);
    }
}
