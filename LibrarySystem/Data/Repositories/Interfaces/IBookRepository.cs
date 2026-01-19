using LibrarySystem.Models;
using System.Collections.Generic;

namespace LibrarySystem.Data.Repositories.Interfaces
{
    public interface IBookRepository
    {
        Author GetAuthorByName(string name);
        void AddAuthor(Author author);
        void AddBook(Book book);
        Book? GetBookById(int id);
        List<Book> Search(string term);
        List<BookCopy> GetCopies(int bookId);
        BookCopy GetCopyById(int copyId); // For simulation
        void UpdateCopy(BookCopy copy); // For simulation
        void AddCopy(BookCopy copy);
        List<Book> GetAll();

    }
}
