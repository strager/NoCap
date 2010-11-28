using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace NoCap.Library.Controls {
    public class NoCapControl : DependencyObject {
        public static readonly DependencyProperty CommandProviderProperty;

        public static readonly DependencyProperty ShowAdvancedProperty;

        static NoCapControl() {
            CommandProviderProperty = DependencyProperty.Register(
                "CommandProvider",
                typeof(ICommandProvider),
                typeof(NoCapControl),
                new PropertyMetadata(null)
            );

            ShowAdvancedProperty = DependencyProperty.RegisterAttached(
                "ShowAdvanced",
                typeof(bool),
                typeof(NoCapControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior
                )
            );
        }

        private NoCapControl() {
            throw new Exception("Do not create an instance of NoCap");
        }

        public static void SetIsAdvanced(DependencyObject dependencyObject, bool isAdvanced) {
            dependencyObject.SetValue(ShowAdvancedProperty, isAdvanced);
        }

        public static bool GetIsAdvanced(DependencyObject dependencyObject) {
            return (bool) dependencyObject.GetValue(ShowAdvancedProperty);
        }
    }
}
