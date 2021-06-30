using System.Windows.Media;
using Color = System.Drawing.Color;

namespace WPFCaptureSample
{
    public static class ThemeColors
    {
        public static Color BackColorMain { get; } = Color.FromArgb(255, 32, 32, 32);
        public static Color BackColorSecondary { get; } = Color.FromArgb(255, 25, 25, 25);
        public static Color BackColorButton { get; } = Color.FromArgb(255, 54, 54, 54);
        public static Color ForeColor { get; } = Color.White;
        public static Color HoverColor { get; } = Color.FromArgb(255, 0, 114, 198);
        public static Color DownColor { get; } = Color.FromArgb(255, 0, 114, 255);


        public static System.Windows.Media.Color BackColorMainMedia => BackColorMain.ToMediaColor();
        public static System.Windows.Media.Color BackColorSecondaryMedia => BackColorSecondary.ToMediaColor();
        public static System.Windows.Media.Color BackColorButtonMedia => BackColorButton.ToMediaColor();
        public static System.Windows.Media.Color ForeColorMedia => ForeColor.ToMediaColor();
        public static System.Windows.Media.Color HoverColorMedia => HoverColor.ToMediaColor();
        public static System.Windows.Media.Color DownColorMedia => DownColor.ToMediaColor();



        public static System.Windows.Media.Color ToMediaColor(this Color c)
        {
            return new System.Windows.Media.Color() {A = c.A, B = c.B, R = c.R, G = c.G};
        }

        public static System.Windows.Media.Brush ToBrush(this Color c)
        {
            return new SolidColorBrush(c.ToMediaColor());
        }

        public static Brush BackColorMainBrush => BackColorMain.ToBrush();
        public static Brush BackColorSecondaryBrush => BackColorSecondary.ToBrush();
        public static Brush BackColorButtonBrush => BackColorButton.ToBrush();
        public static Brush ForeColorBrush => ForeColor.ToBrush();
        public static Brush HoverColorBrush => HoverColor.ToBrush();
        public static Brush DownColorBrush => DownColor.ToBrush();

    }
}