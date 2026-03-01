using Autodesk.Revit.UI;
using AdWindows = Autodesk.Windows;
using System.Collections.Generic;
using System.Windows.Media;
using System;
using System.Collections;
using System.Reflection;

namespace CommandPalette.Core
{
    public class RevitCommandProvider
    {
        private readonly UIApplication _uiApp;
        private readonly HashSet<string> _seenIds = new HashSet<string>();

        public RevitCommandProvider(UIApplication uiApp)
        {
            _uiApp = uiApp;
        }

        public List<CommandItem> GetAllCommands()
        {
            var result = new List<CommandItem>();
            _seenIds.Clear();

            try
            {
                var ribbon = AdWindows.ComponentManager.Ribbon;
                foreach (AdWindows.RibbonTab tab in ribbon.Tabs)
                {
                    if (!tab.IsVisible) continue;
                    foreach (AdWindows.RibbonPanel panel in tab.Panels)
                    {
                        if (!panel.IsVisible) continue;
                        CollectItems(panel.Source.Items, result);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
            }

            return result;
        }

        private void CollectItems(IEnumerable items, List<CommandItem> result)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                if (item == null) continue;
                try
                {
                    if (item is AdWindows.RibbonButton btn && btn.IsVisible)
                    {
                        AddButton(btn, result);
                        // Не заходим внутрь кнопки — у неё нет дочерних команд
                        continue;
                    }

                    // Для всех остальных — рекурсивно ищем дочерние Items
                    var itemsProp = item.GetType().GetProperty("Items",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (itemsProp != null)
                    {
                        var children = itemsProp.GetValue(item) as IEnumerable;
                        if (children != null)
                            CollectItems(children, result);
                    }
                }
                catch { }
            }
        }

        private void AddButton(AdWindows.RibbonButton btn, List<CommandItem> result)
        {
            string name = btn.Text ?? btn.Name ?? "";
            if (string.IsNullOrWhiteSpace(name)) return;
            
            // Убираем переносы строк и лишние пробелы
            name = name.Replace("\n", " ").Replace("\r", "").Trim();
            
            // Убираем множественные пробелы
            while (name.Contains("  "))
                name = name.Replace("  ", " ");

            string cmdId = btn.Id ?? "";

            // Фильтруем кнопки без команды — это группы/контейнеры
            if (string.IsNullOrEmpty(cmdId)) return;

            // Фильтруем дубликаты по Id
            if (_seenIds.Contains(cmdId)) return;
            _seenIds.Add(cmdId);

            // Правильно извлекаем описание из RibbonToolTip
            string description = "";
            try
            {
                if (btn.ToolTip is AdWindows.RibbonToolTip ribbonTip)
                {
                    // Берём расширенное описание если есть, иначе краткое
                    description = ribbonTip.ExpandedContent?.ToString()
                                  ?? ribbonTip.Content?.ToString()
                                  ?? "";
                }
                else if (btn.ToolTip != null)
                {
                    description = btn.ToolTip.ToString();
                }
            }
            catch { }

            ImageSource icon = btn.LargeImage ?? btn.Image;

            result.Add(new CommandItem
            {
                Name = name,
                Description = description,
                Icon = icon,
                Execute = () =>
                {
                    try
                    {
                        var revitCmdId = RevitCommandId.LookupCommandId(cmdId);
                        if (revitCmdId != null && _uiApp.CanPostCommand(revitCmdId))
                            _uiApp.PostCommand(revitCmdId);
                    }
                    catch { }
                }
            });
        }
    }
}