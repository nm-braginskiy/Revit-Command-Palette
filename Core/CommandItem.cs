using System.Windows;
using System.Windows.Media;

namespace CommandPalette.Core
{
    public class CommandItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ImageSource Icon { get; set; }
        public System.Action Execute { get; set; }
        public int UsageCount { get; set; } = 0;

        public string SearchText => $"{Name} {Description}".ToLower();

        // Скрываем строку описания если оно пустое
        public Visibility HasDescription =>
            string.IsNullOrWhiteSpace(Description)
                ? Visibility.Collapsed
                : Visibility.Visible;
    }
}