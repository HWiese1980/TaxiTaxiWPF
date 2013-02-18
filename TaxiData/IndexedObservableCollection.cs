using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SeveQsDataBase;

namespace TaxiTaxiWPF.TaxiData
{
    public class IndexedObservableCollection<T> : ObservableCollection<T> where T : IIndexed
    {
        public IndexedObservableCollection()
        {
            RefreshIndices();
        }

        public IndexedObservableCollection(IEnumerable<T> i) : base(i)
        {
            RefreshIndices();
        }

        public IndexedObservableCollection(List<T> l)
            : base(l)
        {
            RefreshIndices();
        }

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            RefreshIndices();
        }

        private void RefreshIndices()
        {
            var items = this.OrderBy(p => p.Index).ToArray();
            
            if (items.Length <= 0) return;
            items[0].Index = 1;

            for(int i = 0; i < items.Length-1; i++)
            {
                items[i + 1].Index = items[i].Index + 1;
            }
        }
    }
}
