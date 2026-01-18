using System;

namespace LibrarySystem.Services
{
    public interface IMemberService
    {
        void RegisterMember(string firstName, string lastName, string email, string memberNumber);
        void ListMembers();
    }
}
