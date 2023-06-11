namespace LeagueBackupper.Common.Utils;


public static class OSHelper
{
    private static readonly List<Tuple<string, DirectoryInfo[], FileInfo[]>> DirInfo = new();

    /// <summary>
    ///     Traverse the specific folder in a python-style way.
    /// </summary>
    /// <param name="rootPath">Folder you want to traverse</param>
    /// <returns>Traverse result</returns>
    public static List<Tuple<string, DirectoryInfo[], FileInfo[]>> Walk(string rootPath)
    {
        DirInfo.Clear();
        return InternalWalk(rootPath);
    }

    private static List<Tuple<string, DirectoryInfo[], FileInfo[]>> InternalWalk(string rootPath)
    {
        var root = new DirectoryInfo(rootPath);
        var directories = root.GetDirectories();
        var files = root.GetFiles();
        var currentDirInfo
            = Tuple.Create(root.FullName, directories, files);
        DirInfo.Add(currentDirInfo);

        // 递归调用子文件夹
        foreach (var dir in directories) InternalWalk(dir.FullName);

        return DirInfo;
    }
}