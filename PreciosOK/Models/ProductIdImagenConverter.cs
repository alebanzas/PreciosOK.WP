using System;
using System.Globalization;
using System.Windows.Data;

namespace PreciosOK.Models
{
    public class ProductIdImagenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            try
            {
                return string.Format("/Images/Products/product_{0}.jpg", value);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
