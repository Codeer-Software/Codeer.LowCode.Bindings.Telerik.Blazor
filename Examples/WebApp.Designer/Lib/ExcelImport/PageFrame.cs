using Codeer.LowCode.Blazor.Json;
using Codeer.LowCode.Blazor.Repository.Design;
using System.IO;

namespace WebApp.Designer.Lib.ExcelImport
{
    internal static class PageFrame
    {
        internal static void WriteToMain(string path, List<ModuleDesign> modules)
        {
            var pageFrame = new PageFrameDesign();
            try
            {
                var pageFrameJson = File.ReadAllText(path);
                pageFrame = JsonConverterEx.DeserializeObject<PageFrameDesign>(pageFrameJson) ?? new PageFrameDesign();
            }
            catch { }

            foreach (var module in modules)
            {
                if (pageFrame.Left.Links.Any(e => e.Module == module.Name)) continue;
                pageFrame.Left.Links.Add(new PageLink
                {
                    Module = module.Name,
                    Title = module.Name,
                });
            }

            File.WriteAllText(path, JsonConverterEx.SerializeObject(pageFrame));
        }
    }
}
