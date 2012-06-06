using System.Windows;
using System.Windows.Controls;
using TaxiTaxiWPF.TaxiData;
using System.Linq;

namespace TaxiTaxiWPF.TaxiControls
{
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
    }
}
