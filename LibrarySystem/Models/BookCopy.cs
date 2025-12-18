using System;
using System.Collections.Generic;

namespace LibrarySystem.Models;

public partial class BookCopy
{
    public int Id { get; set; }

    public int BookId { get; set; }

    public bool IsAvailAble { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual Book Book { get; set; } = null!;

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
