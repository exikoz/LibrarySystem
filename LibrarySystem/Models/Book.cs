using System;
using System.Collections.Generic;

namespace LibrarySystem.Models;

public partial class Book
{
    public int Id { get; set; }

    public int AuthorId { get; set; }

    public string Title { get; set; } = null!;

    public string Isbn { get; set; } = null!;

    public int PublishedYear { get; set; }

    public virtual Author Author { get; set; } = null!;

    public virtual ICollection<BookCopy> BookCopies { get; set; } = new List<BookCopy>();
}
