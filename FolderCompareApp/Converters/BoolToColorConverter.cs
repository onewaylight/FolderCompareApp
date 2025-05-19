using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FolderCompareApp.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public Brush TrueColor { get; set; } = Brushes.LightPink;
        public Brush FalseColor { get; set; } = Brushes.Transparent;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? TrueColor : FalseColor;
            }
            return FalseColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
