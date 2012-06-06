#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

#endregion

namespace TaxiTaxiWPF.TaxiData
{
    public class Schicht : TaxiBase
    {
        private bool _abgerechnet;
        private DateTime _end;
        private ObservableCollection<Fahrzeug> _fahrzeuge;
        private float _moneyEnd;
        private float _moneyStart;
        private ObservableCollection<float> _privateausgaben;
        private ObservableCollection<float> _sonderausgaben;
        private DateTime _start;

        public Schicht()
        {
            Fahrzeuge = new ObservableCollection<Fahrzeug>();
            Sonderausgaben = new ObservableCollection<float>();
            Anfang = DateTime.Now.Date.AddDays(-(int) DateTime.Now.DayOfWeek).AddDays(-1).AddHours(20);
            Ende = Anfang.AddHours(10);
        }

        [ObserveProperty]
        public bool IstValide
        {
            get
            {
                var errors = new List<Validierungsfehler>();
                var valid = Validierung.Validieren(this, errors, 1);
                return valid;
            }
        }

        public ObservableCollection<float> Sonderausgaben
        {
            get { return _sonderausgaben; }
            set
            {
                _sonderausgaben = value;
                _sonderausgaben.CollectionChanged += (x, y) => OnPropertyChanged("SonderausgabenSumme");
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "Fahrzeuge")]
        public ObservableCollection<Fahrzeug> Fahrzeuge
        {
            get { return _fahrzeuge; }
            set
            {
                _fahrzeuge = value;
                foreach (var item in value)
                {
                    item.Parent = this;
                }
                _fahrzeuge.CollectionChanged += (x, y) =>
                                                {
                                                    foreach (IHasParent z in y.NewItems)
                                                    {
                                                        z.Parent = this;
                                                    }
                                                    OnPropertyChanged("Fahrzeuge");
                                                };
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "Anfang")]
        public DateTime Anfang
        {
            get { return _start; }
            set
            {
                var diff = _end - _start;
                _start = value;
                Ende = value.Add(diff);
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "Ende")]
        public DateTime Ende
        {
            get { return _end; }
            set
            {
                _end = value;
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "GeldVorher")]
        public float GeldVorher
        {
            get { return _moneyStart; }
            set
            {
                _moneyStart = value;
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "GeldNachher")]
        public float GeldNachher
        {
            get { return GetWert(_moneyEnd); }
            set
            {
                _moneyEnd = value;
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "Fahrzeug.Fahrten")]
        [ObserveProperty(Dependency = "Fahrzeuge")]
        public IEnumerable<Fahrt> TourEinträgeA
        {
            get
            {
                return (from fzg in Fahrzeuge
                        from frt in fzg.Fahrten
                        select frt).Take(22);
            }
        }

        [ObserveProperty(Dependency = "Fahrzeug.Fahrten")]
        [ObserveProperty(Dependency = "Fahrzeuge")]
        public IEnumerable<Fahrt> TourEinträgeB
        {
            get
            {
                return (from fzg in Fahrzeuge
                        from frt in fzg.Fahrten
                        select frt).Skip(22);
            }
        }

        [ObserveProperty(Dependency = "Fahrzeug.Fahrten")]
        [ObserveProperty(Dependency = "Fahrzeug.Fahrt.KM")]
        [ObserveProperty(Dependency = "Fahrzeuge")]
        public float ÜbertragKM { get { return GetWert(TourEinträgeA.Sum(p => p.KM ?? 0.0F)); } }

        [ObserveProperty(Dependency = "Fahrzeug.Fahrten")]
        [ObserveProperty(Dependency = "Fahrzeug.Fahrt.APES")]
        [ObserveProperty(Dependency = "Fahrzeuge")]
        public float ÜbertragAPES { get { return GetWert(TourEinträgeA.Sum(p => p.APES ?? 0.0F)); } }

        [ObserveProperty(Dependency = "Fahrzeug.Fahrten")]
        [ObserveProperty(Dependency = "Fahrzeug.Fahrt.Preis")]
        [ObserveProperty(Dependency = "Fahrzeuge")]
        public float ÜbertragPreis { get { return GetWert(TourEinträgeA.Sum(p => p.Preis ?? 0.0F)); } }

        [ObserveProperty(Dependency = "Fahrzeug.Fahrten")]
        [ObserveProperty(Dependency = "Fahrzeug.Fahrt.KM")]
        [ObserveProperty(Dependency = "Fahrzeuge")]
        public float AbrechnungGesamtKM { get { return GetWert(TourEinträgeA.Concat(TourEinträgeB).Sum(p => p.KM ?? 0.0F)); } }

        [ObserveProperty(Dependency = "Fahrzeug.Fahrten")]
        [ObserveProperty(Dependency = "Fahrzeug.Fahrt.OhneUhr")]
        [ObserveProperty(Dependency = "Fahrzeug.Fahrt.Preis")]
        [ObserveProperty(Dependency = "Fahrzeuge")]
        public float AbrechnungGesamtOhneUhr { get { return GetWert(TourEinträgeA.Concat(TourEinträgeB).Where(p => p.OhneUhr).Sum(p => p.Preis ?? 0.0F)); } }

        [ObserveProperty(Dependency = "Fahrzeug.Fahrten")]
        [ObserveProperty(Dependency = "Fahrzeug.Fahrt.Preis")]
        [ObserveProperty(Dependency = "Fahrzeuge")]
        public float AbrechnungGesamtFahrPreis { get { return GetWert(TourEinträgeA.Concat(TourEinträgeB).Sum(p => p.Preis ?? 0.0F)); } }

        [ObserveProperty(Dependency = "GeldVorher")]
        [ObserveProperty(Dependency = "GeldNachher")]
        public float BargeldDifferenz { get { return GetWert(GeldNachher - GeldVorher); } }

        [ObserveProperty(Dependency = "Fahrzeug.BargeldErhaltenOhneTip")]
        public float BargeldOhneTip { get { return GetWert(Fahrzeuge.Sum(f => f.BargeldErhaltenOhneTip)); } }

        [ObserveProperty(Dependency = "BargeldDifferenz")]
        [ObserveProperty(Dependency = "BargeldOhneTip")]
        public float Trinkgeld { get { return GetWert(BargeldDifferenz - BargeldOhneTip); } }

        [ObserveProperty(Dependency = "OffiziellEinnahmen")]
        [ObserveProperty(Dependency = "OffiziellRabatt")]
        [ObserveProperty(Dependency = "OffiziellLohn")]
        [ObserveProperty(Dependency = "OffiziellRechnung")]
        public float Abzaehlen { get { return GetWert(OffiziellEinnahmen - OffiziellRabatt - OffiziellLohn - OffiziellRechnung); } }

        public float SonderausgabenSumme { get { return GetWert(Sonderausgaben.Sum()); } }

        [ObserveProperty(Dependency = "OffiziellLohn")]
        [ObserveProperty(Dependency = "Trinkgeld")]
        public float EigenerVerdienst { get { return OffiziellLohn + Trinkgeld; } }

        [ObserveProperty(Dependency = "Anfang")]
        [ObserveProperty(Dependency = "Ende")]
        public double Stunden { get { return (Ende - Anfang).TotalHours; } }

        public bool Abgerechnet
        {
            get { return _abgerechnet; }
            set
            {
                _abgerechnet = value;
                OnPropertyChanged();
            }
        }

        public override string Name { get { return String.Format("Schicht {0}h {1}", Stunden, Anfang.ToString(new CultureInfo("de-DE"))); } }

        public ObservableCollection<float> Privateausgaben
        {
            get { return _privateausgaben; }
            set
            {
                _privateausgaben = value;
                _privateausgaben.CollectionChanged += (x, y) => OnPropertyChanged("PrivatausgabenSumme");
                OnPropertyChanged();
            }
        }

        #region Summen der Uhrwerte

        [ObserveProperty(Dependency = "Fahrzeug.TotalAnfang")]
        public float TotalAnfang { get { return GetWert(Fahrzeuge.Sum(p => p.TotalAnfang)); } }

        [ObserveProperty(Dependency = "Fahrzeug.TotalEnde")]
        public float TotalEnde { get { return GetWert(Fahrzeuge.Sum(p => p.TotalEnde)); } }

        [ObserveProperty(Dependency = "Fahrzeug.TotalTotal")]
        public float TotalTotal { get { return GetWert(Fahrzeuge.Sum(p => p.TotalTotal)); } }

        [ObserveProperty(Dependency = "Fahrzeug.BesetztAnfang")]
        public float BesetztAnfang { get { return GetWert(Fahrzeuge.Sum(p => p.BesetztAnfang)); } }

        [ObserveProperty(Dependency = "Fahrzeug.BesetztEnde")]
        public float BesetztEnde { get { return GetWert(Fahrzeuge.Sum(p => p.BesetztEnde)); } }

        [ObserveProperty(Dependency = "Fahrzeug.BesetztTotal")]
        public float BesetztTotal { get { return GetWert(Fahrzeuge.Sum(p => p.BesetztTotal)); } }

        [ObserveProperty(Dependency = "Fahrzeug.Besetzt2x")]
        public float Besetzt2x { get { return GetWert(Fahrzeuge.Sum(p => p.Besetzt2x)); } }

        [ObserveProperty(Dependency = "Fahrzeug.TourenAnfang")]
        public float TourenAnfang { get { return GetWert(Fahrzeuge.Sum(p => p.TourenAnfang)); } }

        [ObserveProperty(Dependency = "Fahrzeug.TourenEnde")]
        public float TourenEnde { get { return GetWert(Fahrzeuge.Sum(p => p.TourenEnde)); } }

        [ObserveProperty(Dependency = "Fahrzeug.TourenTotal")]
        public float TourenTotal { get { return GetWert(Fahrzeuge.Sum(p => p.TourenTotal)); } }

        [ObserveProperty(Dependency = "Fahrzeug.PreisAnfang")]
        public float PreisAnfang { get { return GetWert(Fahrzeuge.Sum(p => p.PreisAnfang)); } }

        [ObserveProperty(Dependency = "Fahrzeug.PreisEnde")]
        public float PreisEnde { get { return GetWert(Fahrzeuge.Sum(p => p.PreisEnde)); } }

        [ObserveProperty(Dependency = "Fahrzeug.PreisTotal")]
        public float PreisTotal { get { return GetWert(Fahrzeuge.Sum(p => p.PreisTotal)); } }

        #endregion

        #region Offizielle Werte für Abrechnung

        [ObserveProperty(Dependency = "Fahrzeug.PreisTotal")]
        public float OffiziellEinnahmen { get { return GetWert(Fahrzeuge.Sum(f => f.PreisTotal)); } }

        [ObserveProperty(Dependency = "Fahrzeug.RabattTotal")]
        public float OffiziellRabatt { get { return GetWert(Fahrzeuge.Sum(f => f.RabattTotal)); } }

        [ObserveProperty(Dependency = "Fahrzeug.AbzugRechnungsfahrten")]
        public float OffiziellRechnung { get { return GetWert(Fahrzeuge.Sum(f => f.AbzugRechnungsfahrten)); } }

        [ObserveProperty(Dependency = "Stunden")]
        public float OffiziellLohn { get { return (float) (6.0F*Stunden); } }

        #endregion

        private T GetWert<T>(T value)
        {
            return (Ende > DateTime.Now) ? default(T) : value;
        }
    }
}