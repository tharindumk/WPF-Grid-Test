using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles
{
    [Serializable]
    public class BorderStyle : ICloneable
    {
        #region Fields

        internal int borderSize = 0;

        public int BorderSize
        {
            get { return borderSize; }
        }

        public Color BorderColor = Color.Black;

        private static BorderStyle defaultBorders;
        private ButtonBorderStyle buttonBorderStyle = ButtonBorderStyle.None;
        private GridOptimizedBorderStyle style = GridOptimizedBorderStyle.None;

        public GridOptimizedBorderStyle Style
        {
            get { return style; }
            set 
            { 
                style = value;

                if (style == GridOptimizedBorderStyle.None)
                    borderSize = 0;
                else
                {
                    switch (borderWeight)
                    {
                        case GridOptimizedBorderWeight.Thin:
                            this.borderSize = 1;
                            break;
                        case GridOptimizedBorderWeight.Medium:
                            this.borderSize = 2;
                            break;
                        case GridOptimizedBorderWeight.Thick:
                            this.borderSize = 3;
                            break;
                    }
                }
            }
        }

        private GridOptimizedBorderWeight borderWeight = GridOptimizedBorderWeight.Thin;

        public GridOptimizedBorderWeight BorderWeight
        {
            get { return borderWeight; }
            set 
            {
                borderWeight = value;

                switch (borderWeight)
                {
                    case GridOptimizedBorderWeight.Thin:
                        this.borderSize = 1;
                        break;
                    case GridOptimizedBorderWeight.Medium:
                        this.borderSize = 2;
                        break;
                    case GridOptimizedBorderWeight.Thick:
                        this.borderSize = 3;
                        break;
                }
            }
        }

        public ButtonBorderStyle ButtonBorderStyle
        {
            get { return buttonBorderStyle; }
            set { buttonBorderStyle = value; }
        }

        #endregion

        #region Default

        public static BorderStyle Default
        {
            get
            {
                if (defaultBorders == null)
                {
                    defaultBorders = new BorderStyle();
                    defaultBorders.BorderColor = Color.FromArgb(255, 30, 30, 30);
                    defaultBorders.borderSize = 1;
                    defaultBorders.ButtonBorderStyle = ButtonBorderStyle.Solid;
                    defaultBorders.Style = GridOptimizedBorderStyle.None;
                    defaultBorders.BorderWeight = GridOptimizedBorderWeight.Thin;
                }

                return defaultBorders.Clone() as BorderStyle;
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            //
            //Using this method is costly. It uses Reflection. Instead used Manual Clone.
            //
            //BorderStyle db = ObjectCloneHelper.Clone<BorderStyle>(this);

            //return db;

            BorderStyle db = new BorderStyle
            {
                borderSize = this.BorderSize,
                BorderColor = this.BorderColor,
                buttonBorderStyle = this.buttonBorderStyle,
                style = this.Style,
                borderWeight = this.BorderWeight
            };

            return db;
        }

        #endregion

        #region Helper Methods

        internal int GetBorderSizeFromWeight()
        {
            switch (borderWeight)
            {
                case GridOptimizedBorderWeight.Thin:
                    return 1;
                case GridOptimizedBorderWeight.Medium:
                    return 2;
                case GridOptimizedBorderWeight.Thick:
                    return 3;
            }

            return 0;
        }

        #endregion
    }

    [Serializable]
    public class BorderStyleHeader : ICloneable
    {
        #region Fields

        public int BorderSize = 0;
        public Color BorderUpColor = Color.Black;
        public Color BorderBottomColor = Color.Black;
        public Color BorderVerticalColor = Color.Black;

        public bool HasTopBorder = false;
        public bool HasLeftBorder = false;
        public bool HasBottomBorder = false;
        public bool HasRightBorder = false;
        private static BorderStyleHeader defaultBorders;
        private ButtonBorderStyle buttonBorderStyle = ButtonBorderStyle.None;
        private GridOptimizedBorderStyle style = GridOptimizedBorderStyle.Solid;

        public GridOptimizedBorderStyle Style
        {
            get { return style; }
            set { style = value; }
        }

        private GridOptimizedBorderWeight borderWeight = GridOptimizedBorderWeight.Thin;

        public GridOptimizedBorderWeight BorderWeight
        {
            get { return borderWeight; }
            set
            {
                borderWeight = value;

                switch (borderWeight)
                {
                    case GridOptimizedBorderWeight.Thin:
                        this.BorderSize = 1;
                        break;
                    case GridOptimizedBorderWeight.Medium:
                        this.BorderSize = 2;
                        break;
                    case GridOptimizedBorderWeight.Thick:
                        this.BorderSize = 3;
                        break;
                }
            }
        }

        public ButtonBorderStyle ButtonBorderStyle
        {
            get { return buttonBorderStyle; }
            set { buttonBorderStyle = value; }
        }

        #endregion

        #region Default

        public static BorderStyleHeader Default
        {
            get
            {
                if (defaultBorders == null)
                {
                    defaultBorders = new BorderStyleHeader();
                    defaultBorders.BorderBottomColor = Color.FromArgb(255, 15, 15, 15);
                    defaultBorders.BorderVerticalColor = SystemColors.WindowFrame;
                    defaultBorders.BorderUpColor = Color.FromArgb(255, 30, 30, 30);
                    defaultBorders.BorderSize = 1;
                    defaultBorders.HasBottomBorder = true;
                    defaultBorders.HasLeftBorder = false;
                    defaultBorders.HasRightBorder = false;
                    defaultBorders.HasTopBorder = true;
                    defaultBorders.ButtonBorderStyle = ButtonBorderStyle.Solid;
                }

                return defaultBorders.Clone() as BorderStyleHeader;
            }
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            //
            //Using this method is costly. It uses Reflection. Instead used Manual Clone.
            //
            //BorderStyle db = ObjectCloneHelper.Clone<BorderStyle>(this);

            //return db;

            BorderStyleHeader db = new BorderStyleHeader
            {
                BorderSize = this.BorderSize,
                BorderUpColor = this.BorderUpColor,
                BorderBottomColor = this.BorderBottomColor,
                BorderVerticalColor = this.BorderVerticalColor,
                HasTopBorder = this.HasTopBorder,
                HasLeftBorder = this.HasLeftBorder,
                HasBottomBorder = this.HasBottomBorder,
                HasRightBorder = this.HasRightBorder,
                buttonBorderStyle = this.buttonBorderStyle,
                style = this.Style,
                borderWeight = this.BorderWeight
            };

            return db;
        }

        #endregion
    }

    public enum GridOptimizedBorderWeight
    {
        Thin = 0,
        Medium = 1,
        Thick = 2
    }

    public enum GridOptimizedBorderStyle
    {
        None = 0,
        Solid = 1
    }
}
