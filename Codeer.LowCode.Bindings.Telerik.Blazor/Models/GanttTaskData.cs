using Telerik.Blazor;

namespace Codeer.LowCode.Bindings.Telerik.Blazor.Models
{
    public class GanttTaskData
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public double Progress { get; set; } = 0;
        public string? ParentId { get; set; } = string.Empty;
    }

    public class DependencyListItem
    {
        public string DependencyId { get; set; } = string.Empty;
        public string PredecessorId { get; set; } = string.Empty;
        public string SuccessorId { get; set;} = string.Empty;
        public GanttDependencyType Type { get; set;} = GanttDependencyType.FinishStart;
    }
}
