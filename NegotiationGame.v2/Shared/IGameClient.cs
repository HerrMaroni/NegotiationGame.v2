namespace NegotiationGame.v2.Shared;

public interface IGameClient
{
    Task UpdateRoomsListAsync(List<Room> rooms);
}