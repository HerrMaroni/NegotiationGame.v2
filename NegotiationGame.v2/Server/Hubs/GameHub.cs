using System.Collections.Concurrent;
using System.Net;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NegotiationGame.v2.Server.Data;
using NegotiationGame.v2.Shared;
using NegotiationGame.v2.Shared.GameDomain;
using NegotiationGame.v2.Shared.Interfaces;

namespace NegotiationGame.v2.Server.Hubs;

[Authorize]
public class GameHub : Hub<IGameClient>, IGameHub
{
    public GameHub(ApplicationDbContext applicationDbContext)
    {
        ApplicationDbContext = applicationDbContext;
    }

    private ApplicationDbContext ApplicationDbContext { get; }

    private async Task<string> GetUserNameAsync(string? userId = null)
    {
        userId ??= Context.UserIdentifier;
        var email = (await ApplicationDbContext.Users.FindAsync(userId))?.Email;
        return $"{email?[0..email.IndexOf('@')]}-{userId?[0..3]}";
    }

    public Task SendMessageToLobbyAsync(string message)
    {
        throw new NotImplementedException();
    }

    #region RoomManagement

    private static ConcurrentDictionary<Guid, (string Pin, Room Room)> Rooms { get; } = new();

    public Task<string> GetUserIdAsync()
    {
        return Task.FromResult(Context.UserIdentifier)!;
    }

    public async Task UpdateRoomsAsync()
    {
        foreach (var (id, _) in Rooms.Where(r => r.Value.Room.Date < DateTime.Today.AddHours(-1)).ToList())
            Rooms.Remove(id, out _);
        var rooms = Rooms.Select(r => r.Value.Room).OrderBy(r => r.Date).ToList();
        await Clients.All.UpdateRoomsListAsync(rooms);
    }

    private async Task<User> GetCurrentUserAsync()
    {
        return new User
        {
            Id = Context.UserIdentifier ?? throw new InvalidOperationException(),
            Name = await GetUserNameAsync()
        };
    }

    public async Task<Room> CreateRoomAsync(string pin, string name)
    {
        if (!RoomPinValidator.IsValidPin(pin).IsNullOrEmpty())
            throw new HubException("Invalid PIN.");

        var user = await GetCurrentUserAsync();
        var room = new Room
        {
            Date = DateTime.Now,
            Id = Guid.NewGuid(),
            Name = name,
            Started = false,
            Host = user,
            Users = new List<User> { user }
        };
        Rooms.TryAdd(room.Id, (pin, room));
        await UpdateRoomsAsync();
        return room;
    }

    public async Task DeleteRoomAsync(Guid roomId)
    {
        if (Rooms.TryGetValue(roomId, out var item) && item.Room.Host?.Id == Context.UserIdentifier)
            Rooms.Remove(roomId, out _);
        await UpdateRoomsAsync();
    }

    public async Task<EnterRoomResult> EnterRoomAsync(Guid roomId, string pin)
    {
        var check = (
            RoomExists: Rooms.TryGetValue(roomId, out var item),
            PinIsCorrect: item.Pin == pin,
            UserIsInRoom: item.Room.Users.Any(u => u.Id == Context.UserIdentifier),
            GameInProgress: item.Room.Started
        );
        var result = check switch
        {
            (true, true, false, false) => EnterRoomResult.Ok,
            (true, true, false, true) => EnterRoomResult.GameAlreadyInProgress,
            (true, true, true, _) => EnterRoomResult.UserAlreadyInRoom,
            (true, false, _, _) => EnterRoomResult.InvalidPin,
            (false, _, _, _) => EnterRoomResult.InvalidRoomId
        };
        if (result != EnterRoomResult.Ok) return result;
        item.Room.Users.Add(await GetCurrentUserAsync());
        await UpdateRoomsAsync();
        return result;
    }

    #endregion

    #region GameManagement

    private static ConcurrentDictionary<Guid, GameState> Games { get; } = new();

    public async Task UpdateGameStateAsync(Guid roomId)
    {
        if (Rooms.TryGetValue(roomId, out var item) && Games.TryGetValue(roomId, out var game))
            await Clients.Users(item.Room.Users.Select(u => u.Id).ToList()).UpdateGameStateAsync(game, game.TurnEnds.HasValue ? (game.TurnEnds.Value - DateTime.Now).TotalMilliseconds : null);
    }

    private static void NextPlayer(GameState state)
    {
        state.CurrentUserId = state.Users[(state.Users.FindIndex(u => u.Id == state.CurrentUserId) + 1) % state.Users.Count].Id;
    }
    
    public async Task StartGameAsync(Guid roomId)
    {
        if (Rooms.TryGetValue(roomId, out var item) &&
            !item.Room.Started &&
            (item.Room.Users.FirstOrDefault(u => u.Id == Context.UserIdentifier)) != null)
        {
            item.Room.Started = true;
            var random = new Random();
            var state = new GameState(roomId, DateTime.Now.AddSeconds(600),
                item.Room.Users.OrderBy(_ => random.Next()).First().Id,
                item.Room.Users.Select(u => new User { Id = u.Id, Name = u.Name, Balance = 0 }).ToList());
            state.CurrentCorrelation = state.Correlations.Dequeue();
            state.CurrentEvent = Event.MakeOffer;
            var user = state.Users.First(u => u.Id == Context.UserIdentifier);
            
            
            Games.TryAdd(roomId, state);
            _ = Task.Run(async () =>
            {
                while (state.TurnEnds != null)
                {
                    await Task.Delay(500);
                    if (state.TurnEnds < DateTime.Now)
                    {
                        NextPlayer(state);
                    }
                    await UpdateGameStateAsync(roomId);
                }
            });
        }
        await UpdateGameStateAsync(roomId);
    }

    public async Task MakeOfferAsync(Guid roomId, Offer offer)
    {
        if (Games.TryGetValue(roomId, out var game) && game.CurrentUserId == Context.UserIdentifier)
        {
            game.CurrentEvent = Event.MakeDecision;

            NextPlayer(game);
            if (Rooms.TryGetValue(roomId, out var item))
                await Clients.User(item.Room.Users.First(u => u.Id != Context.UserIdentifier).Id).UpdateOfferAsync(offer);
            await UpdateGameStateAsync(roomId);
        }
    }
    
    public async Task MakeDecisionAsync(Guid roomId, Event decision)
    {
        if (Games.TryGetValue(roomId, out var game) && game.CurrentUserId == Context.UserIdentifier)
        {
            switch (decision)
            {
                case Event.AcceptOffer:
                    game.ResetGameState();
                    break;
                case Event.DeclineOffer:
                    game.CurrentEvent = Event.MakeOffer;
                    game.CurrentTurn++;
                    game.MaxCoinAmount--;
                    break;
                default:
                    throw new ArgumentException();
            }
            var offer = new Offer()
            {
                RedCoin = game.MaxCoinAmount,
                BlueCoin = game.MaxCoinAmount,
                YellowCoin = game.MaxCoinAmount
            };
            if (Rooms.TryGetValue(roomId, out var item))
                await Clients.Users(item.Room.Users.Select(u => u.Id).ToList()).UpdateOfferAsync(offer);
            await UpdateGameStateAsync(roomId);
        }
    }

    #endregion
}