using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using TaxiTaxiWPF.TaxiData;

namespace TaxiTaxiWPF.AbrechnungConverter
{
    class Wagen : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public Wagen()
        {}

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Schicht)) return null;

            var s = (Schicht) value;
            if (!s.Fahrzeuge.Any()) return null;
            
            var firstFzg = s.Fahrzeuge.FirstOrDefault();
            var otherFzg = s.Fahrzeuge.Skip(1);

            var first = firstFzg.Nummer.ToString();
            var others = otherFzg.Any() ? String.Format("({0})", String.Join(", ", otherFzg.Select(p => p.Nummer).ToArray())) : "";
            return first + others;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
