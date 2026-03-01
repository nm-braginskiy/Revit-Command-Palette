using Autodesk.Revit.UI;
using CommandPalette.Core;
using CommandPalette.UI;
using System.Collections.Generic;

namespace CommandPalette
{
    public class App : IExternalApplication
    {
        public static UIApplication CurrentApp { get; set; }
        public static UsageTracker Tracker { get; private set; }
        private static List<CommandItem> _cachedCommands = null;
        private static bool _paletteOpen = false;
        private HotkeyManager _hotkeyManager;

        public Result OnStartup(UIControlledApplication application)
        {
            Tracker = new UsageTracker();

            ShortcutInstaller.EnsureShortcutRegistered();

            application.CreateRibbonTab("Command Palette");
            RibbonPanel panel = application.CreateRibbonPanel("Command Palette", "Поиск");
            panel.AddItem(new PushButtonData(
                "OpenPalette", "Открыть\nпалитру",
                typeof(App).Assembly.Location,
                "CommandPalette.OpenPaletteCommand"));

            application.ControlledApplication.ApplicationInitialized += OnAppInitialized;
            return Result.Succeeded;
        }

        private void OnAppInitialized(object sender,
            Autodesk.Revit.DB.Events.ApplicationInitializedEventArgs e)
        {
            CurrentApp = new UIApplication(
                sender as Autodesk.Revit.ApplicationServices.Application);

            CommandExecutor.Initialize();

            _hotkeyManager = new HotkeyManager(() =>
            {
                if (!_paletteOpen)
                    OpenPalette();
            });
            _hotkeyManager.Register();
        }

        public static void OpenPalette()
        {
            if (CurrentApp == null || _paletteOpen) return;
            _paletteOpen = true;
            try
            {
                if (_cachedCommands == null)
                {
                    var provider = new RevitCommandProvider(CurrentApp);
                    _cachedCommands = provider.GetAllCommands();
                }
                var window = new PaletteWindow(_cachedCommands, Tracker);
                window.ShowDialog();
            }
            finally
            {
                _paletteOpen = false;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            _hotkeyManager?.Dispose();
            return Result.Succeeded;
        }
    }
}