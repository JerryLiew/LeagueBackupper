using System.Reflection.Metadata;

namespace LeagueBackupper.Core;

public enum ImportPipelineType
{
    Standard
}

public class PipelineFactory
{
    IImportPipeline GetImportPipeline(ImportPipelineType pipelineType)
    {
        switch (pipelineType)
        {
            case ImportPipelineType.Standard:
                return BuildStandardImportPipeline();
                    
            default:
                return BuildStandardImportPipeline();
        }
    }

    private IImportPipeline BuildStandardImportPipeline()
    {
        throw new NotImplementedException();
    }
}