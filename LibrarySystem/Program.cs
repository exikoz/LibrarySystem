using LibrarySystem.UI;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 1. Configure Services
            var serviceProvider = ApplicationSetup.ConfigureServices();

            // 2. Run App
            var app = serviceProvider.GetRequiredService<App>();
            app.Run();
        }
    }
}
