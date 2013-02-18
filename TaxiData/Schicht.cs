#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using SeveQsDataBase;

#endregion

namespace TaxiTaxiWPF.TaxiData
{
    public class Schicht : DataBase
    {
        private bool _abgerechnet;
        private DateTime _end;
        private ObservableCollection<Fahrzeug> _fahrzeuge;
        private float _moneyEnd;
        private float _moneyStart;
        private ObservableCollection<Privatausgabe> _privatausgaben;
        private ObservableCollection<float> _sonderausgaben;
        private DateTime _start;

        private float geliehenesWechselgeld;

        private bool abrechnungFertig;

        public Schicht(IEnumerable<Schicht> schichten = null)
        {
#if USE_SEVEQ_DB
            Fahrzeuge = new ObservableCollection<Fahrzeug>();
#else
            Fahrzeuge = new BindingList<Fahrzeug>();
#endif
            Sonderausgaben = new ObservableCollection<float>();
            Privatausgaben = new ObservableCollection<Privatausgabe>();
            Anfang = DateTime.Now.Date.AddDays(-(int)DateTime.Now.DayOfWeek).AddDays(-1).AddHours(20);
            while(schichten != null && schichten.Any(p => p.Anfang.Date == Anfang.Date))
            {
                Anfang = Anfang.AddDays(1);
                if (Anfang.DayOfWeek == DayOfWeek.Sunday) Anfang = Anfang.AddHours(-2);
            }
            Ende = Anfang.AddHours(Anfang.DayOfWeek == DayOfWeek.Sunday ? 7 : 10);
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

        [ObserveProperty]
        public int Validität
        {
            get
            {
                var errors = new List<Validierungsfehler>();
                Validierung.Validieren(this, errors, 1);
                if (!errors.Any()) return 0;
                return errors.Max(p => p.Schwere);
            }
        }

        public ObservableCollection<float> Sonderausgaben
        {
            get { return _sonderausgaben; }
            set
            {
                _sonderausgaben = value;
                _sonderausgaben.CollectionChanged += (x, y) => OnPropertyChanged("SonderausgabenSumme");
                OnPropertyChanged("Sonderausgaben");
            }
        }

        public ObservableCollection<Privatausgabe> Privatausgaben
        {
            get { return _privatausgaben; }
            set
            {
                _privatausgaben = value;
                _privatausgaben.CollectionChanged += (x, y) => OnPropertyChanged("Privatausgaben");
                foreach (var item in value)
                {
                    item.Parent = this;
                }
                _privatausgaben.CollectionChanged += (x, y) =>
                {
                    if (y.NewItems == null || !y.NewItems.Cast<IHasParent>().Any()) return;
                    foreach (IHasParent z in y.NewItems)
                    {
                        z.Parent = this;
                    }
                    OnPropertyChanged("Privatausgaben");
                };
                OnPropertyChanged("Privatausgaben");
            }
        }

        //[ObserveProperty(Dependency = "Fahrzeuge")]
#if USE_SEVEQ_DB
        public ObservableCollection<Fahrzeug> Fahrzeuge
        {
            get { return _fahrzeuge; }
            set
            {
                _fahrzeuge = value;
                foreach (var item in value)
                {
                    item.Parent = this;
                    item.IndexGroup = _fahrzeuge;
                }
                _fahrzeuge.CollectionChanged += (x, y) =>
                {
                    foreach (IHasParent z in y.NewItems ?? new List<IHasParent>())
                    {
                        z.Parent = this;
                    }
                    foreach (IIndexed z in y.NewItems ?? new List<IIndexed>())
                    {
                        z.IndexGroup = _fahrzeuge;
                    }
                    OnPropertyChanged("Fahrzeuge");
                };
                OnPropertyChanged("Fahrzeuge");
            }
        }
#else
        private BindingList<Fahrzeug> _blFahrzeuge;
        public BindingList<Fahrzeug> Fahrzeuge
        {
            get { return _blFahrzeuge; }
            set 
            { 
                _blFahrzeuge = value;  
                _blFahrzeuge.ListChanged += _fahrzeugeChanged;
            }
        }

        private void _fahrzeugeChanged(object sender, ListChangedEventArgs listChangedEventArgs)
        {
            OnPropertyChanged("AbrechnungGesamtKM");
        }
#endif
        //[ObserveProperty(Dependency = "Anfang")]
        public DateTime Anfang
        {
            get { return _start; }
            set
            {
                var diff = _end - _start;
                _start = value;
                Ende = value.Add(diff);
                OnPropertyChanged("Anfang");
            }
        }

        //[ObserveProperty(Dependency = "Ende")]
        public DateTime Ende
        {
            get { return _end; }
            set
            {
                _end = value;
                OnPropertyChanged("Ende");
            }
        }

        //[ObserveProperty(Dependency = "GeldVorher")]
        public float GeldVorher
        {
            get { return _moneyStart; }
            set
            {
                _moneyStart = value;
                OnPropertyChanged("GeldVorher");
            }
        }

        //[ObserveProperty(Dependency = "GeldNachher")]
        public float GeldNachher
        {
            get { return GetWert(_moneyEnd); }
            set
            {
                _moneyEnd = value;
                OnPropertyChanged("GeldNachher");
            }
        }

        //[ObserveProperty(Dependency = "GeliehenesWechselgeld")]
        public float GeliehenesWechselgeld
        {
            get
            {
                return this.geliehenesWechselgeld;
            }
            set
            {
                this.geliehenesWechselgeld = value;
                this.OnPropertyChanged("GeliehenesWechselgeld");
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
                        orderby fzg.Index
                        orderby frt.Index
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
                        orderby fzg.Index
                        orderby frt.Index
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
        public float AbrechnungGesamtOhneUhr { get { return GetWert(TourEinträgeA.Concat(TourEinträgeB).Sum(p => p.APES ?? 0.0F)); } }

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
        [ObserveProperty(Dependency = "Privatausgaben")]
        [ObserveProperty(Dependency = "Privatausgabe.Wert")]
        public float Trinkgeld { get { return GetWert(BargeldDifferenz - BargeldOhneTip + (float)Privatausgaben.Sum(p => p.Wert)); } }

        [ObserveProperty(Dependency = "GeliehenesWechselgeld")]
        [ObserveProperty(Dependency = "OffiziellEinnahmenInklAPES")]
        [ObserveProperty(Dependency = "OffiziellSonstiges")]
        [ObserveProperty(Dependency = "OffiziellLohn")]
        [ObserveProperty(Dependency = "OffiziellRechnung")]
        public float Abzaehlen { get { return GetWert(GeliehenesWechselgeld + OffiziellEinnahmenInklAPES - OffiziellSonstiges - OffiziellLohn - OffiziellRechnung); } }

        public float SonderausgabenSumme { get { return GetWert(Sonderausgaben.Sum()); } }

        [ObserveProperty(Dependency = "OffiziellLohn")]
        [ObserveProperty(Dependency = "Trinkgeld")]
        public float EigenerVerdienst { get { return OffiziellLohn + Trinkgeld; } }

        [ObserveProperty(Dependency = "Anfang")]
        [ObserveProperty(Dependency = "Ende")]
        [PSCached]
        public double Stunden { get { return (Ende - Anfang).TotalHours; } }

        public bool Abgerechnet
        {
            get { return _abgerechnet; }
            set
            {
                _abgerechnet = value;
                OnPropertyChanged("Abgerechnet");
                if (value) AbrechnungFertig = true;
            }
        }

        [PSCached]
        public override string Name { get { return String.Format("Schicht {0}h {1}", Stunden, Anfang.ToString(new CultureInfo("de-DE"))); } }

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

        [ObserveProperty(Dependency = "Fahrzeug.PreisInklAPES")]
        public float OffiziellEinnahmenInklAPES { get { return GetWert(Fahrzeuge.Sum(f => f.PreisInklAPES)); } }

        [ObserveProperty(Dependency = "SonderausgabenSumme")]
        public float OffiziellSonstiges { get { return SonderausgabenSumme; } }

        [ObserveProperty(Dependency = "Fahrzeug.AbzugRechnungsfahrten")]
        public float OffiziellRechnung { get { return GetWert(Fahrzeuge.Sum(f => f.AbzugRechnungsfahrten)); } }

        [ObserveProperty(Dependency = "Stunden")]
        public float OffiziellLohn { get { return (float)(6.0F * Stunden); } }

        [ObserveProperty(Dependency = "Fahrzeuge")]
        [ObserveProperty(Dependency = "Fahrzeug.PreisTotal")]
        [ObserveProperty(Dependency = "Fahrzeuge.BesetztTotal")]
        public double PreisProKM
        {
            get
            {
                return Fahrzeuge.Sum(p => (p.PreisTotal - p.Fahrten.Count * 5.0D) / (p.BesetztTotal - p.Fahrten.Count * 1.6D));
            }
        }

        public bool AbrechnungFertig
        {
            get
            {
                return abrechnungFertig;
            }
            set
            {
                abrechnungFertig = value;
                this.OnPropertyChanged("AbrechnungFertig");
            }
        }

        #endregion

        private T GetWert<T>(T value)
        {
            return (Ende > DateTime.Now) ? default(T) : value;
        }

        public void ReindexVehicles()
        {
            int uoIdx = 1;
            foreach (var item in Fahrzeuge)
            {
                if (item.Index == 0)
                {
                    while (Fahrzeuge.Any(p => p.Index != 0 && p.Index == uoIdx)) uoIdx++;
                    item.Index = uoIdx;
                }
                item.ReindexTours();
            }
        }
    }

    public class PSCachedAttribute : Attribute
    {
    }
}