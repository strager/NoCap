using System;
using System.Globalization;
using System.Windows.Data;

namespace NoCap.Extensions.Default.Helpers {
    class UriStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value == null ? null : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            Uri uri;

            if (!Uri.TryCreate((string) value, UriKind.RelativeOrAbsolute, out uri)) {
                return null;
            }

            return uri;
        }
    }
}