namespace LeagueBackupper.Core;

public class PatchExistException : Exception
{

    public PatchExistException(string msg)
    {
        Message = msg;
    }
    public override string Message { get; }

}
public class PatchNotFoundException : Exception
{

    public PatchNotFoundException(string msg)
    {
        Message = msg;
    }
    public override string Message { get; }

}
