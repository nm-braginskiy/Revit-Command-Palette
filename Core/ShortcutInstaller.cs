using System;
using System.IO;
using System.Xml.Linq;

namespace CommandPalette.Core
{
    public static class ShortcutInstaller
    {
        private static string GetShortcutsFilePath()
        {
            string appData = Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData,
                "Autodesk", "Revit", "Autodesk Revit 2022",
                "KeyboardShortcuts.xml");
        }

        // Правильный формат — двойной CustomCtrl_% префикс
        private const string CommandId =
            "CustomCtrl_%CustomCtrl_%Command Palette%Поиск%OpenPalette";

        // Буквенный бинд как в Revit: CP = Command Palette
        private const string Shortcut = "CP";

        public static void EnsureShortcutRegistered()
        {
            try
            {
                string path = GetShortcutsFilePath();

                if (!File.Exists(path))
                {
                    CreateNewFile(path);
                    return;
                }

                var doc = XDocument.Load(path);
                var root = doc.Root;
                if (root == null) return;

                // Проверяем есть ли уже наша команда
                foreach (var elem in root.Elements("ShortcutItem"))
                {
                    if (elem.Attribute("CommandId")?.Value == CommandId)
                        return;
                }

                root.Add(new XElement("ShortcutItem",
                    new XAttribute("CommandName", "Открыть палитру"),
                    new XAttribute("CommandId", CommandId),
                    new XAttribute("Shortcuts", Shortcut),
                    new XAttribute("Paths", "Command Palette&gt;Поиск")));

                doc.Save(path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ShortcutInstaller error: {ex.Message}");
            }
        }

        private static void CreateNewFile(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("Shortcuts",
                    new XElement("ShortcutItem",
                        new XAttribute("CommandName", "Открыть палитру"),
                        new XAttribute("CommandId", CommandId),
                        new XAttribute("Shortcuts", Shortcut),
                        new XAttribute("Paths", "Command Palette&gt;Поиск"))));

            doc.Save(path);
        }
    }
}