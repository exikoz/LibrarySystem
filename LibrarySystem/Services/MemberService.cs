using LibrarySystem.Models;
using LibrarySystem.Services.Validation;
using LibrarySystem.Services.Interfaces;
using LibrarySystem.Data.Repositories.Interfaces;

namespace LibrarySystem.Services
{
    public class MemberService : IMemberService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidator _validator;

        public MemberService(IUnitOfWork unitOfWork, IValidator validator)
        {
            _unitOfWork = unitOfWork;
            _validator = validator;
        }

        public void RegisterMember(string firstName, string lastName, string email)
        {
            if (!_validator.IsValidEmail(email)) throw new ArgumentException("Invalid Email.");

            // 1. Validate Uniqueness
            if (_unitOfWork.Members.EmailExists(email))
            {
                Console.WriteLine($"Error: A member with email '{email}' already exists.");
                return;
            }

            // 2. Auto-Generate Member Number
            string memberNumber;
            do
            {
                memberNumber = $"MEM-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
            } while (_unitOfWork.Members.MemberNumberExists(memberNumber));
            
            // 3. Create Member
            var member = new Member
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                MemberNumber = memberNumber,
                CreatedAt = DateTime.Now
            };

            _unitOfWork.Members.Add(member);
            _unitOfWork.Complete();

            Console.WriteLine($"Successfully registered member: {firstName} {lastName} ({memberNumber})");
        }

        public IEnumerable<Member> ListMembers()
        {
            return _unitOfWork.Members.GetAll();
        }
    }
}
