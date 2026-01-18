using LibrarySystem.Data;
using LibrarySystem.Services;
using LibrarySystem.UI;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 1. Setup DI Container
            var services = new ServiceCollection();

            // 2. Register Services
            services.AddDbContext<LibrarySystemDbContext>(); // Scoped by default
            services.AddScoped<IBookService, BookService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<ILoanService, LoanService>();
            services.AddSingleton<App>(); // App entry point

            // 3. Build Provider
            var serviceProvider = services.BuildServiceProvider();

            // 4. Run App
            var app = serviceProvider.GetRequiredService<App>();
            app.Run();
        }
    }
}
