namespace BuildLib.Globals;

public static class RootEnvironment
{
    public static readonly string KeyUri = Environment.GetEnvironmentVariable("KEY_URI") ??
                                           throw new InvalidOperationException("KEY_URI is not set");
}