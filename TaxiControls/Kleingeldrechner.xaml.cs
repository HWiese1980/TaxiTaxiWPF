using System;
using System.Collections.Generic;
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

namespace TaxiTaxiWPF.TaxiControls
{
    using System.Globalization;

    /// <summary>
    /// Interaktionslogik für Kleingeldrechner.xaml
    /// </summary>
    public partial class Kleingeldrechner : UserControl
    {
        public Kleingeldrechner()
        {
            InitializeComponent();
        }

        private void RecalcSum(object sender, TextChangedEventArgs e)
        {
            try
            {
                int v50e, v20e, v10e, v5e, v2e, v1e, v50c, v20c, v10c;

                bool parseOK = true;

                parseOK &= int.TryParse(tb50e.Text, out v50e);
                parseOK &= int.TryParse(tb20e.Text, out v20e);
                parseOK &= int.TryParse(tb10e.Text, out v10e);
                parseOK &= int.TryParse(tb05e.Text, out v5e);
                parseOK &= int.TryParse(tb02e.Text, out v2e);
                parseOK &= int.TryParse(tb01e.Text, out v1e);
                parseOK &= int.TryParse(tb50c.Text, out v50c);
                parseOK &= int.TryParse(tb20c.Text, out v20c);
                parseOK &= int.TryParse(tb10c.Text, out v10c);
                if (!parseOK) return;

                var sum = new[] { v50e * 50.0F, v20e * 20.0F, v10e * 10.0F, v5e * 5.0F, v2e * 2.0F, v1e * 1.0F, v50c * 0.5F, v20c * 0.2F, v10c * 0.1F }.Sum();

                sumLbl.Content = sum.ToString("c", new CultureInfo("de-DE"));
            }
            catch
            {

            }
        }
    }
}
