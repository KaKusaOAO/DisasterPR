using DisasterPR.Extensions;
using DisasterPR.Net.Packets.Play;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Login;

public class ServerboundLoginPacket : IPacket<IServerLoginPacketHandler>
{
    public string PlayerName { get; set; }
    
    public ServerboundLoginPacket(string name)
    {
        PlayerName = name;
    }

    public ServerboundLoginPacket(BufferReader stream)
    {
        PlayerName = stream.ReadUtf8String();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteUtf8String(PlayerName);
    }

    public void Handle(IServerLoginPacketHandler handler) => handler.HandleLogin(this);
}