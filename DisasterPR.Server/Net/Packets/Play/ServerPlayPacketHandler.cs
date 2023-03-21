using DisasterPR.Net.Packets.Play;
using DisasterPR.Server.Sessions;
using DisasterPR.Sessions;
using KaLib.Utils;

namespace DisasterPR.Server.Net.Packets.Play;

public class ServerPlayPacketHandler : IServerPlayPacketHandler
{
    public ServerToPlayerConnection Connection { get; }

    public ServerPlayer Player => Connection.Player;

    public ServerPlayPacketHandler(ServerToPlayerConnection connection)
    {
        Connection = connection;
    }
    
    public Task HandleChatAsync(ServerboundChatPacket packet)
    {
        throw new NotImplementedException();
    }

    public async Task HandleHostRoomAsync(ServerboundHostRoomPacket packet)
    {
        var server = GameServer.Instance;
        var sessions = server.Sessions;

        try
        {
            var roomId = ServerSession.CreateNewRoomId();
            var session = CreateSessionWithId(roomId);
            await JoinSessionAsync(session);
        }
        catch (IndexOutOfRangeException)
        {
            await Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.NoRoomLeft);
        }
    }

    private ServerSession CreateSessionWithId(int roomId)
    {
        var server = GameServer.Instance;
        var sessions = server.Sessions;
        Logger.Verbose($"Created room #{roomId}");

        var session = new ServerSession(roomId);
        session.Emptied += () =>
        {
            session.Invalidate();
            sessions.Remove(roomId);
            Logger.Verbose($"Removed room #{roomId}");
        };
            
        sessions.Add(roomId, session);
        return session;
    }

    private async Task JoinSessionAsync(ServerSession session)
    {
        if (Player.Session != null)
        {
            Logger.Warn("Cannot join the session because the player is already in a session.");
            return;
        }
        
        await session.AcquireAsync(async () =>
        {
            if (session.GameState.CurrentState != StateOfGame.Waiting)
            {
                await Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.RoomPlaying);
                return;
            }
            
            if (session.Players.Count >= Constants.SessionMaxPlayers)
            {
                await Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.RoomFull);
                return;
            }

            if (session.Players.Find(p => p.Id == Player.Id) != null)
            {
                await Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.GuidDuplicate);
                return;
            }

            await Connection.SendPacketAsync(new ClientboundJoinedRoomPacket(session));
            await session.PlayerJoinAsync(Player);
        });
    }

    public async Task HandleJoinRoomAsync(ServerboundJoinRoomPacket packet)
    {
        var sessions = GameServer.Instance.Sessions;
        var id = packet.RoomId;

        if (Constants.EnableTestRoom && id == Constants.TestRoomId)
        {
            if (!sessions.ContainsKey(id))
            {
                CreateSessionWithId(id);
            }
        }
        
        if (!sessions.ContainsKey(id))
        {
            await Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.NotFound);
            return;
        }

        var session = sessions[id];
        await JoinSessionAsync(session);
    }

    public Task HandleHeartbeatAsync(ServerboundHeartbeatPacket packet) => Task.CompletedTask;

    public async Task HandleChooseTopicAsync(ServerboundChooseTopicPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        if (session.GameState.CurrentPlayer != Player)
        {
            Logger.Verbose("Ignoring since this player is not the current player of session");
            return;
        }
        
        await session.ServerGameState.ChooseTopicAsync(packet.Side);
    }

    public async Task HandleRequestRoomStartAsync(ServerboundRequestRoomStartPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        if (session.HostPlayer != Player)
        {
            Logger.Warn("Non-host player cannot start the game!");
            return;
        }

        var state = session.ServerGameState;
        if (state.CurrentState != StateOfGame.Waiting)
        {
            Logger.Warn("Room is not in waiting state!");
            return;
        }

        await session.ServerGameState.StartAsync();
    }

    public async Task HandleChooseWordAsync(ServerboundChooseWordPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        var state = session.ServerGameState;
        if (state.CurrentState != StateOfGame.ChoosingWord)
        {
            Logger.Warn("Room is not in choosing word state!");
            return;
        }

        var words = packet.Indices.Select(i => Player.HoldingCards[i]).ToList();
        await session.ServerGameState.ChooseWordAsync(Player, words);
    }
    
    public async Task HandleChooseFinalAsync(ServerboundChooseFinalPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        var state = session.ServerGameState;
        if (state.CurrentState != StateOfGame.ChoosingFinal)
        {
            Logger.Warn("Room is not in choosing final state!");
            return;
        }

        await session.ServerGameState.ChooseFinalAsync(Player, packet.Index);
    }

    public async Task HandleRevealChosenWordEntryAsync(ServerboundRevealChosenWordEntryPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;

        var state = session.ServerGameState;
        if (state.CurrentState != StateOfGame.ChoosingFinal)
        {
            Logger.Warn("Room is not in choosing final state!");
            return;
        }
        if (state.CurrentPlayer != Player)
        {
            Logger.Verbose("Ignoring since this player is not the current player of session");
            return;
        }

        await state.RevealChosenWordEntryAsync(packet.Guid);
    }

    public async Task HandleUpdateSessionOptionsAsync(ServerboundUpdateSessionOptionsPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();
        
        var options = session.Options;
        options.WinScore = packet.WinScore;
        options.CountdownTimeSet = packet.CountdownTimeSet;
        options.EnabledCategories = packet.EnabledCategories
            .Select(g => session.CardPack.Categories.First(c => c.Guid == g)).ToList();

        await Task.WhenAll(session.Players.Select(p =>
            p.Connection.SendPacketAsync(new ClientboundUpdateSessionOptionsPacket(session))));
    }

    public async Task HandleLeaveRoomAsync(ServerboundLeaveRoomPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        await Player.Session!.PlayerLeaveAsync(Player);
        Player.Session = null;
    }
}