using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Accessors
{
    public class UnboundDataAccessor
    {
        #region Main Getter / Setter

        /// <summary>
        /// Gets the property value from the specified cell.
        /// </summary>
        /// <param name="target">Target cell.</param>
        /// <returns>Property value.</returns>
        public static object Get(TableInfo.CellStruct cell)
        {
            return GetValue(cell);
        }

        private static object GetValue(TableInfo.CellStruct cell)
        {
            object returnbject = null;
            TableInfo.CellStructType type = cell.CellStructType;

            switch (type)
            {
                case TableInfo.CellStructType.String:
                    returnbject = cell.TextString;
                    break;
                case TableInfo.CellStructType.Double:
                    returnbject = cell.TextDouble;
                    break;
                case TableInfo.CellStructType.Integer:
                    returnbject = cell.TextInt;
                    break;
                case TableInfo.CellStructType.DateTime:
                    returnbject = cell.TextDateTime;
                    break;
                case TableInfo.CellStructType.Long:
                    returnbject = cell.TextLong;
                    break;
                case TableInfo.CellStructType.Decimal:
                    returnbject = cell.TextDecimal;
                    break;
                case TableInfo.CellStructType.Style:
                    returnbject = cell.Style;
                    break;
            }

            return returnbject;
        }

        #endregion
    }
}
