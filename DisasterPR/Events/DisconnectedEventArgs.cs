namespace DisasterPR.Events;

public delegate Task DisconnectedEventDelegate(DisconnectedEventArgs args);

public class DisconnectedEventArgs : EventArgs
{
    public PlayerKickReason Reason { get; set; }
    public string? Message { get; set; }
}