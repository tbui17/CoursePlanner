namespace BuildLib.CloudServices.GooglePlay;

public class ExpiryTime(int seconds = 3600)
{
    public int GetValue()
    {
        if (seconds <= 0)
        {
            throw new ArgumentException("Expiry time must be greater than 0");
        }

        return seconds;
    }

    public static implicit operator string(ExpiryTime expiryTime) => expiryTime.GetValue().ToString();
    public static implicit operator ExpiryTime(int seconds) => new(seconds);
}