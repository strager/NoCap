using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NoCap.Plugins.Helpers {
    class Crosshair : FrameworkElement {
        public static readonly DependencyProperty XProperty;
        public static readonly DependencyProperty YProperty;

        public static readonly DependencyProperty SizeProperty;

        public static readonly DependencyProperty LeftProperty;
        public static readonly DependencyProperty TopProperty;
        public static readonly DependencyProperty RightProperty;
        public static readonly DependencyProperty BottomProperty;
        
        public double X {
            get { return (double) GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public double Y {
            get { return (double) GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public Size Size {
            get { return (Size) GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public double Left   { get { return (double) GetValue(LeftProperty  ); } }
        public double Top    { get { return (double) GetValue(TopProperty   ); } }
        public double Right  { get { return (double) GetValue(RightProperty ); } }
        public double Bottom { get { return (double) GetValue(BottomProperty); } }

        static Crosshair() {
            XProperty = DependencyProperty.Register("X", typeof(double), typeof(Crosshair));
            YProperty = DependencyProperty.Register("Y", typeof(double), typeof(Crosshair));

            SizeProperty = DependencyProperty.Register("Size", typeof(Size), typeof(Crosshair));

            LeftProperty   = DependencyProperty.Register("Left",   typeof(double), typeof(Crosshair));
            TopProperty    = DependencyProperty.Register("Top",    typeof(double), typeof(Crosshair));
            RightProperty  = DependencyProperty.Register("Right",  typeof(double), typeof(Crosshair));
            BottomProperty = DependencyProperty.Register("Bottom", typeof(double), typeof(Crosshair));
        }

        public Crosshair() {
            SetBinding(LeftProperty,   CreateSideBinding(-1, Orientation.Horizontal));
            SetBinding(TopProperty,    CreateSideBinding(-1, Orientation.Vertical));
            SetBinding(RightProperty,  CreateSideBinding( 1, Orientation.Horizontal));
            SetBinding(BottomProperty, CreateSideBinding( 1, Orientation.Vertical));
        }

        private MultiBinding CreateSideBinding(int scale, Orientation orientation) {
            var binding = new MultiBinding {
                Converter = new CrosshairSideConverter(scale, orientation)
            };

            string axis = orientation == Orientation.Horizontal ? "X" : "Y";

            binding.Bindings.Add(new Binding { Source = this, Path = new PropertyPath(axis) });
            binding.Bindings.Add(new Binding { Source = this, Path = new PropertyPath("Size") });

            return binding;
        }
    }

    class CrosshairSideConverter : IMultiValueConverter {
        private readonly int scale;
        private readonly Orientation orientation;

        internal CrosshairSideConverter(int scale, Orientation orientation) {
            this.scale = scale;
            this.orientation = orientation;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            double val = (double) values[0];
            var size = (Size) values[1];

            double dim;

            if (orientation == Orientation.Horizontal) {
                dim = size.Width;
            } else {
                dim = size.Height;
            }

            return val + scale * (dim / 2);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
