namespace NegotiationGame.v2.Shared.GameDomain;

public class Correlation
{
    public string Mode { get; set; }
    public List<int> PlayerAValues { get; set; }
    public List<int> PlayerBValues { get; set; }

    public static Queue<Correlation> InitCorrelationList()
    {
        var random = new Random();
        var correlationList = new List<Correlation>(new Correlation[] 
        {
            new()
            {
                Mode = "competitive",
                PlayerAValues = new List<int>(new[] { 5, 4, 3 }),
                PlayerBValues = new List<int>(new[] { 6, 4, 2 })
            },
            new()
            {
                Mode = "neutral",
                PlayerAValues = new List<int>(new[] { 3, 4, 3 }),
                PlayerBValues = new List<int>(new[] { 6, 4, 2 })
            },
            new()
            {
                Mode = "cooperative",
                PlayerAValues = new List<int>(new[] { 5, 4, 3 }),
                PlayerBValues = new List<int>(new[] { 2, 4, 6 })
            },
        });
        correlationList = correlationList.OrderBy(_ => random.Next()).ToList();
        return new Queue<Correlation>(correlationList);
    }
}
