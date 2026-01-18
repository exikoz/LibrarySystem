using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace LibrarySystem.Services
{
    public class BookService : IBookService
    {
        private readonly LibrarySystemDbContext _context;

        public BookService()
        {
            _context = new LibrarySystemDbContext();
        }

        public void AddBook(string title, string isbn, int year, string authorName)
        {
            // 1. Find or Create Author
            var author = _context.Authors.FirstOrDefault(a => a.Name == authorName);
            if (author == null)
            {
                author = new Author { Name = authorName };
                _context.Authors.Add(author);
                // We don't save yet, we can save everything at once when adding the book
            }

            // 2. Create Book
            var book = new Book
            {
                Title = title,
                Isbn = isbn,
                PublishedYear = year,
                Author = author // EF Core will handle the relationship (and Author creation if new)
            };

            _context.Books.Add(book);
            _context.SaveChanges();

            Console.WriteLine($"Successfully added '{title}' by {authorName}.");
            
            // Optional: Add a copy automatically? The requirements say "Registrera nya böcker", usually implies adding the title to the catalog.
            // Managing copies might be a separate step or part of this. Let's keep it simple for now: Catalog entry.
         }

        public void SearchBooks(string searchTerm)
        {
            var books = _context.Books
                .Include(b => b.Author)
                .Where(b => b.Title.Contains(searchTerm) || b.Author.Name.Contains(searchTerm) || b.Isbn.Contains(searchTerm))
                .ToList();

            if (books.Any())
            {
                Console.WriteLine($"\nFound {books.Count} book(s):");
                foreach (var book in books)
                {
                    Console.WriteLine($"- ID: {book.Id} | {book.Title} ({book.PublishedYear}) by {book.Author.Name} [ISBN: {book.Isbn}]");
                }
            }
            else
            {
                Console.WriteLine("\nNo books found matching your search.");
            }
        }

        public void ListBookCopies(int bookId)
        {
            var copies = _context.BookCopies
                .Where(bc => bc.BookId == bookId)
                .ToList();

            if (copies.Any())
            {
                Console.WriteLine($"\nCopies for Book ID {bookId}:");
                foreach (var copy in copies)
                {
                    string status = copy.IsAvailAble ? "Available" : "Loaned Out";
                    Console.WriteLine($"- Copy ID: {copy.Id} | Status: {status}");
                }
            }
            else
            {
                Console.WriteLine("No copies found for this book.");
            }
        }
    }
}
