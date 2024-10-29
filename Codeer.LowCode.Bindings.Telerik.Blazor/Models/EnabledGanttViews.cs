namespace Codeer.LowCode.Bindings.Telerik.Blazor.Models
{
    [Flags]
    public enum EnabledGanttViews
    {
        Day = 1 << 0,
        Week = 1 << 1,
        Month = 1 << 2,
        Year =  1 << 3
    }
}
