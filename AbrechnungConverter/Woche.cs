using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;
using TaxiTaxiWPF.TaxiData;

namespace TaxiTaxiWPF.AbrechnungConverter
{
    enum WochenDatum
    {
        Nummer,
        Anfang,
        Ende
    }

    class Woche : MarkupExtension, IValueConverter
    {
        public WochenDatum Auswahl { get; set; }

        public Woche()
        {
            
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var schicht = value as Schicht;

            if (schicht != null)
            {
                var nr = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear((schicht).Anfang, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                var dow = CultureInfo.CurrentCulture.Calendar.GetDayOfWeek((schicht).Anfang);
                var anf = schicht.Anfang.AddDays(-((int)dow - 1));
                var end = anf.AddDays(6);
                switch(Auswahl)
                {
                    case WochenDatum.Nummer:
                        return nr;
                    case WochenDatum.Anfang:
                        return anf.Date;
                    case WochenDatum.Ende:
                        return end.Date;
                }
            }
            
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
