using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers
{
    /// <summary>
    /// TODO : Need to finlaize the rendering methods
    /// </summary>
    public class GridPushButtonCellRenderer : CellRendererBase
    {
        #region Fields

        static SolidBrush cellBrush = new SolidBrush(Color.Gray);
        static SolidBrush mainBrushData = new SolidBrush(Color.Gray);
        static Pen borderPen = new Pen(GridColorTable.Instance.RowBottomBorderColor, 1f);
        static Pen internalBorderPen = new Pen(GridColorTable.Instance.RowBottomBorderColor, 0.8f);

        #endregion

        #region Constructors

        public GridPushButtonCellRenderer(Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.GridGroupingControl grid)
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

            //if (!style.WrapText)
            format.FormatFlags |= StringFormatFlags.NoWrap;
            //else
            //{
            //    format.FormatFlags = StringFormatFlags.LineLimit;
            //}

            //if (format.Alignment == StringAlignment.Far)
            //{
            //    if (g.TextRenderingHint == System.Drawing.Text.TextRenderingHint.SystemDefault)
            //    {
            //        int stringWidth = MeasureDisplayStringWidth(g, displayText, font);
            //        if (stringWidth < textRectangle.Width)
            //        {
            //            stringWidth = Math.Min(stringWidth, textRectangle.Width);
            //            textRectangle = new Rectangle(textRectangle.Right - stringWidth, textRectangle.Y, stringWidth, textRectangle.Height);
            //            format.Alignment = StringAlignment.Near;
            //            format.Trimming = StringTrimming.None;
            //            format.FormatFlags = StringFormatFlags.NoWrap;
            //        }
            //    }
            //}


            if (cell.IsBlinkCell)
            {
                BlinkType type = GridControl.GetBlinkType(cell.Key);

                switch (type)
                {
                    case BlinkType.Up:
                        cellBrush.Color = GridColorTable.Instance.TextColorBlinkUp;
                        break;
                    case BlinkType.Down:
                        cellBrush.Color = GridColorTable.Instance.TextColorBlinkDown;
                        break;
                    case BlinkType.Equal:
                        cellBrush.Color = GridColorTable.Instance.TextColorBlinkUpdate;
                        break;
                }

            }
            else
                cellBrush.Color = style.TextColor;

            textRectangle.Height = textRectangle.Height - 1;
            textRectangle.Width = textRectangle.Width - 5;

            if (style.HorizontalAlignment == GridHorizontalAlignment.Left)
            {
                if (isRightToLeft)
                    textRectangle.X -= 3;
                else
                    textRectangle.X += 3;
            }

            if (cell.Column.CellModelType != "Image" && cell.Style.BackgroundImage == null)
            {
                if (cell.StrikeThrough)
                {
                    Font fontStrikethrough = FontUtil.CreateFont(font.FontFamily.Name, font.Size, FontStyle.Strikeout | font.Style);

                    if (fontStrikethrough != null)
                        g.DrawString(displayText, fontStrikethrough, cellBrush, textRectangle, format);
                    else
                        g.DrawString(displayText, font, cellBrush, textRectangle, format);
                }
                else
                    g.DrawString(displayText, font, cellBrush, textRectangle, format);
            }
            else
            {
                if (cell.Style.BackgroundImage == null)
                {
                    if (style.ImageList != null && style.ImageIndex >= 0 && style.ImageList.Images.Count > style.ImageIndex)
                        g.DrawImage(style.ImageList.Images[style.ImageIndex], (textRectangle.X + textRectangle.Width / 2) - 2, (textRectangle.Y + textRectangle.Height / 2) - 4, style.ImageList.Images[style.ImageIndex].Size.Width, style.ImageList.Images[style.ImageIndex].Size.Height);
                }
                else
                {
                    g.DrawImage(cell.Style.BackgroundImage, (textRectangle.X + textRectangle.Width / 2) - 4, (textRectangle.Y + textRectangle.Height / 2) - 6, cell.Style.BackgroundImage.Size.Width, cell.Style.BackgroundImage.Size.Height);
                }
            }


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

        private void DrawCellBackGround(int x, int y, int width, int height, Graphics g, GridStyleInfo style, Color colorBegin, Color colorEnd, bool doBlink, BlinkType blinkState, bool isSelectedCell)
        {
            try
            {

                GraphicsState state = null;
                if (GridControl.isPrinting)
                    state = g.Save();

                width -= 2; height -= 3;
                y++;

                float widthR = width / 10f;
                float heightR = height / 10f;

                //mainBrushData.ScaleTransform(widthR, heightR);
                //mainBrushData.RotateTransform(90f);
                g.TranslateTransform(x, y);
                x = 0; y = 0;

                using (GraphicsPath rr1 = new GraphicsPath())
                {
                    //if (buttonType == CustomCellButtonType.Rounded)
                    //{
                    //    rr1.AddBezier(x, y + r1, x, y, x + r1, y, x + r1, y);
                    //    rr1.AddLine(x + r1, y, x + w - r2, y);
                    //    rr1.AddBezier(x + w - r2, y, x + w, y, x + w, y + r2, x + w, y + r2);
                    //    rr1.AddLine(x + w, y + r2, x + w, y + h - r3);
                    //    rr1.AddBezier(x + w, y + h - r3, x + w, y + h, x + w - r3, y + h, x + w - r3, y + h);
                    //    rr1.AddLine(x + w - r3, y + h, x + r4, y + h);
                    //    rr1.AddBezier(x + r4, y + h, x, y + h, x, y + h - r4, x, y + h - r4);
                    //    rr1.AddLine(x, y + h - r4, x, y + r1);
                    //}
                    //else
                    //if (buttonType == CustomCellButtonType.Square)
                    //{
                    rr1.AddLine(x + 1, y, x + width - 2, y);
                    rr1.AddLine(x + width, y, x + width, y + height);
                    rr1.AddLine(x + width, y + height, x, y + height);
                    rr1.AddLine(x, y + height, x, y);
                    //}

                    Color textColor = style.TextColor;
                    Color internalBorderColor = GridColorTable.Instance.ButtonSelectedInternalBorderColor;
                    Color borderColor = GridColorTable.Instance.ButtonDefaultInternalBorderColor;

                    Color bottomColor = colorBegin;
                    Color topColor = Color.Empty;

                    if (colorBegin == colorEnd)
                        topColor = GraphicsHelperGrid.AdjustColorLuminosity(colorBegin, 20);
                    else
                        topColor = colorEnd;

                    mainBrushData.Color = topColor;

                    if (doBlink)
                    {
                        //    if (blinkState != ButtonBlinkState.Default)
                        //    {
                        if (blinkState == BlinkType.Up)
                        {
                            g.FillPath(mainBrushData, rr1);
                            borderPen.Color = GridColorTable.Instance.ButtonUpBorderColor;
                            borderPen.Width = 0.8f;
                            g.DrawPath(borderPen, rr1);

                            internalBorderColor = GraphicsHelperGrid.AdjustColorLuminosity(GridColorTable.Instance.ButtonUpBottomColor, 45);
                            borderColor = GridColorTable.Instance.ButtonUpBorderColor;
                            textColor = style.TextColorBlinkUp;
                        }
                        else if (blinkState == BlinkType.Down)
                        {
                            g.FillPath(mainBrushData, rr1);
                            borderPen.Color = GridColorTable.Instance.ButtonDownBorderColor;
                            borderPen.Width = 0.8f;
                            g.DrawPath(borderPen, rr1);

                            internalBorderColor = GraphicsHelperGrid.AdjustColorLuminosity(GridColorTable.Instance.ButtonDownBottomColor, 45);
                            borderColor = GridColorTable.Instance.ButtonDownBorderColor;
                            textColor = style.TextColorBlinkDown;
                        }
                        else if (blinkState == BlinkType.Equal)
                        {
                            g.FillPath(mainBrushData, rr1);
                            borderPen.Color = GridColorTable.Instance.ButtonValueUpdateBorderColor;
                            borderPen.Width = 0.8f;
                            g.DrawPath(borderPen, rr1);

                            internalBorderColor = GraphicsHelperGrid.AdjustColorLuminosity(GridColorTable.Instance.ButtonValueUpdateBottomColor, 45);
                            borderColor = GridColorTable.Instance.ButtonValueUpdateBorderColor;
                            textColor = style.TextColorBlinkUpdate;
                        }
                        //    }
                    }
                    else
                    {
                        //if (buttonState == ButtonState.Flat)
                        //{

                        //
                        //Border color should be draker than back color
                        //
                        borderColor = GraphicsHelperGrid.AdjustColorLuminosity(style.BackColorUp, -25);

                        g.FillPath(mainBrushData, rr1);
                        borderPen.Color = GridColorTable.Instance.ButtonValueUpdateBorderColor;
                        borderPen.Width = 0.8f;
                        g.DrawPath(borderPen, rr1);

                        if (isSelectedCell)
                            internalBorderColor = GraphicsHelperGrid.AdjustColorLuminosity(style.BackColorSelectedDown, 45);
                        else
                            internalBorderColor = GraphicsHelperGrid.AdjustColorLuminosity(style.BackColor, 45);

                        HslColorGrid hslColor = new HslColorGrid(internalBorderColor);

                        //
                        //If the brightness is too much, revert to back color
                        //
                        if (hslColor.Luminosity > 240)
                            internalBorderColor = style.BackColor;

                        //borderColor = Office2007Colors.Default.ButtonDefaultInternalBorderColor;
                        //}

                        //}
                        //else if (buttonState == ButtonState.Checked)
                        //{
                        //    Color bottomColor = Office2007Colors.Default.ButtonSelectedBottomColor;
                        //    Color topColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(bottomColor, 20);

                        //    //
                        //    //Border color should be draker than back color
                        //    //
                        //    borderColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(bottomColor, -25);

                        //    using (Brush brsh = new LinearGradientBrush(rect, topColor, bottomColor, 90F))
                        //    {
                        //        g.FillPath(brsh, rr1);
                        //        g.DrawPath(new Pen(Office2007Colors.Default.ButtonSelectedBorderColor, 0.8f), rr1);

                        //        internalBorderColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(bottomColor, 45);

                        //        HslColor hslColor = new HslColor(internalBorderColor);

                        //        //
                        //        //If the brightness is too much, revert to back color
                        //        //
                        //        if (hslColor.Luminosity > 240)
                        //            internalBorderColor = bottomColor;

                        //        //internalBorderColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(Office2007Colors.Default.ButtonSelectedBorderColor, 45);
                        //        //borderColor = Office2007Colors.Default.ButtonSelectedBorderColor;
                        //    }
                        //}
                        ////else if (buttonState == ButtonState.Pushed)
                        ////{
                        ////    using (Brush brsh = new LinearGradientBrush(rect, Office2007Colors.Default.ButtonPressedTopColor, Office2007Colors.Default.ButtonPressedBottomColor, 90F))
                        ////    {
                        ////        g.FillPath(brsh, rr1);
                        ////        g.DrawPath(new Pen(Office2007Colors.Default.ButtonPressedBorderColor, 0.8f), rr1);
                        ////        internalBorderColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(Office2007Colors.Default.ButtonPressedBorderColor, 45);
                        ////        borderColor = Office2007Colors.Default.ButtonPressedBorderColor;
                        ////    }
                        ////}
                        //else
                        //{
                        //    Color bottomColor = Office2007Colors.Default.ButtonSelectedBottomColor;
                        //    Color topColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(bottomColor, 20);

                        //    //
                        //    //Border color should be draker than back color
                        //    //
                        //    borderColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(bottomColor, -25);

                        //    using (Brush brsh = new LinearGradientBrush(rect, topColor, bottomColor, 90f))
                        //    {
                        //        g.FillPath(brsh, rr1);
                        //        g.DrawPath(new Pen(borderColor, 0.8f), rr1);
                        //        internalBorderColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(bottomColor, 45);

                        //        HslColor hslColor = new HslColor(internalBorderColor);

                        //        //
                        //        //If the brightness is too much, revert to back color
                        //        //
                        //        if (hslColor.Luminosity > 240)
                        //            internalBorderColor = bottomColor;

                        //        //borderColor = Office2007Colors.Default.ButtonDefaultInternalBorderColor;
                        //    }
                        //}
                    }

                    //
                    //Draw Outer Borders
                    //
                    //if (buttonType == CustomCellButtonType.Square)
                    //{
                    if (!GridControl.Table.Culture.TextInfo.IsRightToLeft)
                    {
                        //g.DrawLine(new Pen(borderColor, 0.8f), x, y, x + width - 1, y);
                        internalBorderPen.Color = internalBorderColor;
                        g.DrawLine(internalBorderPen, x + 1, y + 1, x + width - 1, y + 1);
                        g.DrawLine(internalBorderPen, x + 1, y + height - 1, x + 1, y + 1);
                    }
                    else
                    {
                        //g.DrawLine(new Pen(borderColor, 0.8f), x, y, x + width - 1, y);
                        internalBorderPen.Color = internalBorderColor;
                        g.DrawLine(internalBorderPen, x + 1, y + 1, x + width - 1, y + 1);
                        g.DrawLine(internalBorderPen, x + width - 1, y + height - 1, x + width - 1, y + 1);
                    }
                    //}

                    //if (!isCustomDrawing && style.BackgroundImage == null)
                    //{
                    //    //
                    //    //Draw Text on Button using Static Cell Renderer
                    //    //
                    //    bool disabled = !style.Enabled || style.HasError;

                    //    bool isTextRightToLeft = style.RightToLeft == RightToLeft.Inherit && Grid.IsRightToLeft() || style.RightToLeft == RightToLeft.Yes;
                    //    Rectangle buttonBounds = Bounds;
                    //    buttonBounds.Width -= 1;
                    //    GridStaticCellRenderer.DrawText(g, style.FormattedText, style.GdipFont, buttonBounds, style, textColor, isTextRightToLeft);
                    //}

                    //if (style.BackgroundImage != null)
                    //{ 
                    //    Image img = style.BackgroundImage;

                    //    int xImage = rect.X + (rect.Width / 2) - (img.Width / 2);
                    //    int yImage = rect.Y;

                    //    Rectangle rectImage = new Rectangle(xImage, yImage, img.Width, img.Height);

                    //    g.DrawImage(img, rectImage);
                    //}

                    if (!GridControl.isPrinting)
                        g.ResetTransform();
                    else
                        g.Restore(state);

                    GridControl.Table.SetScale(g);
                    //mainBrushData.ResetTransform();
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
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
            base.DrawBackground(g, rect, style, ref cell, fillBackground, isSelectedCell);

            if (!cell.DrawText)
                return;

            try
            {
                if (cell.IsBlinkCell)
                {
                    BlinkType type = GridControl.GetBlinkType(cell.Key);

                    switch (type)
                    {
                        case BlinkType.Up:
                            DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, style, style.BackColorBlinkUp, style.BackColorBlinkUp, true, type, false);
                            break;
                        case BlinkType.Down:
                            DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, style, style.BackColorBlinkDown, style.BackColorBlinkDown, true, type, false);
                            break;
                        case BlinkType.Equal:
                            DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, style, style.BackColorBlinkUpdate, style.BackColorBlinkUpdate, true, type, false);
                            break;
                        case BlinkType.None:
                        default:
                            if (cell.RowIndex % 2 == 0)
                                DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, style, style.BackColorUp, style.BackColorDown, false, type, false);
                            else
                                DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, style, style.BackColorAltUp, style.BackColorAltDown, false, type, false);
                            break;
                    }
                }
                else
                {
                    if (cell.RowIndex % 2 == 0)
                        DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, style, style.BackColorUp, style.BackColorDown, false, BlinkType.None, false);
                    else
                        DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, style, style.BackColorAltUp, style.BackColorAltDown, false, BlinkType.None, false);
                }
                
                if (isSelectedCell)
                {
                    DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, style, style.BackColorSelectedUp, style.BackColorSelectedDown, false, BlinkType.None, true);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void DrawBorders(Graphics g, Rectangle rect, GridStyleInfo style, int rowIndex, int colIndex, bool isSelectedCell)
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
