using DisasterPR.Net.Packets.Play;

namespace DisasterPR.Net.Packets;

public interface IClientSystemChatHandler : IClientPacketHandler
{
    public void HandleSystemChat(ClientboundSystemChatPacket packet);
}