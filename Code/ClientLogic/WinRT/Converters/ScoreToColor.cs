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
    public class ScoreToColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush b = new SolidColorBrush(Color.FromArgb(255, 0, 113, 166));
            if (value is int)
            {
                int val = (int) value;
                if (val < 0)
                    b = new SolidColorBrush(Color.FromArgb(255, 155, 0, 0));
                else if (val > 0)
                    b = new SolidColorBrush(Color.FromArgb(255, 25, 108, 6));
            }
            return b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
