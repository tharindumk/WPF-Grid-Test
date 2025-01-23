using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Globalization;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;
using Mubasher.ClientTradingPlatform.Infrastructure.Module.Logger;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles
{
    [Serializable]
    public class GridStyleInfo : ICloneable
    {
        private static GridStyleInfo defaultStyle = null;

        private Color backColorUp = Color.Empty;
        private Color backColorDown = Color.Empty;
        private Color backColor = Color.Empty;

        public Color BackColorUp
        {
            get
            {
                if (backColorUp == Color.Empty)
                    return backColor;

                return backColorUp;
            }
            set
            {
                backColorUp = value;
            }
        }

        public Color BackColorDown
        {
            get
            {
                if (backColorDown == Color.Empty)
                    return backColor;

                return backColorDown;
            }
            set
            {
                backColorDown = value;
            }
        }

        public Color BackColor
        {
            get
            {
                return backColor;
            }
            set
            {
                backColor = value;
                backColorUp = value;
                backColorDown = value;
            }
        }

        private Color backColorAlt;

        public Color BackColorAlt
        {
            get { return backColorAlt; }
            set
            {
                backColorAlt = value;
                BackColorAltUp = value;
                BackColorAltDown = value;
            }
        }

        public Color BackColorAltUp;
        public Color BackColorAltDown;
        public Color BackColorBlinkUp;
        public Color BackColorBlinkDown;
        public Color BackColorBlinkUpdate;
        public Color BackColorSelectedUp;
        public Color BackColorSelectedDown;
        public Color TextColor;
        public Color TextColorAlt;
        public Color TextColorSelected;
        public Color TextColorBlinkUp;
        public Color TextColorBlinkDown;
        public Color TextColorBlinkUpdate;
        public BorderStyle BorderBottom = null;
        public BorderStyle BorderUp = null;
        public BorderStyle BorderVertical = null;
        private string cellType = Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes.CellType.Static;
        private GridHorizontalAlignment horizontalAlignment;
        private GridFontInfo font;
        private GridFontInfo headerFont;
        public CharacterCasing CharacterCasing;
        public GridVerticalAlignment VerticalAlignment;
        public bool WrapText;
        public StringTrimming Trimming;
        public bool AutoSize;
        public bool Enabled;
        public RightToLeft RightToLeft;
        private string format = string.Empty;
        public GridTextAlign TextAlign = GridTextAlign.Left;
        public string Format
        {
            get
            {
                return format;
            }
            set
            {
                format = value;
            }
        }
        public Type CellValueType;
        public CultureInfo CultureInfo;
        public GridBackgroundImageAlign BackgroundImageMode = GridBackgroundImageAlign.Normal;
        private Image backImage;
        public Color HeaderBackColor1;
        public Color HeaderBackColor2;
        public Color HeaderTextColor;
        public BorderStyleHeader HeaderBorders = null;
        public GridHorizontalAlignment HeaderHorizontalAlignment;
        public GridVerticalAlignment HeaderVerticalAlignment;
        public StringTrimming HeaderTextTrimming;
        private object cellValue;
        public bool IsGradientBackColor = false;
        public bool BlinkBackground = true;
        public bool IsSelectedBackColorGradient = true;

        public object CellValue
        {
            get
            {
                return cellValue;
            }
            set { cellValue = value; }
        }

        public GridHorizontalAlignment HorizontalAlignment
        {
            get { return horizontalAlignment; }
            set
            {
                horizontalAlignment = value;
            }
        }

        public string CellType
        {
            get { return cellType; }
            set
            {
                cellType = value;
            }
        }

        private bool isCustomized = false;

        public bool IsCustomized
        {
            get { return isCustomized; }
            set { isCustomized = value; }
        }

        private string autoNumberFormat = string.Empty;

        public string AutoNumberFormat
        {
            get { return autoNumberFormat; }
            set { autoNumberFormat = value; }
        }


        public Font GdipFont
        {
            get
            {
                try
                {
                    //
                    //TODO : Get DPI Adjusted Font
                    //
                    Font fnt;

                    if (font == null)
                        fnt = Default.GdipFont;
                    else
                        fnt = font.GdipFont;

                    return fnt;

                }
                catch (Exception ex)
                {
                    RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
                    return Default.GdipFont;
                }
            }
        }

        public GridFontInfo Font
        {
            get { return font; }
            set
            {
                font = value;
                //font.ResetFont();
                //font = new GridFontInfo(value.GdipFont);
            }
        }

        public GridFontInfo HeaderFont
        {
            get { return headerFont; }
            set { headerFont = value; }
        }

        public Font GdipHeaderFont
        {
            get
            {
                try
                {
                    Font fnt;

                    if (headerFont == null)
                        fnt = Default.GdipFont;
                    else
                        fnt = headerFont.GdipFont;

                    return fnt;

                }
                catch
                {
                    return Default.GdipFont;
                }
            }
        }

        public Image BackgroundImage
        {
            get { return backImage; }
            set { backImage = value; }
        }

        public GridStyleInfo()
        {
            BackColor = Color.Black;
            BackColorUp = Color.Black;
            BackColorBlinkUpdate = Color.Yellow;
            BackColorDown = Color.Black;
            TextColor = Color.White;
            TextColorAlt = Color.White;
            BackColorSelectedUp = Color.Gold;
            BackColorSelectedDown = Color.Orange;
            TextColorSelected = Color.Black;
            horizontalAlignment = GridHorizontalAlignment.Right;
        }
        public GridStyleInfo(Color xBackColor1, Color xBackColor2, Color xForeColor)
        {
            BackColor = xBackColor1;
            BackColorUp = xBackColor1;
            BackColorDown = xBackColor2;
            BackColorBlinkUpdate = xBackColor1;
            TextColor = xForeColor;
            TextColorAlt = xForeColor;
            horizontalAlignment = GridHorizontalAlignment.Right;
        }

        public GridStyleInfo(Color xBackColor1, Color xBackColor2, Color xForeColor, BorderStyle border)
        {
            BackColor = xBackColor1;
            BackColorUp = xBackColor1;
            BackColorDown = xBackColor2;
            TextColor = xForeColor;
            TextColorAlt = xForeColor;
            this.BorderBottom = border;
            this.BorderUp = border;
            this.BorderVertical = border;
            horizontalAlignment = GridHorizontalAlignment.Right;
        }

        /// <summary>
        /// Returns a <see cref="GridStyleInfo"/> with default settings.
        /// </summary>
        public static GridStyleInfo Default
        {
            get
            {
                if (GridStyleInfo.defaultStyle == null)
                {
                    defaultStyle = new GridStyleInfo();
                    defaultStyle.BackColor = GridColorTable.Instance.BackColor;
                    defaultStyle.BackColorUp = GridColorTable.Instance.BackColorUp;
                    defaultStyle.BackColorDown = GridColorTable.Instance.BackColorDown;
                    defaultStyle.BackColorAlt = GridColorTable.Instance.BackColorAlt;
                    defaultStyle.BackColorAltUp = GridColorTable.Instance.BackColorAltUp;
                    defaultStyle.BackColorAltDown = GridColorTable.Instance.BackColorAltDown;
                    defaultStyle.BackColorSelectedUp = GridColorTable.Instance.BackColorSelectedUp;
                    defaultStyle.BackColorSelectedDown = GridColorTable.Instance.BackColorSelectedDown;
                    defaultStyle.BackColorBlinkUp = GridColorTable.Instance.BackColorBlinkUp;
                    defaultStyle.BackColorBlinkDown = GridColorTable.Instance.BackColorBlinkDown;
                    defaultStyle.BackColorBlinkUpdate = GridColorTable.Instance.BackColorBlinkUpdate;
                    defaultStyle.TextColorBlinkUp = GridColorTable.Instance.TextColorBlinkUp;
                    defaultStyle.TextColorBlinkDown = GridColorTable.Instance.TextColorBlinkDown;
                    defaultStyle.TextColorBlinkUpdate = GridColorTable.Instance.TextColorBlinkUpdate;
                    defaultStyle.TextColorSelected = GridColorTable.Instance.TextColorSelected;
                    defaultStyle.TextColor = GridColorTable.Instance.TextColor;
                    defaultStyle.TextColorAlt = GridColorTable.Instance.TextColorAlt;

                    defaultStyle.Font = new GridFontInfo(GridFontInfo.Default.GdipFont);
                    defaultStyle.HeaderFont = new GridFontInfo(GridFontInfo.Default.GdipFont);
                    defaultStyle.HeaderFont.Bold = true;

                    defaultStyle.BorderBottom = (BorderStyle)BorderStyle.Default.Clone();
                    defaultStyle.BorderBottom.BorderColor = GridColorTable.Instance.RowBottomBorderColor;

                    defaultStyle.BorderUp = (BorderStyle)BorderStyle.Default.Clone();
                    defaultStyle.BorderUp.BorderColor = GridColorTable.Instance.RowTopBorderColor;

                    defaultStyle.BorderVertical = (BorderStyle)BorderStyle.Default.Clone();
                    defaultStyle.BorderVertical.BorderColor = GridColorTable.Instance.RowVerticalBorderColor;
                    defaultStyle.BorderVertical.Style = GridOptimizedBorderStyle.None;

                    defaultStyle.HeaderBorders = (BorderStyleHeader)BorderStyleHeader.Default.Clone();
                    defaultStyle.HeaderBorders.HasBottomBorder = true;
                    defaultStyle.HeaderBorders.HasLeftBorder = false;
                    defaultStyle.HeaderBorders.HasTopBorder = true;
                    defaultStyle.HeaderBorders.HasRightBorder = true;
                    defaultStyle.HeaderBorders.BorderBottomColor = GridColorTable.Instance.RowBottomBorderColor;
                    defaultStyle.HeaderBorders.BorderUpColor = GridColorTable.Instance.RowTopBorderColor;
                    defaultStyle.HeaderBorders.BorderVerticalColor = GridColorTable.Instance.RowVerticalBorderColor;

                    defaultStyle.HorizontalAlignment = GridHorizontalAlignment.Left;
                    defaultStyle.CharacterCasing = CharacterCasing.Normal;
                    defaultStyle.VerticalAlignment = GridVerticalAlignment.Middle;
                    defaultStyle.CellType = Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes.CellType.Static;

                    defaultStyle.WrapText = true;
                    defaultStyle.Trimming = StringTrimming.EllipsisCharacter;
                    defaultStyle.AutoSize = false;
                    defaultStyle.Enabled = true;

                    defaultStyle.RightToLeft = RightToLeft.Inherit;
                    defaultStyle.Format = "";
                    defaultStyle.CellValueType = null;
                    defaultStyle.CultureInfo = null;

                    defaultStyle.HeaderBackColor1 = GridColorTable.Instance.HeaderBackColor1;
                    defaultStyle.HeaderBackColor2 = GridColorTable.Instance.HeaderBackColor2;
                    defaultStyle.HeaderTextColor = GridColorTable.Instance.HeaderTextColor;
                    defaultStyle.HeaderHorizontalAlignment = GridHorizontalAlignment.Center;
                    defaultStyle.HeaderVerticalAlignment = GridVerticalAlignment.Middle;
                    defaultStyle.HeaderTextTrimming = StringTrimming.EllipsisCharacter;
                }

                return (GridStyleInfo)GridStyleInfo.defaultStyle;
            }
        }

        public static GridStyleInfo GetDefaultStyle()
        {
            return GetDefaultStyle(false);
        }

        public static GridStyleInfo GetDefaultStyle(bool recreate)
        {
            if (recreate)
                defaultStyle = null;

            return (GridStyleInfo)Default.Clone();
        }

        public object Clone()
        {
            //
            //Using this method is costly. It uses Reflection. Instead used Manual Clone.
            //
            //GridStyleInfo gs = ObjectCloneHelper.Clone<GridStyleInfo>(this);

            //return gs;

            GridStyleInfo gs = new GridStyleInfo
            {
                BackColor = this.BackColor,
                BackColorUp = this.BackColorUp,
                BackColorDown = this.BackColorDown,
                BackColorAlt = this.BackColorAlt,
                BackColorAltUp = this.BackColorAltUp,
                BackColorAltDown = this.BackColorAltDown,
                BackColorBlinkUp = this.BackColorBlinkUp,
                BackColorBlinkDown = this.BackColorBlinkDown,
                BackColorBlinkUpdate = this.BackColorBlinkUpdate,
                BackColorSelectedUp = this.BackColorSelectedUp,
                BackColorSelectedDown = this.BackColorSelectedDown,
                TextColor = this.TextColor,
                TextColorAlt = this.TextColorAlt,
                TextColorSelected = this.TextColorSelected,
                TextColorBlinkUp = this.TextColorBlinkUp,
                TextColorBlinkDown = this.TextColorBlinkDown,
                TextColorBlinkUpdate = this.TextColorBlinkUpdate,
                BorderBottom = (BorderStyle)this.BorderBottom.Clone(),
                BorderUp = (BorderStyle)this.BorderUp.Clone(),
                BorderVertical = (BorderStyle)this.BorderVertical.Clone(),
                cellType = this.cellType,
                HeaderBorders = this.HeaderBorders,
                Font = this.Font,
                HeaderFont = this.HeaderFont,
                HorizontalAlignment = this.HorizontalAlignment,
                CharacterCasing = this.CharacterCasing,
                VerticalAlignment = this.VerticalAlignment,
                CellType = this.CellType,
                WrapText = this.WrapText,
                Trimming = this.Trimming,
                AutoSize = this.AutoSize,
                Enabled = this.Enabled,
                RightToLeft = this.RightToLeft,
                Format = this.Format,
                CellValueType = this.CellValueType,
                CultureInfo = this.CultureInfo,
                HeaderBackColor1 = this.HeaderBackColor1,
                HeaderBackColor2 = this.HeaderBackColor2,
                HeaderTextColor = this.HeaderTextColor,
                HeaderHorizontalAlignment = this.HeaderHorizontalAlignment,
                HeaderVerticalAlignment = this.HeaderVerticalAlignment,
                HeaderTextTrimming = this.HeaderTextTrimming,
                BackgroundImageMode = this.BackgroundImageMode,
                BackgroundImage = this.BackgroundImage,
                cellValue = this.cellValue,
                IsGradientBackColor = this.IsGradientBackColor,
                BlinkBackground = this.BlinkBackground,
                ImageList = this.ImageList,
                ImageIndex = this.ImageIndex,
                IsCustomized = this.IsCustomized,
                TextAlign = this.TextAlign,
                AutoNumberFormat = this.AutoNumberFormat
            };

            return gs;
        }

        /// <overload>
        /// Return formatted text for the specified value.
        /// GridStyleInfo.CultureInfo is used for conversion to string.
        /// </overload>
        /// <summary>
        /// Return formatted text for the specified value.
        /// </summary>
        /// <param name="value">The value to be formatted.</param>
        /// <returns>A string that holds the formatted text.</returns>
        public string GetFormattedText(object value)
        {
            try
            {
                CultureInfo ci = CultureInfo;
                NumberFormatInfo nfi = ci != null ? ci.NumberFormat : null;
                return GridCellValueConvert.FormatValue(value, CellValueType, Format, ci, nfi);
            }
            catch (Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
                return value != null ? value.ToString() : "";
            }
        }

        public string GetFormattedText()
        {
            try
            {
                CultureInfo ci = CultureInfo;
                NumberFormatInfo nfi = ci != null ? ci.NumberFormat : null;
                return GridCellValueConvert.FormatValue(this.cellValue, CellValueType, Format, ci, nfi);
            }
            catch (Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
                return this.cellValue != null ? this.cellValue.ToString() : "";
            }
        }

        public bool ApplyFormattedText(string text)
        {
            CultureInfo ci = CultureInfo;
            NumberFormatInfo nfi = ci != null ? ci.NumberFormat : null;
            try
            {
                CellValue = GridCellValueConvert.Parse(text, CellValueType, nfi, Format);
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex.InnerException is FormatException)
                {
                    CellValue = text;
                    // possibly could also change CellValueType here based on input string
                    // e.Style.CellValueType = typeof(string);
                }
                else
                    throw;
            }
            return true;
        }

        public void ResetFont()
        {
            this.Font = defaultStyle.Font;
        }

        public ImageList ImageList
        {
            get;
            set;
        }

        private int imageIndex = -1;

        public int ImageIndex
        {
            get
            {
                return imageIndex;
            }
            set
            {
                imageIndex = value;
            }
        }
    }

    public class GridColorTable
    {
        public Color BackColor;
        public Color BackColorUp;
        public Color BackColorDown;
        public Color GridColorUp;
        public Color GridColorDown;
        public Color GridColorAltUp;
        public Color GridColorAltDown;
        public Color RowBottomBorderColor;
        public Color RowTopBorderColor;
        public Color RowVerticalBorderColor;
        public Color BackColorBlinkUp;
        public Color BackColorBlinkDown;
        public Color BackColorBlinkUpdate;
        public Color BackColorAlt;
        public Color BackColorAltUp;
        public Color BackColorAltDown;
        public Color BackColorSelectedUp;
        public Color BackColorSelectedDown;
        public Color TextColor;
        public Color TextColorAlt;
        public Color TextColorSelected;
        public Color TextColorBlinkUp;
        public Color TextColorBlinkDown;
        public Color TextColorBlinkUpdate;
        public Color ButtonSelectedInternalBorderColor;
        public Color ButtonDefaultInternalBorderColor;
        public Color ButtonUpBorderColor;
        public Color ButtonDownBorderColor;
        public Color ButtonValueUpdateBorderColor;
        public Color ButtonUpBottomColor;
        public Color ButtonDownBottomColor;
        public Color ButtonValueUpdateBottomColor;
        public Color ProgressViewDarkColor;
        public Color ProgressLightColor;

        //Header
        public Color HeaderBackColor1;
        public Color HeaderBackColor2;
        public Color HeaderTextColor;

        private static GridColorTable instance;

        public static GridColorTable Instance
        {
            get
            {
                if (instance == null)
                    instance = new GridColorTable();

                return instance;
            }
        }
    }

    public class HslColorGrid
    {
        // Private data members below are on scale 0-1
        // They are scaled for use externally based on scale
        private double hue = 1.0;
        private double saturation = 1.0;
        private double luminosity = 1.0;

        private const double scale = 240.0;

        public double Hue
        {
            get { return hue * scale; }
            set { hue = CheckRange(value / scale); }
        }
        public double Saturation
        {
            get { return saturation * scale; }
            set { saturation = CheckRange(value / scale); }
        }
        public double Luminosity
        {
            get { return luminosity * scale; }
            set { luminosity = CheckRange(value / scale); }
        }

        private double CheckRange(double value)
        {
            if (value < 0.0)
                value = 0.0;
            else if (value > 1.0)
                value = 1.0;
            return value;
        }

        public override string ToString()
        {
            return String.Format("H: {0:#0.##} S: {1:#0.##} L: {2:#0.##}", Hue, Saturation, Luminosity);
        }

        public string ToRGBString()
        {
            Color color = (Color)this;
            return String.Format("R: {0:#0.##} G: {1:#0.##} B: {2:#0.##}", color.R, color.G, color.B);
        }

        #region Casts to/from System.Drawing.Color
        public static implicit operator Color(HslColorGrid hslColor)
        {
            double r = 0, g = 0, b = 0;
            if (hslColor.luminosity != 0)
            {
                if (hslColor.saturation == 0)
                    r = g = b = hslColor.luminosity;
                else
                {
                    double temp2 = GetTemp2(hslColor);
                    double temp1 = 2.0 * hslColor.luminosity - temp2;

                    r = GetColorComponent(temp1, temp2, hslColor.hue + 1.0 / 3.0);
                    g = GetColorComponent(temp1, temp2, hslColor.hue);
                    b = GetColorComponent(temp1, temp2, hslColor.hue - 1.0 / 3.0);
                }
            }
            return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
        }

        private static double GetColorComponent(double temp1, double temp2, double temp3)
        {
            temp3 = MoveIntoRange(temp3);
            if (temp3 < 1.0 / 6.0)
                return temp1 + (temp2 - temp1) * 6.0 * temp3;
            else if (temp3 < 0.5)
                return temp2;
            else if (temp3 < 2.0 / 3.0)
                return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
            else
                return temp1;
        }
        private static double MoveIntoRange(double temp3)
        {
            if (temp3 < 0.0)
                temp3 += 1.0;
            else if (temp3 > 1.0)
                temp3 -= 1.0;
            return temp3;
        }
        private static double GetTemp2(HslColorGrid hslColor)
        {
            double temp2;
            if (hslColor.luminosity < 0.5)  //<=??
                temp2 = hslColor.luminosity * (1.0 + hslColor.saturation);
            else
                temp2 = hslColor.luminosity + hslColor.saturation - (hslColor.luminosity * hslColor.saturation);
            return temp2;
        }

        public static implicit operator HslColorGrid(Color color)
        {
            HslColorGrid hslColor = new HslColorGrid();
            hslColor.hue = color.GetHue() / 360.0; // we store hue as 0-1 as opposed to 0-360 
            hslColor.luminosity = color.GetBrightness();
            hslColor.saturation = color.GetSaturation();
            return hslColor;
        }
        #endregion

        public void SetRGB(int red, int green, int blue)
        {
            HslColorGrid hslColor = (HslColorGrid)Color.FromArgb(red, green, blue);
            this.hue = hslColor.hue;
            this.saturation = hslColor.saturation;
            this.luminosity = hslColor.luminosity;
        }

        public HslColorGrid() { }
        public HslColorGrid(Color color)
        {
            SetRGB(color.R, color.G, color.B);
        }
        public HslColorGrid(int red, int green, int blue)
        {
            SetRGB(red, green, blue);
        }
        public HslColorGrid(double hue, double saturation, double luminosity)
        {
            this.Hue = hue;
            this.Saturation = saturation;
            this.Luminosity = luminosity;
        }


    }
}
