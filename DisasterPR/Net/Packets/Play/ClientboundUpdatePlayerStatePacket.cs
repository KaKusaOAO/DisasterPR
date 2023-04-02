﻿using DisasterPR.Cards;
using DisasterPR.Extensions;
using DisasterPR.Sessions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundUpdatePlayerStatePacket : IPacket<IClientPlayPacketHandler>
{
    public Guid Id { get; set; }
    public PlayerState State { get; set; }

    public ClientboundUpdatePlayerStatePacket(IPlayer player)
    {
        Id = player.Id;
        State = player.State;
    }

    public ClientboundUpdatePlayerStatePacket(BufferReader stream)
    {
        Id = stream.ReadGuid();
        State = (PlayerState)stream.ReadVarInt();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteGuid(Id);
        stream.WriteVarInt((int) State);
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleUpdatePlayerState(this);
}