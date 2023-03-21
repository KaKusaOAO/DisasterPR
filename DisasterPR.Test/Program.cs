// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using DisasterPR.Cards;
using DisasterPR.Cards.Providers;
using DisasterPR.Extensions;
using KaLib.Utils;

Logger.Level = LogLevel.Verbose;
Logger.Logged += Logger.LogToEmulatedTerminalAsync;
Logger.RunThreaded();

var pack = await CardPack.GetUpstreamAsync();
var buffer = new MemoryStream();
pack.Serialize(buffer);
buffer.Position = 0;

var read = CardPack.Deserialize(buffer);
Logger.Info($"Test passed");
await Logger.FlushAsync();