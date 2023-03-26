// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using DisasterPR.Cards;
using DisasterPR.Cards.Providers;
using DisasterPR.Extensions;
using KaLib.Utils;

Logger.Level = LogLevel.Verbose;
Logger.Logged += Logger.LogToEmulatedTerminalAsync;
Logger.RunThreaded();

var buffer = new MemoryStream();
buffer.WriteEnum(LogLevel.Fatal);
buffer.Position = 0;

var read = buffer.ReadEnum<LogLevel>();
Logger.Info($"Test passed");
await Logger.FlushAsync();