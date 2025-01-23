using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Comparer;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers
{
    public class GridHeaderCellRenderer : CellRendererBase
    {
        #region Fields & Properties

        protected static SolidBrush brushText = new SolidBrush(Color.Yellow);
        protected static SolidBrush mainBrushData = new SolidBrush(Color.Gray);
        protected static Pen borderPen = new Pen(GridColorTable.Instance.RowBottomBorderColor, 1f);

        #endregion

        #region Constructors

        public GridHeaderCellRenderer(GridGroupingControl grid)
            : base(grid)
        { }

        #endregion

        #region Methods

        protected override void DrawBackground(Graphics g, Rectangle rect, GridStyleInfo style, int rowIndex, int colIndex, bool fillBackground, bool isSelectedCell)
        {
            try
            {
                Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles.BorderStyleHeader border = style.HeaderBorders;
                int rightWidth = (border.HasRightBorder) ? border.BorderSize : 0;

                if (rect.X > 0)
                    rect.X += rightWidth;

                Color topColor = style.HeaderBackColor1;

                bool isLightColor = false;

                isLightColor = style.HeaderBackColor2.R > 120 || style.HeaderBackColor2.G > 120 || style.HeaderBackColor2.B > 120;

                if (topColor == style.HeaderBackColor2)
                    topColor = GraphicsHelperGrid.AdjustColorLuminosity(style.HeaderBackColor2, 20);

                if (isSelectedCell)
                {
                    if (isLightColor)
                        topColor = GraphicsHelperGrid.AdjustColorLuminosity(style.HeaderBackColor2, -20);
                    else
                        topColor = GraphicsHelperGrid.AdjustColorLuminosity(style.HeaderBackColor2, 20);
                }

                if (colIndex >= 0 && GridControl.Table.VisibleColumns[colIndex].IsPrimaryColumn)
                {
                    if (isLightColor)
                        topColor = GraphicsHelperGrid.AdjustColorLuminosity(style.HeaderBackColor2, -80);
                    else
                        topColor = GraphicsHelperGrid.AdjustColorLuminosity(style.HeaderBackColor2, 80);
                }

                if (GridControl != null && !string.IsNullOrEmpty(GridControl.Table.FrozenColumn))
                {
                    TableInfo.TableColumn col = GridControl.Table.GetVisibleColumnFromName(GridControl.Table.FrozenColumn);

                    if (col != null)
                    {
                        int frozenIndex = col.CurrentPosition;

                        if (colIndex <= frozenIndex)
                        {
                            if (isLightColor)
                                topColor = GraphicsHelperGrid.AdjustColorLuminosity(style.HeaderBackColor2, -80);
                            else
                                topColor = GraphicsHelperGrid.AdjustColorLuminosity(style.HeaderBackColor2, 80);
                        }
                    }
                }

                DrawCellBackGround(rect.X, rect.Y, rect.Width, rect.Height, g, topColor, style.HeaderBackColor2);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void DrawCellBackGround(int x, int y, int width, int height, Graphics g, Color colorBegin, Color colorEnd)
        {
            try
            {
                if (height == 0 || width == 0)
                    return;

                mainBrushData.Color = colorBegin;

                float widthR = width / 10f;
                float heightR = height / 10f;

                //mainBrushData.ScaleTransform(widthR, heightR);
                //mainBrushData.RotateTransform(90f);

                GraphicsState state = null;
                if (GridControl.isPrinting)
                    state = g.Save();

                g.TranslateTransform(x, y);
                g.FillRectangle(mainBrushData, new Rectangle(0, 0, width, height));

                if (!GridControl.isPrinting)
                    g.ResetTransform();
                else
                    g.Restore(state);

                GridControl.Table.SetScale(g);

                //mainBrushData.ResetTransform();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void DrawBorders(Graphics g, Rectangle rect, GridStyleInfo style, int rowIndex, int colIndex, bool isSelectedCell)
        {
            //return;
            try
            {
                if (style != null && style.HeaderBorders != null)
                {
                    Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles.BorderStyleHeader border = style.HeaderBorders;
                    int topWidth = (border.HasTopBorder) ? border.BorderSize : 0;
                    int rightWidth = (border.HasRightBorder) ? border.BorderSize : 0;
                    int bottomWidth = (border.HasBottomBorder) ? border.BorderSize : 0;
                    int lefttWidth = (border.HasLeftBorder) ? border.BorderSize : 0;

                    if (bottomWidth > 0)
                    {
                        borderPen.Width = bottomWidth;
                        borderPen.Color = style.HeaderBorders.BorderBottomColor;
                        g.DrawLine(borderPen, rect.X, rect.Bottom - 1, rect.Right, rect.Bottom - 1);
                    }

                    if (topWidth > 0)
                    {
                        borderPen.Width = topWidth;
                        borderPen.Color = style.HeaderBorders.BorderUpColor;
                        g.DrawLine(borderPen, rect.X, rect.Top, rect.Right, rect.Top);
                    }

                    if (lefttWidth > 0)
                    {
                        borderPen.Width = lefttWidth;
                        borderPen.Color = style.HeaderBorders.BorderVerticalColor;
                        g.DrawLine(borderPen, rect.X, rect.Top - topWidth, rect.X, rect.Bottom - bottomWidth);
                    }

                    if (rightWidth > 0)
                    {
                        borderPen.Width = rightWidth;
                        borderPen.Color = style.HeaderBorders.BorderVerticalColor;
                        g.DrawLine(borderPen, rect.Right, rect.Top - topWidth, rect.Right, rect.Bottom - (bottomWidth * 2));
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, string text, GridStyleInfo style, bool isSelected)
        {
            try
            {
                if (GridControl.Table.VisibleColumns.Count > colIndex)
                {
                    string Str1 = string.Empty;

                    if (colIndex == -1 || colIndex == -2 || GridControl.Table.MergedHeaderColumns.Count > 0)
                    {
                        Str1 = text;
                        OnDrawDisplayText(g, Str1, clientRectangle, rowIndex, colIndex, this.GridControl.Table.TableStyle);
                    }
                    else
                    {
                        Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.TableInfo.TableColumn column = GridControl.Table.VisibleColumns[colIndex];
                        Str1 = column.DisplayName;
                        OnDrawDisplayText(g, Str1, clientRectangle, rowIndex, colIndex, column.ColumnStyle);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected virtual void OnDrawDisplayText(Graphics g, string displayText, Rectangle textRectangle, int rowIndex, int colIndex, GridStyleInfo style)
        {
            int numColHeader = GridControl.Table.VisibleColumnCount;
            int numRowHeader = GridControl.Table.RowCount;

            if (String.IsNullOrEmpty(displayText))
            {
                string label = String.Empty;
                if (colIndex > numColHeader && rowIndex == 0)
                    label = GridUtil.GetAlphaLabel(colIndex - numColHeader);
                else if (rowIndex > numRowHeader && colIndex == 0)
                    label = GridUtil.GetNumericLabel(rowIndex - numRowHeader);
                displayText = label;
            }

            if (displayText.Length > 0)
            {
                Font font = style.GdipHeaderFont;
                Color textColor = style.HeaderTextColor;

                brushText.Color = textColor;

                StringFormat format = new StringFormat();
                format.LineAlignment = GridUtil.ConvertToStringAlignment(style.HeaderVerticalAlignment);

                if (colIndex == -1)
                    format.Alignment = GridUtil.ConvertToStringAlignment(GridControl.Table.HeaderRowAlignment);
                else if (colIndex == -2)
                    format.Alignment = StringAlignment.Near; //Always Left Aligned                
                else
                    format.Alignment = GridUtil.ConvertToStringAlignment(style.HeaderHorizontalAlignment);

                format.SetTabStops(0f, new float[] { 50 });
                bool isTextRightToLeft = style.RightToLeft == RightToLeft.Inherit && GridControl.RightToLeft == RightToLeft.Yes || style.RightToLeft == RightToLeft.Yes;

                if (isTextRightToLeft)
                {
                    format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
                }

                format.Trimming = style.Trimming;

                //if (!style.WrapText)
                format.FormatFlags |= StringFormatFlags.NoWrap;
                //format.FormatFlags |= StringFormatFlags.FitBlackBox;
                //else
                //{
                //    format.FormatFlags = StringFormatFlags.LineLimit;
                //}

                TableInfo.TableColumn col = this.GridControl.Table.VisibleColumns[colIndex];// gridControl.Table.GetVisibleColumnFromDisplayName(displayText);
                bool isSorted = false;
                int sortOffset = 20;

                if (col != null)
                {
                    if (GridControl.Table.SortedColumnDescriptors[col.MappingName] != null)
                    {
                        SortColumnDescriptor sortDesc = GridControl.Table.SortedColumnDescriptors[col.MappingName];

                        int columnPosition = col.CurrentPosition;
                        bool columnFound = false;

                        for (int i = this.GridControl.Table.VisibleColumns.Count - 1; i >= 0; i--)
                        {
                            if (i <= columnPosition)
                                columnFound = true;

                            if (columnFound && this.GridControl.Table.VisibleColumns[i].IsPrimaryColumn)
                            {
                                columnPosition = i;
                                break;
                            }
                        }

                        bool isSortColumn = false;
                        int sortOrder = 0;

                        if (GridControl.GridType != GridType.MultiColumn)
                        {
                            isSortColumn = true;
                            sortOrder = GridControl.Table.SortedColumnDescriptors.IndexOf(sortDesc) + 1;
                        }
                        else
                        {
                            for (int i = 0; i < GridControl.Table.SortedColumnDescriptors.Count; i++)
                            {
                                string id = GridControl.Table.SortedColumnDescriptors[i].Id;

                                if (!string.IsNullOrEmpty(id))
                                {
                                    string[] arr = id.Split(':');
                                    if (arr.Length > 1 && Convert.ToInt32(arr[1]) >= columnPosition && Convert.ToInt32(arr[1]) <= col.CurrentPosition)
                                        sortOrder++;

                                    if (GridControl.Table.SortedColumnDescriptors[i].Id == col.MappingName + ":" + columnPosition)
                                    {
                                        isSortColumn = true;
                                        sortDesc = GridControl.Table.SortedColumnDescriptors[i];
                                        break;
                                    }
                                }
                            }
                        }

                        if (sortDesc != null && isSortColumn)
                        {
                            int displayTextWidth = GridStaticCellRenderer.MeasureDisplayStringWidth(g, displayText, font);

                            if (sortOrder >= 10)
                                sortOffset += 5;

                            Rectangle sortRectangle = textRectangle;
                            if (isTextRightToLeft)
                                sortRectangle.X = sortRectangle.X + 2;
                            else
                                sortRectangle.X = sortRectangle.Right - sortOffset;

                            sortRectangle.Width = sortOffset - 8;

                            if (col != null && col.DisplayName == displayText)
                            {
                                if (sortDesc.SortDirection == System.ComponentModel.ListSortDirection.Ascending)
                                {
                                    using (GraphicsPath path = Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Controls.ScrollBarEx.GetTriangle(new Point(sortRectangle.X + sortRectangle.Width / 2, sortRectangle.Y + sortRectangle.Height / 2), 3, 0))
                                    {
                                        g.FillPath(brushText, path);
                                        sortRectangle.Width += 10;
                                        sortRectangle.X += 10;
                                        g.DrawString(sortOrder.ToString(), font, brushText, sortRectangle);
                                        isSorted = true;
                                    }
                                }
                                else
                                {
                                    using (GraphicsPath path = Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Controls.ScrollBarEx.GetTriangle(new Point(sortRectangle.X + sortRectangle.Width / 2, sortRectangle.Y + sortRectangle.Height / 2), 3, 180))
                                    {
                                        g.FillPath(brushText, path);
                                        sortRectangle.Width += 10;
                                        sortRectangle.X += 10;
                                        g.DrawString(sortOrder.ToString(), font, brushText, sortRectangle);
                                        isSorted = true;
                                    }
                                }
                            }
                        }
                    }
                }

                if (GridControl.Table.HeaderRowAlignment == GridHorizontalAlignment.Left)
                    textRectangle.X += 2;

                //int displayTextWidth2 = GridStaticCellRenderer.MeasureDisplayStringWidth(g, displayText, font);

                //if (textRectangle.Width <= displayTextWidth2)
                //    textRectangle.Y += 2;

                if (isSorted)
                {
                    textRectangle.Width -= sortOffset;

                    if (isTextRightToLeft)
                        textRectangle.X += sortOffset;
                }

                if (textRectangle.Width <= 0)
                    textRectangle.Width = 5;

                g.DrawString(displayText, font, brushText, textRectangle, format);

                format.Dispose();
            }
        }

        #endregion
    }
}
