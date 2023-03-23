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
    
    public async Task HandleChatAsync(ServerboundChatPacket packet)
    {
        if (Player.Session == null)
        {
            var server = GameServer.Instance;
            foreach (var p in server.Players.Where(p => p.Session == null))
            {
                await p.Connection.SendPacketAsync(new ClientboundChatPacket(Player.Name, packet.Content));
            }
        }
        else
        {
            foreach (var p in Player.Session.Players)
            {
                await p.Connection.SendPacketAsync(new ClientboundChatPacket(Player.Name, packet.Content));
            }
        }
    }

    public async Task HandleHostRoomAsync(ServerboundHostRoomPacket packet)
    {
        try
        {
            var roomId = ServerSession.CreateNewRoomId();
            var session = CreateSessionWithId(roomId);
            await JoinSessionAsync(session);
        }
        catch (IndexOutOfRangeException ex)
        {
            Logger.Warn(ex.ToString());
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
            await Connection.SendPacketAsync(new ClientboundSetCardPackPacket(session.CardPack!));
            await Connection.SendPacketAsync(new ClientboundUpdateSessionOptionsPacket(session));
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

        if (session.Players.Any(p => p.State != PlayerState.Ready))
        {
            Logger.Warn("Not all players are ready!");
            return;
        }

        if (!session.CardPack.FilteredTopicsByEnabledCategories(session.Options.EnabledCategories).Any())
        {
            Logger.Warn("No topics available!");
            return;
        }
        
        if (!session.CardPack.FilteredWordsByEnabledCategories(session.Options.EnabledCategories).Any())
        {
            Logger.Warn("No words available!");
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

    public async Task HandleRequestKickPlayerAsync(ServerboundRequestKickPlayerPacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();

        if (Player != session.HostPlayer) return;
        foreach (var p in session.Players.Where(p => p.Id == packet.PlayerId))
        {
            await session.KickPlayerAsync(p);
        }
    }

    public async Task HandleUpdatePlayerStateAsync(ServerboundUpdatePlayerStatePacket packet)
    {
        var session = Player.Session;
        if (session == null) return;
        await Task.Yield();
        
        Player.State = packet.State;
        await Task.WhenAll(session.Players
            .Where(p => p != Player)
            .Select(p => p.Connection.SendPacketAsync(new ClientboundUpdatePlayerStatePacket(Player))));
    }
}