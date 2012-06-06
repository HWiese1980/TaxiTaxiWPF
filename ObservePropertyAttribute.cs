using System;
using System.Collections.Generic;
using System.Linq;

namespace TaxiTaxiWPF
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class ObservePropertyAttribute : Attribute
    {
        public string Dependency { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class RefreshPropertyAttribute : Attribute
    {
        public string Tag { get; set; }
    }
}
