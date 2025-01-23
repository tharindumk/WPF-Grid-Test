using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;
using Mubasher.ClientTradingPlatform.Shared.Resources;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping
{
    public class GridPaint : IDisposable
    {
        private GridGroupingControl grid;
        private SolidBrush mainBackBrush = new SolidBrush(Color.Black);
        private List<TableInfo.GroupHeaderRowLevel> collapsedList = new List<TableInfo.GroupHeaderRowLevel>();

        public GridPaint(GridGroupingControl grid)
        {
            this.grid = grid;
        }

        public void PaintHeaders(Graphics g, Rectangle rect, bool paintForPrinting = false)
        {
            try
            {
                int startingYLocation = 0;
                bool isMirrored = this.grid.IsMirrored;

                if (grid.Table.MainHeaderOrientation == TableInfo.HeaderOrientation.Bottom)
                    startingYLocation = (this.grid.Table.mainHeaderHeight * this.grid.Table.NumberOfNestedHeaders);

                Rectangle Rect = rect;

                TableColumnCollection ColumnList = this.grid.Table.VisibleColumns;

                for (int j = 0; j < ColumnList.Count; j++)
                {
                    Rectangle Rect1 = new Rectangle(ColumnList[j].Left, startingYLocation, ColumnList[j].CellWidth, this.grid.Table.mainHeaderHeight);

                    if (paintForPrinting)
                        Rect1 = new Rectangle(ColumnList[j].Left + grid.horizontalOffset, startingYLocation, ColumnList[j].CellWidth, this.grid.Table.mainHeaderHeight);


                    if (grid.Table.IsDragging)
                    {
                        if (grid.GridType == GridType.MultiColumn)
                        {
                            if (grid.Table.draggingColumn != null && !grid.Table.draggingColumn.IsPrimaryColumn)
                            {
                                int currentIndex = grid.Table.VisibleColumns.IndexOf(grid.Table.draggingColumn);
                                int primaryColumn = -1;
                                bool isSelectedCell = j == grid.Table.MouseColNo;

                                for (int i = grid.Table.VisibleColumns.Count - 1; i >= 0; i--)
                                {
                                    if (grid.Table.VisibleColumns[i].IsPrimaryColumn)
                                    {
                                        primaryColumn = i;

                                        if (currentIndex > j)
                                        {
                                            if (i > j && i < currentIndex)
                                            {
                                                isSelectedCell = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (i < j && i > currentIndex)
                                            {
                                                isSelectedCell = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                                grid.CellRenderers[CellType.Header].Draw(g, Rect1, 0, j, ColumnList[j].DisplayName, ColumnList[j].ColumnStyle, isSelectedCell);
                            }
                            else
                                grid.CellRenderers[CellType.Header].Draw(g, Rect1, 0, j, ColumnList[j].DisplayName, ColumnList[j].ColumnStyle, false);
                        }
                        else
                            grid.CellRenderers[CellType.Header].Draw(g, Rect1, 0, j, ColumnList[j].DisplayName, ColumnList[j].ColumnStyle, j == grid.Table.MouseColNo);
                    }
                    else
                        grid.CellRenderers[CellType.Header].Draw(g, Rect1, 0, j, ColumnList[j].DisplayName, ColumnList[j].ColumnStyle, false);

                    Rect = Rectangle.Empty;
                    rect = Rectangle.Empty;
                }

                if (this.grid.Table.AllowNestedHeaders)
                {
                    if (grid.Table.MainHeaderOrientation == TableInfo.HeaderOrientation.Bottom)
                        startingYLocation = 0;

                    if (grid.Table.MergedHeaderColumns.Count > 0)
                    {
                        int start = 0;

                        int width = 0;

                        if (!isMirrored)
                        {
                            for (int i = 0; i < this.grid.cellsNestedHeaders.Count; i++)
                            {
                                TableInfo.CellStruct structure = this.grid.cellsNestedHeaders[i];

                                if (!structure.SuspendDraw)
                                {
                                    if (structure.ColIndex < this.grid.Table.VisibleColumns.Count - 1)
                                    {
                                        width = ColumnList[structure.ColIndex + 1].Left;
                                    }
                                    else
                                    {
                                        width = ColumnList[structure.ColIndex].Left + ColumnList[structure.ColIndex].CellWidth;
                                    }

                                    Rectangle Rect1 = new Rectangle(start, (this.grid.Table.mainHeaderHeight * structure.RowIndex), width - start, this.grid.Table.mainHeaderHeight);

                                    grid.CellRenderers[CellType.Header].Draw(g, Rect1, structure.RowIndex, structure.ColIndex, structure.TextString, ColumnList[structure.ColIndex].ColumnStyle, false);

                                    Rect = Rectangle.Empty;
                                    rect = Rectangle.Empty;
                                }

                                start = width;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < this.grid.cellsNestedHeaders.Count; i++)
                            {
                                if (i >= grid.Table.MergedHeaderColumns.Count)
                                    break;
                                TableInfo.CellStruct structure = this.grid.cellsNestedHeaders[i];

                                if (!structure.SuspendDraw)
                                {
                                    start = ColumnList[structure.ColIndex].Left;
                                    width = (ColumnList[grid.Table.MergedHeaderColumns[i].StartingIndex].Left + ColumnList[grid.Table.MergedHeaderColumns[i].StartingIndex].CellWidth) - ColumnList[structure.ColIndex].Left;

                                    Rectangle Rect1 = new Rectangle(start, (this.grid.Table.mainHeaderHeight * structure.RowIndex), width, this.grid.Table.mainHeaderHeight);

                                    grid.CellRenderers[CellType.Header].Draw(g, Rect1, structure.RowIndex, structure.ColIndex, structure.TextString, ColumnList[structure.ColIndex].ColumnStyle, false);

                                    Rect = Rectangle.Empty;
                                    rect = Rectangle.Empty;
                                }
                            }

                        }
                    }
                    else
                    {
                        for (int i = 0; i < this.grid.cellsNestedHeaders.Count; i++)
                        {
                            TableInfo.CellStruct structure = this.grid.cellsNestedHeaders[i];

                            if (!structure.SuspendDraw)
                            {
                                Rectangle Rect1 = new Rectangle(ColumnList[structure.ColIndex].Left, (this.grid.Table.mainHeaderHeight * structure.RowIndex), ColumnList[structure.ColIndex].CellWidth, this.grid.Table.mainHeaderHeight);

                                grid.CellRenderers[CellType.Header].Draw(g, Rect1, structure.RowIndex, structure.ColIndex, structure.TextString, ColumnList[structure.ColIndex].ColumnStyle, false);

                                Rect = Rectangle.Empty;
                                rect = Rectangle.Empty;
                            }
                        }
                    }
                }

                if (this.grid.Table.AllowRowHeaders)
                {
                    Rectangle Rect1 = Rectangle.Empty;

                    if (ColumnList.Count > 0)
                    {
                        for (int i = 0; i < this.grid.cellsNestedHeaders.Count + 1; i++)
                        {
                            Rect1 = new Rectangle(this.grid.Table.ColumnStartPosition, 0, this.grid.Table.RowHeaderWidth, this.grid.Table.mainHeaderHeight);
                            grid.CellRenderers[CellType.Header].Draw(g, Rect1, -1, -1, string.Empty, ColumnList[0].ColumnStyle, false);
                        }

                        Rect = Rectangle.Empty;
                        for (int i = 0; i < this.grid.cellsRowHeaders.Count; i++)
                        {
                            TableInfo.CellStruct structure = this.grid.cellsRowHeaders[i];

                            if (!structure.SuspendDraw)
                            {
                                Rect1 = new Rectangle(this.grid.Table.ColumnStartPosition, (this.grid.Table.HeaderHeight + (i * this.grid.Table.RowHeight)) - 1, this.grid.Table.RowHeaderWidth, this.grid.Table.mainHeaderHeight - 1);

                                grid.CellRenderers[CellType.Header].Draw(g, Rect1, structure.RowIndex, structure.ColIndex, structure.TextString, ColumnList[0].ColumnStyle, false);

                                Rect = Rectangle.Empty;
                                rect = Rectangle.Empty;
                            }
                        }
                    }
                }
                //
                //TODO
                //
                if (!string.IsNullOrEmpty(grid.Table.FrozenColumn))
                {
                    //
                    //Temp
                    //
                    int frozenColumnIndex = ColumnList[grid.Table.FrozenColumn].CurrentPosition + 1;

                    for (int j = 0; j < frozenColumnIndex; j++)
                    {
                        Rectangle Rect2 = new Rectangle(ColumnList[j].Left, 0, ColumnList[j].CellWidth, this.grid.Table.HeaderHeight);
                        grid.CellRenderers[CellType.Header].Draw(g, Rect2, 0, ColumnList[j].CurrentPosition, ColumnList[j].DisplayName, ColumnList[j].ColumnStyle, false);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private bool IsInCollapseRange(int row)
        {
            if (!this.grid.IsGroupingEnabled())
                return false;

            foreach (var item in grid.GroupHeaderRowIds)
            {
                if (item.RowId < row && row <= item.EndRowId && item.Collapsed)
                {
                    return true;
                }
            }

            return false;
        }

        private int GetBelowCollapseItems(int row)
        {
            if (!this.grid.IsGroupingEnabled())
                return 0;

            collapsedList.Clear();

            int topRow = grid.Table.TopRow;

            foreach (var item in grid.GroupHeaderRowIds)
            {
                if (item.RowId < row && item.Collapsed && item.RowId >= topRow)
                {
                    if (item.ParentGroup == null)
                        collapsedList.Add(item);
                    else
                    {
                        bool isAddToList = true;
                        isAddToList = grid.Table.IsGroupParentAllExpanded(item);

                        if (isAddToList)
                            collapsedList.Add(item);
                    }
                }
            }

            int count = 0;

            foreach (var item in collapsedList)
            {
                count += item.EndRowId - item.RowId;
            }

            return count;
        }

        public void PaintCell(Graphics g, Rectangle rect, int rowIndex, int colIndex, TableInfo.CellStruct cell, string text, GridStyleInfo style, bool isSelected)
        {
            if (isSelected)
            {

            }

            try
            {
                Rectangle Rect = rect;
                bool isHeaderMarked = CheckHeaderGroupAvaialble(cell);

                if (rect.Y < 0 || rect.Width == 0 || rect.Height == 0)
                {
                    return;
                }

                if (cell.Column != null)
                {
                    int headerCount = 0;
                    int headerBelowCount = 0;
                    int collapseItem = GetBelowCollapseItems(rowIndex);

                    int topRow = grid.Table.TopRow;
                    int difference = 0;

                    if (grid.IsGroupingEnabled())
                    {
                        Records collection = null;

                        if (grid.Table.IsFilterEnabled)
                            collection = grid.Table.filteredRecords;
                        else
                            collection = grid.Table.allRecords;

                        if (grid.Table.TopRecord != null)
                        {
                            for (int i = 0; i < collection.Count; i++)
                            {
                                Record item = (Record)collection[i];

                                if (item.GetData() == grid.Table.TopRecord.GetData())
                                {
                                    difference = i - topRow;
                                    break;
                                }
                            }
                        }

                        //One Time Execution only.. Re Set Value
                        if (rowIndex > topRow && difference > 0)
                        {
                            if (grid.Table.ChangedSorting)
                            {
                                grid.Table.SetOriginalRecord();
                                grid.Table.ChangedSorting = false;
                                grid.Table.TopRow = grid.Table.TopRow;
                            }
                            else
                            {
                                grid.Table.TopRow++;// = grid.Table.TopRow--;
                            }
                        }
                    }

                    if (isHeaderMarked)
                    {
                        for (int i = 0; i < grid.GroupHeaderRowIds.Count; i++)
                        {
                            if (grid.GroupHeaderRowIds[i].RowId == rowIndex)
                            {

                                bool isAllExpanded = grid.Table.IsGroupParentAllExpanded(grid.GroupHeaderRowIds[i]);

                                if (isAllExpanded)
                                    headerCount++;

                                if (!grid.GroupHeaderRowIds[i].Collapsed)
                                {
                                    headerBelowCount++;
                                }
                            }
                        }

                        Rect.Y += grid.Table.RowHeight * headerBelowCount;
                        Rect.Height = grid.Table.RowHeight;
                    }

                    Rect.Y -= grid.Table.RowHeight * (collapseItem);

                    bool isInCollapseRange = IsInCollapseRange(rowIndex);

                    if (grid.VirtualGroupHeaderRowIds.Count > 0 && grid.VirtualGroupHeaderRowIds.ContainsKey(rowIndex))
                    {
                        Rectangle rectTop = rect;
                        rectTop.Y = rectTop.Y;
                        rectTop.X = 0;
                        rectTop.Height = grid.Table.RowHeight;
                        rectTop.Width = grid.Width;
                        var currentHeader = grid.VirtualGroupHeaderRowIds[rowIndex];

                        string arrowString = string.Empty;

                        if (currentHeader.IsCollapsed)
                            arrowString = " ► ";
                        else
                            arrowString = " ▼ ";

                        if (colIndex == 0)
                            grid.CellRenderers[CellType.Header].Draw(g, rectTop, rowIndex, -2, "  " + arrowString + "  " + currentHeader.Title, style, false);

                    }
                    else
                    {
                        if (!isInCollapseRange)
                        {
                            if (string.IsNullOrEmpty(cell.CellModelType))
                                grid.CellRenderers[cell.Column.CellModelType].Draw(g, Rect, ref cell, text, style, isSelected);
                            else
                                grid.CellRenderers[cell.CellModelType].Draw(g, Rect, ref cell, text, style, isSelected);
                        }
                    }

                    if (isHeaderMarked && this.grid.IsGroupingEnabled())
                    {
                        Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.TableInfo.GroupHeaderRowLevel[] headerItems = new TableInfo.GroupHeaderRowLevel[headerCount];
                        int lastAddedIndex = 0;

                        for (int i = 0; i < grid.GroupHeaderRowIds.Count; i++)
                        {
                            if (grid.GroupHeaderRowIds[i].RowId == rowIndex)
                            {
                                bool isExpanded = true;
                                isExpanded = grid.Table.IsGroupParentAllExpanded(grid.GroupHeaderRowIds[i]);

                                if (isExpanded)
                                {
                                    headerItems[lastAddedIndex] = grid.GroupHeaderRowIds[i];
                                    lastAddedIndex++;
                                }
                            }
                        }

                        for (int i = 0; i < headerItems.Count(); i++)
                        {
                            var headerRowLevel = headerItems.ElementAt(i).RowLevel;
                            Rectangle rectTop = rect;
                            rectTop.Y = rectTop.Y + (grid.Table.RowHeight * i);
                            rectTop.X = 0;
                            rectTop.Height = grid.Table.RowHeight;
                            rectTop.Width = grid.Width;
                            rectTop.Y -= grid.Table.RowHeight * collapseItem;

                            //if (colIndex == 0)
                            {
                                var curString = headerItems[i].TitleText;
                                string arrowString = string.Empty;

                                if (headerItems[i].Collapsed)
                                    arrowString = " ► ";
                                else
                                    arrowString = " ▼ ";

                                string groupTitle = new string(' ', headerRowLevel * 5);
                                string titleText;

                                if (this.grid.GroupByNameList[headerRowLevel].Name != "*")
                                {
                                    titleText = grid.Table.Columns[this.grid.GroupByNameList[headerRowLevel].Name].DisplayName;
                                    groupTitle += arrowString + titleText + " : " + curString.ToString();
                                }
                                else
                                {
                                    titleText = string.Empty;
                                    groupTitle += arrowString + titleText + " " + curString.ToString();
                                }

                                int itemCount = ((headerItems[i].EndRowId + 1) - headerItems[i].RowId);
                                groupTitle += " (" + string.Format(SharedResources.ITEMS, itemCount.ToString()) + ")";
                                grid.CellRenderers[CellType.Header].Draw(g, rectTop, rowIndex, -2, groupTitle, style, false);
                            }
                        }
                    }

                }
                else if (grid.GridType == GridType.MultiColumn)
                {
                    grid.CellRenderers["Static"].Draw(g, Rect, ref cell, text, style, isSelected);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private bool CheckHeaderGroupAvaialble(TableInfo.CellStruct cell)
        {
            if (!this.grid.IsGroupingEnabled())
                return false;

            var celllocal = cell;
            bool isFound = false;

            for (int i = 0; i < grid.GroupHeaderRowIds.Count; i++)
            {
                if (this.grid.GroupHeaderRowIds[i].RowId == celllocal.RowIndex)
                {
                    isFound = true;
                    break;
                }
            }

            return this.grid.GroupHeaderRowIds.Count > 0 && isFound;
        }

        public void DrawMainGridBackGround(Graphics g, Color backColor, Rectangle rect)
        {
            mainBackBrush.Color = backColor;
            g.FillRectangle(mainBackBrush, rect);
        }

        public void Dispose()
        {
            if (mainBackBrush != null)
                mainBackBrush.Dispose();
        }
    }
}
