using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace TaxiTaxiWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
            {
                if ((e == null) || (e.Exception == null))
                {
                    return;
                }

                using (var sw = File.AppendText(@".\exceptions.txt"))
                {
                    sw.WriteLine(e.Exception);
                }
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if ((e == null) || (e.ExceptionObject == null))
                {
                    return;
                }

                using (var sw = File.AppendText(@".\exceptions.txt"))
                {
                    sw.WriteLine(e.ExceptionObject);
                }
            };
        }
    }
}
