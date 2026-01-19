using LibrarySystem.Models;

namespace LibrarySystem.Services.Interfaces
{
    public interface IMemberService
    {
        void RegisterMember(string firstName, string lastName, string email); // Auto-ID
        IEnumerable<Member> ListMembers();
    }
}
