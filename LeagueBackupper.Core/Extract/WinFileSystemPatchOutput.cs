using LeagueBackupper.Core.Pipeline;
using LeagueBackupper.Core.Structure;

namespace LeagueBackupper.Core.PatchOutput;

public class GameClientRebuilder : ExtractedPatchDataProcessor
{
    private readonly string _dstFolder;

    public GameClientRebuilder(string exportTo)
    {
        _dstFolder = exportTo;
    }

    public override void Init(PatchInfo version)
    {
        if (!Directory.Exists(_dstFolder))
        {
            Directory.CreateDirectory(_dstFolder);
            Log.Info($"The destination folder for exporting version files does not exist, creat it now.");
        }
    }

    public override void ProcessPatchFileStream(PatchFileInfo patchFileInfo, Stream outputStream)
    {
        var fullPath = Path.Combine(_dstFolder, patchFileInfo.Filename);
        string directoryName = Path.GetDirectoryName(fullPath)!;
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        using var fs = File.OpenWrite(fullPath);
        byte[] buffer = new byte[1024 * 1024 * 2];
        Span<byte> asSpan = buffer.AsSpan();
        int readCnt = 0;
        while ((readCnt = outputStream.Read(asSpan)) > 0)
        {
            fs.Write(buffer, 0, readCnt);
        }
    }


    public override void Complete()
    {
        //nothing to do.
    }
}