using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using NegotiationGame.v2.Shared.GameDomain;

namespace NegotiationGame.v2.Client.Service;

public class GameData
{
    public Coin Red { get; set; } = new() { Color = CoinColor.Red, Amount = 10, Value = 5};
    public Coin Blue { get; set; } = new() { Color = CoinColor.Blue, Amount = 10, Value = 3};
    public Coin Yellow { get; set; } = new() { Color = CoinColor.Yellow, Amount = 10, Value = 4};

    public int CurrentBalance => Red.Amount * Red.Value + Blue.Amount * Blue.Value + Yellow.Amount * Yellow.Value;
    public int MaxBalance => MaxCoinAmount * Red.Value + MaxCoinAmount * Blue.Value + MaxCoinAmount * Yellow.Value;
    public int MaxCoinAmount { get; set; } = 10;

    public int CurrentGame { get; set; } = 0;
    public int CurrentTurn { get; set; } = 0;

    public Event CurrentEvent { get; set; } = Event.StartGame;

    public GameView CurrentView
    {
        get
        {
            (Event, bool) tuple = (CurrentEvent, CurrentUser == PlayerName);
            return tuple switch
            {
                (Event.MakeOffer, true) => GameView.MakeOfferView,
                (Event.MakeOffer, false) => GameView.StandardViewWaitingMakeOffer,
                (Event.MakeDecision, true) => GameView.MakeDecisionView,
                (Event.MakeDecision, false) => GameView.StandardViewWaitingMakeDecision,
                (Event.StartGame,_) => GameView.StandardView,
                (_,_) => GameView.StandardView,
            };
        }
    }

    public TimeSpan? TurnEndsIn { get; set; }
    
    public string PlayerName { get; set; } = "";
    public string OpponentName { get; set; } = "";
    public string CurrentUser { get; set; } = "";
    
    public event Action<Offer> OnMakeOffer = _ => Console.WriteLine("MakeOffer Event triggered");
    public event Action<Event> OnMakeDecision = _ => Console.WriteLine("MakeDecision Event triggered");
    
    public void MakeOffer(Offer offer) => OnMakeOffer.Invoke(offer);
    public void MakeDecision(Event decision) => OnMakeDecision.Invoke(decision);
    
    
    public void UpdateGameData(GameState gameState, string userId, TimeSpan? turnEndsIn)
    {
        CurrentUser = gameState.Users.First(u => u.Id == gameState.CurrentUserId).Name;
        CurrentEvent = gameState.CurrentEvent;
        CurrentGame = gameState.CurrentGame;
        CurrentTurn = gameState.CurrentTurn;
        
        MaxCoinAmount = gameState.MaxCoinAmount;
        PlayerName = gameState.Users.First(u => u.Id == userId).Name;
        OpponentName = gameState.Users.First(u => u.Id != userId).Name;
        TurnEndsIn = turnEndsIn;
        var userIndex = gameState.Users.FindIndex(u => u.Id == userId);
        var correlation = new List<int>(new[] { 0, 0, 0 });
        if (gameState.CurrentCorrelation is not null)
        {
            correlation = userIndex switch 
            {
                0 => gameState.CurrentCorrelation.PlayerAValues,
                1 => gameState.CurrentCorrelation.PlayerBValues,
                _ => throw new ArgumentException()
            };
        }

        Red.Value = correlation[0];
        Blue.Value = correlation[1];
        Yellow.Value = correlation[2];
        
        // Red.Amount = gameState.Offer.RedCoin;
        // Blue.Amount = gameState.Offer.BlueCoin;
        // Yellow.Amount = gameState.Offer.YellowCoin;
    }
}

public enum GameView
{
    StandardView,
    StandardViewWaitingMakeOffer,
    StandardViewWaitingMakeDecision,
    MakeOfferView,
    MakeDecisionView
}