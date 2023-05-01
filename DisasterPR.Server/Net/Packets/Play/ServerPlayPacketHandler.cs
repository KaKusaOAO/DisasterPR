using DisasterPR.Net.Packets.Play;
using DisasterPR.Server.Commands;
using DisasterPR.Server.Sessions;
using DisasterPR.Sessions;
using Mochi.Utils;

namespace DisasterPR.Server.Net.Packets.Play;

public class ServerPlayPacketHandler : IServerPlayPacketHandler
{
    public ServerToPlayerConnection Connection { get; }

    public ServerPlayer Player => Connection.Player;

    public ServerPlayPacketHandler(ServerToPlayerConnection connection)
    {
        Connection = connection;
    }
    
    public void HandleChat(ServerboundChatPacket packet)
    {
        Task.Run(async () =>
        {
            if (packet.Content.StartsWith(Constants.CommandPrefix))
            {
                await Command.ExecuteCommandAsync(Player, packet.Content);
                return;
            }

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
                    await p.OnSessionChat(Player.Name, packet.Content);
                }
            }
        }).Wait();
    }

    public void HandleHostRoom(ServerboundHostRoomPacket packet)
    {
        Task.Run(async () =>
        {
            try
            {
                var roomId = ServerSession.CreateNewRoomId();
                
                try
                {
                    var session = CreateSessionWithId(roomId);
                    await JoinSessionAsync(session);
                }
                catch (AggregateException ex)
                {
                    var inner = ex.InnerExceptions.First();
                    var message = inner.Message;
                    if (inner is HttpRequestException hex)
                    {
                        message = $"無法從伺服器取得卡包資料！ ({(int?) hex.StatusCode})";
                    }
                
                    await Connection.SendPacketAsync(new ClientboundRoomDisconnectedPacket(message));
                    ServerSession.RevertRoomId();
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                Logger.Warn(ex.ToString());
                await Connection.SendPacketAsync(ClientboundRoomDisconnectedPacket.NoRoomLeft);
            }
        }).Wait();
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
            if (!await session.CheckPlayerCanJoinAsync(Player)) return;
            await session.PlayerJoinAsync(Player);
        });
    }

    public void HandleJoinRoom(ServerboundJoinRoomPacket packet)
    {
        Task.Run(async () =>
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
        }).Wait();
    }

    public void HandleHeartbeat(ServerboundHeartbeatPacket packet) { }

    public void HandleChooseTopic(ServerboundChooseTopicPacket packet)
    {
        Task.Run(async () =>
        {
            var session = Player.Session;
            if (session == null) return;

            if (session.GameState.CurrentPlayer != Player)
            {
               Logger.Verbose("Ignoring since this player is not the current player of session");
               return;
            }

            await session.ServerGameState.ChooseTopicAsync(packet.Side); 
        }).Wait();

    }

    public void HandleRequestRoomStart(ServerboundRequestRoomStartPacket packet)
    {
        Task.Run(async () =>
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
    
            if (!session.CardPack!.FilteredTopicsByEnabledCategories(session.Options.EnabledCategories).Any())
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
        }).Wait();
        
    }

    public void HandleChooseWord(ServerboundChooseWordPacket packet)
    {
        Task.Run(async () =>
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
        }).Wait();
    }
    
    public void HandleChooseFinal(ServerboundChooseFinalPacket packet)
    {
        Task.Run(async () =>
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
        }).Wait();
    }

    public void HandleRevealChosenWordEntry(ServerboundRevealChosenWordEntryPacket packet)
    {
        Task.Run(async () =>
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
        }).Wait();
    }

    public void HandleUpdateSessionOptions(ServerboundUpdateSessionOptionsPacket packet)
    {
        Task.Run(async () =>
        {
            var session = Player.Session;
            if (session == null) return;
            await Task.Yield();

            if (Player != session.HostPlayer) return;
            
            var options = session.Options;
            options.WinScore = packet.WinScore;
            options.CountdownTimeSet = packet.CountdownTimeSet;
            options.EnabledCategories = packet.EnabledCategories
                .Select(g => session.CardPack!.Categories.First(c => c.Guid == g)).ToList();

            await Task.WhenAll(session.Players.Select(p =>
                p.UpdateSessionOptions(session)));
        }).Wait();
    }

    public void HandleLeaveRoom(ServerboundLeaveRoomPacket packet)
    {
        Task.Run(async () =>
        {
            var session = Player.Session;
            if (session == null) return;
            await Task.Yield();

            await Player.Session!.PlayerLeaveAsync(Player);
            Player.Session = null;
        }).Wait();
    }

    public void HandleRequestKickPlayer(ServerboundRequestKickPlayerPacket packet)
    {
        Task.Run(async () =>
        {
            var session = Player.Session;
            if (session == null) return;
            await Task.Yield();

            if (Player != session.HostPlayer) return;
            foreach (var p in session.Players.Where(p => p.Id == packet.PlayerId).ToList())
            {
                await session.KickPlayerAsync(p);
            }
        }).Wait();
    }

    public void HandleUpdatePlayerState(ServerboundUpdatePlayerStatePacket packet)
    {
        Task.Run(async () =>
        {
            var session = Player.Session;
            if (session == null) return;
            await Task.Yield();

            Player.State = packet.State;
            await Task.WhenAll(session.Players
                .Where(p => p != Player)
                .Select(p => p.UpdatePlayerStateAsync(Player)));
        }).Wait();
    }

    public void HandleUpdateLockedWord(ServerboundUpdateLockedWordPacket packet)
    {
        Task.Run(async () =>
        {
            Player.HoldingCards[packet.Index].IsLocked = packet.IsLocked;
            await Player.Connection.SendPacketAsync(new ClientboundUpdateLockedWordPacket(packet.Index, packet.IsLocked));
        }).Wait();
    }

    public void HandleRequestShuffleWords(ServerboundRequestShuffleWordsPacket packet)
    {
        if (Player.Session == null)
        {
            Logger.Warn("Attempt to shuffle words while not in a session");
            return;
        }

        if (Player.Session.GameState.CurrentState != StateOfGame.ChoosingWord)
        {
            Logger.Warn("Attempt to shuffle words at the wrong time");
            return;
        }

        if (Player.Session.GameState.CurrentPlayer == Player)
        {
            Logger.Warn("The current player cannot shuffle words.");
            return;
        }
        
        Task.Run(async () =>
        {
            if (Player.IsManuallyShuffled)
            {
                await Player.SendErrorMessageAsync("你已經洗過牌了。");
                return;
            }
            
            ((ISessionPlayer) Player).ShuffleHoldingCards();
            await Player.UpdateHoldingWordsAsync(Player.HoldingCards);
            await Player.SendToastAsync("已完成洗牌！");
            Player.IsManuallyShuffled = true;
        }).Wait();
    }
}