using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdatePlayerNamePacket : IPacket<IClientPlayPacketHandler>
{
    public string Name { get; set; }

    public ClientboundUpdatePlayerNamePacket(string name)
    {
        Name = name;
    }

    public ClientboundUpdatePlayerNamePacket(BufferReader stream)
    {
        Name = stream.ReadUtf8String();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteUtf8String(Name);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdatePlayerName(this);
}