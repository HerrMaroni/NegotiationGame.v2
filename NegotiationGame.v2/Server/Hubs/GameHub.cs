using System.Collections.Concurrent;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NegotiationGame.v2.Server.Data;
using NegotiationGame.v2.Shared;

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

    private static ConcurrentDictionary<Guid, (string Pin, Room Room)> Rooms { get; } = new();

    public Task<string> GetUserIdAsync() => Task.FromResult(Context.UserIdentifier)!;
    
    public async Task UpdateRoomsAsync()
    {
        foreach (var (id, _) in Rooms.Where(r => r.Value.Room.Date < DateTime.Today.AddHours(-1)).ToList())
            Rooms.Remove(id, out _);
        var rooms = Rooms.Select(r => r.Value.Room).OrderBy(r => r.Date).ToList();
        await Clients.All.UpdateRoomsListAsync(rooms);
    }
    
    private async Task<User> GetCurrentUserAsync() =>
        new User
        {
            Id = Context.UserIdentifier ?? throw new InvalidOperationException(),
            Name = await GetUserNameAsync()
        };
    
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
            (false, _, _, _) => EnterRoomResult.InvalidRoomId,
        };
        if (result != EnterRoomResult.Ok) return result;
        item.Room.Users.Add(await GetCurrentUserAsync());
        await UpdateRoomsAsync();
        return result;
    }

    public Task UpdateGameStateAsync(Guid roomId)
    {
        throw new NotImplementedException();
    }

    public Task StartGameAsync(Guid roomId)
    {
        throw new NotImplementedException();
    }

    public Task MakeMoveAsync(Guid roomId)
    {
        throw new NotImplementedException();
    }
}