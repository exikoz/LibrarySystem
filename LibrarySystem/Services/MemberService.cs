using LibrarySystem.Data;
using LibrarySystem.Models;
using System;
using System.Linq;

namespace LibrarySystem.Services
{
    public class MemberService : IMemberService
    {
        private readonly LibrarySystemDbContext _context;

        public MemberService()
        {
            _context = new LibrarySystemDbContext();
        }

        public void RegisterMember(string firstName, string lastName, string email, string memberNumber)
        {
            // 1. Validate Uniqueness
            if (_context.Members.Any(m => m.Email == email))
            {
                Console.WriteLine($"Error: A member with email '{email}' already exists.");
                return;
            }

            if (_context.Members.Any(m => m.MemberNumber == memberNumber))
            {
                Console.WriteLine($"Error: Member number '{memberNumber}' is already taken.");
                return;
            }

            // 2. Create Member
            var member = new Member
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                MemberNumber = memberNumber,
                CreatedAt = DateTime.Now
            };

            _context.Members.Add(member);
            _context.SaveChanges();

            Console.WriteLine($"Successfully registered member: {firstName} {lastName} ({memberNumber})");
        }

        public void ListMembers()
        {
            var members = _context.Members.ToList();
            if (members.Any())
            {
                Console.WriteLine("\n--- Current Members ---");
                foreach (var m in members)
                {
                    Console.WriteLine($"ID: {m.Id} | {m.FirstName} {m.LastName} | {m.Email} | No: {m.MemberNumber}");
                }
            }
            else
            {
                Console.WriteLine("No members found.");
            }
        }
    }
}
