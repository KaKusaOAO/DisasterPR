namespace DisasterPR.Net.Packets.Play;

public class ClientboundDismissNameChangeModalPacket : IPacketNoContent<IClientPlayPacketHandler>
{
    public void Handle(IClientPlayPacketHandler handler) => handler.HandleDismissNameChangeModal(this);
}