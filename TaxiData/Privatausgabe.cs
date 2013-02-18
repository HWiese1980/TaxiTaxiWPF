using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SeveQsDataBase;

namespace TaxiTaxiWPF.TaxiData
{
    public class Privatausgabe : DataBase
    {
        private string _bemerkung;
        private double _wert;

        public string Bemerkung { get { return _bemerkung; } set { _bemerkung = value; OnPropertyChanged("Bemerkung"); } }
        public double Wert { get { return _wert; } set { _wert = value; OnPropertyChanged("Wert"); } }

        public override string Name
        {
            get { return Bemerkung; }
        }
    }
}
