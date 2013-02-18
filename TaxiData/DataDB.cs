#region Usings

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Xml.Linq;
using SeveQsDataBase;

#endregion

namespace TaxiTaxiWPF.TaxiData
{
    using System.Threading.Tasks;
    using System.Windows.Data;

    public class DataDB : DataBaseDB
    {
        private ObservableCollection<Schicht> _schichten;

        public DateTime DatumAusgewählteSchicht
        {
            get
            {
                return (View != null && View.CurrentItem != null) ? (View.CurrentItem as Schicht).Anfang.Date : DateTime.Now.Date;
            }
        }

        public int SelectedWeek { get; private set; }

        [ObserveProperty]
        [ObserveProperty(Dependency = "Schicht.Anfang")]
        [IsGlobalProperty]
        public ICollectionView SchichtenView
        {
            get
            {
                if (View == null)
                {
                    View = CollectionViewSource.GetDefaultView(_schichten);
                    View.CollectionChanged += (x, y) =>
                        {
                            if (!Equals(y.NewItems, y.OldItems)) this.OnPropertyChanged("SchichtenView", "Collection");
                        };
                }
                View.SortDescriptions.Clear();
                View.SortDescriptions.Add(new SortDescription("Anfang", ListSortDirection.Descending));
                return View;
            }
        }

        public ObservableCollection<Schicht> Schichten
        {
            get { return _schichten; }
            set
            {
                _schichten = value;
                var shs = value.OrderBy(p => p.Anfang).ToList();
                foreach (var item in shs)
                {
                    item.Parent = this;
                    item.Index = shs.IndexOf(item);
                    item.IndexGroup = _schichten;
                }
                _schichten.CollectionChanged += (sender, args) =>
                                                {
                                                    foreach (Schicht sh in args.NewItems ?? new List<Schicht>())
                                                    {
                                                        sh.Index = Schichten.Max(p => p.Index) + 1;
                                                        sh.IndexGroup = _schichten;
                                                        sh.Parent = this;
                                                    }
                                                    OnPropertyChanged("Schichten");
                                                };
                OnPropertyChanged("Schichten");
            }
        }

        [ObserveProperty(Dependency = "Schichten")]
        [ObserveProperty(Dependency = "Schicht.Stunden")]
        [ObserveProperty(Dependency = "Schicht.Trinkgeld")]
        [ObserveProperty(Dependency = "Schicht.Fahrzeuge")]
        [ObserveProperty(Dependency = "Schicht.Fahrzeug.Fahrten")]
        [IsGlobalProperty]
        public double TipProTrip
        {
            get
            {
                var tripCount = Schichten.Where(p => p.Trinkgeld >= 0.0D).Sum(p => p.Fahrzeuge.Sum(q => q.Fahrten.Count));
                var tipSum = Schichten.Sum(p => Math.Max(0.0, p.Trinkgeld));
                return (tripCount == 0) ? 0.0D : tipSum / tripCount;
            }
        }

        [ObserveProperty(Dependency = "Schichten")]
        [ObserveProperty(Dependency = "Schicht.Stunden")]
        [ObserveProperty(Dependency = "Schicht.Trinkgeld")]
        [IsGlobalProperty]
        public double TipProStunde
        {
            get
            {
                var stunden = Schichten.Where(p => p.Trinkgeld >= 0.0D).Sum(p => p.Stunden);
                var tipSum = Schichten.Sum(p => Math.Max(0.0, p.Trinkgeld));
                return (stunden == 0) ? 0.0D : tipSum / stunden;
            }
        }

        public DataDB()
        {
            Schichten = new ObservableCollection<Schicht>();
        }

        [ObserveProperty(Dependency = "Schicht.Stunden")]
        [ObserveProperty(Dependency = "Schichten")]
        [IsGlobalProperty]
        public double Reststunden
        {
            get { return 66 - Monatsstatistik.Where(p => p.Key.Year == DatumAusgewählteSchicht.Year && p.Key.Month == DatumAusgewählteSchicht.Month).Sum(p => p.Stunden); }
        }


        [ObserveProperty(Dependency = "Schicht.Stunden")]
        [ObserveProperty(Dependency = "Schicht.EignerVerdienst")]
        [ObserveProperty(Dependency = "Schichten")]
        [IsGlobalProperty]
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
        [IsGlobalProperty]
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
        [IsGlobalProperty]
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

        public override void Save(string txiFile)
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
                                    new XAttribute("Index", m.Index),
                                    new XAttribute("IstAbgerechnet", m.Abgerechnet),
                                    new XAttribute("AbrechnungFertig", m.AbrechnungFertig),
                                    new XAttribute("Anfang", m.Anfang.ToString(cultureInfo)),
                                    new XAttribute("Ende", m.Ende.ToString(cultureInfo)),
                                    new XAttribute("GeldVorher", m.GeldVorher.ToString(cultureInfo)),
                                    new XAttribute("GeldNachher", m.GeldNachher.ToString(cultureInfo)),
                                    new XAttribute("GeliehenesWechselgeld", m.GeliehenesWechselgeld.ToString(cultureInfo)),
                                    from n in m.Sonderausgaben
                                    select new XElement(
                                        "Sonderausgabe", new XAttribute("Wert", n.ToString(cultureInfo))),
                                    from n in m.Privatausgaben
                                    select new XElement(
                                        "Privatausgabe",
                                        new XAttribute("Index", n.Index),
                                        new XAttribute("Bemerkung", n.Bemerkung ?? ""),
                                        new XAttribute("Wert", n.Wert.ToString(cultureInfo))),
                                    from n in m.Fahrzeuge
                                    select
                                            new XElement(
                                            "Fahrzeug",
                                            new XAttribute("Index", n.Index),
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
                                            where o.KM != null || o.Preis != null || o.APES != null || !String.IsNullOrEmpty(o.Description)
                                            orderby o.Index
                                            select
                                                    new XElement(
                                                    "Fahrt",
                                                    new XAttribute("Index", o.Index),
                                                    new XAttribute("Bemerkung", (o.Description ?? "")),
                                                    new XAttribute("km", ((o.KM == null) ? "NULL" : o.KM.Value.ToString(cultureInfo))),
                                                    new XAttribute("APES", ((o.APES == null) ? "NULL" : o.APES.Value.ToString(cultureInfo))),
                                                    new XAttribute("Preis", ((o.Preis == null) ? "NULL" : o.Preis.Value.ToString(cultureInfo))),
                                                    new XAttribute("IstRechnungsfahrt", o.Rechnungsfahrt))))));
            doc.Save(txiFile);
        }

        public event EventHandler<ValueEventArgs<string>> FileLoaded;

        private void LoadAsync(string txiFile)
        {
            var doc = XDocument.Load(txiFile);

            var shiftCount = doc.Element("Schichten").Descendants().Count();
            int verarb = 0;

            Schichten = new ObservableCollection<Schicht>();

            // Parallel.ForEach(doc.Element("Schichten").Elements("Schicht"), ig =>
            foreach (var ig in doc.Element("Schichten").Elements("Schicht"))
            {
                var i = ig;
                var schicht = new Schicht()
                                  {
                                      Index = i.AttValue<int>("Index"),
                                      AbrechnungFertig = i.AttValue<bool>("AbrechnungFertig"),
                                      Abgerechnet = i.AttValue<bool>("IstAbgerechnet"),
                                      Anfang = i.AttValue<DateTime>("Anfang"),
                                      Ende = i.AttValue<DateTime>("Ende"),
                                      GeldVorher = i.AttValue<float>("GeldVorher"),
                                      GeldNachher = i.AttValue<float>("GeldNachher"),
                                      GeliehenesWechselgeld = i.AttValue<float>("GeliehenesWechselgeld")
                                  };

                foreach (var j in i.Elements("Sonderausgabe"))
                {
                    schicht.Sonderausgaben.Add(j.AttValue<float>("Wert"));
                }

                foreach (var j in i.Elements("Privatausgabe"))
                {
                    var priv = new Privatausgabe { Index = j.AttValue<int>("Index"), Bemerkung = j.AttValue<string>("Bemerkung") ?? "n/a", Wert = j.AttValue<double>("Wert") };
                    schicht.Privatausgaben.Add(priv);
                }

                foreach (var n in i.Elements("Fahrzeug"))
                {
                    var fzg = new Fahrzeug
                                  {
                                      Index = n.AttValue<int>("Index"),
                                      Nummer = n.AttValue<int>("Nummer"),
                                      TotalAnfang = n.AttValue<Single>("TotalAnfang"),
                                      TotalEnde = n.AttValue<Single>("TotalEnde"),
                                      BesetztAnfang = n.AttValue<Single>("BesetztAnfang"),
                                      BesetztEnde = n.AttValue<Single>("BesetztEnde"),
                                      TourenAnfang = n.AttValue<int>("TourenAnfang"),
                                      TourenEnde = n.AttValue<int>("TourenEnde"),
                                      PreisAnfang = n.AttValue<Single>("EinnahmenAnfang"),
                                      PreisEnde = n.AttValue<Single>("EinnahmenEnde"),
                                  };

                    schicht.Fahrzeuge.Add(fzg);

                    foreach (var p in n.Elements("Fahrt"))
                    {
                        var _kmV = p.Attribute("km").Value;
                        var _km = (int?)p.AttValue<int>("km");
                        var _apesV = p.Attribute("APES").Value;
                        var _apes = (float?)p.AttValue<Single>("APES");
                        var _priceV = p.Attribute("Preis").Value;
                        var _price = (float?)p.AttValue<Single>("Preis");
                        var ft = new Fahrt
                                     {
                                         Index = p.AttValue<int>("Index"),
                                         Description = p.AttValue<string>("Bemerkung"),
                                         KM = (_kmV == "NULL") ? null : _km,
                                         APES = (_apesV == "NULL") ? null : _apes,
                                         Preis = (_priceV == "NULL") ? null : _price,
                                         Rechnungsfahrt = p.AttValue<bool>("IstRechnungsfahrt")
                                     };
                        this.OnProgress(ref verarb, shiftCount);
                        fzg.Fahrten.Add(ft);
                    }
                    this.OnProgress(ref verarb, shiftCount);
                }

                UIDispatcher.Invoke(new Action(() => this.Schichten.Add(schicht)));

                this.OnProgress(ref verarb, shiftCount);
            }
            int uoSIdx = 1;
            foreach (var item in Schichten.OrderBy(p => p.Anfang))
            {
                if (item.Index == 0)
                {
                    while (Schichten.Any(p => p.Index != 0 && p.Index == uoSIdx)) uoSIdx++;
                    item.Index = uoSIdx;
                    item.Parent = this;
                }
                item.ReindexVehicles();
            }
        }

        public async override void Load(string txiFile)
        {
            IsLoading = true;
            await Task.Run(() => LoadAsync(txiFile));
            IsLoading = false;
            OnFileLoaded(txiFile);
        }

        private void OnProgress(ref int verarb, int shiftCount)
        {
            verarb++;
            this.OnStatusChanged("Lade Daten: {0:f2}%", ((double)verarb / (double)shiftCount) * 100.0D);
        }

        [ObserveProperty(Dependency = "Schichten")]
        [ObserveProperty(Dependency = "Schicht.Fahrzeuge")]
        [ObserveProperty(Dependency = "Schicht.Fahrzeug.PreisTotal")]
        [ObserveProperty(Dependency = "Schicht.Fahrzeug.BesetztTotal")]
        [IsGlobalProperty]
        public double DurchschnittKMPreis
        {
            get
            {
                if (!Schichten.Any()) return 0;
                return Schichten.Average(p => p.PreisProKM);
            }
        }

        public override string Name { get { return "DataDB"; } }
    }
}