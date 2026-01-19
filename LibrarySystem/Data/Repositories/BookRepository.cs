using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;
using LibrarySystem.Data.Repositories.Interfaces;

namespace LibrarySystem.Data.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly LibrarySystemDbContext _context;

        public BookRepository(LibrarySystemDbContext context)
        {
            _context = context;
        }

        public Author GetAuthorByName(string name)
        {
            return _context.Authors.FirstOrDefault(a => a.Name == name);
        }

        public void AddAuthor(Author author)
        {
            _context.Authors.Add(author);
        }

        public void AddBook(Book book)
        {
            _context.Books.Add(book);
        }

        public List<Book> Search(string term)
        {
            return _context.Books
                .Include(b => b.Author)
                .Where(b => b.Title.Contains(term) || b.Author.Name.Contains(term) || b.Isbn.Contains(term))
                .ToList();
        }

        public List<BookCopy> GetCopies(int bookId)
        {
            return _context.BookCopies
                .Where(bc => bc.BookId == bookId)
                .ToList();
        }

        public BookCopy GetCopyById(int copyId)
        {
             return _context.BookCopies.Find(copyId);
        }

        public void UpdateCopy(BookCopy copy)
        {
            _context.BookCopies.Update(copy);
        }



        public void AddCopy(BookCopy copy)
        {
            _context.BookCopies.Add(copy);
        }

        public Book? GetBookById(int id)
        {
            return _context.Books.Find(id);
        }

        public List<Book> GetAll()
        {
            return _context.Books
                .Include(b => b.Author)
                .Include(b => b.BookCopies)
                .ToList();
        }
    }
}
