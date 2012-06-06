using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaxiTaxiWPF.TaxiData
{
    public class Statistikdaten<TKey>
    {
        public TKey Key { get; set; }
        public double Stunden { get; set; }
        public double Verdienst { get; set; }
        public double VerdienstProStunde { get; set; }
        public string Bemerkung { get; set; }
    }
}
