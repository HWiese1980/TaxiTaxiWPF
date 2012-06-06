using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TaxiTaxiWPF.TaxiData
{
    public class Validierungsfehler
    {
        public string Meldung { get; set; }
        public int Schwere { get; set; }

        public override string ToString()
        {
            return String.Format("[{0}] {1}", Schwere, Meldung);
        }
    }
    public static class Validierung
    {
        public static bool Validieren(Schicht schicht, IList<Validierungsfehler> fehler, int okGrenzwert = int.MaxValue)
        {
            if (schicht == null) return false;
            if (schicht.GeldNachher < schicht.GeldVorher) fehler.Add(new Validierungsfehler { Meldung = "Schicht: nachher weniger Geld in der Kasse als vorher", Schwere = 1 });
            if (schicht.Trinkgeld < 0.0F) fehler.Add(new Validierungsfehler { Meldung = "Schicht: Trinkgeld negativ", Schwere = 1 });
            if (schicht.Abzaehlen < 0.0F) fehler.Add(new Validierungsfehler { Meldung = "Schicht: Abzählen negativ", Schwere = 1 });
            if (schicht.GeldNachher < schicht.GeldVorher) fehler.Add(new Validierungsfehler { Meldung = "Schicht: nachher weniger Geld in der Kasse als vorher", Schwere = 1 });
            if (schicht.Ende < schicht.Anfang) fehler.Add(new Validierungsfehler { Meldung = "Schicht: Ende liegt vor Anfang", Schwere = 0 });
            if (!schicht.Fahrzeuge.Any()) fehler.Add(new Validierungsfehler { Meldung = "Schicht: kein Fahrzeug", Schwere = 0 });
            if (!schicht.Fahrzeuge.All(p => p.Fahrten.Any())) fehler.Add(new Validierungsfehler { Meldung = "Fahrzeug: keine Fahrten", Schwere = 0 });
            if (schicht.Fahrzeuge.Any(p => p.Nummer == 0)) fehler.Add(new Validierungsfehler { Meldung = "Fahrzeug: Nummer ungültig", Schwere = 1 });
            if (Math.Round(schicht.Fahrzeuge.Sum(p => p.Preisdifferenz), 2) >= 0.01F) fehler.Add(new Validierungsfehler { Meldung = "Fahrzeug: Diff. zw. Uhr und Einzelfahrpreisen ist nicht 0.00 €", Schwere = 0 });

            return fehler.All(p => p.Schwere >= okGrenzwert);
        }
    }
}
