using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using TaxiTaxiWPF.TaxiData;

namespace TaxiTaxiWPF.AbrechnungConverter
{
    class Datum : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public Datum() {}

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Schicht)) return null;
            var s = (Schicht) value;
            return s.Anfang.ToString("dd.MM.yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
