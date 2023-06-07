using LeagueBackupper.Core.ClientData;
using LeagueBackupper.Core.DataBlock;
using LeagueBackupper.Core.MultiChunkFile;
using LeagueBackupper.Core.MultiChunkFileDataStorage;

namespace LeagueBackupper.Core.Pipeline;

public class DefaultPatchBackupPipelineBuilder : PatchBackupPipelineBuilder
{
    private readonly string _gameClientFolder;
    private string _patchInfoFolder;
    private string _patchDataFolder;
    private string _multiChunkFileInfoFolder;
    private readonly string _backupStorageFolder;


    public DefaultPatchBackupPipelineBuilder(string gameClientFolder, string backupStorageFolder)
    {
        _gameClientFolder = gameClientFolder;
        _backupStorageFolder = backupStorageFolder;
        _patchInfoFolder = Path.Combine(backupStorageFolder, "Patches");
        _patchDataFolder = Path.Combine(backupStorageFolder, "Data");
        _multiChunkFileInfoFolder = Path.Combine(backupStorageFolder, "MultiChunkFileInfo");
    }

    public override PatchBackupPipeline Build()
    {
        ClientDataProvider clientDataProvider =
            new GameFolderClientDataProvider(_gameClientFolder, new DefaultClientFileFilter());
        PatchManager patchManager = new JsonPatchManager(_patchInfoFolder);
        ChunkDataStorager chunkDataStorager = new ZipChunkDataStorager(_patchDataFolder);
        ChunkExistChecker chunkExistChecker = new ZippedDataBlockChecker().WithZipFilesFolder(_patchDataFolder);
        PatchMultiChunkFileInfoManager patchMultiChunkFileInfoManager =
            new JsonPatchMultiChunkFileInfoManager(_multiChunkFileInfoFolder);
        PatchFileDataStorager dataStorager = new DefaultPathDataStorager(
            chunkDataStorager,
            chunkExistChecker,
            patchMultiChunkFileInfoManager);
        PatchBackupPipeline pipeline = new PatchBackupPipeline(
            clientDataProvider,
            patchManager,
            dataStorager);
        return pipeline;
    }
}