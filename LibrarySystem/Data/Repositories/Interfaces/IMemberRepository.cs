using LibrarySystem.Models;
using System.Collections.Generic;

namespace LibrarySystem.Data.Repositories.Interfaces
{
    public interface IMemberRepository
    {
        bool EmailExists(string email);
        bool MemberNumberExists(string number);
        void Add(Member member);
        List<Member> GetAll();
        Member GetById(int id);

    }
}
