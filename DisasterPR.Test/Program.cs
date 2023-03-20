// See https://aka.ms/new-console-template for more information

using DisasterPR.Extensions;
using KaLib.Utils;

Logger.Level = LogLevel.Verbose;
Logger.Logged += Logger.LogToEmulatedTerminalAsync;
Logger.RunThreaded();

var stream = new MemoryStream();
stream.WriteVarInt(25);

stream.Seek(0, SeekOrigin.Begin);
if (stream.ReadVarInt() != 25)
{
    throw new Exception("VarInt read error, expected 25");
}

Logger.Info("Test passed");
await Logger.FlushAsync();