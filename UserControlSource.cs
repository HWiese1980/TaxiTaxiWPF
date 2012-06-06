using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Xaml;

namespace TaxiTaxiWPF
{
    public class UserControlSource : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            var root = rootObjectProvider.RootObject;
            return root;
        }
    }
}
