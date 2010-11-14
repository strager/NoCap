using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NoCap.Library.Controls {
    public class CommandFeatureConverter : TypeConverter, IValueConverter {
        public static CommandFeatures GetCommandFeatures(object value) {
            if (value is CommandFeatures) {
                return (CommandFeatures) value;
            } else {
                return (CommandFeatures) Enum.Parse(typeof(CommandFeatures), (string) value);
            }
        }

        public static bool TryGetCommandFeatures(object value, out CommandFeatures commandFeatures) {
            if (value is CommandFeatures) {
                commandFeatures = (CommandFeatures) value;

                return true;
            }

            string valueString = value as string;

            if (valueString != null) {
                return Enum.TryParse(valueString, true, out commandFeatures);
            }

            commandFeatures = 0;

            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            return GetCommandFeatures(value);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string) || sourceType == typeof(CommandFeatures);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return GetCommandFeatures(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return DependencyProperty.UnsetValue;
        }
    }

    public class CommandFeatureFilterConverter : TypeConverter, IValueConverter {
        public static Predicate<object> GetFeatureFilterPredicate(CommandFeatures commandFeatures) {
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

        public static Predicate<object> GetPredicate(object filter) {
            if (filter == null) {
                return (obj) => true;
            }

            // HACK Hacky way to unbox and "cast" to Predicate<object>
            // (in case e.g. filter is a Predicate<ICommand>)
            bool isPredicate = filter.GetType().Name == "Predicate`1";

            if (isPredicate) {
                return (obj) => (bool) filter.GetType().GetMethod("Invoke").Invoke(filter, new[] { obj });
            }

            return GetFeatureFilterPredicate(CommandFeatureConverter.GetCommandFeatures(filter));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            return GetPredicate(value);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string) || sourceType == typeof(CommandFeatures) || sourceType == typeof(Predicate<object>);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return GetPredicate(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return DependencyProperty.UnsetValue;
        }
    }
}