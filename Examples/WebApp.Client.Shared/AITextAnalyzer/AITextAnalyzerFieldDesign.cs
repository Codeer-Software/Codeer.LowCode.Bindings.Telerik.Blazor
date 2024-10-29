using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using WebApp.Client.Shared.AITextAnalyzer;
using WebApp.Client.Shared.Samples.AIDocumentAnalyzer;

//Namespace fixed for consistency with sample designer project
namespace Design.Samples.AIDocumentAnalyzer
{
    [ToolboxIcon(PackIconMaterialKind = "HeadSnowflakeOutline")]
    public class AITextAnalyzerFieldDesign() : FieldDesignBase(typeof(AITextAnalyzerFieldDesign).FullName!)
    {
        public override string GetWebComponentTypeFullName() => typeof(AITextAnalyzerFieldComponent).FullName!;
        public override string GetSearchWebComponentTypeFullName() => string.Empty;
        public override string GetSearchControlTypeFullName() => string.Empty;
        public override FieldBase CreateField() => new AITextAnalyzerField(this);
        public override FieldDataBase? CreateData() => null;
    }
}
