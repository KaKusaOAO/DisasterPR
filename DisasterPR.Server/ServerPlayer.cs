using DisasterPR.Cards;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Server.Commands.Senders;
using DisasterPR.Server.Sessions;
using DisasterPR.Sessions;
using ISession = DisasterPR.Sessions.ISession;

namespace DisasterPR.Server;

public class ServerPlayer : IPlayer, ICommandSender
{
    public Guid Id { get; }
    public string Name { get; set; }
    
    public async Task SendMessageAsync(string content)
    {
        await Connection.SendPacketAsync(new ClientboundChatPacket("系統", "訊息：" + content));
    }

    public async Task SendErrorMessageAsync(string content)
    {
        await Connection.SendPacketAsync(new ClientboundChatPacket("系統", "錯誤：" + content));
    }

    public ServerSession? Session { get; set; }
    ISession? IPlayer.Session => Session;
    
    public int Score { get; set; }
    public List<HoldingWordCardEntry> HoldingCards { get; } = new();
    public PlayerState State { get; set; }

    public ShuffledPool<WordCard>? CardPool { get; set; }
    public ServerToPlayerConnection Connection { get; }

    public ServerPlayer(ServerToPlayerConnection connection)
    {
        Id = Guid.NewGuid();
        Connection = connection;
    }
}