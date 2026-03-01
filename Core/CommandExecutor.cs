using Autodesk.Revit.UI;
using CommandPalette.Core;
using System;

namespace CommandPalette.Core
{
    // Правильный способ вызова Revit API из WPF окна
    public class CommandExecutor : IExternalEventHandler
    {
        private CommandItem _pendingCommand;

        public static CommandExecutor Instance { get; private set; }
        public static ExternalEvent Event { get; private set; }

        public static void Initialize()
        {
            Instance = new CommandExecutor();
            Event = ExternalEvent.Create(Instance);
        }

        public void RequestExecute(CommandItem command)
        {
            _pendingCommand = command;
            Event.Raise(); // Говорим Revit "выполни когда освободишься"
        }

        // Вызывается Revit в валидном API контексте
        public void Execute(UIApplication app)
        {
            try
            {
                _pendingCommand?.Execute?.Invoke();
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Ошибка", ex.Message);
            }
            finally
            {
                _pendingCommand = null;
            }
        }

        public string GetName() => "CommandPaletteExecutor";
    }
}