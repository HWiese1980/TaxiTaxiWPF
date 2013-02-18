using System.Windows;
using System.Windows.Controls;
using TaxiTaxiWPF.TaxiData;
using System.Linq;

namespace TaxiTaxiWPF.TaxiControls
{
    using SeveQsDataBase;

    /// <summary>
    /// Interaction logic for ShiftDetail.xaml
    /// </summary>
    public partial class ShiftDetail : UserControl
    {
        public ShiftDetail()
        {
            InitializeComponent();

            DataContextChanged += (sender, args) =>
                                  {
                                      if (args.NewValue ==
                                      null || !(args.NewValue is Schicht)) return;

                                      var dc = (Schicht) args.NewValue;
                                      selectedVehicle.SelectedItem = dc.Fahrzeuge.FirstOrDefault();
                                  };
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var shf = (Schicht) DataContext;
            if (shf == null) return;

            shf.Fahrzeuge.Add(new Fahrzeug());

        }

        private void VonVorschichtClick(object sender, RoutedEventArgs e)
        {
            var sh = DataContext as Schicht;
            var db = Application.Current.MainWindow.FindResource("taxiDB") as DataDB;
            if (sh == null || db == null) return;

            var shs = db.Schichten.OrderBy(p => p.Anfang).ToList();
            var psh_idx = shs.IndexOf(sh) - 1;
            var psh = shs[psh_idx];

            FrameworkElement elm = (FrameworkElement)sender;
            var tag = elm.Tag.ToString();
            if (tag == "1")
            {
                sh.GeldVorher = psh.GeldNachher;
            }
            else if(tag == "2")
            {
                sh.GeldVorher = psh.GeldNachher - psh.Abzaehlen;
            }
        }
    }
}
