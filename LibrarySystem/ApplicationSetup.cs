using LibrarySystem.UI;
using LibrarySystem.Data;
using LibrarySystem.Services;
using LibrarySystem.Data.Repositories;
using LibrarySystem.Services.Interfaces;
using LibrarySystem.Services.Validation;
using Microsoft.Extensions.DependencyInjection;
using LibrarySystem.Data.Repositories.Interfaces;

namespace LibrarySystem
{
    public static class ApplicationSetup
    {
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // 1. Database
            services.AddDbContext<LibrarySystemDbContext>();

            // 2. Repositories
            services.AddScoped<IBookRepository, BookRepository>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<ILoanRepository, LoanRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 3. Services
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<ILoanService, LoanService>();
            services.AddScoped<IValidator, Validator>();

            // 4. App Entry Point
            services.AddSingleton<App>();

            // 5. Build and Return Provider
            return services.BuildServiceProvider();
        }
    }
}
