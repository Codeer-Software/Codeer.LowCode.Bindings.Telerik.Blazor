using Codeer.LowCode.Blazor.Repository.Data;

namespace WebApp.Client.Shared.AITextAnalyzer
{
    public interface IAITextAnalyzerCore
    {
        Task<ModuleData?> FileToModuleData(string moduleName, string fileName, StreamContent content);
        Task<ModuleData?> TextToModuleData(string moduleName, string text);
    }
}
