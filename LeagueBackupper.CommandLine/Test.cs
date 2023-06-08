using System.Security.Cryptography;
using LeagueBackupper.Common.Utils;
using LeagueBackupper.Core;
using LeagueBackupper.Core.ClientData;
using LeagueBackupper.Core.DataBlock;
using LeagueBackupper.Core.MultiChunkFile;
using LeagueBackupper.Core.MultiChunkFileDataStorage;
using LeagueBackupper.Core.Pipeline;
using LeagueBackupper.Core.Pipeline.PatchOutput;
using LeagueBackupper.Core.Structure;
using Serilog;

namespace LeagueBackupper.CommandLine;

public static class Test
{
    public static void Backup()
    {
        ClientDataProvider clientDataProvider =
            new GameFolderClientDataProvider(@"E:\Temp\vcs\game13_4", new LogFilter());
        IPatchManager patchManager = new JsonPatchManager("E:/leaguaVCS/versions");
        BackupDataStorager backupDataStorager = new MultiChunkFileBackupDataStorager("E:/leaguaVCS/storage");
        string clientVersion = clientDataProvider.GetClientVersion();
        PatchBackupPipeline pipeline = new PatchBackupPipeline(clientDataProvider, patchManager, backupDataStorager);
        Log.Information(clientVersion);
        pipeline.Backup();
        PatchInfo patchInfo = clientDataProvider.GetPatch();
        patchManager.RecordPatch(patchInfo);
    }

    public static void Export()
    {
        IPatchManager patchManager = new JsonPatchManager("E:/leaguaVCS/versions");
        IBackupDataProvider backupDataProvider = new MultiChunkFileBackupDataProvider("E:/leaguaVCS/storage",
            new JsonPatchMultiChunkFileInfoManager("e"),
            new ZipFilesDataBlockProvider()
        );
        IPatchFileOutputStreamWriter exporter =
            new WinFileSystemPatchOutputStreamWriter(@"D:\Game\WeGameApps\英雄联盟\Game444");
        IPatchFileOutputStreamWriter verifier = new PatchFileVerificationOutputStreamWriter();
        PatchExtractPipeline patchExtractPipeline =
            new PatchExtractPipeline(backupDataProvider, patchManager, verifier);
        patchExtractPipeline.Extract("13.4.494.4321");
    }

    public static void Verify()
    {
        IPatchManager patchManager = new JsonPatchManager("E:/leaguaVCS/versions");
        IBackupDataProvider backupDataProvider = new MultiChunkFileBackupDataProvider("E:/leaguaVCS/storage",
            new JsonPatchMultiChunkFileInfoManager(""),
            new ZipDataBlockProvider(""));
        IPatchFileOutputStreamWriter exporter =
            new WinFileSystemPatchOutputStreamWriter(@"D:\Game\WeGameApps\英雄联盟\Game444");
        IPatchFileOutputStreamWriter verifier = new PatchFileVerificationOutputStreamWriter();
        var root = @"D:\Game\WeGameApps\英雄联盟\Game444";
        PatchInfo patchInfo = patchManager.GetPatchInfo("13.8.504.8951");
        MD5 md5 = MD5.Create();

        foreach (var file in patchInfo.PatchFiles)
        {
            if (file.Filename.EndsWith(".client"))
            {
                using var fs = File.OpenRead(Path.Combine(root, file.Filename));
                byte[] computeHash = md5.ComputeHash(fs);
                string hashString = Utils.GetHashString(computeHash);
                if (hashString != file.Hash)
                {
                    Log.Error($" {file.Filename} {file.Hash} {hashString}");
                }
            }
        }
    }


    public static void Import2()
    {
        ClientDataProvider clientDataProvider =
            new GameFolderClientDataProvider(@"E:\VerTest\Game\", new LogFilter());
        IPatchManager patchManager = new JsonPatchManager("E:/VerTest/versions");
        BackupDataStorager backupDataStorager = new MultiChunkFileBackupDataStorager("E:/VerTest/storage");
        string clientVersion = clientDataProvider.GetClientVersion();
        PatchBackupPipeline pipeline = new PatchBackupPipeline(clientDataProvider, patchManager, backupDataStorager);
        Log.Information(clientVersion);
        pipeline.Backup();
        PatchInfo patchInfo = clientDataProvider.GetPatch();
        patchManager.RecordPatch(patchInfo);
    }

    public static void Export2()
    {
        string exportRoot = "E:/VerTest/Export";
        IPatchManager patchManager = new JsonPatchManager("E:/VerTest/versions");
        IBackupDataProvider backupDataProvider = new MultiChunkFileBackupDataProvider("E:/VerTest/storage",
            new JsonPatchMultiChunkFileInfoManager("patchFiles.json"),
            new ZipFilesDataBlockProvider().WithZipFiles(new List<string>()
            {
            }).Load()
        );
        IPatchFileOutputStreamWriter exporter = new WinFileSystemPatchOutputStreamWriter(exportRoot);
        IPatchFileOutputStreamWriter verifier = new PatchFileVerificationOutputStreamWriter();
        PatchExtractPipeline patchExtractPipeline =
            new PatchExtractPipeline(backupDataProvider, patchManager, exporter);

        // var root = @"D:\Game\WeGameApps\英雄联盟\Game444";
        MD5 md5 = MD5.Create();
        patchExtractPipeline.Extract("13.8.504.8951");

        PatchInfo patchInfo = patchManager.GetPatchInfo("13.8.504.8951");
        foreach (var file in patchInfo.PatchFiles)
        {
            if (file.Filename.EndsWith(".client"))
            {
                using var fs = File.OpenRead(Path.Combine(exportRoot, file.Filename));
                byte[] computeHash = md5.ComputeHash(fs);
                string hashString = Utils.GetHashString(computeHash);
                if (hashString != file.Hash)
                {
                    Log.Error($" {file.Filename} {file.Hash} {hashString}");
                }
            }
        }
    }

    public static void MyStreamTest()
    {
        MemoryStream ms1 = new MemoryStream(4);
        MemoryStream ms2 = new MemoryStream(4);
        MemoryStream ms3 = new MemoryStream(4);
        BinaryWriter bw = new BinaryWriter(ms1);
        bw.Write((byte)1);
        bw.Write((byte)2);
        bw.Write((byte)3);
        bw.Write((byte)4);
        bw = new BinaryWriter(ms2);
        bw.Write((byte)5);
        bw.Write((byte)6);
        bw.Write((byte)7);
        bw.Write((byte)8);
        bw = new BinaryWriter(ms3);
        bw.Write((byte)9);
        bw.Write((byte)10);
        bw.Write((byte)11);
        bw.Write((byte)12);

        AggregatedStream mbs = new AggregatedStream(new List<Stream>() { ms1, ms2, ms3 });
        long mbsLength = mbs.Length;
        int readCnt = 0;
        var buff = new byte[5];
        mbs.Seek(3, SeekOrigin.Begin);
        while (true)
        {
            readCnt = mbs.Read(buff, 0, buff.Length);
            if (readCnt <= 0)
            {
                break;
            }

            for (var index = 0; index < readCnt; index++)
            {
                var by = buff[index];
                Console.WriteLine(by);
            }
        }
    }

    public static void test()
    {
        var fsO = File.OpenRead(@"E:\VerTest\Game\Aatrox.en_US.wad.client");
        var fsO2 = File.OpenRead(@"E:\VerTest\Export\Aatrox.en_US.wad.client");

        int pos = 0;
        while (true)
        {
            int readByte = fsO.ReadByte();
            int b = fsO2.ReadByte();
            if (readByte != b)
            {
            }

            pos++;
        }
    }
}