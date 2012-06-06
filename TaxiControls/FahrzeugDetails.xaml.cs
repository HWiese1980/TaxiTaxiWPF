using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaxiTaxiWPF.TaxiData;

namespace TaxiTaxiWPF.TaxiControls
{
    /// <summary>
    /// Interaction logic for FahrzeugDetails.xaml
    /// </summary>
    public partial class FahrzeugDetails : UserControl
    {
        public FahrzeugDetails()
        {
            InitializeComponent();

            //Loaded += (x, y) => SortOrderUpdate();
            //DataContextChanged += (x, y) => SortOrderUpdate();
        }

        private void SortOrderUpdate()
        {
            if (!(DataContext is Fahrzeug)) return;
            var fzg = (Fahrzeug)DataContext;
            if (fzg == null) return;

            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(fzg.Fahrten);
            if (view == null) return;

            view.CustomSort = new TourSort();
            view.Refresh();
        }

        private void AddTourClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(
                                      () =>
                                      {
                                          var fahrzeug = (Fahrzeug)DataContext;
                                          if (fahrzeug == null) return;

                                          fahrzeug.Fahrten.Add(new Fahrt());
                                      }));
        }

        private void AddSpecialTourClick(object sender, RoutedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
                                    {
                                        var r = new Random((int)DateTime.Now.Ticks);
                                        var fahrzeug = (Fahrzeug)DataContext;
                                        if (fahrzeug == null) return;

                                        if(!fahrzeug.Fahrten.Any(p => p.Description.StartsWith("Hof")))
                                            fahrzeug.Fahrten.Add(new Fahrt { Description = "Hof-Scheeßel-Hof", KM = 20 });

                                        int fehlendeFahrten = fahrzeug.TourenTotal - fahrzeug.TourCount;
                                        for (int i = 1; i <= fehlendeFahrten; i++)
                                        {
                                            float preis = 50.0F + (float)Math.Round(r.NextDouble() * 3.0F - 2.0F, 2);

                                            if (i == fehlendeFahrten) preis = fahrzeug.Preisdifferenz;

                                            preis = (float)Math.Round(preis / 10.0F, 2) * 10.0F;
                                            var f = new Fahrt { Description = "Scheeßel-Seedorf", APES = preis - 40.0F, Preis = preis };
                                            fahrzeug.Fahrten.Add(f);

                                        }
                                    }));
        }

        private void MarkierenAlsSoldatenfahrtClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in fahrtenDG.SelectedItems.Cast<Fahrt>())
            {
                if (item.Preis != null) item.APES = item.Preis - 40.0F;
                item.Description = "Scheeßel-Seedorf";
                if (item.KM != null) item.Description = "Hof-Scheeßel-Hof";
            }
        }

    }
}
