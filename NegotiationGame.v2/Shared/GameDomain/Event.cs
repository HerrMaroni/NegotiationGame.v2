namespace NegotiationGame.v2.Shared.GameDomain;

public enum Event
{
    StartGame = 1,
    StartRound = 2,
    DefaultView = 3,
    MakeOffer = 4,
    AcceptOffer = 5,
    DeclineOffer = 6,
    EndRound = 7,
    EndGame = 8
}