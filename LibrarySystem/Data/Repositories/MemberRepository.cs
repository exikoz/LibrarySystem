using LibrarySystem.Models;
using LibrarySystem.Data.Repositories.Interfaces;

namespace LibrarySystem.Data.Repositories
{
    public class MemberRepository : IMemberRepository
    {
        private readonly LibrarySystemDbContext _context;

        public MemberRepository(LibrarySystemDbContext context)
        {
            _context = context;
        }

        public bool EmailExists(string email)
        {
            return _context.Members.Any(m => m.Email == email);
        }

        public bool MemberNumberExists(string number)
        {
            return _context.Members.Any(m => m.MemberNumber == number);
        }

        public void Add(Member member)
        {
            _context.Members.Add(member);
        }

        public List<Member> GetAll()
        {
            return _context.Members.ToList();
        }

        public Member GetById(int id)
        {
            return _context.Members.Find(id);
        }


    }
}
