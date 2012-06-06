using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;
using TaxiTaxiWPF.TaxiData;

namespace TaxiTaxiWPF.TaxiControls
{
    internal class TourSort : IComparer
    {
        public int Compare(object x, object y)
        {
            return ((Fahrt)x).SortOrder.CompareTo(((Fahrt)y).SortOrder);
        }
    }
}