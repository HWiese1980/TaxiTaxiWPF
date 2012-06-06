#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

#endregion

namespace TaxiTaxiWPF.TaxiData
{
    public class TaxiDB : TaxiBase
    {
        private ObservableCollection<Schicht> _schichten;

        public ObservableCollection<Schicht> Schichten
        {
            get { return _schichten; }
            set
            {
                _schichten = value;
                foreach (var item in value)
                {
                    item.DB = this;
                }
                _schichten.CollectionChanged += (sender, args) =>
                                                {
                                                    foreach (Schicht sh in args.NewItems ?? new List<Schicht>())
                                                    {
                                                        sh.DB = this;
                                                    }
                                                    OnPropertyChanged("Schichten");
                                                };
                OnPropertyChanged();
            }
        }

        public TaxiDB()
        {
            Schichten = new ObservableCollection<Schicht>();
        }

        [ObserveProperty(Dependency = "Schicht.Stunden")]
        [ObserveProperty(Dependency = "Schichten")]
        public double Reststunden
        {
            get { return 66 - Monatsstatistik.Where(p => p.Key.Month == DateTime.Now.Month).Sum(p => p.Stunden); }
        }


        [ObserveProperty(Dependency = "Schicht.Stunden")]
        [ObserveProperty(Dependency = "Schicht.EignerVerdienst")]
        [ObserveProperty(Dependency = "Schichten")]
        public IList<Statistikdaten<DateTime>> Wochenstatistik
        {
            get
            {
                var ret = (from s in Schichten.AsParallel()
                           let dow = CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(s.Anfang)
                           let fday = s.Anfang.AddDays(-((int)dow - 1)).Date
                           group s by fday
                               into sg
                               orderby sg.Key ascending
                               let h = sg.Sum(p => p.Stunden)
                               let v = sg.Sum(p => p.EigenerVerdienst)
                               let vh = v / h
                               select new Statistikdaten<DateTime>
                               {
                                   Key = sg.Key,
                                   Stunden = h,
                                   Verdienst = v,
                                   VerdienstProStunde = vh,
                                   Bemerkung = String.Format("Woche von {0:ddd, dd.MM.} bis {1:ddd, dd.MM.yyyy}", sg.Key, sg.Key.AddDays(6)),
                               }).ToList();

                return ret;
            }
        }

        [ObserveProperty(Dependency = "Schicht.Stunden")]
        [ObserveProperty(Dependency = "Schicht.EignerVerdienst")]
        [ObserveProperty(Dependency = "Schichten")]
        public IList<Statistikdaten<DateTime>> Monatsstatistik
        {
            get
            {
                var ret = (from s in Schichten.AsParallel()
                           let date = new DateTime(s.Anfang.Year, s.Anfang.Month, 1)
                           group s by date
                               into sg
                               orderby sg.Key ascending
                               let h = sg.Sum(p => p.Stunden)
                               let v = sg.Sum(p => p.EigenerVerdienst)
                               let vh = v / h
                               select new Statistikdaten<DateTime>
                                      {
                                          Key = sg.Key,
                                          Stunden = h,
                                          Verdienst = v,
                                          VerdienstProStunde = vh
                                      }).ToList();

                return ret;
            }
        }

        [ObserveProperty(Dependency = "Schicht.Stunden")]
        [ObserveProperty(Dependency = "Schicht.EignerVerdienst")]
        [ObserveProperty(Dependency = "Schichten")]
        public IList<Statistikdaten<DateTime>> Jahresstatistik
        {
            get
            {
                var ret = (from s in Schichten.AsParallel()
                           let date = new DateTime(s.Anfang.Year, 1, 1)
                           group s by date
                               into sg
                               orderby sg.Key ascending
                               let h = sg.Sum(p => p.Stunden)
                               let v = sg.Sum(p => p.EigenerVerdienst)
                               let vh = v / h
                               select new Statistikdaten<DateTime> { Key = sg.Key, Stunden = h, Verdienst = v, VerdienstProStunde = vh }).ToList();

                return ret;
            }
        }

        public void Save(string txiFile)
        {
            var cultureInfo = new CultureInfo("de-DE");
            var doc = new XDocument(
                    new XElement(
                            "Schichten",
                            from m in Schichten
                            orderby m.Anfang
                            select
                                    new XElement(
                                    "Schicht",
                                    new XAttribute("IstAbgerechnet", m.Abgerechnet),
                                    new XAttribute("Anfang", m.Anfang.ToString(cultureInfo)),
                                    new XAttribute("Ende", m.Ende.ToString(cultureInfo)),
                                    new XAttribute("GeldVorher", m.GeldVorher.ToString(cultureInfo)),
                                    new XAttribute("GeldNachher", m.GeldNachher.ToString(cultureInfo)),
                                    from n in m.Sonderausgaben
                                    select new XElement(
                                        "Sonderausgabe", new XAttribute("Wert", n.ToString(cultureInfo))),
                                    from n in m.Fahrzeuge
                                    select
                                            new XElement(
                                            "Fahrzeug",
                                            new XAttribute("Nummer", n.Nummer.ToString(cultureInfo)),
                                            new XAttribute("TotalAnfang", n.TotalAnfang.ToString(cultureInfo)),
                                            new XAttribute("TotalEnde", n.TotalEnde.ToString(cultureInfo)),
                                            new XAttribute("BesetztAnfang", n.BesetztAnfang.ToString(cultureInfo)),
                                            new XAttribute("BesetztEnde", n.BesetztEnde.ToString(cultureInfo)),
                                            new XAttribute("TourenAnfang", n.TourenAnfang.ToString(cultureInfo)),
                                            new XAttribute("TourenEnde", n.TourenEnde.ToString(cultureInfo)),
                                            new XAttribute("EinnahmenAnfang", n.PreisAnfang.ToString(cultureInfo)),
                                            new XAttribute("EinnahmenEnde", n.PreisEnde.ToString(cultureInfo)),
                                            from o in n.Fahrten
                                            select
                                                    new XElement(
                                                    "Fahrt",
                                                    new XAttribute("Bemerkung", (o.Description ?? "")),
                                                    new XAttribute("km", (o.KM ?? -1).ToString(cultureInfo)),
                                                    new XAttribute("APES", (o.APES ?? -1.0F).ToString(cultureInfo)),
                                                    new XAttribute("Preis", (o.Preis ?? -1.0F).ToString(cultureInfo)),
                                                    new XAttribute("IstRechnungsfahrt", o.Rechnungsfahrt),
                                                    new XAttribute("IstOhneUhr", o.OhneUhr),
                                                    new XAttribute("IstSonderausgabe", o.Sonderausgabe))))));
            doc.Save(txiFile);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public void Load(string txiFile)
        {
            IsLoading = true;
            var tsk = new BackgroundWorker();
            tsk.DoWork += (x, y) =>
                    {
                        var doc = XDocument.Load(txiFile);

                        var schichten = (from m in doc.Element("Schichten").Elements("Schicht").AsParallel()
                                         let shift =
                                                 new Schicht
                                                 {
                                                     Abgerechnet = m.Attribute("IstAbgerechnet").Value.ToLower() == "true",
                                                     Anfang = Convert.ToDateTime(m.Attribute("Anfang").Value),
                                                     Ende = Convert.ToDateTime(m.Attribute("Ende").Value),
                                                     GeldVorher = Convert.ToSingle(m.Attribute("GeldVorher").Value),
                                                     GeldNachher = Convert.ToSingle(m.Attribute("GeldNachher").Value),
                                                     Sonderausgaben = new ObservableCollection<float>(
                                                             from s in m.Elements("Sonderausgabe").AsParallel()
                                                             select s.AttValue<float>("Wert")),
                                                     Fahrzeuge = new ObservableCollection<Fahrzeug>(
                                                             from n in m.Elements("Fahrzeug").AsParallel()
                                                             select
                                                                     new Fahrzeug
                                                                     {
                                                                         Nummer = n.AttValue<int>("Nummer"),
                                                                         TotalAnfang = n.AttValue<Single>("TotalAnfang"),
                                                                         TotalEnde = n.AttValue<Single>("TotalEnde"),
                                                                         BesetztAnfang = n.AttValue<Single>("BesetztAnfang"),
                                                                         BesetztEnde = n.AttValue<Single>("BesetztEnde"),
                                                                         TourenAnfang = n.AttValue<int>("TourenAnfang"),
                                                                         TourenEnde = n.AttValue<int>("TourenEnde"),
                                                                         PreisAnfang = n.AttValue<Single>("EinnahmenAnfang"),
                                                                         PreisEnde = n.AttValue<Single>("EinnahmenEnde"),
                                                                         Fahrten = new ObservableCollection<Fahrt>(
                                                                                 from p in n.Elements("Fahrt").AsParallel()
                                                                                 let _km = (int?)p.AttValue<int>("km")
                                                                                 let _apes = (float?)p.AttValue<Single>("APES")
                                                                                 let _price = (float?)p.AttValue<Single>("Preis")
                                                                                 select
                                                                                         new Fahrt
                                                                                         {
                                                                                             Description = p.AttValue<string>("Bemerkung"),
                                                                                             KM = (_km < 0) ? null : _km,
                                                                                             APES = (_apes < 0) ? null : _apes,
                                                                                             Preis = (_price < 0) ? null : _price,
                                                                                             Rechnungsfahrt = p.AttValue<bool>("IstRechnungsfahrt"),
                                                                                             OhneUhr = p.AttValue<bool>("IstOhneUhr")
                                                                                         })
                                                                     })
                                                 }
                                         orderby shift.Anfang descending
                                         select shift).ToArray();

                        Schichten = new ObservableCollection<Schicht>(schichten);
                    };

            tsk.RunWorkerCompleted += (x, y) => { IsLoading = false; };
            tsk.RunWorkerAsync();

        }

        public override string Name { get { return "TaxiDB"; } }
    }
}