using DisasterPR.Extensions;
using DisasterPR.Net.Packets.Play;

namespace DisasterPR.Net.Packets.Login;

public class ServerboundLoginPacket : IPacket<IServerLoginPacketHandler>
{
    public string PlayerName { get; set; }
    
    public ServerboundLoginPacket(string name)
    {
        PlayerName = name;
    }

    public ServerboundLoginPacket(MemoryStream stream)
    {
        PlayerName = stream.ReadUtf8String();
    }
    
    public void Write(MemoryStream stream)
    {
        stream.WriteUtf8String(PlayerName);
    }

    public void Handle(IServerLoginPacketHandler handler) => handler.HandleLogin(this);
}