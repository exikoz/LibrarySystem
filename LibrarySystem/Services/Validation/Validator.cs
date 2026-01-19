using System.Text.RegularExpressions;

namespace LibrarySystem.Services.Validation
{
    public class Validator : IValidator
    {
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            // Simple regex for demonstration
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public bool IsValidMemberNumber(string number)
        {
            return !string.IsNullOrWhiteSpace(number);
        }

        public bool IsValidISBN(string isbn)
        {
            // Simple check
            return !string.IsNullOrWhiteSpace(isbn) && (isbn.Length == 10 || isbn.Length == 13);
        }

        public bool IsValidYear(int year)
        {
            return year > 1000 && year <= System.DateTime.Now.Year + 1;
        }
    }
}
