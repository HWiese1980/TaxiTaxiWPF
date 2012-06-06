using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace TaxiTaxiWPF.TaxiData
{
    public abstract class TaxiBase : INotifyPropertyChanged, IHasParent
    {
        public void Refresh()
        {
            var properties = GetType().GetProperties();
            foreach (var prop in properties) OnPropertyChanged(prop.Name);
        }

        public TaxiDB DB { get; set; }

        private readonly List<TaxiBase> _otherAncestors = new List<TaxiBase>();
        public IList<TaxiBase> OtherAncestors
        {
            get { return _otherAncestors; }
        }

        public TaxiBase Parent { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;
        public abstract string Name { get; }
        protected Guid _id;

        protected TaxiBase()
        {
            _id = Guid.NewGuid();
        }

        ~TaxiBase()
        {
            PropertyChanged = null;
        }

        public virtual void OnPropertyChanged(string property = "", string by = "", int recursionLevel = 0)
        {
            if (by == "") by = "Application";

            if (property == "")
            {
                // Console.WriteLine("------");
                var st = new StackTrace();
                var frames = st.GetFrames();
                property = frames[1].GetMethod().Name.Substring(4);
            }


            if(PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
            if (property == by) return;
            // Console.WriteLine("->".Times(recursionLevel) + "{0} refreshes {1} in {2}", by, property, GetType().Name);

            var ownDependingProperties = from p in GetType().GetProperties()
                                         let attrib = p.GetCustomAttributes(typeof (ObservePropertyAttribute), true).Cast<ObservePropertyAttribute>()
                                         where attrib.Any(q => q.Dependency == property || q.Dependency == String.Empty)
                                         select p;
            foreach(var ownProperty in ownDependingProperties) OnPropertyChanged(ownProperty.Name, property, recursionLevel + 1);

            string tag = GetType().Name + "." + property;
            if (Parent != null) Parent.OnPropertyChanged(tag, property, recursionLevel + 1);
            if (DB != null) DB.OnPropertyChanged(tag, property, recursionLevel + 1);


        }
        
        public override string ToString()
        {
            return string.Format("[{0} - {1} - {2}]", GetType().Name, _id, Name);
        }

    }

    public interface IHasParent
    {
        TaxiBase Parent { get; set; }
        IList<TaxiBase> OtherAncestors { get; }
    }
}