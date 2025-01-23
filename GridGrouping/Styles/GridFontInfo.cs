using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles
{
    [Serializable]
    public class GridFontInfo
    {
        private static GridFontInfo defaultFont;
        private FontStyle fontStyle;
        private string faceName = string.Empty;
        public string Facename
        {
            get 
            {
                return faceName;
            }
            set
            {
                if (faceName != value)
                {
                    faceName = value;
                    font = GetGdipFont();
                }
            }
        }

        private float size = 8.25f;

        public float Size 
        {
            get
            {
                return size;
            }
            set
            {
                float current = size;
                size = value;

                if (GdipFont != null)
                {
                    if (size != current)
                    {
                        font = GetGdipFont();

                        Facename = font.FontFamily.Name;
                        bold = font.Bold;
                        FontStyle = font.Style;
                    }
                }
            }
        }

        public FontStyle FontStyle 
        {
            get 
            {
                if (Bold)
                    fontStyle |= System.Drawing.FontStyle.Bold;
                else if (Italic)
                    fontStyle |= System.Drawing.FontStyle.Italic;
                else if (Underline)
                    fontStyle |= System.Drawing.FontStyle.Underline;

                return fontStyle; 
            }
            set
            {
                fontStyle = value;
            }
        }

        private bool bold = false;
        public bool Bold
        {
            get
            {
                return bold;
            }
            set
            {
                bold = value;

                if (GdipFont != null)
                {
                    if (bold)
                        FontStyle = System.Drawing.FontStyle.Bold;
                    else
                        FontStyle = System.Drawing.FontStyle.Regular;

                    if (GdipFont.Bold != value)
                    {
                        font = GetGdipFont();

                        Facename = font.FontFamily.Name;
                        size = font.Size;
                        FontStyle = font.Style;
                    }

                    //FontStyle = System.Drawing.FontStyle.Regular;
                }
            }
        }

        public bool Strikeout { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }

        #region Constructors

        public GridFontInfo()
        { }

        public GridFontInfo(Font font):base()
        {
            Facename = font.FontFamily.Name;
            Size = font.Size;
            FontStyle = font.Style;
            this.font = font;
        }

        #endregion

        #region Default

        public static GridFontInfo Default
        {
            get
            {
                if (defaultFont == null)
                {
                    defaultFont = new GridFontInfo();
                    Font font = System.Windows.Forms.Control.DefaultFont;
                    defaultFont.Facename = font.FontFamily.Name;
                    defaultFont.Size = font.Size;
                    defaultFont.FontStyle = font.Style;
                }

                return defaultFont;
            }
            set
            {
                defaultFont = value;
            }
        }

        public static void ResetDefualt()
        {
            defaultFont = null;
        }

        private Font font = null;

        public Font GdipFont
        {
            get
            {
                if(font == null || defaultFont == null)
                    font = GetGdipFont();
                
                return font;
            }
        }

        public void ResetFont()
        {
            //if (font != null)
            //{
            // //   font.Dispose();
            //}
            //font = GetGdipFont();
        }

        #endregion

        Font GetGdipFont()
        {
            Font newFont = FontUtil.CreateFont(Facename, (float)Size, FontStyle);

            return newFont;
        }
    }
}
