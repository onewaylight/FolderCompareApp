using FolderCompareApp.Models;
using System.Globalization;
using System.Windows.Data;

namespace FolderCompareApp.Converters
{
    public class EnumDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "All";

            if (value is ComparisonStatus status)
            {
                return status switch
                {
                    ComparisonStatus.Identical => "Identical",
                    ComparisonStatus.Different => "Different",
                    ComparisonStatus.LeftOnly => "Left Only",
                    ComparisonStatus.RightOnly => "Right Only",
                    _ => value.ToString()
                };
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
