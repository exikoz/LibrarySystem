using System;
using System.Collections.Generic;

namespace LibrarySystem.Models;

public partial class Loan
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    public int BookCopyId { get; set; }

    public DateTime? LoanDate { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? ReturnDate { get; set; }

    public virtual BookCopy BookCopy { get; set; } = null!;

    public virtual Member Member { get; set; } = null!;
}
