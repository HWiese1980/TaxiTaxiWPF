#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Microsoft.Win32;
using TaxiTaxiWPF.TaxiData;

#endregion

namespace TaxiTaxiWPF
{
    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _openFileName = "";
        private OpenFileDialog ofd = new OpenFileDialog() { Filter = "TaxiTaxi Datei|*.txi" };
        private SaveFileDialog sfd = new SaveFileDialog() { Filter = "TaxiTaxi Datei|*.txi" };

        public MainWindow()
        {
            LanguageProperty.OverrideMetadata(
                                              typeof(FrameworkElement),
                                              new FrameworkPropertyMetadata(
                                                      XmlLanguage.GetLanguage(
                                                                              CultureInfo.CurrentCulture.IetfLanguageTag)));
            InitializeComponent();
        }

        private TaxiDB DB { get { return (TaxiDB)FindResource("taxiDB"); } }

        private void AddShiftClick(object sender, RoutedEventArgs e)
        {
            DB.Schichten.Add(new Schicht());
        }

        private void CanAlways(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void DoExit(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void DoOpen(object sender, ExecutedRoutedEventArgs e)
        {
            Dispatcher.Invoke(
                              new Action(() =>
                                    {
                                        if (!(bool) ofd.ShowDialog())
                                        {
                                            return;
                                        }
                                        DB.Load(ofd.FileName);
                                        _openFileName = ofd.FileName;
                                    }));
        }

        private void DoSaveAs(object sender, ExecutedRoutedEventArgs e)
        {
            if (!(bool)sfd.ShowDialog())
            {
                return;
            }
            DB.Save(sfd.FileName);
            _openFileName = sfd.FileName;
        }

        private void DoSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_openFileName))
            {
                DoSaveAs(sender, e);
            }
            else
            {
                DB.Save(_openFileName);
            }
        }

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !String.IsNullOrEmpty(_openFileName);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach(var schicht in DB.Schichten)
            {
                foreach(var fahrzeug in schicht.Fahrzeuge)
                {
                    foreach(var fahrt in fahrzeug.Fahrten)
                    {
                        fahrt.Refresh();
                    }
                    fahrzeug.Refresh();
                }
                schicht.Refresh();
            }
        }

        private void shiftsDG_Selected_1(object sender, RoutedEventArgs e)
        {
            
        }

        private void SelectedShiftChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            var shift = shiftsDG.SelectedItem as Schicht;
            shiftDetails.DataContext = shift;
            var fehler = new List<Validierungsfehler>();
            lbValidierung.ItemsSource = !Validierung.Validieren(shift, fehler, 1) ? fehler.OrderBy(p => p.Schwere) : null;
            var selectedObjects = shiftsDG.SelectedItems as IEnumerable<object>;
            if (selectedObjects != null)
            {
                var selectedShifts = selectedObjects.Where(p => p is Schicht).Cast<Schicht>();
                SummeAbzaehlen.Content = String.Format("{0:c}", selectedShifts.Sum(p => p.Abzaehlen));
            }
            else
            {
                SummeAbzaehlen.Content = "keine Schicht ausgewählt";
            }
        }
    }
}