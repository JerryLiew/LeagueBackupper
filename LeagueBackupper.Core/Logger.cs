using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace LeagueBackupper.Core;

internal static class Log
{
    private static Action<string>? _infoLogger;
    private static Action<string>? _warnLogger;
    private static Action<string>? _errLogger;

    internal static void SetLogger(Action<string> infoLogger,Action<string> warnLogger, Action<string> errorLogger)
    {
        _infoLogger = infoLogger;
        _warnLogger = warnLogger;
        _errLogger = errorLogger;

    }

    internal static void Info(string msg)
    {
        _infoLogger?.Invoke(msg);
    }

    internal static void Warn(string msg)
    {
        _warnLogger?.Invoke(msg);
    }

    internal static void Err(string msg)
    {
        _errLogger?.Invoke(msg);
    }
}