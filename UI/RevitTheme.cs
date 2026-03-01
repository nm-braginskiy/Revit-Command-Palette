using System.Windows.Media;

namespace CommandPalette.UI
{
    public static class RevitTheme
    {
        public static Color Background => Color.FromRgb(240, 240, 240);
        public static Color BackgroundSecondary => Color.FromRgb(255, 255, 255);
        public static Color BackgroundHover => Color.FromRgb(220, 232, 246);
        public static Color BackgroundSelected => Color.FromRgb(0, 120, 215);
        public static Color TextPrimary => Color.FromRgb(30, 30, 30);
        public static Color TextSecondary => Color.FromRgb(100, 100, 100);
        public static Color Separator => Color.FromRgb(200, 200, 200);
        public static Color InputBackground => Color.FromRgb(255, 255, 255);

        public static SolidColorBrush BgBrush => new SolidColorBrush(Background);
        public static SolidColorBrush BgSecondaryBrush => new SolidColorBrush(BackgroundSecondary);
        public static SolidColorBrush BgHoverBrush => new SolidColorBrush(BackgroundHover);
        public static SolidColorBrush BgSelectedBrush => new SolidColorBrush(BackgroundSelected);
        public static SolidColorBrush TextPrimaryBrush => new SolidColorBrush(TextPrimary);
        public static SolidColorBrush TextSecondaryBrush => new SolidColorBrush(TextSecondary);
        public static SolidColorBrush SeparatorBrush => new SolidColorBrush(Separator);
        public static SolidColorBrush InputBgBrush => new SolidColorBrush(InputBackground);
    }
}