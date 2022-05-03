namespace NegotiationGame.v2.Shared.GameDomain;

public class GameState
{
    public GameState(Guid gameId, DateTime turnEnds, string currentUserId, List<User> users)
    {
        GameId = gameId;
        TurnEnds = turnEnds;
        CurrentUserId = currentUserId;
        Users = users;
    }

    public Guid GameId { get; set; }
    public DateTime? TurnEnds { get; set; }
    public string CurrentUserId { get; set; }
    public Event CurrentEvent { get; set; } = Event.StartGame;
    public int CurrentGame { get; set; } = 1;
    public int CurrentTurn { get; set; } = 1;
    public Correlation? CurrentCorrelation { get; set; }

    // public Offer Offer { get; set; } = new() { RedCoin = 10, BlueCoin = 10, YellowCoin = 10};
    
    public int MaxCoinAmount { get; set; } = 10;
    
    public Queue<Correlation> Correlations { get; } = Correlation.InitCorrelationList();
    public List<User> Users { get; set; }

    public void ResetGameState()
    {
        CurrentGame++;
        CurrentTurn = 1;
        CurrentEvent = Event.MakeOffer;
        MaxCoinAmount = 10;
        CurrentCorrelation = Correlations.Dequeue();
    } 
}
