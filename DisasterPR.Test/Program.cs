// See https://aka.ms/new-console-template for more information

using Mochi.IO;
using Mochi.Utils;

Logger.Level = LogLevel.Verbose;
Logger.Logged += Logger.LogToEmulatedTerminalAsync;
Logger.RunThreaded();

var buffer = new MemoryStream();
var writer = new BufferWriter(buffer);
writer.WriteEnum(LogLevel.Fatal);
buffer.Position = 0;

var reader = new BufferReader(buffer);
var read = reader.ReadEnum<LogLevel>();
Logger.Info($"Test passed");
await Logger.FlushAsync();