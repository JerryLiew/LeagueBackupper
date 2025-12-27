using System.Diagnostics;
using ZRUtils;
using ZRUtils.Extensions;

namespace LeagueBackupper.Utility;

public static class ClientUtility
{
   public const string LolExeName = "League of Legends.exe";
    public static string? GetPatchVersion(string fileName)
    {
        if (fileName != LolExeName)
        {
            return null;
        }
        return FileVersionInfo.GetVersionInfo(fileName).ProductVersion;
    }
    public static string? FindClientFolder(string folderToSearch)
    { 
        FileTreeWalker walker = new FileTreeWalker(folderToSearch,false);
        foreach (var (root, folders, files) in walker)
        {
            foreach (var f in files)
            {
                if (f == LolExeName)
                {
                    return root;
                }
            }
        }

        return null;
    }
    
    public static void SearchAndRenameClientParentFolderName(string folderToSearch)
    {
        var tuples = OSUtils.Walk(folderToSearch);
        foreach (var (root, folders, files) in tuples)
        {
            bool containsExe = false;
            foreach (var f in files)
            {
                if (f.Name == "League of Legends.exe")
                {
                    containsExe = true;
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(f.FullName);
                    string folderName = PathUtility.GetFolderName(root);
                    // string replace = root.Replace(folderName,fileVersionInfo.ProductVersion);
                    // string result = string.Empty;
                    string trimEnd = root.TrimEnd(folderName.ToCharArray());
                    var combine = Path.Combine(trimEnd, fileVersionInfo.ProductVersion);
                    Console.WriteLine(combine);
                    if (root != combine)
                    {
                        Directory.Move(root, combine);
                    }

                    break;
                }
            }

            if (containsExe)
            {
                continue;
            }
        }
    }
}