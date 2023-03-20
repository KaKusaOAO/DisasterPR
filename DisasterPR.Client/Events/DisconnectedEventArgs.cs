namespace DisasterPR.Client.Events;

public delegate void DisconnectedEventDelegate(object sender, DisconnectedEventArgs args);

public class DisconnectedEventArgs : EventArgs
{
    public PlayerKickReason Reason { get; set; }
    public string? Message { get; set; }
}