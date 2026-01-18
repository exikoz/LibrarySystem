using System;

namespace LibrarySystem.Services
{
    public interface IBookService
    {
        void AddBook(string title, string isbn, int year, string authorName);
        void SearchBooks(string searchTerm);
        void ListBookCopies(int bookId);
    }
}
