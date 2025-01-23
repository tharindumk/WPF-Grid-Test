using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;
using Mubasher.ClientTradingPlatform.Infrastructure.Module.Logger;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers
{
    public class GridCheckBoxCellRenderer : CellRendererBase
    {
        #region Fields

        static SolidBrush cellBrush = new SolidBrush(Color.Gray);
        static Pen markPen = new Pen(GridColorTable.Instance.TextColor, 1f);
        static Pen borderPen = new Pen(GridColorTable.Instance.RowBottomBorderColor, 1f);

        #endregion

        #region Constructors

        public GridCheckBoxCellRenderer(Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.GridGroupingControl grid)
            : base(grid)
        { }

        #endregion

        #region Draw Methods

        private void DrawText(Graphics g, Font font, Rectangle textRectangle, string displayText, TableInfo.CellStruct cell, GridStyleInfo style, bool drawDisabled, bool isRightToLeft)
        {
            StringFormat format = new StringFormat();
            format.LineAlignment = GridUtil.ConvertToStringAlignment(style.VerticalAlignment);
            format.Alignment = GridUtil.ConvertToStringAlignment(style.HorizontalAlignment);

            if (isRightToLeft)
            {
                format.Alignment = StringAlignment.Far;
            }

            format.Trimming = style.Trimming;
            format.SetTabStops(0f, new float[] { 50 });

            format.FormatFlags |= StringFormatFlags.NoWrap;

            textRectangle.Height = textRectangle.Height - 1;
            textRectangle.Width = textRectangle.Width - 5;

            if (style.HorizontalAlignment == GridHorizontalAlignment.Left)
            {
                if (isRightToLeft)
                    textRectangle.X -= 3;
                else
                    textRectangle.X += 3;
            }

            Rectangle checkBoxRectangle = textRectangle;
            checkBoxRectangle.Y = checkBoxRectangle.Y + 1;
            checkBoxRectangle.Width = checkBoxRectangle.Width - 3;
            checkBoxRectangle.Height = checkBoxRectangle.Height - 3;
            checkBoxRectangle.X = checkBoxRectangle.X + ( (checkBoxRectangle.Width / 2) - (checkBoxRectangle.Height / 2) );
            checkBoxRectangle.Width = checkBoxRectangle.Height;
            
            if (cell.TextInt == 1)
            {
                g.DrawRectangle(markPen, checkBoxRectangle);
                Rectangle inner = checkBoxRectangle;
                inner.Width = inner.Width - 2;
                inner.Height = inner.Height - 2;
                inner.X++;
                inner.Y++;
                g.DrawRectangle(markPen, inner);
                markPen.Width = 2;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawLine(markPen, new Point(checkBoxRectangle.X + 3, checkBoxRectangle.Y + ((checkBoxRectangle.Height / 3) + 2)), new Point(checkBoxRectangle.X + (checkBoxRectangle.Width / 3) + 1, checkBoxRectangle.Y + checkBoxRectangle.Height - 3));
                g.DrawLine(markPen, new Point(checkBoxRectangle.X + (checkBoxRectangle.Width / 3) + 1, checkBoxRectangle.Y + checkBoxRectangle.Height - 3), new Point(checkBoxRectangle.X - 3 + checkBoxRectangle.Width, checkBoxRectangle.Y + 3));
            }
            else
            {
                g.DrawRectangle(markPen, checkBoxRectangle);
                Rectangle inner = checkBoxRectangle;
                inner.Width = inner.Width - 2;
                inner.Height = inner.Height - 2;
                inner.X++;
                inner.Y++;
                g.DrawRectangle(markPen, inner);
            }

            g.SmoothingMode = SmoothingMode.None;
            markPen.Width = 1;

            format.Dispose();
        }

        static internal int MeasureDisplayStringWidth(Graphics graphics, string text, Font font)
        {
            RectangleF rect = RectangleF.Empty;

            if (String.IsNullOrEmpty(text))
                return 0;

            using (StringFormat format = new System.Drawing.StringFormat())
            {
                rect = new System.Drawing.RectangleF(0, 0, 1000, 1000);
                CharacterRange[] ranges = { new System.Drawing.CharacterRange(0, text.Length) };
                Region[] regions = new System.Drawing.Region[1];
                format.SetMeasurableCharacterRanges(ranges);
                regions = graphics.MeasureCharacterRanges(text, font, rect, format);
                rect = regions[0].GetBounds(graphics);
            }

            return (int)(rect.Right + 1.0f);
        }

        #endregion

        #region Base Overrides

        protected override void OnDraw(Graphics g, Rectangle clientRectangle, ref TableInfo.CellStruct cell, string text, GridStyleInfo style, bool isSelected)
        {
            try
            {
                DrawText(g, style.GdipFont, clientRectangle, text, cell, style, false, style.RightToLeft == RightToLeft.Yes);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override Rectangle OnLayout(int rowIndex, int colIndex, GridStyleInfo style, Rectangle innerBounds)
        {
            innerBounds = base.OnLayout(rowIndex, colIndex, style, innerBounds);

            return innerBounds;
        }

        public void DrawButtonBackground(Graphics g, Rectangle rect, GridStyleInfo style, TableInfo.CellStruct cell, bool fillBackground, bool isSelectedCell)
        {
            DrawBackground(g, rect, style, ref cell, fillBackground, isSelectedCell);
        }

        protected override void DrawBackground(Graphics g, Rectangle rect, GridStyleInfo style, int rowIndex, int colIndex, bool fillBackground, bool isSelectedCell)
        {

        }

        protected override void DrawBackground(Graphics g, Rectangle rect, GridStyleInfo style, ref TableInfo.CellStruct cell, bool fillBackground, bool isSelectedCell)
        {
            try
            {
                if (!isSelectedCell)
                {
                    if (cell.RowIndex % 2 == 0)
                    {
                        using (SolidBrush br = new SolidBrush(cell.Style.BackColor))
                        {
                            g.FillRectangle(br, rect);
                        }
                    }
                    else
                    {
                        using (SolidBrush br = new SolidBrush(cell.Style.BackColorAlt))
                        {
                            g.FillRectangle(br, rect);
                        }
                    }
                }
                else
                {
                    using (SolidBrush br = new SolidBrush(GridColorTable.Instance.BackColorSelectedUp))
                    {
                        g.FillRectangle(br, rect);
                    }
                }
            }
            catch (Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
            }
        }

        protected override void DrawBorders(Graphics g, Rectangle rect, GridStyleInfo style,int rowIndex, int colIndex, bool isSelectedCell)
        {
            try
            {
                if (style != null && style.BorderBottom != null)
                {
                    int topWidth = (style.BorderUp.Style != GridOptimizedBorderStyle.None) ? style.BorderUp.borderSize : 0;
                    int vertWidth = (style.BorderVertical.Style != GridOptimizedBorderStyle.None) ? style.BorderVertical.borderSize : 0;
                    int bottomWidth = (style.BorderBottom.Style != GridOptimizedBorderStyle.None) ? style.BorderBottom.borderSize : 0;

                    //rect.Width = rect.Width + 5;

                    if (bottomWidth > 0)
                    {
                        borderPen.Width = topWidth;
                        if (isSelectedCell)
                            borderPen.Color = style.BackColorSelectedDown;
                        else
                            borderPen.Color = style.BorderBottom.BorderColor;
                        g.DrawLine(borderPen, rect.X, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
                    }

                    if (topWidth > 0)
                    {
                        borderPen.Width = bottomWidth;
                        if (isSelectedCell)
                            borderPen.Color = GetTopColor(style.BackColorSelectedDown, style.BackColorSelectedUp, true);
                        else
                            borderPen.Color = style.BorderUp.BorderColor;
                        g.DrawLine(borderPen, rect.X, rect.Top, rect.Right, rect.Top);
                    }

                    if (vertWidth > 0)
                    {
                        borderPen.Width = vertWidth;
                        borderPen.Color = style.BorderVertical.BorderColor;
                        g.DrawLine(borderPen, rect.X, rect.Top - topWidth, rect.X, rect.Bottom - bottomWidth);
                    }

                    if (vertWidth > 0)
                    {
                        borderPen.Width = vertWidth;
                        borderPen.Color = style.BorderVertical.BorderColor;
                        g.DrawLine(borderPen, rect.Right, rect.Top - topWidth, rect.Right, rect.Bottom - bottomWidth);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private Color GetTopColor(Color bottomColor, Color topColor, bool isGradient)
        {
            Color topColorNew = topColor;

            if (bottomColor == topColor && isGradient)
                topColor = GraphicsHelperGrid.AdjustColorLuminosity(bottomColor, 20);

            return topColor;
        }

        #endregion
    }
}
