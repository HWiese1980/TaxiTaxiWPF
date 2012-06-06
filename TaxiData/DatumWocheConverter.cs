using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace TaxiTaxiWPF.TaxiData
{
    public class DatumWocheConverter : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public DatumWocheConverter()
        {}

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return culture.Calendar.GetWeekOfYear((DateTime)value, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
