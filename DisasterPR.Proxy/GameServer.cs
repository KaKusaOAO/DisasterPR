using DisasterPR.Proxy.Commands;
using DisasterPR.Proxy.Commands.Senders;
using DisasterPR.Proxy.Sessions;
using Firebase.Database;
using KaLib.Brigadier;
using KaLib.Brigadier.TerminalHelper;
using KaLib.Utils;

namespace DisasterPR.Proxy;

public class GameServer
{
    private static GameServer _instance;

    public static GameServer Instance => _instance;

    public Random Random { get; } = new();

    public Dashboard Dashboard { get; }
    public FirebaseClient FirebaseClient { get; }
    
    public CommandDispatcher<CommandSource> Dispatcher { get; private set; }

    public GameServer()
    {
        if (_instance != null)
        {
            throw new Exception("Server is already registered");
        }
        
        _instance = this;
        Dashboard = new Dashboard(this);
        FirebaseClient = new FirebaseClient("https://disasterpr-server.asia-southeast1.firebasedatabase.app");
        Bootstrap.Boot();
        
        RegisterConsoleCommands();
        _ = StartCommandLineLoopAsync();
    }

    private async Task StartCommandLineLoopAsync()
    {
        await Task.Delay(5000);
        while (true)
        {
            var source = CommandSource.OfConsole();
            
            var line = Terminal.ReadLine("> ",
                (input, index) =>
                {
                    var dispatcher = Dispatcher;
                    return BrigadierTerminal.AutoComplete(input, index, dispatcher, source);
                },
                (input, suggestion, index) =>
                {
                    var dispatcher = Dispatcher;
                    BrigadierTerminal.Render(input, suggestion, index, dispatcher, source);
                });
            await ExecuteConsoleCommandAsync(line);
        }
    }
    
    private void RegisterConsoleCommands()
    {
        Dispatcher = new CommandDispatcher<CommandSource>();
        RegisterConsole<HelpCommand>()
            .RegisterConsole<ExecuteCommand>()
            .RegisterConsole<SessionCommand>()
            .RegisterConsole<AiCommand>();
    }

    private GameServer RegisterConsole<T>() where T : IRegisteredCommand
    {
        T.Register(Dispatcher);
        return this;
    }

    private async Task ExecuteConsoleCommandAsync(string line)
    {
        if (string.IsNullOrEmpty(line)) return;
        await Command.ExecuteCommandByConsoleAsync(line);
    }

    public List<ServerPlayer> Players { get; } = new();

    public Dictionary<int, ServerSession> Sessions { get; } = new();
}