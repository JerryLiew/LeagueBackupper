using LeagueBackupper.Utility;

namespace Test;

[TestClass]
public class UtilityTest
{

    [TestMethod]
    public void Test_SearchAndRenameClientParentFolderName()
    {
        
        GameFolderUtility.SearchAndRenameClientParentFolderName("G:\\z2");
    }
    
    
}