using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace ClientLogic.WinRT.Converters
{
    public class BoolToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //Thumbs up: #186C06
            //Thumbs down: #9b0000

            SolidColorBrush b;
            b = (value is bool && (bool)value) ? new SolidColorBrush(Color.FromArgb(255, 25, 108, 6)) : new SolidColorBrush(Color.FromArgb(255, 155, 0, 0));

            return b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
