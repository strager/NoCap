using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NoCap.Library.Controls {
    public class FeatureFilterConverter : TypeConverter, IValueConverter {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            return Convert(value);
        }

        private static Predicate<object> Convert(object value) {
            var commandFeatures = value is CommandFeatures
                ? (CommandFeatures) value
                : (CommandFeatures) Enum.Parse(typeof(CommandFeatures), (string) value);

            return GetFeatureFilterPredicate(commandFeatures);
        }

        private static Predicate<object> GetFeatureFilterPredicate(CommandFeatures commandFeatures) {
            return (obj) => {
                var objAsCommand = obj as ICommand;

                if (objAsCommand != null) {
                    return objAsCommand.HasFeatures(commandFeatures);
                }

                var objAsFactory = obj as ICommandFactory;

                if (objAsFactory != null) {
                    return objAsFactory.HasFeatures(commandFeatures);
                }

                return false;
            };
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string) || sourceType == typeof(CommandFeatures)) {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Convert(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return DependencyProperty.UnsetValue;
        }
    }
}