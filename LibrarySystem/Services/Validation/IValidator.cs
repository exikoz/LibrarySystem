
namespace LibrarySystem.Services.Validation
{
    public interface IValidator
    {
        bool IsValidEmail(string email);
        bool IsValidMemberNumber(string number);
        bool IsValidISBN(string isbn);
        bool IsValidYear(int year);
    }
}
