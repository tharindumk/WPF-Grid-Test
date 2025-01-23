using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.ComponentModel;
using System.Collections;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers
{
    public class GridCellValueConvert
    {
        /// <summary>
        /// Generates display text using the specified format, culture info and number format.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <param name="valueType">The value type on which formatting is based. The original value will first be converted to this type.</param>
        /// <param name="format">The format like in ToString(string format).</param>
        /// <param name="ci">The <see cref="CultureInfo"/> for formatting the value.</param>
        /// <param name="nfi">The <see cref="NumberFormatInfo"/> for formatting the value.</param>
        /// <returns>The string with the formatted text for the value.</returns>
        public static string FormatValue(object value, Type valueType, string format, CultureInfo ci, NumberFormatInfo nfi)
        {
            string strResult;
            object obj;
            try
            {
                if (value is string)
                    return (string)value;
                else if (value is byte[] || value is System.Drawing.Image) // Picture
                    return "";
                else if (value == null || valueType == null || value.GetType() == valueType)
                    obj = value;
                else
                {
                    try
                    {
                        obj = ChangeType(value, valueType, ci, true);
                    }
                    catch (Exception ex)
                    {
                        obj = value;
                        if (!(ex is FormatException || ex.InnerException is FormatException))
                            throw;
                    }
                }

                if (obj == null || obj is System.DBNull)
                    strResult = String.Empty;	// or "NullString"
                else
                {
                    if (obj is IFormattable)
                    {
                        IFormattable formattableValue = (IFormattable)obj;
                        IFormatProvider provider = null;
                        if (nfi != null && !(obj is DateTime))
                            provider = nfi;
                        else if (ci != null)
                            provider = obj is DateTime ? (IFormatProvider)ci.DateTimeFormat : (IFormatProvider)ci.NumberFormat;

                        if (format.Length > 0 || nfi != null)
                            strResult = formattableValue.ToString(format, provider);
                        else
                            strResult = formattableValue.ToString();
                    }
                    else
                    {
                        TypeConverter tc = TypeDescriptor.GetConverter(obj.GetType());
                        if (tc.CanConvertTo(typeof(string)))
                        {
                            strResult = (string)tc.ConvertTo(null, ci, obj, typeof(string));
                        }
                        else if (obj is IConvertible)
                            strResult = Convert.ToString(obj, ci);
                        else
                            strResult = obj.ToString();
                    }
                }
            }
            catch
            {
                strResult = String.Empty;
                throw;   // TODO: should I throw a more specific instead?
            }
            if (strResult == null)
                strResult = String.Empty;

            if (allowFormatValueTrimEnd)
                strResult = strResult.TrimEnd();
            return strResult;
        }

        static bool allowFormatValueTrimEnd = false;

        /// <summary>
        /// Converts value from one type to another using an optional <see cref="IFormatProvider"/>.
        /// </summary>
        /// <param name="value">The original value.</param>
        /// <param name="type">The target type.</param>
        /// <param name="provider">A <see cref="IFormatProvider"/> used to format or parse the value.</param>
        /// <param name="returnDbNUllIfNotValid">Indicates whether exceptions should be avoided or catched and return value should be DBNull if
        /// it cannot be converted to the target type.</param>
        /// <returns>The new value in the target type.</returns>
        public static object ChangeType(object value, Type type, IFormatProvider provider, bool returnDbNUllIfNotValid)
        {

            return ChangeType(value, type, provider, "", returnDbNUllIfNotValid);
        }

        public static object FixDbNUllasNull(object value, Type type)
        {
            if (type == null)
                return value;

            /*
             * Do not return DBNull for strong typed properties of an object. For example, if Parsing a string failed 
             * (e.g. if an empty string was passed in as argument) we need to check if it as object and in that
             * case return null. Only if it is a ValueType type (that is not nullable) then we should return DBNull
             * so that it also works with DataRowView.
             * */
            if (!type.IsValueType)
            {
                if (value is DBNull)
                    return null;
            }
            return value;
        }

        /// <summary>
        /// Converts value from one type to another using an optional <see cref="IFormatProvider"/>.
        /// </summary>
        /// <param name="value">The original value.</param>
        /// <param name="type">The target type.</param>
        /// <param name="provider">A <see cref="IFormatProvider"/> used to format or parse the value.</param>
        /// <param name="format">Format string.</param>
        /// <param name="returnDbNUllIfNotValid">Indicates whether exceptions should be avoided or catched and return value should be DBNull if
        /// it cannot be converted to the target type.</param>
        /// <returns>The new value in the target type.</returns>
        public static object ChangeType(object value, Type type, IFormatProvider provider, string format, bool returnDbNUllIfNotValid)
        {
            if (value != null && !type.IsAssignableFrom(value.GetType()))
            {
                try
                {
                    if (value is string)
                    {
                        if (format != null && format.Length > 0)
                            value = Parse((string)value, type, provider, format, returnDbNUllIfNotValid);
                        else
                            value = Parse((string)value, type, provider, "", returnDbNUllIfNotValid);

                    }
                    else if (value is System.DBNull)
                    {
                    }
                    else if (type.IsEnum)
                    {
                        value = Convert.ChangeType(value, typeof(int), provider);
                        value = Enum.ToObject(type, (int)value);
                    }
                    else if (type == typeof(string) && !(value is IConvertible))
                    {
                        value = value != null ? value.ToString() : "";
                    }
                    else
                        value = ChangeType(value, type, provider);
                }
                catch
                {
                    if (returnDbNUllIfNotValid)
                        return Convert.DBNull;

                    throw;
                }
            }

            if ((value == null || value is DBNull) && type == typeof(string))
                return "";

            return value;
        }

        public static object Parse(string s, Type resultType, IFormatProvider provider, string format)
        {
            return Parse(s, resultType, provider, format, false);
        }

        /// <summary>
        /// Parse the given text using the resultTypes "Parse" method or using a type converter.
        /// </summary>
        /// <param name="s">The text to parse.</param>
        /// <param name="resultType">The requested result type.</param>
        /// <param name="provider">A <see cref="IFormatProvider"/> used to format or parse the value. Can be NULL.</param>
        /// <param name="format">A format string used in a <see cref="System.Object.ToString"/> call. Right now
        /// format is only interpreted to enable roundtripping for formatted dates.
        /// </param>
        /// <param name="returnDbNUllIfNotValid">Indicates whether DbNull should be returned if value cannot be parsed. Otherwise an exception is thrown.</param>
        /// <returns>The new value in the target type.</returns>
        public static object Parse(string s, Type resultType, IFormatProvider provider, string format, bool returnDbNUllIfNotValid)
        {
            object value = _Parse(s, resultType, provider, format, returnDbNUllIfNotValid);
            return FixDbNUllasNull(value, resultType);
        }

        static object _Parse(string s, Type resultType, IFormatProvider provider, string format, bool returnDbNUllIfNotValid)
        {
            return _Parse(s, resultType, provider, format, null, returnDbNUllIfNotValid);
        }
        static object _Parse(string s, Type resultType, IFormatProvider provider, string format, string[] formats, bool returnDbNUllIfNotValid)
        {
            if (resultType == null || resultType == typeof(string))
                return s;

            object result;

            try
            {
                if (typeof(double).IsAssignableFrom(resultType))
                {
                    if (String.IsNullOrEmpty(s))
                        return Convert.DBNull;

                    double d;
                    if (double.TryParse(s, NumberStyles.Any, provider, out d))
                    {
                        result = Convert.ChangeType(d, resultType, provider);
                        return result;
                    }

                    if (returnDbNUllIfNotValid)
                    {
                        if (resultType == typeof(double) || resultType == typeof(float))
                            return Convert.DBNull;
                    }
                }
                else if (typeof(decimal).IsAssignableFrom(resultType))
                {
                    if (String.IsNullOrEmpty(s))
                        return Convert.DBNull;

                    decimal d;
                    d = decimal.Parse(s, NumberStyles.Any, provider);
                    result = Convert.ChangeType(d, resultType, provider);
                }
                else if (typeof(DateTime).IsAssignableFrom(resultType))
                {
                    if (String.IsNullOrEmpty(s))
                        return Convert.DBNull;

                    if (formats == null || formats.GetLength(0) == 0 && format.Length > 0)
                        formats = new string[] { format, "G", "g", "f", "F", "d", "D" };

                    if (formats != null && formats.GetLength(0) > 0)
                    {
                        try
                        {
                            return DateTime.Parse(s, provider, DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowWhiteSpaces);
                            //return DateTime.ParseExact(s, formats, provider, DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowWhiteSpaces);
                        }
                        catch
                        {
                        }
                    }

                    return DateTime.Parse(s, provider, DateTimeStyles.AllowInnerWhite | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite | DateTimeStyles.AllowWhiteSpaces);
                }
                else if (typeof(bool).IsAssignableFrom(resultType))
                {
                    if (String.IsNullOrEmpty(s))
                        return Convert.DBNull;

                    if (s == "1" || s.ToUpper() == bool.TrueString.ToUpper())
                        return true;
                    else if (s == "0" || s.ToUpper() == bool.TrueString.ToUpper())
                        return false;
                }
                else if (typeof(long).IsAssignableFrom(resultType))
                {
                    if (String.IsNullOrEmpty(s))
                        return Convert.DBNull;

                    long d; 
                    
                    try
                    {
                        d = long.Parse(s, NumberStyles.Any, provider);
                        result = Convert.ChangeType(d, resultType, provider);
                        return result;
                    }
                    catch
                    {
                    }

                    if (returnDbNUllIfNotValid)
                    {
                        if (resultType.IsPrimitive && !resultType.IsEnum)
                            return Convert.DBNull;
                    }
                }
                else if (typeof(ulong).IsAssignableFrom(resultType))
                {
                    if (String.IsNullOrEmpty(s))
                        return Convert.DBNull;

                    ulong d;

                    try
                    {
                        d = ulong.Parse(s, NumberStyles.Any, provider);
                        result = Convert.ChangeType(d, resultType, provider);
                        return result;
                    }
                    catch
                    {
                    }

                    if (returnDbNUllIfNotValid)
                    {
                        if (resultType.IsPrimitive && !resultType.IsEnum)
                            return Convert.DBNull;
                    }
                }
                else if (typeof(int).IsAssignableFrom(resultType)
                    || typeof(short).IsAssignableFrom(resultType)
                    || typeof(float).IsAssignableFrom(resultType)
                    || typeof(uint).IsAssignableFrom(resultType)
                    || typeof(ushort).IsAssignableFrom(resultType)
                    || typeof(byte).IsAssignableFrom(resultType))
                {
                    if (String.IsNullOrEmpty(s))
                        return Convert.DBNull;

                    double d;
                    if (double.TryParse(s, NumberStyles.Any, provider, out d))
                    {
                        result = Convert.ChangeType(d, resultType, provider);
                        return result;
                    }

                    if (returnDbNUllIfNotValid)
                    {
                        if (resultType.IsPrimitive && !resultType.IsEnum)
                            return Convert.DBNull;
                    }
                }
                else if (resultType == typeof(Type))
                {
                    result = Type.GetType(s);
                    return result;
                }


                TypeConverter typeConverter = TypeDescriptor.GetConverter(resultType);
                if (typeConverter != null &&
                    typeConverter.CanConvertFrom(typeof(System.String)) &&
                    s != null && s.Length > 0
                    )
                {
                    if (provider is CultureInfo)
                        result = typeConverter.ConvertFrom(null, (CultureInfo)provider, s);
                    else
                        result = typeConverter.ConvertFrom(s);
                    return result;
                }
            }
            catch
            {
                if (returnDbNUllIfNotValid)
                    return Convert.DBNull;

                throw;
            }

            // throw new InvalidCastException(SR.GetString("InvalidCast_IConvertible"));
            return Convert.DBNull;
        }


        static Hashtable cachedDefaultValues = new Hashtable();

        public static object ChangeType(object value, Type type, IFormatProvider provider)
        {
            return TypeConverterHelper.ChangeType(value, type, provider);
        }

        public static bool CompareString(string value1, string value2, Type type)
        {
            bool isDifferent = false;

            if (string.IsNullOrEmpty(value2))
                return true;

            if (string.IsNullOrEmpty(value1))
                return true;
            
            try
            {                
                if (typeof(string).IsAssignableFrom(type))
                {
                    if (!string.Equals(value1, value2))
                        isDifferent = true;
                }
                else if (typeof(double).IsAssignableFrom(type))
                {
                    double d1;
                    double d2;

                    if (double.TryParse(value1, out d1) && double.TryParse(value2, out d2))
                    {
                        if (d1 != d2)
                            isDifferent = true;
                    }
                }
                else if (typeof(int).IsAssignableFrom(type))
                {
                    int d1;
                    int d2;

                    if (int.TryParse(value1, out d1) && int.TryParse(value2, out d2))
                    {
                        if (d1 != d2)
                            isDifferent = true;
                    }
                }
            }
            catch
            { 
            
            }

            return isDifferent;
        }
    }
}
