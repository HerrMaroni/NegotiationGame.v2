namespace NegotiationGame.v2.Shared.Interfaces;

public interface IGameClient
{
    Task UpdateRoomsListAsync(List<Room> rooms);
}