#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace TaxiTaxiWPF.TaxiData
{
    public class Fahrt : TaxiBase
    {
        private float? _apes;
        private string _description;
        private int? _km;
        private bool _ohneUhr;
        private float? _preis;
        private bool _rechnungsfahrt;
        private bool _sonderausgabe;

        [ObserveProperty(Dependency = "KM")]
        [ObserveProperty(Dependency = "Description")]
        public int SortOrder
        {
            get
            {
                if ((Description ?? "").StartsWith("Hof")) return 0;
                return KM != null ? 2 : 1;
            }
        }

        [ObserveProperty(Dependency = "Rechnungsfahrt")]
        public bool Rechnungsfahrt
        {
            get { return _rechnungsfahrt; }
            set
            {
                _rechnungsfahrt = value;
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "OhneUhr")]
        public bool OhneUhr
        {
            get { return _ohneUhr; }
            set
            {
                _ohneUhr = value;
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "Sonderausgabe")]
        public bool Sonderausgabe
        {
            get { return _sonderausgabe; }
            set
            {
                _sonderausgabe = value;
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "KM")]
        public int? KM
        {
            get { return _km; }
            set
            {
                _km = value;
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "APES")]
        public float? APES
        {
            get { return _apes; }
            set
            {
                _apes = value;
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "Sonderausgabe")]
        [ObserveProperty(Dependency = "Preis")]
        public float? Preis
        {
            get { return _preis; }
            set
            {
                _preis = value;
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "Description")]
        public string Description
        {
            get { return _description; }
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        [ObserveProperty(Dependency = "Rechnungsfahrt")]
        [ObserveProperty(Dependency = "Preis")]
        [ObserveProperty(Dependency = "APES")]
        [ObserveProperty(Dependency = "Soldatenfahrt")]
        public float Bezahlt { get { return (Rechnungsfahrt) ? 0.0F : (Preis ?? 0.0F) - (APES ?? 0.0F); } }

        public override string Name
        {
            get { return string.Format("Fahrt '{0}'", Description); }
        }
    }
}