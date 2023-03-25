using System.Text;
using DisasterPR;
using DisasterPR.Client;
using DisasterPR.Events;
using DisasterPR.Extensions;
using DisasterPR.Net.Packets;
using DisasterPR.Net.Packets.Play;
using DisasterPR.Sessions;
using KaLib.Utils;
using KaLib.Utils.Extensions;

public static class Program
{
    private static CancellationToken CancelToken => new CancellationTokenSource(TimeSpan.FromSeconds(6)).Token;
    
    public static async Task Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            Logger.Error(e.ExceptionObject.ToString());
        };

        Logger.Level = LogLevel.Verbose;
        Logger.Logged += Logger.LogToEmulatedTerminalAsync;
        Logger.RunThreaded();

        var game = Game.Instance;
        game.Init(new GameOptions
        {
            PlayerName = "Test" + Random.Shared.Next(100, 1000)
        });

        try
        {
            await game.LoginPlayerAsync(CancelToken);
            var player = game.Player!;

            if (Constants.EnableTestRoom)
            {
                await game.JoinRoomAsync(Constants.TestRoomId, CancelToken);
            }
            else
            {
                await game.HostRoomAsync(CancelToken);
            }

            while (true)
            {
                player.State = PlayerState.Ready;
                await player.Connection.SendPacketAsync(new ServerboundUpdatePlayerStatePacket(player));

                var session = player.Session!;
                var playersCount = Constants.TestRoomPlayersCount;
                Logger.Info($"Waiting for the session to have {playersCount} or more players");
                SpinWait.SpinUntil(() => session.Players.Count(p => p.State == PlayerState.Ready) >= playersCount);

                if (player == session.HostPlayer)
                {
                    Logger.Info($"Starting the session...");
                    await session.RequestStartAsync();
                }
                else
                {
                    Logger.Info($"Waiting the host to start the session...");
                }

                _ = Task.Run(() =>
                {
                    SpinWait.SpinUntil(() => !session.IsValid);
                    session = null;
                });

                SpinWait.SpinUntil(() => session.LocalGameState.CurrentState != StateOfGame.Waiting);
                Logger.Info("Game session started");

                try
                {
                    while (true)
                    {
                        SpinWait.SpinUntil(() => session.LocalGameState.CurrentState == StateOfGame.ChoosingTopic);

                        if (session.LocalGameState.CandidateTopics.HasValue)
                        {
                            var topics = session.LocalGameState.CandidateTopics.Value;
                            Logger.Info($"Left: {topics.Left.Texts.JoinStrings("____")}");
                            Logger.Info($"Right: {topics.Right.Texts.JoinStrings("____")}");

                            Logger.Info("Choosing left topic...");
                            await session.LocalGameState.ChooseTopicAsync(HorizontalSide.Left);

                            Logger.Info("Waiting for all the players to choose the words...");
                            SpinWait.SpinUntil(() => session.LocalGameState.CurrentState == StateOfGame.ChoosingFinal);

                            var chosenWords = session.LocalGameState.CurrentChosenWords;
                            Logger.Info("Revealing all the chosen words...");
                            var revealed = Enumerable.Range(0, chosenWords.Count).Shuffled()
                                .Select(i => chosenWords[i]);
                            foreach (var r in revealed)
                            {
                                await session.LocalGameState.RevealChosenWordEntryAsync(r.Id);
                            }

                            SpinWait.SpinUntil(() => chosenWords.All(r => r.IsRevealed));

                            Logger.Info("Choosing a random chosen word as the final...");
                            await session.LocalGameState.ChooseFinalAsync(Random.Shared.Next(chosenWords.Count));
                        }
                        else
                        {
                            Logger.Info("Waiting for the player to choose the topic...");
                            SpinWait.SpinUntil(() => session.LocalGameState.CurrentState == StateOfGame.ChoosingWord);

                            var topic = session.LocalGameState.CurrentTopic;
                            var words = player.HoldingCards;
                            Logger.Info($"Topic: {topic.Texts.JoinStrings("____")}");
                            Logger.Info($"Holding words: {words.Select(w => w.Card.Label).JoinStrings(", ")}");

                            var shuffledWords = Enumerable.Range(0, words.Count).Shuffled().ToList();
                            var cardCount = topic.AnswerCount;
                            var chosen = new List<int>();
                            for (var i = 0; i < cardCount; i++)
                            {
                                chosen.Add(shuffledWords[i]);
                            }

                            var chosenWords = chosen.Select(i => words[i]);
                            Logger.Info($"Choosing word: {chosenWords.Select(w => w.Card.Label).JoinStrings(", ")}");
                            await session.LocalGameState.ChooseWordAsync(chosen.ToHashSet());

                            Logger.Info("Waiting for all the players to choose the words...");
                            SpinWait.SpinUntil(() => session.LocalGameState.CurrentState == StateOfGame.ChoosingFinal);

                            Logger.Info("Waiting for the player to choose the final...");
                        }

                        SpinWait.SpinUntil(() => session.LocalGameState.CurrentState != StateOfGame.ChoosingFinal);
                        var f = session.LocalGameState.FinalChosenWord!.Words
                            .Select(w => w.Label)
                            .Concat(new[] {""}).ToList();
                        var finalSb = new StringBuilder();
                        for (var i = 0; i < session.LocalGameState.CurrentTopic.Texts.Count; i++)
                        {
                            finalSb.Append(session.LocalGameState.CurrentTopic.Texts[i]);
                            finalSb.Append(f[i]);
                        }

                        Logger.Info($"Final sentence: {finalSb}");

                        if (session.LocalGameState.CurrentState != StateOfGame.WinResult &&
                            session.LocalGameState.CurrentState != StateOfGame.Waiting)
                        {
                            Logger.Info("Continuing next round...");
                            continue;
                        }

                        Logger.Info($"Ended! Winner is {session.LocalGameState.WinnerPlayer!.Name}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            }

            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
        }
    }
}
