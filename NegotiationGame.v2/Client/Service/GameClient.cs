using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using NegotiationGame.v2.Shared;
using NegotiationGame.v2.Shared.Interfaces;

namespace NegotiationGame.v2.Client.Service;

public class GameClient : IGameClient
{
    private HubConnection HubConnection { get; }
    public bool IsConnected => HubConnection?.State == HubConnectionState.Connected;
    public event Action StateHasChanged = () => Console.WriteLine("State changed");

    public GameClient(HubConnection hubConnection)
    {
        HubConnection = hubConnection;
        foreach (var method in typeof(IGameClient).GetMethods())
        {
            HubConnection.On(method.Name, method.GetParameters().Select(p => p.ParameterType).ToArray(), parameters =>
                {
                    Console.WriteLine($"Calling {method.Name} with {parameters.Length} parameters.");
                    var result = (Task)method.Invoke(this, parameters)!;
                    Console.WriteLine($"Called {method.Name} with {parameters.Length} parameters.");
                    StateHasChanged?.Invoke();
                    return result;
                });
        }
    }

    public static async Task<HubConnection> HubConnectionFactory(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        if (!(await scope.ServiceProvider.GetRequiredService<IAccessTokenProvider>().RequestAccessToken())
            .TryGetToken(out var token))
            return null!;
        var hubConnection = new HubConnectionBuilder().WithUrl(
            scope.ServiceProvider.GetRequiredService<NavigationManager>().ToAbsoluteUri("/gamehub"),
            HttpTransportType.WebSockets, option =>
            {
                option.SkipNegotiation = true;
                option.Url = new Uri(option.Url + $"?access_token={token.Value}");
            }).WithAutomaticReconnect(Enumerable.Range(0, 10).Select(i => TimeSpan.FromSeconds(i)).ToArray()).Build();
        await hubConnection.StartAsync();
        return hubConnection;
    }

    public async Task InvokeAsync(Expression<Func<IGameHub, Task>> sendFunction)
    {
        var methodCallExpression = (MethodCallExpression)sendFunction.Body;
        await HubConnection.InvokeCoreAsync(
            methodCallExpression.Method.Name, 
            methodCallExpression.Arguments.Select(exp => Expression.Lambda(exp).Compile().DynamicInvoke()).ToArray());
    }
    
    public async Task<TResult> InvokeAsync<TResult>(Expression<Func<IGameHub, Task<TResult>>> sendFunction)
    {
        var methodCallExpression = (MethodCallExpression)sendFunction.Body;
        return await HubConnection.InvokeCoreAsync<TResult>(
            methodCallExpression.Method.Name, 
            methodCallExpression.Arguments.Select(exp => Expression.Lambda(exp).Compile().DynamicInvoke()).ToArray());
    }
    
    public IEnumerable<Room> Rooms => _rooms ?? Enumerable.Empty<Room>();

    private List<Room>? _rooms;

    public Task UpdateRoomsListAsync(List<Room> rooms)
    {
        _rooms = rooms;
        return Task.CompletedTask;
    }
    
    public event Action<GameState, TimeSpan?>? GameStateChanged;
    public Task UpdateGameStateAsync(GameState gameState, double? turnEndsInMilliseconds)
    {
        GameStateChanged?.Invoke(gameState, turnEndsInMilliseconds == null ? null : TimeSpan.FromMilliseconds(turnEndsInMilliseconds.Value));
        return Task.CompletedTask;
    }
}