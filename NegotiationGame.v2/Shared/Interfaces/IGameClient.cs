using NegotiationGame.v2.Shared.GameDomain;

namespace NegotiationGame.v2.Shared.Interfaces;

public interface IGameClient
{
    Task UpdateRoomsListAsync(List<Room> rooms);
    Task UpdateGameStateAsync(GameState gameState, double? turnEndsInMilliseconds);
    Task UpdateOfferAsync(Offer offer);
}