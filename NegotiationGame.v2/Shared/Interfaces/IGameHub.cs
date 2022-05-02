namespace NegotiationGame.v2.Shared.Interfaces;

public interface IGameHub
{
    Task SendMessageToLobbyAsync(string message);
    
    #region RoomManagement
    Task<string> GetUserIdAsync();
    Task UpdateRoomsAsync();
    Task<Room> CreateRoomAsync(string pin, string name);
    Task DeleteRoomAsync(Guid roomId);
    Task<EnterRoomResult> EnterRoomAsync(Guid roomId, string pin);
    #endregion
    
    #region GameManagement
    Task UpdateGameStateAsync(Guid roomId);
    Task StartGameAsync(Guid roomId);
    Task MakeMoveAsync(Guid roomId);
    #endregion
}