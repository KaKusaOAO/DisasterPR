using DisasterPR.Server.Commands.Senders;
using DisasterPR.Server.Sessions;
using KaLib.Brigadier;
using KaLib.Brigadier.Arguments;

namespace DisasterPR.Server.Commands;

public class ExecuteCommand : Command, IRegisteredCommand
{
    public const string CommandName = "execute";

    public static void Register(CommandDispatcher<CommandSource> d)
    {
        var root = d.Register(Literal(CommandName)
            .Requires(c => c.Sender is ConsoleCommandSender));
        
        d.Register(Literal(CommandName)
            // -> /execute run ...
            .Then(Literal("run")
                .Redirect(d.GetRoot()))
            // -> /execute in ...
            .Then(Literal("in")
                // -> /execute in session ...
                .Then(Literal("session")
                    // -> /execute in session all ...
                    .Then(Literal("all")
                        .Fork(root, context =>
                        {
                            var source = context.GetSource();
                            return GameServer.Instance.Sessions.Values.Select(session => source.Copy().Modify(s =>
                            {
                                s.Session = session;
                            }));
                        })
                    )
                    // -> /execute in session id <roomId> ...
                    .Then(Literal("id")
                        .Then(Argument("roomId", IntegerArgumentType.Integer())
                            .Redirect(root, context =>
                            {
                                var source = context.GetSource();
                                var id = context.GetArgument<int>("roomId");
                                GameServer.Instance.Sessions.TryGetValue(id, out var session);
                                return source.Copy().Modify(s =>
                                {
                                    s.Session = session;
                                });
                            })
                        )
                    )
                )
            )
        );
    }
}