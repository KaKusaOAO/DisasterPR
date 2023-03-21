namespace DisasterPR;

public enum RoomDisconnectReason
{
    Custom,
    NotFound,
    RoomFull,
    NoRoomLeft,
    RoomPlaying,
    GuidDuplicate,
    SomeoneLeftWhileInGame,
    Kicked
}