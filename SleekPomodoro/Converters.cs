using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SleekPomodoro
{
    public class WorkBreakToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isWorkSession)
            {
                var workColor = (Color)Application.Current.FindResource("WorkColor");
                var breakColor = (Color)Application.Current.FindResource("BreakColor");
                return isWorkSession ? workColor : breakColor;
            }
            return DependencyProperty.UnsetValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}