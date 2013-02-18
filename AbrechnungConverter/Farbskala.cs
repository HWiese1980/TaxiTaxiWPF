using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaxiTaxiWPF.AbrechnungConverter
{
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;

    /// <summary>
    /// The farbskala.
    /// </summary>
    public class Farbskala : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public Color ColorA { get; set; }
        public Color ColorB { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int)) return null;
            int val = (int)value;

            LinearGradientBrush br = new LinearGradientBrush(ColorA, ColorB, 0.0D);
            br.ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation;
            br.GradientStops.Add(new GradientStop { Offset = val });
            return br.GradientStops.First().Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
