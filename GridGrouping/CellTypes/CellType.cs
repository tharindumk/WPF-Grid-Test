using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes
{
    public struct CellType
    {
        public static string Header = "Header";
        public static string Static = "Static";
        public static string Summary = "Summary";
        public static string TextBox = "TextBox";
        public static string ComboBox = "ComboBox";
        public static string PushButton = "PushButton";
        public static string ProgressView = "ProgressView";
        public static string CheckBox = "CheckBox";
    }

    public enum BrushTypes
    {
        HeaderCellBackground,
        StaticCellBackground,
        HeaderText,
        StaticText
    }

    public enum CellSelectionType
    {
        Cell,
        Row
    }

    public enum RowCelectionType
    {
        None,
        Single,
        Multiple
    }
}
