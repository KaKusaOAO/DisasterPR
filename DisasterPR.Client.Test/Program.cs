using DisasterPR.Client;
using KaLib.Utils;

public static class Program
{
    public static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Logger.Error(e.ExceptionObject.ToString());
        };

        Logger.Level = LogLevel.Verbose;
        Logger.Logged += Logger.LogToEmulatedTerminalAsync;
        Logger.RunThreaded();

        var game = Game.Instance;
        game.Init(new GameOptions
        {
            PlayerName = "Test"
        });

        try
        {
            await game.LoginPlayerAsync();
            await game.HostRoomAsync();
            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
        }
    }
}
