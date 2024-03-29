﻿using System.Text.Json.Nodes;
using DisasterPR.Extensions;
using Mochi.IO;

namespace DisasterPR.Net.Packets.Play;

public class ClientboundSetTopicPacket : IPacket<IClientPlayPacketHandler>
{
    public int Index { get; set; }
    
    public ClientboundSetTopicPacket(int id)
    {
        Index = id;
    }

    public ClientboundSetTopicPacket(BufferReader stream)
    {
        Index = stream.ReadVarInt();
    }

    public ClientboundSetTopicPacket(JsonObject payload)
    {
        Index = payload["index"]!.GetValue<int>();
    }
    
    public void Write(BufferWriter stream)
    {
        stream.WriteVarInt(Index);
    }

    public void Write(JsonObject obj)
    {
        obj["index"] = Index;
    }

    public void Handle(IClientPlayPacketHandler handler) => handler.HandleSetTopic(this);
}