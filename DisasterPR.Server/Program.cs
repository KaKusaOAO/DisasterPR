using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore;
using Mochi.Utils;
using LogLevel = Mochi.Utils.LogLevel;

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
            .UseStartup<Startup>()
            .ConfigureLogging((context, builder) =>
            {
                builder.AddConsole();
            });
    // .ConfigureKestrel((context, options) =>
    // {
    //     options.ConfigureEndpointDefaults(listenOptions =>
    //     {
    //         listenOptions.UseHttps(adapterOptions =>
    //         {
    //             var rsa = PemKeyUtils.GetRSAProviderFromPemFile("certificates/private.key");
    //             var cert = new X509Certificate2("certificates/certificate.crt").CopyWithPrivateKey(rsa);
    //             adapterOptions.ServerCertificate = cert;
    //
    //             var chain = new X509Certificate2Collection();
    //             chain.ImportFromPemFile("certificates/certificate.crt");
    //             adapterOptions.ServerCertificateChain = chain;
    //         });
    //
    //         listenOptions.UseConnectionLogging();
    //     });
    // });
}