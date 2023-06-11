using LeagueBackupper.Core.Pipeline;

namespace LeagueBackupper.Core;

public abstract class ClientDataProvider
{
    public abstract string GetClientVersion();
    
    public abstract List<ClientFileInfo> EnumerateFiles();
    
    public abstract Stream GetClientFileStream(string filename);
    
    
    public abstract byte[] GetClientFileHash(string filename);

}