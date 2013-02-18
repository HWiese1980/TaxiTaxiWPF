using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Windows.Controls;
using System.Windows.Data;
using SeveQsDataBase;
using TaxiTaxiWPF.TaxiData;

namespace TaxiTaxiWPF.TaxiControls
{
    public class MyDataGrid : DataGrid
    {
        public event EventHandler<ValueEventArgs<CollectionView>> SetupSortDescriptions;

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (SetupSortDescriptions != null && (newValue != null)) 
                SetupSortDescriptions(this, new ValueEventArgs<CollectionView>((CollectionView)newValue)); 

            base.OnItemsSourceChanged(oldValue, newValue);
        }
    }
}