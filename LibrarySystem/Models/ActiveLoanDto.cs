using System;

namespace LibrarySystem.Models
{
    // DTO for the v_ActiveLoans View
    public class ActiveLoanDto
    {
        public int LoanId { get; set; }
        public string Title { get; set; }
        public string ISBN { get; set; }
        public int CopyId { get; set; }
        public string MemberNumber { get; set; }
        public string MemberName { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
