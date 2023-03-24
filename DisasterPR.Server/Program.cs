using KaLib.Utils;
using Microsoft.AspNetCore;
using LogLevel = KaLib.Utils.LogLevel;

namespace DisasterPR.Server;

public static class Program
{
    public static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Logger.Warn(e.ExceptionObject.ToString());
        };
        
        Logger.Level = LogLevel.Verbose;
        Logger.Logged += Logger.LogToEmulatedTerminalAsync;
        Logger.RunThreaded();

        _ = new GameServer();
        BuildWebHost(args).Build().Run();
    }

    private static IWebHostBuilder BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>();
}