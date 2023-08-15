using DisasterPR.Events;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Play;
using Mochi.Utils;

namespace DisasterPR.Client;

public class Game
{
    private static Game? _instance;
    public static Game Instance => _instance ??= new Game();

    public event DisconnectedEventDelegate Disconnected;
    public event PlayerChatEventDelegate ReceivedPlayerChat;

    public LocalPlayer? Player { get; set; }

    public void Init(GameOptions options)
    {
        Player = new LocalPlayer(options.PlayerName);
        Player.Connection.Disconnected += async e =>
        {
            await Task.Yield();
            Disconnected?.Invoke(e);
        };
    }

    public async Task LoginPlayerAsync(CancellationToken token)
    {
        if (Player == null) return;
        
        await Player.LoginAsync(token);
        Logger.Verbose($"Player logged in as {Player.Name} ({Player.Id})");
    }

    public async Task HostRoomAsync(CancellationToken token)
    {
        if (Player == null) return;
        await Player.HostRoomAsync(token);
    }
    
    public async Task JoinRoomAsync(int roomId, CancellationToken token)
    {
        if (Player == null) return;
        await Player.JoinRoomAsync(roomId, token);
        Logger.Verbose($"Joined room: #{Player.Session?.RoomId}");
    }

    internal void InternalOnDisconnected(DisconnectedEventArgs args) => Disconnected?.Invoke(args);

    internal void InternalOnPlayerChat(PlayerChatEventArgs args) => ReceivedPlayerChat?.Invoke(args);

    private async Task<TOut> SendAndGetResponseAsync<TIn, TOut>(TIn sent) where TIn : INoncePacket where TOut : INoncePacket
    {
        if (Player == null)
        {
            throw new Exception("Player not exist");
        }
        
        var result = Optional.Empty<TOut>();
        Task OnConnectionOnReceivedPacket(ReceivedPacketEventArgs e)
        {
            var packet = e.Packet;
            if (packet is not TOut p) return Task.CompletedTask;
            if (p.Nonce != sent.Nonce) return Task.CompletedTask;
            result = Optional.Of(p);
            return Task.CompletedTask;
        }

        Player.Connection.ReceivedPacket += OnConnectionOnReceivedPacket;
        await Player.Connection.SendPacketAsync(sent);
        SpinWait.SpinUntil(() => result.IsPresent);
        
        Player.Connection.ReceivedPacket -= OnConnectionOnReceivedPacket;
        return result.Value;
    }
    
    public async Task<string> GetRandomNameAsync()
    {
        var sent = new ServerboundRequestRandomNamePacket();
        var packet =
            await SendAndGetResponseAsync<ServerboundRequestRandomNamePacket,
                ClientboundRandomNameResponsePacket>(sent);
        return packet.Name;
    }
}

public class GameOptions
{
    public string PlayerName { get; set; }
}