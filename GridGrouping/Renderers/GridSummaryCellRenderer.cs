﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers
{
    public class GridSummaryCellRenderer : CellRendererBase
    {
        #region Fields

        static SolidBrush cellBrush = new SolidBrush(Color.Yellow);
        static SolidBrush mainBrushData = new SolidBrush(Color.Gray);
        static Pen borderPen = new Pen(GridColorTable.Instance.RowBottomBorderColor, 1f);
        private const int verticalPadding = 1;
        private const int horizontalPadding = 5;

        #endregion

        #region Constructors

        public GridSummaryCellRenderer(Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.GridGroupingControl grid)
            : base(grid)
        { }

        #endregion

        #region Draw Methods

        public void DrawText(Graphics g, Font font, Rectangle textRectangle, string displayText, TableInfo.CellStruct cell, GridStyleInfo style, bool drawDisabled, bool isRightToLeft)
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
                        cellBrush.Color = style.TextColorBlinkUp;
                        break;
                    case BlinkType.Down:
                        cellBrush.Color = style.TextColorBlinkDown;
                        break;
                    case BlinkType.Equal:
                        cellBrush.Color = style.TextColorBlinkUpdate;
                        break;
                }

            }
            else
                cellBrush.Color = style.TextColor;

            textRectangle.Height = textRectangle.Height - verticalPadding;
            textRectangle.Width = textRectangle.Width - horizontalPadding;

            if (style.HorizontalAlignment == GridHorizontalAlignment.Left)
            {
                if (isRightToLeft)
                    textRectangle.X -= verticalPadding * 3;
                else
                    textRectangle.X += verticalPadding * 3;
            }

            if (cell.Column.CellModelType != "Image" && cell.Style.BackgroundImage == null)
                g.DrawString(displayText, font, cellBrush, textRectangle, format);
            else
            {
                if (cell.Style.BackgroundImage == null)
                {
                    if (style.ImageList != null && style.ImageIndex >= 0 && style.ImageList.Images.Count > style.ImageIndex)
                        g.DrawImage(style.ImageList.Images[style.ImageIndex], (textRectangle.X + textRectangle.Width / 2) - 2, (textRectangle.Y + textRectangle.Height / 2) - 4, style.ImageList.Images[style.ImageIndex].Size.Width, style.ImageList.Images[style.ImageIndex].Size.Height);
                }
                else
                {
                    g.DrawImage(cell.Style.BackgroundImage, (textRectangle.X + textRectangle.Width / 2) - 2, (textRectangle.Y + textRectangle.Height / 2) - 4, cell.Style.BackgroundImage.Size.Width, cell.Style.BackgroundImage.Size.Height);
                }
            }

            format.Dispose();
        }

        public static void DrawText(Graphics g, Font font, Rectangle textRectangle, string displayText, TableInfo.CellStruct cell, GridStyleInfo style, bool drawDisabled, bool isRightToLeft, BlinkType blinkInfo)
        {
            StringFormat format = new StringFormat();
            format.LineAlignment = GridUtil.ConvertToStringAlignment(style.VerticalAlignment);
            format.Alignment = GridUtil.ConvertToStringAlignment(style.HorizontalAlignment);

            if (isRightToLeft)
            {
                format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
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
                BlinkType type = blinkInfo;

                switch (type)
                {
                    case BlinkType.Up:
                        cellBrush.Color = style.TextColorBlinkUp;
                        break;
                    case BlinkType.Down:
                        cellBrush.Color = style.TextColorBlinkDown;
                        break;
                    case BlinkType.Equal:
                        cellBrush.Color = style.TextColorBlinkUpdate;
                        break;
                }

            }
            else
                cellBrush.Color = style.TextColor;

            textRectangle.Height = textRectangle.Height - verticalPadding;
            textRectangle.Width = textRectangle.Width - horizontalPadding;

            if (style.HorizontalAlignment == GridHorizontalAlignment.Left)
                textRectangle.X += verticalPadding * 3;

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

        private void DrawCellBackGround(int x, int y, int width, int height, Graphics g, Color colorBegin, Color colorEnd)
        {
            try
            {
                GraphicsState st = null;
                if (GridControl.isPrinting)
                    st = g.Save();

                if (height == 0 || width == 0)
                    return;

                mainBrushData.Color = colorBegin;

                float widthR = width / 10f;
                float heightR = height / 10f;

                // mainBrushData.ScaleTransform(widthR, heightR);
                // mainBrushData.RotateTransform(90f);
                g.TranslateTransform(x, y);
                g.FillRectangle(mainBrushData, new Rectangle(0, 0, width, height));

                if (GridControl.isPrinting)
                    g.Restore(st);
                else
                    g.ResetTransform();

                GridControl.Table.SetScale(g);
                //mainBrushData.ResetTransform();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void DrawBackground(Graphics g, Rectangle rect, GridStyleInfo style, int rowIndex, int colIndex, bool fillBackground, bool isSelectedCell)
        {
            try
            {
                if (rowIndex % 2 == 0)
                    DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, style.BackColorUp, style.BackColorDown);
                else
                    DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, style.BackColorAltUp, style.BackColorAltDown);

                if (isSelectedCell)
                {
                    DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, style.BackColorSelectedUp, style.BackColorSelectedDown);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void DrawBackground(Graphics g, Rectangle rect, GridStyleInfo style, ref TableInfo.CellStruct cell, bool fillBackground, bool isSelectedCell)
        {
            try
            {
                Color topColor = Color.Empty;

                if (cell.IsBlinkCell)
                {
                    BlinkType type = GridControl.GetBlinkType(cell.Key);

                    switch (type)
                    {
                        case BlinkType.Up:
                            topColor = GetTopColor(style.BackColorBlinkUp, style.BackColorBlinkUp, style.IsGradientBackColor);
                            DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, topColor, style.BackColorBlinkUp);
                            break;
                        case BlinkType.Down:
                            topColor = GetTopColor(style.BackColorBlinkDown, style.BackColorBlinkDown, style.IsGradientBackColor);
                            DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, topColor, style.BackColorBlinkDown);
                            break;
                        case BlinkType.Equal:
                            topColor = GetTopColor(style.BackColorBlinkUpdate, style.BackColorBlinkUpdate, style.IsGradientBackColor);
                            DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, topColor, style.BackColorBlinkUpdate);
                            break;
                        case BlinkType.None:
                        default:
                            if (cell.RowIndex % 2 == 0)
                            {
                                topColor = GetTopColor(style.BackColorDown, style.BackColorUp, style.IsGradientBackColor);
                                DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, topColor, style.BackColorDown);
                            }
                            else
                            {
                                topColor = GetTopColor(style.BackColorAltDown, style.BackColorAltUp, style.IsGradientBackColor);
                                DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, topColor, style.BackColorAltDown);
                            }
                            break;
                    }
                }
                else
                {
                    if (cell.RowIndex % 2 == 0)
                    {
                        topColor = GetTopColor(style.BackColorDown, style.BackColorUp, style.IsGradientBackColor);
                        DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, topColor, style.BackColorDown);
                    }
                    else
                    {
                        topColor = GetTopColor(style.BackColorAltDown, style.BackColorAltUp, style.IsGradientBackColor);
                        DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, topColor, style.BackColorAltDown);
                    }
                }
                
                if (isSelectedCell)
                {
                    topColor = GetTopColor(style.BackColorSelectedDown, style.BackColorSelectedUp, true);
                    DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, topColor, style.BackColorSelectedUp);
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

        protected override void DrawBorders(Graphics g, Rectangle rect, GridStyleInfo style, int rowIndex, int colIndex, bool isSelectedCell)
        {
            try
            {
                if (style != null && style.BorderBottom != null)
                {
                    int topWidth = style.BorderUp.GetBorderSizeFromWeight();
                    int vertWidth = 0;
                    int bottomWidth = style.BorderBottom.GetBorderSizeFromWeight();

                    if (bottomWidth > 0)
                    {
                        borderPen.Width = bottomWidth;
                        if (isSelectedCell)
                            borderPen.Color = style.BackColorSelectedDown;
                        else
                            borderPen.Color = style.BorderBottom.BorderColor;
                        g.DrawLine(borderPen, rect.X, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
                    }

                    if (topWidth > 0)
                    {
                        borderPen.Width = topWidth;
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

        #endregion
    }
}
