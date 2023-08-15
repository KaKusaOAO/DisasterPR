using DisasterPR.Extensions;
using DisasterPR.Server.Commands;
using DisasterPR.Server.Commands.Senders;
using DisasterPR.Server.Sessions;
using Mochi.Brigadier;
using Mochi.Brigadier.Bridge;
using Mochi.Utils;

namespace DisasterPR.Server;

public class GameServer
{
    private static GameServer _instance;

    public static GameServer Instance => _instance;

    public Random Random { get; } = new();

    public Dashboard Dashboard { get; }
    
    public CommandDispatcher<CommandSource> Dispatcher { get; private set; }

    public GameServer()
    {
        if (_instance != null)
        {
            throw new Exception("Server is already registered");
        }
        
        _instance = this;
        Dashboard = new Dashboard(this);
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

    private static int[] _sessionIds = Enumerable.Range(1000, 9000).ToArray();
    
    public Dictionary<int, ServerSession> Sessions { get; } = new();
    private SemaphoreSlim _sessionLock = new(1, 1);
    
    public bool TryCreateSession(out ServerSession? session)
    {
        _sessionLock.Wait();
        try
        {
            var availableIds = _sessionIds.Where(i => !Sessions.ContainsKey(i)).ToList();
            if (!availableIds.Any())
            {
                session = null;
                return false;
            }

            var id = availableIds.Shuffled().First();
            var result = GetOrCreateSession(id);
            Sessions[id] = result;
            session = result;
            return true;
        }
        finally
        {
            _sessionLock.Release();
        }
    }
    
    public ServerSession GetOrCreateSession(int roomId)
    {
        if (Sessions.TryGetValue(roomId, out var session)) return session;
        var result = new ServerSession(roomId);
        result.Emptied += () =>
        {
            result.Invalidate();
            Sessions.Remove(result.RoomId);
            Logger.Verbose($"Removed room #{result.RoomId}");
        };
        return result;
    }
}