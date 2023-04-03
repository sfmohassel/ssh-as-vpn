using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ssh_proxifier
{
  public class BoolToColorConverter : IValueConverter
  {
    private static SolidColorBrush TRUE = new SolidColorBrush(Colors.LightGreen);
    private static SolidColorBrush FALSE = new SolidColorBrush(Colors.White);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (bool)value ? TRUE : FALSE;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return ((SolidColorBrush)value).Equals(TRUE);
    }
  }
}