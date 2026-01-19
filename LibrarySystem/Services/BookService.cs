using LibrarySystem.Models;
using LibrarySystem.Services.Validation;
using LibrarySystem.Services.Interfaces;
using LibrarySystem.Data.Repositories.Interfaces;

namespace LibrarySystem.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator _validator;

        public BookService(IUnitOfWork unitOfWork, IValidator validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public void AddBook(string title, string isbn, int year, string authorName, int copies = 1)
        {
            if (!_validator.IsValidISBN(isbn)) throw new ArgumentException("Invalid ISBN.");
            if (!_validator.IsValidYear(year)) throw new ArgumentException("Invalid Year.");

            // 1. Find or Create Author
            var author = _unitOfWork.Books.GetAuthorByName(authorName);
            if (author == null)
            {
                author = new Author { Name = authorName };
                _unitOfWork.Books.AddAuthor(author);
            }

            // 2. Create Book
            var book = new Book
            {
                Title = title,
                Isbn = isbn,
                PublishedYear = year,
                Author = author
            };

            _unitOfWork.Books.AddBook(book);

            // 3. Add Copies
            for (int i = 0; i < copies; i++)
            {
                _unitOfWork.Books.AddCopy(new BookCopy { Book = book, IsAvailAble = true });
            }

            _unitOfWork.Complete();

            Console.WriteLine($"Successfully added '{title}' by {authorName} with {copies} copies.");
        }

        public void AddCopies(int bookId, int count)
        {
             // Verify book exists explicitly
             var book = _unitOfWork.Books.GetBookById(bookId);
             if (book == null)
             {
                 Console.WriteLine($"Error: Book with ID {bookId} does not exist.");
                 return;
             }
             
             // Create copies
             for (int i = 0; i < count; i++)
             {
                 _unitOfWork.Books.AddCopy(new BookCopy { BookId = bookId, IsAvailAble = true });
             }
             _unitOfWork.Complete();
             Console.WriteLine($"Successfully added {count} copies to Book {bookId} ('{book.Title}').");
        }

        public IEnumerable<Book> GetAllBooks()
        {
            return _unitOfWork.Books.GetAll();
        }

        public IEnumerable<Book> SearchBooks(string searchTerm)
        {
            return _unitOfWork.Books.Search(searchTerm);
        }

        public IEnumerable<BookCopy> GetCopies(int bookId)
        {
            return _unitOfWork.Books.GetCopies(bookId);
        }
    }
}
