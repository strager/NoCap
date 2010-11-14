using System;
using System.ComponentModel;

namespace NoCap.Library.Controls {
    public class FeatureFilterConverter : TypeConverter {
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value) {
            string commandFeatureString = (string) value;
            var commandFeatures = (CommandFeatures) Enum.Parse(typeof(CommandFeatures), commandFeatureString);

            return new Predicate<object>((obj) => {
                var objAsCommand = obj as ICommand;

                if (objAsCommand != null) {
                    return objAsCommand.HasFeatures(commandFeatures);
                }

                var objAsFactory = obj as ICommandFactory;

                if (objAsFactory != null) {
                    return objAsFactory.HasFeatures(commandFeatures);
                }

                return false;
            });
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            if (sourceType == typeof(string)) {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }
    }
}