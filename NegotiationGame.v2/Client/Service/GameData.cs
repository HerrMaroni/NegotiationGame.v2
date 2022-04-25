using NegotiationGame.v2.Shared.GameDomain;

namespace NegotiationGame.v2.Client.Service;

public class GameData
{
    public Coin Red { get; set; } = new() { Color = CoinColor.Red, Amount = 10, Value = 5};
    public Coin Blue { get; set; } = new() { Color = CoinColor.Blue, Amount = 10, Value = 3};
    public Coin Yellow { get; set; } = new() { Color = CoinColor.Yellow, Amount = 10, Value = 4};
    
    public int CurrentBalance { get; set; } = 0;
    public int MaxBalance { get; set; } = 0;
    public int MaxCoinAmount { get; set; } = 10;

    public int CurrentGame { get; set; } = 0;
    public int CurrentRound { get; set; } = 0;

    public Event CurrentEvent { get; set; } = Event.StartGame;
    public int Time { get; set; } = 0;
    
    public string PlayerName { get; set; } = "";
    public string OpponentName { get; set; } = "";

    public string CurrentPlayer { get; set; } = "";
    
    public Task CalculateCurrentBalance()
    {
        CurrentBalance = Red.Amount * Red.Value + Blue.Amount * Blue.Value + Yellow.Amount * Yellow.Value;
        return Task.CompletedTask;
    }
    
    public Task CalculateMaxBalance()
    {
        MaxBalance = MaxCoinAmount * Red.Value + MaxCoinAmount * Blue.Value + MaxCoinAmount * Yellow.Value;
        return Task.CompletedTask;
    }
}