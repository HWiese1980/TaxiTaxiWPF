using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiTaxiWPF.TaxiData
{
    public class ObservableNotifyingObjectCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public ObservableNotifyingObjectCollection()
        {
        }

        public ObservableNotifyingObjectCollection(IEnumerable<T> t)
            : base(t)
        {
            if (t != null)
            {
                foreach (var f in this)
                {
                    f.PropertyChanged += OnObjectPropertyChanged;
                }
            }
        }

        public ObservableNotifyingObjectCollection(List<T> t)
            : base(t)
        {
            foreach (var f in this)
            {
                f.PropertyChanged += OnObjectPropertyChanged;
            }
        }

        public event EventHandler<PropertyChangedEventArgs> ObjectPropertyChanged;
        private void OnObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ObjectPropertyChanged != null) ObjectPropertyChanged(sender, e);
        }
        
        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach(T obj in e.NewItems ?? new List<T>[]{})
            {
                obj.PropertyChanged += OnObjectPropertyChanged;
            }
            foreach(T obj in e.OldItems ?? new List<T>[]{})
            {
                obj.PropertyChanged -= OnObjectPropertyChanged;
            }

            base.OnCollectionChanged(e);
        }
    }
}
