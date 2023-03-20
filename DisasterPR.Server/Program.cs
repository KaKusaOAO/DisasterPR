using KaLib.Utils;
using Microsoft.AspNetCore;
using LogLevel = KaLib.Utils.LogLevel;

namespace DisasterPR.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        Logger.Level = LogLevel.Verbose;
        Logger.Logged += Logger.LogToEmulatedTerminalAsync;
        Logger.RunThreaded();
        
        BuildWebHost(args).Build().Run();
    }

    private static IWebHostBuilder BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}