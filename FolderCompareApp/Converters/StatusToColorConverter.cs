using FolderCompareApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace FolderCompareApp.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ComparisonStatus status)
            {
                return status switch
                {
                    ComparisonStatus.Identical => Brushes.Green,
                    ComparisonStatus.Different => Brushes.Orange,
                    ComparisonStatus.LeftOnly => Brushes.Red,
                    ComparisonStatus.RightOnly => Brushes.Blue,
                    _ => Brushes.Gray
                };
            }
            return Brushes.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
