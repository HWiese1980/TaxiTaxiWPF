using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TaxiTaxiWPF.TaxiData
{
    internal static class Helper
    {
        public static T AttValue<T>(this XElement elm, string attributeName)
        {
            var attr = elm.Attribute(attributeName);
            if (attr == null) return default(T);
            return (T) Convert.ChangeType(attr.Value, typeof (T));
        }

        public static string Times(this string input, int count)
        {
            string ret = "";
            for (int i = 0; i < count; i++) ret += input;
            return ret;
        }
    }
}
