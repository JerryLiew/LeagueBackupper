using System.Security.Cryptography;
using System.Text;

namespace LeagueBackupper.Common.Utils;

public class Utils
{
    public static string GetMD5Hash(string fileName)
    {
        try
        {
            using var file = new FileStream(fileName, FileMode.Open);
            var md5 = MD5.Create();
            var retVal = md5.ComputeHash(file);
            file.Close();
            var sb = new StringBuilder(30);
            for (var i = 0; i < retVal.Length; i++) sb.Append(retVal[i].ToString("x2"));
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
        }
    }

    public static string GetHashString(byte[] hash)
    {
        
        var sb = new StringBuilder(30);
        for (var i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("x2"));
        return sb.ToString();
    }
    
    public static byte[] GetBytesFromHashString(string hashString)
    {
        if (hashString.Length % 2 != 0)
            throw new ArgumentException("Invalid hash string length");

        var bytes = new byte[hashString.Length / 2];
        for (int i = 0; i < hashString.Length; i += 2)
            bytes[i / 2] = Convert.ToByte(hashString.Substring(i, 2), 16);

        return bytes;
    }

}