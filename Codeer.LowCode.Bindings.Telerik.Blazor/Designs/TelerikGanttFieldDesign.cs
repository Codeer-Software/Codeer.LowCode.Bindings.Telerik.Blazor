using Codeer.LowCode.Bindings.Telerik.Blazor.Components;
using Codeer.LowCode.Bindings.Telerik.Blazor.Fields;
using Codeer.LowCode.Bindings.Telerik.Blazor.Models;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;
using Telerik.Blazor;

namespace Codeer.LowCode.Bindings.Telerik.Blazor.Designs
{
    public class TelerikGanttFieldDesign() : FieldDesignBase(typeof(TelerikGanttFieldDesign).FullName!), IDisplayName,
        ISearchResultsViewFieldDesign
    {
        [Designer]
        public string DisplayName { get; set; } = string.Empty;

        [Designer]
        public EnabledGanttViews EnabledGanttViews { get; set; } =
            EnabledGanttViews.Day | EnabledGanttViews.Week | EnabledGanttViews.Month;

        [Designer]
        public GanttView InitialView { get; set; } = GanttView.Day;

        [Designer(Category = nameof(SearchCondition))]
        public SearchCondition SearchCondition { get; set; } = new();

        [Designer(Category = nameof(SearchCondition))]
        public string DetailLayoutName { get; set; } = string.Empty;

        [Designer(CandidateType = CandidateType.Field, Category = nameof(SearchCondition))]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        [TargetFieldType(Types = [typeof(IdFieldDesign)])]
        public string? IdField { get; set; }

        [Designer(CandidateType = CandidateType.Field, Category = nameof(SearchCondition))]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        [TargetFieldType(Types = [typeof(TextFieldDesign)])]
        public string? NameField { get; set; }

        [Designer(CandidateType = CandidateType.Field, Category = nameof(SearchCondition))]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        [TargetFieldType(Types = [typeof(DateTimeFieldDesign)])]
        public string? StartDateField { get; set; }

        [Designer(CandidateType = CandidateType.Field, Category = nameof(SearchCondition))]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        [TargetFieldType(Types = [typeof(DateTimeFieldDesign)])]
        public string? EndDateField { get; set; }

        [Designer(CandidateType = CandidateType.Field, Category = nameof(SearchCondition))]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        [TargetFieldType(Types = [typeof(NumberFieldDesign)])]
        public string? ProgressField { get; set; }

        [Designer(CandidateType = CandidateType.Field, Category = nameof(SearchCondition))]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        [TargetFieldType(Types = [typeof(LinkFieldDesign)])]
        public string? ParentIdField { get; set; }

        [Designer(CandidateType = CandidateType.Field, Category = nameof(SearchCondition))]
        [ModuleMember(Member = $"{nameof(SearchCondition)}.{nameof(SearchCondition.ModuleName)}")]
        [TargetFieldType(Types = [typeof(NumberFieldDesign)])]
        public string ProcessingCounterField { get; set; } = "";

        [Designer(Category = nameof(DependenciesModule))]
        public SearchCondition DependenciesModule { get; set; } = new();

        [Designer(CandidateType = CandidateType.Field, Category = nameof(DependenciesModule))]
        [ModuleMember(Member = $"{nameof(DependenciesModule)}.{nameof(DependenciesModule.ModuleName)}")]
        public string DependencySourceIdField { get; set; } = "";

        [Designer(CandidateType = CandidateType.Field, Category = nameof(DependenciesModule))]
        [ModuleMember(Member = $"{nameof(DependenciesModule)}.{nameof(DependenciesModule.ModuleName)}")]
        public string DependencyDestinationIdField { get; set; } = "";

        [Designer(CandidateType = CandidateType.ScriptEvent)]
        public string OnDataChanged { get; set; } = string.Empty;

        public override string GetWebComponentTypeFullName() => typeof(TelerikGanttFieldComponent).FullName!;

        public override string GetSearchWebComponentTypeFullName() => string.Empty;

        public override string GetSearchControlTypeFullName() => string.Empty;

        public override FieldDataBase? CreateData() => null;

        public override FieldBase CreateField() => new TelerikGanttField(this);

    }
}
