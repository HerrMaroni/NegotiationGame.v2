using System.Text.RegularExpressions;
namespace NegotiationGame.v2.Shared;

public class RoomPinValidator 
{
    public static IEnumerable<string> IsValidRoomName(string roomName)
    {
        if (string.IsNullOrWhiteSpace(roomName))
        {
            yield return "Room name is required!";
            yield break;
        }
        if (roomName.Length > 20)
            yield return "Room name must not be longer than 20 characters";
    }
    
    public static IEnumerable<string> IsValidPin(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin))
        {
            yield return "Pin is required!";
            yield break;
        }
        if (pin.Length != 4)
            yield return "Pin must be of length 4";
        if (!Regex.IsMatch(pin, @"^\d{4}$"))
            yield return "Pin must consist of digits only";
    }
}
