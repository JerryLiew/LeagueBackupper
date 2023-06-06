using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
using System.Text.Json;
using LeagueBackupper.Core.Structure;

namespace Test;

[TestClass]
public class UnitTest1
{
    class A
    {
        public int Age { get; set; }
        public required string Name { get; set; }

        public byte [] Data { get; set; }

        [SetsRequiredMembers]
        public A(string name)
        {
            Name = name;
        }
    }
    [TestMethod]
    public void TestMethod1()
    {
       
        var options = new JsonSerializerOptions();
        // options.Converters.Add(new LeagueVersionJsonConvertFactory());
        string serialize = JsonSerializer.Serialize(new A("dsf") { Age = 1, Name = "草拟吗" ,Data = new byte[] {0,1,2,3}}, options);
        // string serialize2 = JsonSerializer.Serialize(new LeagueVersion("123"){VersionFiles = new List<VersionFile>()});
        string serialize2 = JsonSerializer.Serialize(new PatchInfo("123"){PatchFiles = new List<PatchFileInfo>()});

        Console.WriteLine(serialize2);
        // ImportPipeline pipeline = new ImportPipeline("D://",@"D:\Game\WeGameApps\英雄联盟\Game33");
        // pipeline.Import();
    }

    [TestMethod]
    public void TestMethod2()
    {
        // WadFile wadFile = new WadFile(File.OpenRead(@"D:\Game\WeGameApps\英雄联盟\Game33\DATA\FINAL\Maps\Shipping\Map11.en_US.wad.client"));
        
    }
}