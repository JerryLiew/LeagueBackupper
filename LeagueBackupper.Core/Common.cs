namespace LeagueBackupper.Core;

public static class Core
{
    public static void SetLogger(Action<string> infoLogger, Action<string> warnLogger, Action<string> errorLogger)
    {
        Log.SetLogger(infoLogger, warnLogger, errorLogger);
    }
}