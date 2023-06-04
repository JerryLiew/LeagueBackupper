using LeagueBackupper.Core.ClientData;
using LeagueBackupper.Core.DataBlock;
using LeagueBackupper.Core.MultiChunkFile;
using LeagueBackupper.Core.MultiChunkFileDataStorage;
using LeagueBackupper.Core.MultiChunkFileDataStorage.DataBlock;
using LeagueBackupper.Core.PatchOutput;

namespace LeagueBackupper.Core.Pipeline;

public class DefaultPatchExtractPipelineBuilder : PatchExtractPipelineBuilder
{
    private readonly string dstFolder;
    private string _patchInfoFolder;
    private string _chunkDataFolder;
    private string _multiChunkFileInfoFolder;
    private bool _validateOnly = false;


    public DefaultPatchExtractPipelineBuilder(string backupStorageFolder, string dst, bool validateOnly = false)
    {
        dstFolder = dst;
        _patchInfoFolder = Path.Combine(backupStorageFolder, "Patches");
        _chunkDataFolder = Path.Combine(backupStorageFolder, "Data");
        _multiChunkFileInfoFolder = Path.Combine(backupStorageFolder, "MultiChunkFileInfo");
        _validateOnly = validateOnly;
    }

    public override PatchExtractPipeline Build()
    {
        PatchManager patchManager = new JsonPatchManager(_patchInfoFolder);
        PatchMultiChunkFileInfoManager patchMultiChunkFileInfoManager =
            new JsonPatchMultiChunkFileInfoManager(_multiChunkFileInfoFolder);

        ChunkDataProvider chunkDataProvider = new ZipFilesChunkDataProvider().WithFolder(_chunkDataFolder);
        PatchDataProvider dataProvider = new DefaultPatchDataProvider(
            patchMultiChunkFileInfoManager,
            chunkDataProvider
        );
        ExtractedPatchDataProcessor extractedPatchDataProcessor = null!;
        if (_validateOnly)
        {
            extractedPatchDataProcessor = new ClientDataValidator();
        }
        else
        {
            extractedPatchDataProcessor = new GameClientRebuilder(dstFolder);
        }

        PatchExtractPipeline pipeline = new PatchExtractPipeline(
            dataProvider,
            patchManager,
            extractedPatchDataProcessor
        );
        return pipeline;
    }
}