using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace TaxiTaxiWPF
{
    public enum ConditionType
    {
        Equal,
        NotEqual,
        LessThan,
        LessOrEqual,
        GreaterThan,
        GreaterOrEqual
    }

    public class ConditionalBooleanConverter : MarkupExtension, IValueConverter
    {
        public ConditionalBooleanConverter()
        {
            Epsilon = 0.01F;
            CompareValue = 0.0F;
        }

        public ConditionType Condition { get; set; }
        public float Epsilon { get; set; }
        public float CompareValue { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var v = (float)value;
            switch (Condition)
            {
                case ConditionType.Equal:
                    return Math.Abs(v - CompareValue) < Epsilon;
                case ConditionType.NotEqual:
                    return Math.Abs(v - CompareValue) >= Epsilon;
                case ConditionType.LessThan:
                    return v < CompareValue;
                case ConditionType.LessOrEqual:
                    return v <= CompareValue;
                case ConditionType.GreaterOrEqual:
                    return v >= CompareValue;
                case ConditionType.GreaterThan:
                    return v > CompareValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
