#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Microsoft.Win32;
using TaxiTaxiWPF.Properties;
using TaxiTaxiWPF.TaxiData;

#endregion

namespace TaxiTaxiWPF
{
    using System.Collections.ObjectModel;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    ///   Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private string _openFileName = "";
        private readonly OpenFileDialog ofd = new OpenFileDialog { Filter = "TaxiTaxi Datei|*.txi" };
        private readonly SaveFileDialog sfd = new SaveFileDialog { Filter = "TaxiTaxi Datei|*.txi" };

        private NativeApplicationClient _native = new NativeApplicationClient(GoogleAuthenticationServer.Description);

        public MainWindow()
        {
            LanguageProperty.OverrideMetadata(
                                              typeof(FrameworkElement),
                                              new FrameworkPropertyMetadata(
                                                      XmlLanguage.GetLanguage(
                                                                              CultureInfo.CurrentCulture.IetfLanguageTag)));
            _native.ClientIdentifier = "1078222205383.apps.googleusercontent.com";
            _native.ClientSecret = "xRTCPHENc7lPxcoRT4N2UMb8";

            InitializeComponent();

            /* SeveQsDataBase.DataBase.StatusChanged += (x, y) => this.Dispatcher.Invoke(new Func<object>(() => this.statusLbl.Content = y.Value)); */

            DB.FileLoaded += (x, y) =>
            {
                _openFileName = y.Value;
                var s = new Settings { LastFile = y.Value };
                s.Save();
            };

            DB.PropertyChanged += (x, y) =>
                {
                    if (y.PropertyName == "Schichten")
                    {
                        Dispatcher.Invoke(
                            new Action(() =>
                                {
                                    var b = new Binding("SchichtenView") { Source = this.DB, Path = new PropertyPath("SchichtenView") };
                                    this.shiftsDG.SetBinding(ItemsControl.ItemsSourceProperty, b);
                                }));
                    }
                };

            Loaded += (x, y) =>
            {

                var s = new Settings();
                DB.UIDispatcher = Dispatcher;
                if (!String.IsNullOrEmpty(s.LastFile))
                {
                    DB.Load(s.LastFile);
                }
            };
        }

        private DataDB DB { get { return (DataDB)FindResource("taxiDB"); } }

        private void AddShiftClick(object sender, RoutedEventArgs e)
        {
            DB.Schichten.Add(new Schicht(DB.Schichten));
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
                                        if (!(bool)ofd.ShowDialog())
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

        private void ButtonClick1(object sender, RoutedEventArgs e)
        {
            foreach (var schicht in DB.Schichten)
            {
                foreach (var fahrzeug in schicht.Fahrzeuge)
                {
                    foreach (var fahrt in fahrzeug.Fahrten)
                    {
                        fahrt.Refresh();
                    }
                    fahrzeug.Refresh();
                }
                schicht.Refresh();
            }
        }

        private void SelectedShiftChanged(object sender, SelectedCellsChangedEventArgs e)
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
                SummeTip.Content = String.Format("{0:c}", selectedShifts.Sum(p => p.Trinkgeld));
                SummeIncome.Content = String.Format("{0:c}", selectedShifts.Sum(p => p.EigenerVerdienst));
            }
            else
            {
                SummeIncome.Content = SummeTip.Content = SummeAbzaehlen.Content = "keine Schicht ausgewählt";
            }
        }

        private void shiftDetails_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}