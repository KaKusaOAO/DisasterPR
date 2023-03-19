using Microsoft.AspNetCore;

namespace DisasterPR.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        BuildWebHost(args).Build().Run();
    }

    private static IWebHostBuilder BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}