namespace BuildLib.CloudServices.GooglePlay;

public class ExpiryTime(long seconds = 3600)
{
    public long GetValue()
    {
        if (seconds <= 0)
        {
            throw new ArgumentException("Expiry time must be greater than 0");
        }

        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() + seconds;
    }

    public static implicit operator string(ExpiryTime expiryTime) => expiryTime.GetValue().ToString();
    public static implicit operator ExpiryTime(int seconds) => new(seconds);
}