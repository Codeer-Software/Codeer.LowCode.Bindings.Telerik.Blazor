using Codeer.LowCode.Bindings.Telerik.Blazor.Designs;
using Codeer.LowCode.Bindings.Telerik.Blazor.Models;
using Codeer.LowCode.Blazor;
using Codeer.LowCode.Blazor.DataIO;
using Codeer.LowCode.Blazor.DesignLogic;
using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.Repository.Data;
using Codeer.LowCode.Blazor.Repository.Design;
using Codeer.LowCode.Blazor.Repository.Match;
using Codeer.LowCode.Blazor.Script;
using Codeer.LowCode.Blazor.Script.Internal.ScriptServices;
using Telerik.Blazor;

namespace Codeer.LowCode.Bindings.Telerik.Blazor.Fields
{
    public class TelerikGanttField(TelerikGanttFieldDesign design) : FieldBase<TelerikGanttFieldDesign>(design), ISearchResultsViewField
    {
        private List<ModuleGanttTaskData> _tasks = new();
        private Dictionary<string, string[]> _dependencies = new();
        private SearchCondition? _additionalCondition;

        private bool AllowLoad { get; set; } = true;

        public GanttView CurrentView { get; set; } = design.InitialView;

        [ScriptHide]
        public Func<SearchCondition?, Task> OnQueryChangedAsync { get; set; } = _ => Task.CompletedTask;

        [ScriptHide]
        public Func<Task> OnSearchDataChangedAsync { get; set; } = async () => await Task.CompletedTask;

        [ScriptHide]
        public Func<Task> OnDataChangedAsync { get; set; } = async () => await Task.CompletedTask;

        [ScriptHide]
        public string ModuleName => Design?.SearchCondition.ModuleName ?? string.Empty;


        public override bool IsModified => false;

        public List<GanttTaskData> Tasks => _tasks.OfType<GanttTaskData>().ToList();

        public List<DependencyListItem> Dependencies { get; set; } = new();

        [ScriptHide]
        public override async Task InitializeDataAsync(FieldDataBase? fieldDataBase)
        {
            if (this.IsInLayout()) await ReloadAsync();
        }

        [ScriptName("SetAdditionalCondition")]
        public async Task SetAdditionalConditionAsync(ModuleSearcher searcher)
            => await SetAdditionalConditionAsync(searcher.GetSearchCondition(), 0);

        [ScriptHide]
        public async Task SetAdditionalConditionAsync(SearchCondition condition, int page)
        {
            if (condition.ModuleName != Design.SearchCondition.ModuleName)
                throw LowCodeException.Create("{0} Invalid Module", Design.SearchCondition.ModuleName,
                    condition.ModuleName);
            _additionalCondition = condition;
            await OnQueryChangedAsync(GetSearchCondition());
        }

        [ScriptHide]
        public override FieldDataBase? GetData() => null;

        [ScriptHide]
        public override FieldSubmitData GetSubmitData() => new();

        [ScriptHide]
        public override Task SetDataAsync(FieldDataBase? fieldDataBase) => Task.CompletedTask;

        [ScriptHide]
        public override async Task OnExternalFieldChangedAsync(string fieldName)
        {
            if (!this.IsInLayout()) return;
            var searchCondition = GetSearchCondition();
            if (searchCondition.GetFieldVariableConditions()
                .All(e => new VariableName(e.Variable).FieldName.Root != fieldName)) return;
            await ReloadAsync();
        }

        [ScriptName("Reload")]
        public async Task ReloadAsync()
        {
            if (!AllowLoad) return;
            // 先にDepsを取得
            var depsItems = await this.GetChildModulesAsync(Design.DependenciesModule, ModuleLayoutType.Detail,
                Design.DetailLayoutName);
            _dependencies = depsItems.Select(ConvertToGanttDeps).GroupBy(e => e.Key)
                .ToDictionary(e => e.Key, e => e.Select(f => f.Value).ToArray());

            // タスク一覧を更新
            var items = await this.GetChildModulesAsync(GetSearchCondition(), ModuleLayoutType.Detail,
                Design.DetailLayoutName);
            _tasks = items.Select(ConvertToGanttTaskData).ToList();
            MakeDependencyList();
            NotifyStateChanged();
        }

        internal async Task ChangeDate(Module? module, DateTime startDate, DateTime endDate)
        {
            if (module is null) return;

            var start = module.GetField<DateTimeField>(Design.StartDateField);
            var end = module.GetField<DateTimeField>(Design.EndDateField);
            if (start != null) await start.SetValueAsync(startDate);
            if (end != null) await end.SetValueAsync(endDate);
            await UpdateAsync(module);
        }

        internal async Task ChangePercent(Module? module, double percent)
        {
            if (module is null) return;

            var progress = module.GetField<NumberField>(Design.ProgressField);
            if (progress != null) await progress.SetValueAsync((decimal)percent);
            await UpdateAsync(module);
        }

        internal Module? GetModule(string? id)
            => _tasks.Where(e => e.Id == id).FirstOrDefault()?.Module;

        internal async Task AddAsync()
        {
            var mod = await this.CreateChildModuleAsync(ModuleName, ModuleLayoutType.Detail, Design.DetailLayoutName);
            await mod.AssignRequiredCondition(Design.SearchCondition);

            var start = mod.GetField<DateTimeField>(Design.StartDateField);
            var end = mod.GetField<DateTimeField>(Design.EndDateField);
            if (start != null) await start.SetValueAsync(DateTime.Now);
            if (end != null) await end.SetValueAsync(DateTime.Now.AddDays(1));

            if (await mod.ShowDialogAsync("OK", "Cancel") != "OK") return;
            if (!mod.ValidateInput())
            {
                await Services.UIService.NotifyError("Input Error");
                return;
            }

            if (await mod.SubmitAsync() != true) return;

            _tasks.Add(ConvertToGanttTaskData(mod));

            await InvokeOnDataChangedAsync();
            await NotifyDataChangedAsync();
        }

        internal async Task AddAsync(GanttTaskData data)
        {
            var mod = await this.CreateChildModuleAsync(ModuleName, ModuleLayoutType.Detail, Design.DetailLayoutName);
            await mod.AssignRequiredCondition(Design.SearchCondition);

            // var id = mod.GetField<IdField>(Design.IdField);
            var name = mod.GetField<TextField>(Design.NameField);
            var start = mod.GetField<DateTimeField>(Design.StartDateField);
            var end = mod.GetField<DateTimeField>(Design.EndDateField);
            var progress = mod.GetField<NumberField>(Design.ProgressField);
            var parentId = mod.GetField<LinkField>(Design.ParentIdField);
            // if (id != null) await id.SetValueAsync(data.Id);
            if (name != null) await name.SetValueAsync(data.Name);
            if (start != null) await start.SetValueAsync(data.Start);
            if (end != null) await end.SetValueAsync(data.End);
            if (progress != null) await progress.SetValueAsync((decimal)data.Progress);
            if (parentId != null) await parentId.SetValueAsync(data.ParentId);

            if (!mod.ValidateInput())
            {
                await Services.UIService.NotifyError("Input Error");
                return;
            }

            if (await mod.SubmitAsync() != true) return;

            _tasks.Add(ConvertToGanttTaskData(mod));

            await InvokeOnDataChangedAsync();
            await NotifyDataChangedAsync();
        }

        internal async Task UpdateAsync(GanttTaskData data)
        {
            var module = _tasks.Find(task => task.Id == data.Id)?.Module;
            if (module is null) return;

            // var id = mod.GetField<IdField>(Design.IdField);
            var name = module.GetField<TextField>(Design.NameField);
            var start = module.GetField<DateTimeField>(Design.StartDateField);
            var end = module.GetField<DateTimeField>(Design.EndDateField);
            var progress = module.GetField<NumberField>(Design.ProgressField);
            var parentId = module.GetField<LinkField>(Design.ParentIdField);
            // if (id != null) await id.SetValueAsync(data.Id);
            if (name != null) await name.SetValueAsync(data.Name);
            if (start != null) await start.SetValueAsync(data.Start);
            if (end != null) await end.SetValueAsync(data.End);
            if (progress != null) await progress.SetValueAsync((decimal)data.Progress);
            if (parentId != null) await parentId.SetValueAsync(data.ParentId);

            if (!module.ValidateInput())
            {
                await Services.UIService.NotifyError("Input Error");
                return;
            }

            if (await module.SubmitAsync() != true) return;

            var index = _tasks.FindIndex(task => task.Id == data.Id);
            if (index >= 0)
            {
                _tasks[index] = ConvertToGanttTaskData(module);
            }

            await InvokeOnDataChangedAsync();
            await NotifyDataChangedAsync();
        }

        internal async Task DeleteAsync(GanttTaskData data)
        {
            var mod = _tasks.FirstOrDefault(task => task.Id == data.Id)?.Module;
            if (mod is null) return;
            await DeleteAsync(mod);

            await InvokeOnDataChangedAsync();
            await NotifyDataChangedAsync();
        }

        internal async Task EditAsync(Module? mod)
        {
            if (mod == null) return;

            switch (await mod.ShowDialogAsync("Update", "Delete", "Cancel"))
            {
                case "Update":
                    if (!mod.ValidateInput())
                    {
                        await Services.UIService.NotifyError("Input Error");
                        return;
                    }

                    if (await mod.SubmitAsync() != true) return;

                    var task = _tasks.FirstOrDefault(e => e.Id == mod.GetIdText());
                    if (task != null)
                    {
                        task.Name = mod.GetField<TextField>(Design.NameField)?.Value;
                        task.Start = mod.GetField<DateTimeField>(Design.StartDateField)?.Value ?? DateTime.MinValue;
                        task.End = mod.GetField<DateTimeField>(Design.EndDateField)?.Value ?? DateTime.MinValue;
                        var progress = mod.GetField<NumberField>(Design.ProgressField)?.Value;
                        task.Progress = int.TryParse(progress?.ToString() ?? string.Empty, out var val) ? val : 0;
                    }

                    break;
                case "Delete":
                    await DeleteAsync(mod);
                    return;
                default:
                    return;
            }

            await OnChildDataChangedAsync();
            await NotifyDataChangedAsync();
        }

        internal async Task AddDependencies(string predecessorId, string successorId)
        {
            var srcTask = _tasks.FirstOrDefault(e => e.Module?.GetIdText() == predecessorId);
            var dstTask = _tasks.FirstOrDefault(e => e.Module?.GetIdText() == successorId);
            if (srcTask == null || dstTask == null) return;

            var sourceCounterField = srcTask.Module?.GetField<NumberField>(Design.ProcessingCounterField);
            var destinationCounterField = dstTask.Module?.GetField<NumberField>(Design.ProcessingCounterField);
            if (sourceCounterField == null || destinationCounterField == null) return;
            await sourceCounterField.SetValueAsync(sourceCounterField.Value + 1);
            await destinationCounterField.SetValueAsync(destinationCounterField.Value + 1);

            var mod = await this.CreateChildModuleAsync(Design.DependenciesModule.ModuleName, ModuleLayoutType.None);

            var src = mod.GetField<IdField>(Design.DependencySourceIdField);
            var dst = mod.GetField<IdField>(Design.DependencyDestinationIdField);
            if (src == null || dst == null) return;
            await src.SetValueAsync(predecessorId);
            await dst.SetValueAsync(successorId);

            if (await mod.SubmitAsync([srcTask.Module, dstTask.Module]) != true) return;
            if (_dependencies.ContainsKey(successorId))
            {
                var deps = _dependencies[successorId].ToList();
                deps.Add(predecessorId);
                _dependencies[successorId] = deps.ToArray();
            }
            else
            {
                _dependencies[successorId] = [predecessorId];
            }

            MakeDependencyList();

            //モジュールの値を取り直す(楽観ロックの値を取得するため)
            var idVariable = new VariableName($"{Design.IdField}.Value");
            var condition = new SearchCondition
            {
                ModuleName = Design.SearchCondition.ModuleName,
                Condition = MultiMatchCondition.Or(idVariable.Equal(predecessorId), idVariable.Equal(successorId))
            };
            var mods = await this.GetChildModulesAsync(condition, ModuleLayoutType.Detail, Design.DetailLayoutName);
            srcTask.Module = mods.FirstOrDefault(e => e.GetIdText() == predecessorId);
            dstTask.Module = mods.FirstOrDefault(e => e.GetIdText() == successorId);

            await InvokeOnDataChangedAsync();
            await NotifyDataChangedAsync();
        }

        internal async Task DeleteDependencies(string predecessorId, string successorId)
        {
            var srcTask = _tasks.FirstOrDefault(e => e.Module?.GetIdText() == predecessorId);
            var dstTask = _tasks.FirstOrDefault(e => e.Module?.GetIdText() == successorId);
            if (srcTask?.Module == null || dstTask?.Module == null) return;
            var srcCounterField = srcTask.Module.GetField<NumberField>(Design.ProcessingCounterField);
            var dstCounterField = dstTask.Module.GetField<NumberField>(Design.ProcessingCounterField);

            var srcSubmitData = srcTask.Module.GetSubmitData();
            srcSubmitData.SearchDelete.Add(new SearchCondition
            {
                ModuleName = Design.DependenciesModule.ModuleName,
                Condition = MultiMatchCondition.And(
                    new VariableName($"{Design.DependencySourceIdField}.Value").Equal(predecessorId),
                    new VariableName($"{Design.DependencyDestinationIdField}.Value").Equal(successorId))
            });

            if (_dependencies.ContainsKey(successorId))
            {
                var deps = _dependencies[successorId].ToList();
                deps.Remove(predecessorId);
                if (deps.Count == 0)
                {
                    _dependencies.Remove(successorId);
                }
                else
                {
                    _dependencies[successorId] = deps.ToArray();
                }
            }

            MakeDependencyList();

            var ret = await Services.ModuleDataService.SubmitAsync(new[] { srcSubmitData, dstTask.Module.GetSubmitData() }.ToList());

            await InvokeOnDataChangedAsync();
            await NotifyDataChangedAsync();
        }

        private async Task DeleteAsync(Module mod)
        {
            var submit = new ModuleSubmitData
            {
                ModuleName = Design.SearchCondition.ModuleName,
                Id = mod.GetIdText(),
                Delete = new[] { mod.CreateModuleDeleteInfo() }.ToList()
            };
            if (!string.IsNullOrEmpty(Design.DependenciesModule.ModuleName))
            {
                submit.SearchDelete.Add(new SearchCondition
                {
                    ModuleName = Design.DependenciesModule.ModuleName,
                    Condition = MultiMatchCondition.Or(
                        new VariableName($"{Design.DependencySourceIdField}.Value").Equal(mod.GetIdText()),
                        new VariableName($"{Design.DependencyDestinationIdField}.Value").Equal(mod.GetIdText()))
                });
            }

            var ret = await Services.ModuleDataService.SubmitAsync(new[] { submit }.ToList());
            if (ret == null) return;
            foreach (var e in ret)
            {
                if (!string.IsNullOrEmpty(e.ExceptionMessage))
                {
                    await Services.Logger.Error(e.ExceptionMessage);
                    return;
                }
            }

            var del = _tasks.FirstOrDefault(e => e.Id == mod.GetIdText());
            if (del != null) _tasks.Remove(del);

            await OnChildDataChangedAsync();
        }

        private async Task UpdateAsync(Module? mod)
        {
            if (mod == null) return;
            if (!mod.ValidateInput())
            {
                await Services.UIService.NotifyError("Input Error");
                return;
            }

            if (await mod.SubmitAsync() != true) return;

            await InvokeOnDataChangedAsync();
            await NotifyDataChangedAsync();
        }

        private SearchCondition GetSearchCondition()
            => Design.SearchCondition.MergeSearchCondition(_additionalCondition);

        private async Task InvokeOnDataChangedAsync()
        {
            await Module.ExecuteScriptAsync(Design.OnDataChanged);
            await OnDataChangedAsync();
        }

        private class ModuleGanttTaskData : GanttTaskData
        {
            internal Module? Module { get; set; }
        }

        private ModuleGanttTaskData ConvertToGanttTaskData(Module data)
        {
            var id = data.GetField<IdField>(Design.IdField)?.Value;
            var name = data.GetField<TextField>(Design.NameField)?.Value;
            var start = data.GetField<DateTimeField>(Design.StartDateField)?.Value;
            var end = data.GetField<DateTimeField>(Design.EndDateField)?.Value;
            var progress = data.GetField<NumberField>(Design.ProgressField)?.Value;
            var parentId = data.GetField<LinkField>(Design.ParentIdField)?.Value;

            return new ModuleGanttTaskData
            {
                Module = data,
                Id = id??string.Empty,
                Name = name,
                Start = start,
                End = end,
                Progress = (double)(progress ?? 0),
                ParentId = parentId,
            };
        }

        private KeyValuePair<string, string> ConvertToGanttDeps(Module data)
        {
            var task = data.GetField<IdField>(Design.DependencySourceIdField)?.Value;
            var dependsOn = data.GetField<IdField>(Design.DependencyDestinationIdField)?.Value;

            return new KeyValuePair<string, string>(dependsOn ?? "", task ?? "");
        }

        private void MakeDependencyList()
        {
            Dependencies.Clear();

            foreach (var pair in _dependencies)
            {
                foreach (var from in pair.Value)
                {
                    Dependencies.Add(new DependencyListItem
                    {
                        PredecessorId = from,
                        SuccessorId = pair.Key,
                        DependencyId = Guid.NewGuid().ToString()
                    });
                }
            }

            Dependencies = Dependencies.OrderBy(item => item.DependencyId).ToList();
        }
    }
}
