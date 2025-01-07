namespace Codeer.LowCode.Bindings.Telerik.Blazor.Models
{
    [Flags]
    public enum GanttColumns : int
    {
        Name = 1 << 0,
        Progress = 1 << 1,
        Start = 1 << 2,
        End = 1 << 3,
    }
}
