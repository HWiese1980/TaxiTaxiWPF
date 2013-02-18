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
            fahrtenDG.SetupSortDescriptions += (x, y) =>
            {
                y.Value.SortDescriptions.Add(new SortDescription("Index", ListSortDirection.Ascending));
                y.Value.Refresh();
            };
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

                                        if (!fahrzeug.Fahrten.Any(p => p.Description.StartsWith("Hof")))
                                            fahrzeug.Fahrten.Add(new Fahrt { Description = "Hof-Scheeßel-Hof", KM = 20 });

                                        int fehlendeFahrten = fahrzeug.TourenTotal - fahrzeug.TourCount;
                                        for (int i = 1; i <= fehlendeFahrten; i++)
                                        {
                                            float preis = 54.0F + (float)Math.Round(r.NextDouble() * 2.0F - 1.0F, 2);

                                            if (i == fehlendeFahrten) preis = fahrzeug.Preisdifferenz;

                                            preis = (float)Math.Round(preis / 10.0F, 2) * 10.0F;
                                            var f = new Fahrt { Description = "Scheeßel-Seedorf", APES = 48.0F - preis, Preis = preis };
                                            fahrzeug.Fahrten.Add(f);

                                        }
                                    }));
        }

        private void MarkierenAlsSoldatenfahrtClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in fahrtenDG.SelectedItems.Cast<Fahrt>())
            {
                if (item.Preis != null) item.APES = item.Preis - 48.0F;
                item.Description = "Scheeßel-Seedorf";
                if (item.KM != null) item.Description = "Hof-Scheeßel-Hof";
            }
        }

        private void FahrtHochClick(object sender, RoutedEventArgs e)
        {
            var fh = fahrtenDG.SelectedItem as Fahrt;
            if (fh == null) return;
            var ig = fh.IndexGroup;
            var otherfh = ig.SingleOrDefault(p => p.Index == fh.Index - 1);
            if(otherfh != null)
            {
                otherfh.Index += 1;
                fh.Index -= 1;
            }
            
        }

        private void FahrtRunterClick(object sender, RoutedEventArgs e)
        {
            var fh = fahrtenDG.SelectedItem as Fahrt;
            if (fh == null) return;
            var ig = fh.IndexGroup;
            var otherfh = ig.SingleOrDefault(p => p.Index == fh.Index + 1);
            if (otherfh != null)
            {
                otherfh.Index -= 1;
                fh.Index += 1;
            }
            
        }

        private void fahrtenDG_InitializingNewItem_1(object sender, InitializingNewItemEventArgs e)
        {
            
        }
    }
}
