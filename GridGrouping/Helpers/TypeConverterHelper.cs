using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers
{
    class TypeConverterHelper
    {
        public static object ChangeType(object value, Type type)
        {
            return ChangeType(value, type, null);
        }

        public static object ChangeType(object value, Type type, IFormatProvider provider)
        {
            TypeConverter typeConverter = TypeDescriptor.GetConverter(value.GetType());
            if (typeConverter != null && typeConverter.CanConvertTo(type))
                return typeConverter.ConvertTo(value, type);
            
            if (value is DBNull)
                return DBNull.Value;

            return Convert.ChangeType(value, type, provider);
        }
    }
}
