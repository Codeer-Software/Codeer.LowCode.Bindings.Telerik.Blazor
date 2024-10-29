using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure.AI.OpenAI;
using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using UglyToad.PdfPig;
using WebApp.Server.Services;

namespace WebApp.Server.Controllers
{
    [ApiController]
    [Route("api/ai_text_analyze")]
    public class AITextAnalyzeController : ControllerBase
    {
        [HttpPost("file")]
        public async Task<ModuleData> FileToDataAsync(string? moduleName, string? fileName)
        {
            var memoryStream = new MemoryStream();
            await Request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            var text = await ExtractText(fileName ?? string.Empty, memoryStream);
            return await TextToDataAsyncCore(moduleName, text);
        }

        [HttpPost("text")]
        public async Task<ModuleData> TextToDataAsync(string? moduleName, [FromForm] string? text)
            => await TextToDataAsyncCore(moduleName, text ?? string.Empty);

        static async Task<ModuleData> TextToDataAsyncCore(string? moduleName, string text)
        {
            var json = await DocumentAnalysisByText(DesignerService.GetDesignData().Modules, moduleName ?? string.Empty, text);
            return CreateModule(DesignerService.GetDesignData().Modules, moduleName ?? string.Empty, JsonSerializer.Deserialize<JsonElement>(json));
        }

        static async Task<string> DocumentAnalysisByText(List<ModuleDesign> moduleDesigns, string moduleName, string text)
        {
            var config = SystemConfig.Instance.AISettings;

            var azureClient = new AzureOpenAIClient(
                new Uri(config.OpenAIEndPoint),
                new ApiKeyCredential(config.OpenAIKey));
            var chatClient = azureClient.GetChatClient(config.ChatModel);

            var completion = await chatClient.CompleteChatAsync(
                [
                    new SystemChatMessage(@"
You are in charge of retrieving specific data from within a text. You will be passed instructions for the data to be extracted and the text, so please return the data in JSON.
The instructions are the field name and, if applicable, enter an auxiliary name in (). Use the field name in JSON.
Some are arrays. In that case, specify recursively with [{child element field instructions}].
Your answers will be used in the program, so please use JSON only.
Please don't really write anything like ""I understand"" or ""``json""."),
                    new UserChatMessage(CreateJsonExplanation(moduleDesigns, moduleName)),
                    new UserChatMessage(text),
                ]);
            return completion.Value.Content.FirstOrDefault()?.Text ?? string.Empty;
        }

        static async Task<string> ExtractText(string fileName, MemoryStream stream)
        {
            switch (Path.GetExtension(fileName).ToLower())
            {
                case ".pdf":
                    return ExtractTextFromPdf(stream);
                case ".jpg":
                case ".jpeg":
                case ".png":
                    return await ExtractTextFromImage(stream);
            }
            throw LowCodeException.Create("Invalid file type");
        }

        static async Task<string> ExtractTextFromImage(MemoryStream stream)
        {
            var config = SystemConfig.Instance.AISettings;
            var client = new DocumentAnalysisClient(new Uri(config.DocumentAnalysisEndPoint), new AzureKeyCredential(config.DocumentAnalysisKey));
            var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", stream);
            return string.Join(Environment.NewLine, operation.Value.Pages.SelectMany(e => e.Lines).Select(e => e.Content));
        }

        static string ExtractTextFromPdf(MemoryStream stream)
        {
            using var document = PdfDocument.Open(stream);
            return string.Join(Environment.NewLine, document.GetPages().Select(p => p.Text));
        }

        static ModuleData CreateModule(List<ModuleDesign> moduleDesigns, string moduleName, JsonElement root)
        {
            var moduleDesign = moduleDesigns.FirstOrDefault(e => e.Name == moduleName);
            if (moduleDesign == null) throw LowCodeException.Create($"Invalid Module {moduleName}");

            var moduleData = new ModuleData { Name = moduleDesign.Name };
            foreach (var element in root.EnumerateObject())
            {
                var fieldDesign = moduleDesign.Fields.FirstOrDefault(e => e.Name == element.Name);
                if (fieldDesign == null) continue;
                var value = GetValue(element.Value);
                var data = fieldDesign.CreateData();

                try
                {
                    if (data is BooleanFieldData booleanData) booleanData.Value = Convert.ToBoolean(value);
                    else if (data is TextFieldData textData) textData.Value = Convert.ToString(value);
                    else if (data is NumberFieldData numberData) numberData.Value = Convert.ToDecimal(value);
                    else if (data is DateFieldData dateData) dateData.Value = DateOnly.FromDateTime(Convert.ToDateTime(value));
                    else if (data is DateTimeFieldData dateTimeData) dateTimeData.Value = Convert.ToDateTime(value);
                    else if (data is TimeFieldData TimeData) TimeData.Value = TimeOnly.Parse(value?.ToString() ?? string.Empty);
                    else if (data is ListFieldData ListData)
                    {
                        var childModuleName = ((ListFieldDesign)fieldDesign).SearchCondition.ModuleName;
                        foreach (var e in element.Value.EnumerateArray())
                        {
                            ListData.Children.Add(CreateModule(moduleDesigns, childModuleName, e));
                        }
                    }
                    else continue;
                }
                catch
                {
                    continue;
                }
                moduleData.Fields.Add(element.Name, data);
            }
            return moduleData;
        }

        static object? GetValue(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.String) return element.GetString();
            else if (element.ValueKind == JsonValueKind.Number) return element.GetDecimal();
            else if (element.ValueKind == JsonValueKind.True) return true;
            else if (element.ValueKind == JsonValueKind.False) return false;
            return element;
        }

        static string CreateJsonExplanation(List<ModuleDesign> moduleDesigns, string moduleName)
        {
            var moduleDesign = moduleDesigns.FirstOrDefault(e => e.Name == moduleName);
            if (moduleDesign == null) throw LowCodeException.Create($"Invalid Module {moduleName}");

            var list = new List<string>();
            foreach (var field in moduleDesign.Fields)
            {
                var explanation = (field is IDisplayName diplayName && !string.IsNullOrEmpty(diplayName.DisplayName)) ? $"{field.Name}({diplayName.DisplayName})" : field.Name;
                if (field is ListFieldDesign listFieldDesign)
                {
                    explanation += $"[{CreateJsonExplanation(moduleDesigns, listFieldDesign.SearchCondition.ModuleName)}]";
                }
                list.Add(explanation);
            }
            return string.Join(",", list);
        }
    }
}
