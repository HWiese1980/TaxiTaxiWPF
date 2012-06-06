using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;
using TaxiTaxiWPF.TaxiData;

namespace TaxiTaxiWPF.AbrechnungConverter
{
    public class UhrWert : MarkupExtension, IValueConverter
    {
        public string Eigenschaft { get; set; }
        public string Format { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public UhrWert()
        {
            Format = "{0}";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Schicht)) return null;

            var s = (Schicht)value;
            if (!s.Fahrzeuge.Any()) return null;

            var property = typeof(Fahrzeug).GetProperty(Eigenschaft);
            var values = s.Fahrzeuge.Select(p => String.Format(Format, property.GetValue(p, null))).ToArray();
            var first = values.FirstOrDefault();
            var others = values.Skip(1).Any() ? String.Format(" (" + Format + ")", String.Join(", ", values.Skip(1).ToArray())) : "";

            return first + others;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
