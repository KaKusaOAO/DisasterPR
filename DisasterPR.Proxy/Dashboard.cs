using System.Collections.Immutable;
using KaLib.Utils;

namespace DisasterPR.Proxy;

public class Dashboard
{
    public GameServer Server { get; }

    private List<DashboardClient> _clients = new();
    public ImmutableList<DashboardClient> Clients => _clients.ToImmutableList();

    public Dashboard(GameServer server)
    {
        Server = server;

        Logger.Logged += async e =>
        {
            foreach (var client in _clients)
            {
                await client.SendMessageAsync(e);
            }
        };
    }

    public void AddClient(DashboardClient client)
    {
        _clients.Add(client);   
    }

    public void RemoveClient(DashboardClient client)
    {
        _clients.Remove(client);
    }
}