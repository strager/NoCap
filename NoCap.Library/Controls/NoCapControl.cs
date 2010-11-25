using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace NoCap.Library.Controls {
    public class NoCapControl : DependencyObject {
        public static readonly DependencyProperty CommandProviderProperty;

        public static readonly DependencyProperty IsAdvancedProperty;

        static NoCapControl() {
            CommandProviderProperty = DependencyProperty.Register(
                "CommandProvider",
                typeof(ICommandProvider),
                typeof(NoCapControl),
                new PropertyMetadata(null)
            );

            IsAdvancedProperty = DependencyProperty.RegisterAttached(
                "IsAdvanced",
                typeof(bool),
                typeof(NoCapControl),
                new PropertyMetadata(false)
            );
        }

        private NoCapControl() {
            throw new Exception("Do not create an instance of NoCap");
        }

        public static void SetIsAdvanced(DependencyObject dependencyObject, bool isAdvanced) {
            dependencyObject.SetValue(IsAdvancedProperty, isAdvanced);
        }

        public static bool GetIsAdvanced(DependencyObject dependencyObject) {
            return (bool) dependencyObject.GetValue(IsAdvancedProperty);
        }
    }
}
