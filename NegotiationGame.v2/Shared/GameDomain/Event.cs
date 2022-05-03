namespace NegotiationGame.v2.Shared.GameDomain;

public enum Event
{
    StartGame = 1,
    StartRound = 2,
    MakeOffer = 4,
    MakeDecision = 5,
    AcceptOffer = 6,
    DeclineOffer = 7,
    EndRound = 8,
    EndGame = 9
}