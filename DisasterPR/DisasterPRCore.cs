using DisasterPR.Backends;

namespace DisasterPR;

public static class DisasterPRCore
{
    public static IBackend Backend { get; set; } = new DefaultBackend();
}