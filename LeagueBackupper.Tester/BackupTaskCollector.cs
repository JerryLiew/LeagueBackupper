using System.Text.RegularExpressions;
using LeagueBackupper.Utility;
using ZRUtils;
using ZRUtils.Extensions;

namespace LeagueBackupper.Tester;

public class BackupTaskCollector
{
    //决定一个根目录，然后收集所有子文件夹下面的所有符合要求的客户端。。他collect collect, collect。那这个？
    public List<BackupTask> Collect(string rootFolder)
    {
        Regex regex = new Regex(@"[\d.]+");
        List<BackupTask> result = new List<BackupTask>();
        FileTreeWalker fileWalker = new FileTreeWalker(rootFolder, useFullPath: false);
        foreach (var ftNode in fileWalker)
        {
            var files = ftNode.Files;
            foreach (var f in files)
            {
                var file = Path.Combine(ftNode.Root, f);
                var patchVersion = ClientUtility.GetPatchVersion(file);
                if (patchVersion != null)
                {
                    BackupTask t = new BackupTask();
                    t.PatchVersion = patchVersion;
                    t.Type = TaskType.Unzipped;
                    t.Path = ftNode.Root;
                    result.Add(t);
                    break;
                }

                string fileName = Path.GetFileNameWithoutExtension(file);
                if (file.EndsWith(".zip") && regex.IsMatch(fileName))
                {
                    BackupTask t = new BackupTask();
                    t.PatchVersion = regex.Match(fileName).Value;
                    t.Path = file;
                    t.Type = TaskType.Zipped;
                    result.Add(t);
                }
            }
        }

        return result;
    }
}