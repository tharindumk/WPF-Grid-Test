using Mubasher.ClientTradingPlatform.Infrastructure.Module.Logger;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Accessors;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Comparer;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Filters;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping
{
    //
    //Added by Ashan D
    //
    [Serializable]
    public class TableInfo : IDisposable
    {
        #region Fields & Properties

        public TableColumnCollection VisibleColumns = null;
        public TableColumnCollection Columns = null;

        private CellStruct[,] cellMatrix;

        internal CellStruct[,] CellMatrix
        {
            get
            {
                if (cellMatrix == null)
                    CreateMatrix(0, 0);

                return cellMatrix;
            }
            set
            {
                cellMatrix = value;
            }
        }

        public SortColumnDescriptorCollection SortedColumnDescriptors = null;

        private TableInfo.CellStruct currentCell;

        public TableInfo.CellStruct CurrentCell
        {
            get
            {
                return currentCell;
            }
            set
            {
                RaiseCurrentCellChangingEvent(currentCell.RowIndex, currentCell.ColIndex, currentCell);

                currentCell = value;

                RaiseCurrentCellChangedEvent(currentCell.RowIndex, currentCell.ColIndex, currentCell);
            }
        }

        public Record CurrentRecord
        {
            get
            {
                if (this.currentRow >= 0)
                {
                    return GetRecordFromIndex(currentRow);
                }

                return null;
            }
            set
            {
                if (value != null)
                {
                    int index = value.CurrentIndex;

                    if (index >= 0)
                        this.currentRow = index;
                }
            }
        }

        private RecordDataComparer comp = null;

        [NonSerialized]
        public Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.GridGroupingControl Grid;

        public bool IsDirty = false;
        public CellSelectionType CellSelectionType = CellSelectionType.Row;

        private RowCelectionType rowCelectionType = RowCelectionType.Single;

        public RowCelectionType RowCelectionType
        {
            get { return rowCelectionType; }
            set { rowCelectionType = value; }
        }

        public const int DefaultColumnWidth = 60;
        private int colCount = 0;
        internal Records allRecords = new Records();
        private Records originalRecords = new Records();
        internal Records filteredRecords = new Records();
        internal bool isSortingEnabled = false;
        internal TableColumn draggingColumn = null;
        internal bool isHeaderMouseDown = false;
        private bool isDragging = false;
        internal bool isCtrlKeyPressed = false;
        internal bool isShiftKeyPressed = false;
        internal bool isMouseDown = false;
        internal bool isMouseDownOnMultiSelect = false;
        internal bool alreadyHasHiddenSortDescriptorInCollection = false;
        public bool ChangeSortOnMouseHeaderClick = true;
        internal event EventHandler OnDraggingChanged;
        private List<Record> preFilterSort = new List<Record>();
        private int sortByLinqExceptionCount = 0;
        private bool collapseAll = false;

        private bool autoResetToFirstRowOnFilterChange = false;

        public bool AutoResetToFirstRowOnFilterChange
        {
            get { return autoResetToFirstRowOnFilterChange; }
            set { autoResetToFirstRowOnFilterChange = value; }
        }


        public bool IsDragging
        {
            get { return isDragging; }
            set
            {
                bool oldValue = isDragging;
                isDragging = value;

                if (OnDraggingChanged != null && oldValue != value)
                    OnDraggingChanged(Grid, new EventArgs());
            }
        }

        private CultureInfo culture;

        public CultureInfo Culture
        {
            get { return culture; }
            set
            {
                culture = value;
                Grid.HScroll.RightToLeftMode = culture.TextInfo.IsRightToLeft;
            }
        }

        public int Version = 0;
        internal Dictionary<string, GridStyleInfo> columnDefaultStyles = new Dictionary<string, GridStyleInfo>();
        internal List<int> listOfEmptyRows = new List<int>();
        public SortColumnDescriptor HiddenDefaultSortColumnDescriptor = null;

        public bool IsSortingEnabled
        {
            get { return isSortingEnabled && allowSort; }
        }

        //ExpressionFieldDescriptorCollection expressionFields;

        //public ExpressionFieldDescriptorCollection ExpressionFields
        //{
        //    get
        //    {
        //        return expressionFields;
        //    }
        //    set
        //    {
        //        if (value != null)
        //            ExpressionFields.InitializeFrom(value);
        //        else
        //            ResetExpressionFields();
        //    }
        //}

        /// <summary>
        /// Clears the <see cref="ExpressionFields"/> collection.
        /// </summary>
        /// <returns></returns>
        //public void ResetExpressionFields()
        //{
        //    if (ShouldSerializeExpressionFields())
        //        ExpressionFields.Clear();
        //}

        private bool allowBlink = false;

        public bool AllowBlink
        {
            get { return allowBlink; }
            set
            {
                allowBlink = value;

                if (!allowBlink)
                {
                    for (int i = 0; i < this.VisibleColumns.Count; i++)
                    {
                        this.VisibleColumns[i].AllowBlink = allowBlink;
                        this.Columns[i].AllowBlink = allowBlink;
                    }
                }
            }
        }

        private bool allowSort = false;

        public bool AllowSort
        {
            get { return allowSort; }
            set
            {
                allowSort = value;

                for (int i = 0; i < this.VisibleColumns.Count; i++)
                {
                    this.VisibleColumns[i].AllowSort = allowSort;
                    this.Columns[i].AllowSort = allowSort;
                }
            }
        }

        private bool allowFilter = false;

        public bool AllowFilter
        {
            get { return allowFilter; }
            set { allowFilter = value; }
        }

        private bool allowCustomFormulas = false;

        public bool AllowCustomFormulas
        {
            get { return allowCustomFormulas; }
            set { allowCustomFormulas = value; }
        }

        private bool allowColumnMouseResize = true;

        public bool AllowColumnMouseResize
        {
            get { return allowColumnMouseResize; }
            set { allowColumnMouseResize = value; }
        }

        private bool queryStyle = false;

        public bool QueryStyleToTable
        {
            get { return queryStyle; }
            set
            {
                queryStyle = value;

                for (int i = 0; i < this.Columns.Count; i++)
                {
                    this.Columns[i].QueryStyle = queryStyle;
                }

                for (int i = 0; i < this.VisibleColumns.Count; i++)
                {
                    this.VisibleColumns[i].QueryStyle = queryStyle;
                }
            }
        }

        private bool isFilterEnabled = false;

        public bool IsFilterEnabled
        {
            get
            {
                return isFilterEnabled && allowFilter && FilterManager.IsFilterAvailable();
            }
            set { isFilterEnabled = value; }
        }

        private bool allowMultipleColumnSort = true;

        public bool AllowMultipleColumnSort
        {
            get { return allowMultipleColumnSort && isSortingEnabled; }
            set { allowMultipleColumnSort = value; }
        }

        private bool allowSummaryRows = false;

        public bool AllowSummaryRows
        {
            get { return allowSummaryRows && NumberOfSummaryRows > 0; }
            set
            {
                allowSummaryRows = value;
                SetRecreateFlag(true);
            }
        }

        public int NumberOfSummaryRows = 0;

        private bool listenToMouseEvents = false;

        public bool ListenToMouseEvents
        {
            get { return listenToMouseEvents; }
            set { listenToMouseEvents = value; }
        }

        private bool allowNestedHeaders = false;

        public bool AllowNestedHeaders
        {
            get { return allowNestedHeaders && NumberOfNestedHeaders > 0; }
            set
            {
                allowNestedHeaders = value;
                SetRecreateFlag(true);
            }
        }

        public List<MergedColumnCell> MergedHeaderColumns = new List<MergedColumnCell>();

        public HeaderOrientation MainHeaderOrientation = HeaderOrientation.Top;

        private bool allowRowHeaders = false;

        public bool AllowRowHeaders
        {
            get { return allowRowHeaders; }
            set
            {
                allowRowHeaders = value;
                SetRecreateFlag(true);
            }
        }

        private int rowHeaderWidth = 0;

        public int RowHeaderWidth
        {
            get { return rowHeaderWidth; }
            set
            {
                if (rowHeaderWidth != value)
                {
                    rowHeaderWidth = value;
                    Grid.SetColumnPositions();
                }
            }
        }

        public int NumberOfNestedHeaders = 0;

        private bool listenToSelectionChangedEvents = false;

        public bool ListenToSelectionChangedEvents
        {
            get { return listenToSelectionChangedEvents; }
            set { listenToSelectionChangedEvents = value; }
        }

        private bool scrollToSelectedRecordOnSort = false;

        public bool ScrollToSelectedRecordOnSort
        {
            get { return scrollToSelectedRecordOnSort; }
            set { scrollToSelectedRecordOnSort = value; }
        }

        private bool allowdrag = true;

        public bool AllowDrag
        {
            get { return allowdrag; }
            set
            {
                allowdrag = value;

                for (int i = 0; i < this.VisibleColumns.Count; i++)
                {
                    this.VisibleColumns[i].AllowDrag = allowdrag;
                    this.Columns[i].AllowDrag = allowdrag;
                }
            }
        }

        public Records AllRecords
        {
            get { return allRecords; }
            set { allRecords = value; }
        }

        /// <summary>
        /// Used only For Multi Column Grid
        /// </summary>
        public Records OriginalRecords
        {
            get { return originalRecords; }
            set { originalRecords = value; }
        }

        public Records FilteredRecords
        {
            get { return filteredRecords; }
            set { filteredRecords = value; }
        }

        public int VisibleColumnCount
        {
            get
            {
                if (VisibleColumns != null && VisibleColumns.Count > 0)
                    return VisibleColumns.Count;
                else
                    return colCount;
            }
            set
            {
                colCount = value;
            }
        }

        public int ColumnCount
        {
            get
            {
                if (Columns != null && Columns.Count > 0)
                    return Columns.Count;
                else
                    return 0;
            }
        }

        public int TotalVisibleColumnWidth
        {
            get
            {
                int width = 0;

                try
                {
                    if (VisibleColumns != null && VisibleColumns.Count > 0)
                    {
                        for (int i = 0; i < VisibleColumns.Count; i++)
                        {
                            width += this.VisibleColumns[i].CellWidth;
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionsLogger.LogError(ex);
                }

                return width;
            }
        }

        private string frozenColumn = string.Empty;

        public string FrozenColumn
        {
            get { return frozenColumn; }
            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(frozenColumn))
                    {
                        if (this.Columns.Contains(frozenColumn))
                            this.Columns[frozenColumn].IsFrozen = false;

                        if (this.VisibleColumns.Contains(frozenColumn))
                            this.VisibleColumns[frozenColumn].IsFrozen = false;
                    }

                    if (string.IsNullOrEmpty(value))
                    {
                        frozenColumn = string.Empty;
                        return;
                    }

                    if (this.Columns.Contains(value))
                        this.Columns[value].IsFrozen = true;

                    if (this.VisibleColumns.Contains(value))
                        this.VisibleColumns[value].IsFrozen = true;

                    frozenColumn = value;
                }
                catch (Exception ex)
                {
                    ExceptionsLogger.LogError(ex);
                }
            }
        }

        //private ExpressionFieldEvaluator expressionColumnEvaluator;

        //public ExpressionFieldEvaluator ExpressionFieldEvaluator
        //{
        //    get
        //    {
        //        if (expressionColumnEvaluator == null)
        //        {
        //            expressionColumnEvaluator = new ExpressionFieldEvaluator(this);
        //        }
        //        return expressionColumnEvaluator;
        //    }
        //}

        //GridConditionalFormatDescriptorCollection _conditionalFormats;

        //public GridConditionalFormatDescriptorCollection ConditionalFormats
        //{
        //    get
        //    {
        //        if (_conditionalFormats == null)
        //        {
        //            _conditionalFormats = new GridConditionalFormatDescriptorCollection(this);
        //            //_conditionalFormats.Changed += new Syncfusion.Collections.ListPropertyChangedEventHandler(_conditionalFormats_Changed);
        //            //_conditionalFormats.Changing += new ListPropertyChangedEventHandler(_conditionalFormats_Changing);
        //        }
        //        return _conditionalFormats;
        //    }
        //    set
        //    {
        //        if (value != null)
        //            ConditionalFormats.InitializeFrom(value);
        //        else
        //            ResetConditionalFormats();
        //    }
        //}


        private int headerHeight = 20;
        internal int mainHeaderHeight = 20;
        private int? rowIndexSelectedBeforeSorting = null;
        //private int columnStartPosition = 0;

        public int ColumnStartPosition
        {
            get
            {
                int width = 0;

                if (AllowRowHeaders)
                {
                    if (rowHeaderWidth == 0)
                        width = 30;
                    else
                        width = rowHeaderWidth;
                }

                if (this.Grid.IsMirrored)
                {
                    return this.Grid.Width - width;
                }
                else
                    return 0;
            }
        }
        private int xOffset = 0;

        public int XOffset
        {
            get
            {
                return (int)(xOffset * GetScale()); ;
            }

            set
            {
                xOffset = value;
            }
        }

        internal int rowCount = 0;

        public int LastRecordIndex = 0;

        internal int sourceDataRowCount = 0;

        public int SourceDataRowCount
        {
            get { return sourceDataRowCount; }
            set
            {
                sourceDataRowCount = value;
            }
        }

        private int GetVisibleHeaders()
        {
            int titleCount = 0; //Show one extra row

            if (Grid.IsGroupingEnabled())
            {
                titleCount += 1;

                for (int i = 0; i < Grid.GroupHeaderRowIds.Count; i++)
                {
                    if (Grid.GroupHeaderRowIds[i].RowId >= this.TopRow && Grid.GroupHeaderRowIds[i].RowId <= this.BottomRow)
                    {
                        titleCount++;

                        //if (Grid.GroupHeaderRowIds[i].Collapsed)
                        //    titleCount -= Grid.GroupHeaderRowIds[i].EndRowId - Grid.GroupHeaderRowIds[i].RowId + 1;
                    }
                }
            }

            return titleCount;
        }

        public int RowCount
        {
            get
            {
                if (rowCount > 0)
                    return rowCount + GetVisibleHeaders();

                else
                {
                    if (IsFilterEnabled)
                    {
                        if (this.FilteredRecords.Count > 0)
                            return this.FilteredRecords.Count + GetVisibleHeaders();
                    }
                    else
                    {
                        if (this.AllRecords.Count > 0)
                            return this.AllRecords.Count + GetVisibleHeaders();
                    }
                }

                return rowCount + GetVisibleHeaders();
            }
            set
            {
                if (Grid.DataSource == null)
                {
                    if (rowCount != value || this.Grid.RecreateMatrixFlag)
                    {
                        if (!Grid.sizeChanging)
                        {
                            if (value <= sourceDataRowCount || sourceDataRowCount <= 0
                                || (rowCount < sourceDataRowCount && value > sourceDataRowCount) || Grid.resetGrid)
                            {
                                rowCount = value;

                                if (rowCount > sourceDataRowCount && sourceDataRowCount > -1)
                                    rowCount = sourceDataRowCount;

                                CreateMatrix(rowCount, this.VisibleColumnCount);

                                //rowCount = this.DisplayRows + 1;

                                this.Grid.RecreateMatrixFlag = false;
                            }
                        }
                    }
                }
                else
                    rowCount = value;
            }
        }

        private Point dragPoint1 = Point.Empty;
        private Point dragPoint2 = Point.Empty;

        public int firstSelectedRow = 0;

        public int lastSelectedRow = 0;

        private bool hideHeader = false;

        public bool HideHeader
        {
            get
            {
                return hideHeader;
            }
            set
            {
                hideHeader = value;
                headerHeight = 0;

                if (this.Grid != null)
                    this.Grid.Table.HeaderHeight = 0;
            }
        }

        private bool isCustomHeader = false;

        public bool IsCustomHeader
        {
            get
            {
                return isCustomHeader;
            }
            set
            {
                isCustomHeader = value;
            }
        }

        public int HeaderHeight
        {
            get
            {
                if (HideHeader)
                    return 0;
                else
                {
                    return headerHeight;
                }
            }
            set
            {
                int tempValue = value;
                mainHeaderHeight = value;

                if (!AllowNestedHeaders)
                    headerHeight = tempValue;
                else
                    headerHeight = tempValue + (NumberOfNestedHeaders * tempValue);
            }
        }

        public GridHorizontalAlignment HeaderRowAlignment = GridHorizontalAlignment.Center;

        IComparer _comparer = null;

        IComparer Comparer
        {
            get
            {
                if (_comparer == null)
                    _comparer = new RecordDataComparer(Comparer);
                return _comparer;
            }
        }

        private int rowHeight = 10;

        public int RowHeight
        {
            get
            {
                return (int)(rowHeight);
            }
            set
            {
                rowHeight = value;
            }
        }

        internal Record TopRecord = null;

        private int topRow = 0;

        public int TopRow
        {
            get
            {
                return topRow;
            }
            set
            {
                if (Grid.IsGroupingEnabled() && Grid.GroupHeaderRowIds.Count > 0)
                {
                    int actualRowItem = 0;
                    List<GroupHeaderRowLevel> omitList = new List<GroupHeaderRowLevel>();

                    if (value > topRow)
                    {
                        for (int i = 0; i < value; i++)
                        {
                            var curHeader = GetRootGroup(i);
                            var lastCollpsedGroup = GetLastCollapsedGroup(curHeader);

                            if (lastCollpsedGroup != curHeader)
                                curHeader = lastCollpsedGroup;

                            if (!omitList.Contains(curHeader))
                            {
                                omitList.Add(curHeader);

                                if (curHeader.Collapsed || curHeader.CollapsedForProsessing)
                                {
                                    actualRowItem += (curHeader.EndRowId - curHeader.RowId) + 1;
                                }
                                else
                                {
                                    actualRowItem++;
                                }
                            }
                            else if (!curHeader.Collapsed && !curHeader.CollapsedForProsessing)
                            {
                                if (i != value)
                                    actualRowItem++;
                            }
                        }

                        GroupHeaderRowLevel selectedHeader = GetRootGroup(actualRowItem);

                        if (selectedHeader != null)
                        {
                            if (actualRowItem == selectedHeader.RowId)
                            {
                                foreach (var item in Grid.GroupHeaderRowIds)
                                {
                                    if (item.RowId < actualRowItem && (item.Collapsed || item.CollapsedForProsessing))
                                    {
                                        item.Collapsed = false;
                                        item.CollapsedForProsessing = true;
                                        //break;
                                    }
                                }

                                topRow = actualRowItem;
                                SetOriginalRecord();
                                return;
                            }
                        }
                    }
                    else
                    {
                        actualRowItem = value;
                        int outRow = value;

                        var selectedHeader = GetRootGroup(outRow);
                        var lastCollpsedGroup = GetLastCollapsedGroup(selectedHeader);

                        if (lastCollpsedGroup != selectedHeader)
                            selectedHeader = lastCollpsedGroup;

                        if (selectedHeader.Collapsed || selectedHeader.CollapsedForProsessing)
                            outRow = selectedHeader.RowId;
                        else
                            outRow = value;

                        foreach (var item in Grid.GroupHeaderRowIds)
                        {
                            if (outRow >= item.RowId && outRow <= item.EndRowId)
                            {
                                if ((item.Collapsed || item.CollapsedForProsessing))
                                {
                                    item.Collapsed = true;
                                }
                            }
                            else if (item.RowId >= outRow)
                            {
                                if ((item.Collapsed || item.CollapsedForProsessing))
                                {
                                    item.Collapsed = true;
                                }
                            }
                        }

                        topRow = outRow;
                        SetOriginalRecord();
                        return;

                    }
                }

                topRow = value;

                if (Grid.GridType == GridType.Virtual)
                {
                    if (value < 0)
                        topRow = 0;
                }
            }
        }

        internal void SetOriginalRecord()
        {
            TopRecord = GetRecordFromIndex(topRow);
        }

        private int bottomRow = 10;

        public int BottomRow
        {
            get
            {
                //Check For Grouping Grid
                if (Grid != null
                    && (Grid.Table.filteredRecords.Count > 0 || Grid.Table.allRecords.Count > 0)
                    && Grid.IsGroupingEnabled())
                {
                    Records collection = null;

                    if (IsFilterEnabled)
                        collection = filteredRecords;
                    else
                        collection = allRecords;

                    return collection.Count;
                }


                if (Grid != null)
                {
                    if (Grid.DataSource != null && bottomRow >= Grid.DataSource.Count && Grid.DataSource.Count > 0)
                    {
                        if (Grid.GridType != GridType.MultiColumn)
                            bottomRow = Grid.DataSource.Count;
                    }
                    if (Grid.DataSource != null && Grid.DataSource.Count > 0 && bottomRow >= cellMatrix.GetLength(0))
                    {
                        bottomRow = cellMatrix.GetLength(0);
                    }

                    if (bottomRow == 0 && Grid.DataSource != null)
                        return Grid.DataSource.Count;
                }
                return bottomRow;
            }
            set
            {
                bottomRow = value;
            }
        }

        private int currentRow = 0;

        public int CurrentRow
        {
            get { return currentRow; }
            set
            {
                if (Grid.GridType == GridType.Virtual)
                {
                    if (Grid.VirtualGroupHeaderRowIds.Count > 0)
                    {
                        if (Grid.VirtualGroupHeaderRowIds.ContainsKey(value))
                        {
                            if (value == currentRow + 1)
                            {
                                value = value + 1;
                            }
                            else if (value == currentRow - 1)
                            {
                                value = value - 1;
                            }
                            else
                            {
                                value = value + 1;
                            }

                            if (value <= 0)
                                value = 1;
                        }
                    }
                }

                if (value < 0)
                {
                    indexRow = null;
                    value = 0;
                }
                else
                {
                    indexRow = GetRecordFromIndex(value);
                }


                if (value < cellMatrix.GetLength(0))
                {
                    if (GetRecordFromIndex(currentRow) != null)
                        RaiseSelectedRecordChangingEvent(currentRow, GetRecordFromIndex(currentRow) as Record);

                    currentRow = value;
                    if (this.cellMatrix.GetLength(0) > currentRow && this.cellMatrix.GetLength(1) > currentCol)
                        CurrentCell = this.cellMatrix[currentRow, currentCol];

                    Record rec = GetRecordFromIndex(currentRow);

                    if (Grid.GridType == GridType.Virtual)
                    {
                        if (!Grid.StopPaint)
                            RaiseSelectedRecordChangedEvent(currentRow, rec);
                    }
                    else
                    {
                        if (!Grid.StopPaint && rec != null)
                            RaiseSelectedRecordChangedEvent(currentRow, rec);
                    }

                }
                else
                {
                    if (this.Grid.GridType == GridType.Virtual)
                    {
                        currentRow = value;
                        RaiseSelectedRecordChangedEvent(-1, null);
                    }
                }
            }
        }

        private int currentCol = 0;

        public int CurrentCol
        {
            get { return currentCol; }
            set
            {
                currentCol = value;

                if (this.Grid.GridType != GridType.Virtual)
                {
                    if (value < cellMatrix.GetLength(1) && currentRow < cellMatrix.GetLength(0))
                    {
                        CurrentCell = this.cellMatrix[currentRow, currentCol];
                    }
                    else
                    {
                        if (cellMatrix.GetLength(0) > 0 && cellMatrix.GetLength(1) > 0)
                            currentCell = cellMatrix[0, 0];
                    }
                }
            }
        }

        public int DisplayRows = 10;
        public int MouseRowNo = 0;
        public int MouseColNo = 0;
        public int MousePosX = 0;
        public int MousePosY = 0;

        public int HorizontalMargin = 0;
        public int VerticalMargin = 0;
        public int HorizontalBorderWidth = 1;
        public GridStyleInfo TableStyle = GridStyleInfo.GetDefaultStyle();
        public GridStyleInfo SummaryStyle = GridStyleInfo.GetDefaultStyle();
        public List<int> selectedRecordIndexes = new List<int>();
        private Records selectedRecords = new Records();

        private Record indexRow;

        public Record IndexRow
        {
            get { return indexRow; }
            set { indexRow = value; }
        }


        public Records SelectedRecords
        {
            get
            {
                this.selectedRecords.Clear();

                if (this.Grid.GridType != GridType.MultiColumn)
                {
                    if (RowCelectionType == RowCelectionType.Multiple)
                    {
                        //
                        //One Row
                        //
                        if (lastSelectedRow == -1)
                        {
                            Record rec = GetRecordFromIndex(currentRow);
                            if (rec != null)
                                this.selectedRecords.Add(rec);
                        }
                        else
                        {
                            //
                            //Multiple Rows
                            //
                            if (firstSelectedRow <= lastSelectedRow)
                            {
                                for (int i = firstSelectedRow; i <= lastSelectedRow; i++)
                                {
                                    Record rec = GetRecordFromIndex(i);
                                    if (rec != null)
                                        this.selectedRecords.Add(rec);
                                }
                            }
                            else
                            {
                                for (int i = firstSelectedRow; i >= lastSelectedRow; i--)
                                {
                                    Record rec = GetRecordFromIndex(i);
                                    if (rec != null)
                                        this.selectedRecords.Add(rec);
                                }
                            }
                        }

                        if (selectedRecordIndexes.Count > 1)
                        {
                            Record rec = null;
                            for (int i = 0; i < selectedRecordIndexes.Count; i++)
                            {
                                rec = GetRecordFromIndex(selectedRecordIndexes[i]);

                                if (rec != null && !this.selectedRecords.Contains(rec))
                                    this.selectedRecords.Add(rec);
                            }
                        }
                    }
                    else if (RowCelectionType == RowCelectionType.Single)
                    {
                        this.selectedRecords.Add(GetRecordFromIndex(currentRow));
                    }
                }
                else
                {
                    if (RowCelectionType == RowCelectionType.Multiple)
                    {
                        //
                        //One Row
                        //
                        if (lastSelectedRow == -1)
                        {
                            if (currentRow <= LastRecordIndex)
                            {
                                Record rec = GetRecordFromIndex(currentRow, currentCol);
                                if (rec != null)
                                    this.selectedRecords.Add(rec);
                            }
                        }
                        else
                        {
                            //
                            //Multiple Rows
                            //
                            if (firstSelectedRow <= lastSelectedRow)
                            {
                                for (int i = firstSelectedRow; i <= lastSelectedRow; i++)
                                {
                                    if (i <= LastRecordIndex)
                                    {
                                        Record rec = GetRecordFromIndex(i, currentCol);
                                        if (rec != null)
                                            this.selectedRecords.Add(rec);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = firstSelectedRow; i >= lastSelectedRow; i--)
                                {
                                    if (i <= LastRecordIndex)
                                    {
                                        Record rec = GetRecordFromIndex(i, currentCol);
                                        if (rec != null)
                                            this.selectedRecords.Add(rec);
                                    }
                                }
                            }
                        }

                        if (selectedRecordIndexes.Count > 1)
                        {
                            Record rec = null;
                            for (int i = 0; i < selectedRecordIndexes.Count; i++)
                            {
                                if (i <= LastRecordIndex)
                                {
                                    rec = GetRecordFromIndex(selectedRecordIndexes[i], currentCol);

                                    if (rec != null && !this.selectedRecords.Contains(rec))
                                        this.selectedRecords.Add(rec);
                                }
                            }
                        }
                    }
                    else if (RowCelectionType == RowCelectionType.Single)
                    {
                        if (currentRow <= LastRecordIndex)
                        {
                            this.selectedRecords.Add(GetRecordFromIndex(currentRow, currentCol));
                        }
                    }
                }

                return selectedRecords;
            }
        }

        private Filters.FilterNew filterManager = new Filters.FilterNew();


        public FilterNew FilterManager
        {
            get { return filterManager; }
            set { filterManager = value; }
        }

        //private RecordFilterDescriptorCollection recordFilters;

        //public RecordFilterDescriptorCollection RecordFilters
        //{
        //    get
        //    {
        //        return null;// recordFilters;
        //    }
        //    set
        //    {
        //        //recordFilters = value;
        //    }
        //}

        #endregion

        #region Events

        public event GridCellMouseDownEventHandler MouseDown;
        public event GridCellMouseUpEventHandler MouseUp;
        public event GridCellMouseHoverEventHandler MouseHover;
        public event GridCellMouseHoverLeaveEventHandler MouseHoverLeave;
        public event GridCellMouseClickEventHandler MouseClick;
        public event GridCellMouseDoubleClickEventHandler MouseDoubleClick;
        public event GridCellButtonClickEventHandler CellButtonClick;
        public event GridCellMouseClickEventHandler MouseOuterRightClick;

        public event GridCurrentCellChangingEventHandler CellChanging;
        public event GridCurrentCellChangedEventHandler CellChanged;

        public event GridSelectedRecordChangingEventHandler SelectedRecordChanging;
        public event GridSelectedRecordChangedEventHandler SelectedRecordChanged;

        public event GridHeaderRightClickEventHandler HeaderRightClicked;
        public event EventHandler GridRecreated;
        public event EventHandler SortApplied;

        #endregion

        #region Constructor

        public TableInfo(Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.GridGroupingControl grid)
        {
            this.Grid = grid;
            Initialize();
        }

        #endregion

        #region Helper Methods

        private void Initialize()
        {
            this.VisibleColumns = new TableColumnCollection(this);
            this.Columns = new TableColumnCollection(this);
            //expressionFields = new ExpressionFieldDescriptorCollection(this);
            //expressionFields.Changed += new ListPropertyChangedEventHandler(expressionFields_Changed);

            this.VisibleColumns.Changed -= new ListPropertyChangedEventHandler(VisibleColumns_Changed);
            this.VisibleColumns.Changed += new ListPropertyChangedEventHandler(VisibleColumns_Changed);

            this.VisibleColumns.Changing -= new ListPropertyChangedEventHandler(VisibleColumns_Changing);
            this.VisibleColumns.Changing += new ListPropertyChangedEventHandler(VisibleColumns_Changing);

            if (this.SortedColumnDescriptors == null)
                SortedColumnDescriptors = new SortColumnDescriptorCollection(this);

            //RecordFilters = new RecordFilterDescriptorCollection(this);
            this.Culture = new CultureInfo("en");

            this.sourceDataRowCount = -1;
        }

        //public virtual bool ShouldSerializeExpressionFields()
        //{
        //    return expressionFields != null && expressionFields.Count > 0;
        //}

        //private void expressionFields_Changed(object sender, ListPropertyChangedEventArgs e)
        //{
        //    //TODO - ashan
        //}

        void VisibleColumns_Changing(object sender, ListPropertyChangedEventArgs e)
        {
            if (e.Action == ListPropertyChangedType.Remove)
            {
                if (e.Item != null)
                {
                    TableInfo.TableColumn col = e.Item as TableInfo.TableColumn;

                    SortColumnDescriptor descriptorToRemove = null;

                    int columnPosition = col.CurrentPosition;
                    bool columnFound = false;

                    for (int i = this.VisibleColumns.Count - 1; i >= 0; i--)
                    {
                        if (i <= columnPosition)
                            columnFound = true;

                        if (columnFound && this.VisibleColumns[i].IsPrimaryColumn)
                        {
                            columnPosition = i;
                            break;
                        }
                    }

                    foreach (SortColumnDescriptor des in this.SortedColumnDescriptors)
                    {
                        if (des.Name == col.MappingName)
                        {
                            string id = des.Id;

                            if (!string.IsNullOrEmpty(id))
                            {
                                string[] arr = id.Split(':');
                                if (arr.Length > 1 && Convert.ToInt32(arr[1]) >= columnPosition && Convert.ToInt32(arr[1]) <= col.CurrentPosition)
                                {
                                    UpdateRecordSortData(col.MappingName, des, false);
                                    descriptorToRemove = des;
                                    break;
                                }
                            }
                        }
                    }

                    if (descriptorToRemove != null && !IsDragging)
                    {
                        SortedColumnDescriptors.Remove(descriptorToRemove);
                        alreadyHasHiddenSortDescriptorInCollection = HiddenDefaultSortColumnDescriptor != null && this.SortedColumnDescriptors[HiddenDefaultSortColumnDescriptor.Name] != null;
                    }
                }
            }
        }

        void VisibleColumns_Changed(object sender, ListPropertyChangedEventArgs e)
        {
            if (e.Action == ListPropertyChangedType.Remove)
            {
                if (e.Item != null)
                {
                    TableColumn col = e.Item as TableColumn;
                    if (col != null)
                    {
                        //for (int i = 0; i < this.recordFilters.FilterList.Count; i++)
                        {
                            //
                            //TODO : Remove record filters if removing column is contained
                            //
                            //if(col.Name == this.recordFilters.FilterList[i] ||
                            //    col.MappingName == this.recordFilters.FilterList[i])
                            //    this.RecordFilters.Remove(
                        }
                    }

                    //
                    //This caused a bug when recreating matrix with filter applied
                    //
                    //this.RecordFilters.Clear();
                    //this.filteredRecords.Clear();
                }
            }
        }

        public void RemoveSortColumn(SortColumnDescriptor desc, TableColumn col)
        {
            if (desc != null)
            {
                UpdateRecordSortData(col.MappingName, desc, false);
                this.SortedColumnDescriptors.Remove(desc);
                alreadyHasHiddenSortDescriptorInCollection = HiddenDefaultSortColumnDescriptor != null && this.SortedColumnDescriptors[HiddenDefaultSortColumnDescriptor.Name] != null;
            }
        }

        /// <summary>
        /// Clears the <see cref="RecordFilters"/> collection.
        /// </summary>
        //public void ResetRecordFilters()
        //{
        //    RecordFilters.Clear();
        //}

        public void SetTableLevelStyleForAllColumns(bool preserveColumnSettings)
        {
            try
            {
                if (TableStyle == null)
                    TableStyle = (GridStyleInfo)GridStyleInfo.Default.Clone();
                columnDefaultStyles.Clear();

                bool isMirrored = this.Culture.TextInfo.IsRightToLeft;

                for (int i = 0; i < Columns.Count; i++)
                {
                    GridStyleInfo style = (GridStyleInfo)TableStyle.Clone();
                    style.HorizontalAlignment = Columns[i].ColumnStyle.HorizontalAlignment;
                    style.VerticalAlignment = Columns[i].ColumnStyle.VerticalAlignment;
                    style.Format = Columns[i].ColumnStyle.Format;
                    if (preserveColumnSettings)
                    {
                        style.BackColor = Columns[i].ColumnStyle.BackColor;
                        style.TextColor = Columns[i].ColumnStyle.TextColor;
                        style.BackColorAlt = Columns[i].ColumnStyle.BackColorAlt;
                        style.Font = Columns[i].ColumnStyle.Font;
                        style.Font.ResetFont();
                    }
                    else
                    {
                        style.Font = TableStyle.Font;
                        //style.Font = new GridFontInfo(TableStyle.Font.GdipFont);
                        style.Font.ResetFont();
                    }
                    style.ImageList = Columns[i].ColumnStyle.ImageList;
                    style.ImageIndex = Columns[i].ColumnStyle.ImageIndex;
                    if (isMirrored)
                        style.RightToLeft = RightToLeft.Yes;
                    Columns[i].ColumnStyle = null;
                    Columns[i].Type = Columns[i].Type;
                    Columns[i].ColumnStyle = style;
                }

                for (int i = 0; i < VisibleColumns.Count; i++)
                {
                    GridStyleInfo style = (GridStyleInfo)TableStyle.Clone();
                    style.HorizontalAlignment = VisibleColumns[i].ColumnStyle.HorizontalAlignment;
                    style.VerticalAlignment = VisibleColumns[i].ColumnStyle.VerticalAlignment;
                    style.Format = VisibleColumns[i].ColumnStyle.Format;
                    if (preserveColumnSettings)
                    {
                        style.BackColor = VisibleColumns[i].ColumnStyle.BackColor;
                        style.TextColor = VisibleColumns[i].ColumnStyle.TextColor;
                        style.BackColorAlt = VisibleColumns[i].ColumnStyle.BackColorAlt;
                        style.Font = VisibleColumns[i].ColumnStyle.Font;
                        style.Font.ResetFont();
                    }
                    else
                    {
                        if (VisibleColumns[i].QueryStyle)
                        {
                            style.Font = new GridFontInfo(TableStyle.Font.GdipFont);
                        }
                        else
                            style.Font = TableStyle.Font;

                        style.Font.ResetFont();
                    }
                    style.ImageList = VisibleColumns[i].ColumnStyle.ImageList;
                    style.ImageIndex = VisibleColumns[i].ColumnStyle.ImageIndex;
                    if (isMirrored)
                        style.RightToLeft = RightToLeft.Yes;
                    VisibleColumns[i].ColumnStyle = null;
                    VisibleColumns[i].Type = VisibleColumns[i].Type;
                    VisibleColumns[i].ColumnStyle = style;
                }

                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < this.VisibleColumns.Count; j++)
                    {
                        if (i >= cellMatrix.GetLength(0) || j >= cellMatrix.GetLength(1))
                            continue;

                        if (cellMatrix[i, j].CellModelType != CellType.Summary)
                        {
                            if (this.VisibleColumns[j].QueryStyle)
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                            }
                            else
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)VisibleColumns[j].ColumnStyle;
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                            }

                            if (Grid != null && Grid.GridType == GridType.MultiColumn)
                                cellMatrix[i, j].Type = VisibleColumns[j].Type;
                        }
                        else
                        {
                            cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                            cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                        }

                        if (!columnDefaultStyles.ContainsKey(VisibleColumns[j].Name))
                        {
                            GridStyleInfo styleNew = new GridStyleInfo();
                            ObjectCloneHelper.Clone<GridStyleInfo>(ref styleNew, VisibleColumns[j].ColumnStyle);
                            columnDefaultStyles.Add(VisibleColumns[j].Name, styleNew);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void UpdateColumnReferences()
        {
            try
            {
                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < this.VisibleColumns.Count; j++)
                    {
                        if (i >= cellMatrix.GetLength(0) || j >= cellMatrix.GetLength(1))
                            continue;

                        if (cellMatrix[i, j].CellModelType != CellType.Summary)
                        {
                            cellMatrix[i, j].Column = this.VisibleColumns[j];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void SetColumnStylesToCells()
        {
            try
            {
                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < Columns.Count; j++)
                    {
                        if (cellMatrix.GetLength(1) > j)
                        {
                            if (CellMatrix.GetLength(0) <= i || CellMatrix.GetLength(1) <= j)
                                continue;

                            if (cellMatrix[i, j].CellModelType != CellType.Summary)
                            {
                                if (this.Columns[j].QueryStyle)
                                {
                                    cellMatrix[i, j].Style = (GridStyleInfo)Columns[j].ColumnStyle.Clone();
                                    cellMatrix[i, j].OriginalStyle = (GridStyleInfo)Columns[j].ColumnStyle.Clone();
                                }
                                else
                                {
                                    cellMatrix[i, j].Style = (GridStyleInfo)Columns[j].ColumnStyle;
                                    cellMatrix[i, j].OriginalStyle = (GridStyleInfo)Columns[j].ColumnStyle.Clone();
                                }
                            }
                            else
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                            }
                        }
                    }
                }

                for (int i = 0; i < RowCount; i++)
                {
                    for (int j = 0; j < VisibleColumns.Count; j++)
                    {
                        if (CellMatrix.GetLength(0) <= i || CellMatrix.GetLength(1) <= j)
                            continue;

                        if (cellMatrix[i, j].CellModelType != CellType.Summary)
                        {
                            if (this.VisibleColumns[j].QueryStyle)
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                            }
                            else
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)VisibleColumns[j].ColumnStyle;
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                            }
                        }
                        else
                        {
                            cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                            cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public bool CopyCellToClipBoard(int colIndex, int rowIndex)
        {
            try
            {
                bool isok = false;

                if (colIndex < VisibleColumns.Count && rowIndex < RowCount)
                {
                    //TODO
                    //Clipboard
                }

                return isok;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
                return false;
            }
        }

        public void CreateMatrix(int rowCount, int columnCount)
        {
            try
            {
                if (this.Grid.GridType == GridType.MultiColumn)
                {
                    CreateMatrixForMultiColumnGrid(rowCount, columnCount);
                    return;
                }

                bool isFiltered = false;

                if (!Grid.isDataSourceChangeInProgress && this.IsFilterEnabled && this.FilteredRecords.Count > 0)
                {
                    rowCount = this.filteredRecords.Count;
                    isFiltered = true;
                }

                if (AllowSummaryRows && rowCount > 0)
                {
                    rowCount += NumberOfSummaryRows;

                    if (Grid.DataSource != null && rowCount > Grid.DataSource.Count + NumberOfSummaryRows)
                    {
                        rowCount = Grid.DataSource.Count + NumberOfSummaryRows;
                    }
                }

                cellMatrix = new TableInfo.CellStruct[rowCount, columnCount];

                if (!isFiltered)
                    this.allRecords.Clear();

                for (int i = 0; i < rowCount; i++)
                {
                    Record r = null;

                    if (AllowSummaryRows)
                    {
                        if (i <= rowCount - NumberOfSummaryRows - 1)
                        {
                            if (!isFiltered && Grid.DataSource != null)
                                r = new Record(i, Grid.DataSource[i], this);
                            else if (this.filteredRecords.Count > 0)
                                r = new Record(i, (this.filteredRecords[i] as Record).ObjectBound, this);
                        }
                    }
                    else
                    {
                        if (!isFiltered && Grid.DataSource != null)
                            r = new Record(i, Grid.DataSource[i], this);
                        else if (this.filteredRecords.Count > 0)
                            r = new Record(i, (this.filteredRecords[i] as Record).ObjectBound, this);
                    }


                    for (int j = 0; j < columnCount; j++)
                    {
                        TableInfo.TableColumn col = VisibleColumns[j];
                        cellMatrix[i, j].Column = col;// VisibleColumns[j];

                        if (col == null)
                            continue;

                        if (cellMatrix[i, j].CellModelType != CellType.Summary)
                        {
                            if (col.QueryStyle)
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                            }
                            else
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle;
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle;
                            }
                        }
                        else
                        {
                            cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                            cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                        }

                        cellMatrix[i, j].Type = col.Type;
                        col.ColumnStyle.CellValueType = col.Type;

                        col.ColumnStyle.CellValueType = VisibleColumns[j].Type;

                        cellMatrix[i, j].Style.CellValueType = col.Type;
                        cellMatrix[i, j].OriginalStyle.CellValueType = col.Type;
                        cellMatrix[i, j].IsEmpty = true;
                        cellMatrix[i, j].TextInt = 0;
                        cellMatrix[i, j].TextDouble = 0;
                        cellMatrix[i, j].TextLong = 0;
                        cellMatrix[i, j].TextString = "";
                        cellMatrix[i, j].RowIndex = i;
                        cellMatrix[i, j].ColIndex = j;
                        cellMatrix[i, j].IsDirty = false;
                        cellMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                        cellMatrix[i, j].CellModelType = col.CellModelType;
                        cellMatrix[i, j].DrawText = true;

                        cellMatrix[i, j].Record = r;
                        if (col.CurrentPosition <= 0)
                            col.CurrentPosition = j;

                        int allColumnsIndex = Columns.IndexOf(col);

                        if (allColumnsIndex >= 0 && Columns[allColumnsIndex].CurrentPosition <= 0)
                            Columns[allColumnsIndex].CurrentPosition = j;

                        if (i > rowCount - NumberOfSummaryRows - 1)
                            cellMatrix[i, j].CellModelType = CellType.Summary;

                        if (!columnDefaultStyles.ContainsKey(col.Name))
                        {
                            GridStyleInfo styleNew = new GridStyleInfo();
                            ObjectCloneHelper.Clone<GridStyleInfo>(ref styleNew, col.ColumnStyle);
                            columnDefaultStyles.Add(col.Name, styleNew);
                        }
                    }

                    if (!isFiltered && r != null)
                        this.allRecords.Add(r);
                }

                if (comp == null)
                    comp = new RecordDataComparer(new SortColumnComparer(this));

                if (isSortingEnabled)
                {
                    UpdateRecordSortKeys();

                    SortSourceList();
                }

                if (rowCount > 0 && columnCount > 0)
                    currentCell = cellMatrix[0, 0];

                if (Grid.GridType == GridType.DataBound)
                    this.rowCount = rowCount;
                this.LastRecordIndex = rowCount;
                if (Grid.GridType == GridType.Virtual)
                {
                    if (TopRow >= sourceDataRowCount)
                        //this.TopRow = 0;
                        this.TopRow = sourceDataRowCount; // Changed to prevent scrolling to top when refreshing the grid

                }
                else
                {
                    //if (TopRow >= rowCount)
                    //this.TopRow = 0; 

                    // Changed to prevent scrolling to top when refreshing the grid
                    if (TopRow > rowCount)
                        TopRow = rowCount;
                }

                this.selectedRecordIndexes.Clear();
                UpdateRecordSortKeys();

                //UpdateExpressionFields(columnCount);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                OnGridRecreated();
            }
        }

        //private void UpdateExpressionFields(int columnCount)
        //{
        //    try
        //    {
        //        for (int j = 0; j < columnCount; j++)
        //        {
        //            TableInfo.TableColumn col = VisibleColumns[j];

        //            if (col.IsCustomFormulaColumn && this.ExpressionFields[col.Name] != null)
        //            {
        //                this.ExpressionFields[col.Name].UpdateRelatedColumnList();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionsLogger.LogError(ex);
        //    }
        //}

        internal void UpdateRecordSortKeys()
        {
            foreach (SortColumnDescriptor col in SortedColumnDescriptors)
            {
                UpdateRecordSortData(col);

                for (int i = 0; i < this.VisibleColumns.Count; i++)
                {
                    if (this.VisibleColumns[i].MappingName == col.Name)
                    {
                        col.Id = this.VisibleColumns[i].MappingName + ":" + i;// this.VisibleColumns[i].CurrentPosition;
                        break;
                    }
                }
            }
        }

        public void CreateMatrixForMultiColumnGrid(int rowCount, int columnCount)
        {
            try
            {
                if (!Grid.isDataSourceChangeInProgress && this.IsFilterEnabled && this.FilteredRecords.Count > 0)
                {
                    rowCount = this.filteredRecords.Count;
                }

                if (AllowSummaryRows && rowCount > 0)
                    rowCount += NumberOfSummaryRows;

                rowCount += listOfEmptyRows.Count;

                if (rowCount < (Grid as GridGroupingControlMC).MinimumRowCount)
                    rowCount = (Grid as GridGroupingControlMC).MinimumRowCount;

                cellMatrix = new TableInfo.CellStruct[rowCount, columnCount];

                if (this.Grid.DataSource == null)
                    return;

                if (!Grid.IsColumnAlterationInProgress)
                    this.allRecords.Clear();

                int recordIndex = 0;
                int allRows = 0;
                bool endOfRecordsFound = false;

                for (int i = 0; i < rowCount; i++)
                {
                    Record r = null;

                    if (recordIndex >= this.Grid.DataSource.Count)
                    {
                        if (i < (Grid as GridGroupingControlMC).MinimumRowCount)
                        {
                            for (int j = 0; j < columnCount; j++)
                            {
                                TableInfo.TableColumn col = VisibleColumns[j];

                                if (col == null)
                                    continue;


                                cellMatrix[i, j].Column = col;

                                if (cellMatrix[i, j].CellModelType != CellType.Summary)
                                {
                                    if (col.QueryStyle)
                                    {
                                        cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                        cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                                    }
                                    else
                                    {
                                        cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle;
                                        cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle;
                                    }
                                }
                                else
                                {
                                    cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                                    cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                                }

                                cellMatrix[i, j].Type = col.Type;
                                col.ColumnStyle.CellValueType = col.Type;

                                col.ColumnStyle.CellValueType = VisibleColumns[j].Type;

                                cellMatrix[i, j].Style.CellValueType = col.Type;
                                cellMatrix[i, j].OriginalStyle.CellValueType = col.Type;
                                cellMatrix[i, j].IsEmpty = true;
                                cellMatrix[i, j].TextInt = 0;
                                cellMatrix[i, j].TextDouble = 0;
                                cellMatrix[i, j].TextLong = 0;
                                cellMatrix[i, j].TextString = "";
                                cellMatrix[i, j].RowIndex = i;
                                cellMatrix[i, j].ColIndex = j;
                                cellMatrix[i, j].IsDirty = false;
                                cellMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                                cellMatrix[i, j].CellModelType = col.CellModelType;
                                cellMatrix[i, j].DrawText = true;

                                cellMatrix[i, j].SourceIndex = -1;

                                if (col.CurrentPosition <= 0)
                                    col.CurrentPosition = j;

                                int allColumnsIndex = Columns.IndexOf(col);

                                if (allColumnsIndex >= 0 && Columns[allColumnsIndex].CurrentPosition <= 0)
                                    Columns[allColumnsIndex].CurrentPosition = j;

                                if (i > rowCount - NumberOfSummaryRows - 1)
                                    cellMatrix[i, j].CellModelType = CellType.Summary;
                            }
                        }
                        continue;
                    }

                    allRows++;

                    if (!listOfEmptyRows.Contains(i))
                    {
                        if (!Grid.IsColumnAlterationInProgress && Grid.DataSource != null)
                            r = new Record(recordIndex, Grid.DataSource[recordIndex], this);
                        else
                            r = this.allRecords[recordIndex] as Record;
                    }
                    else
                    {
                        for (int j = 0; j < columnCount; j++)
                        {
                            TableInfo.TableColumn col = VisibleColumns[j];

                            if (col == null)
                                continue;


                            cellMatrix[i, j].Column = col;

                            if (cellMatrix[i, j].CellModelType != CellType.Summary)
                            {
                                if (col.QueryStyle)
                                {
                                    cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                    cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                                }
                                else
                                {
                                    cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle;
                                    cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle;
                                }
                            }
                            else
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                            }

                            cellMatrix[i, j].Type = col.Type;
                            col.ColumnStyle.CellValueType = col.Type;

                            col.ColumnStyle.CellValueType = VisibleColumns[j].Type;

                            cellMatrix[i, j].Style.CellValueType = col.Type;
                            cellMatrix[i, j].OriginalStyle.CellValueType = col.Type;
                            cellMatrix[i, j].IsEmpty = true;
                            cellMatrix[i, j].TextInt = 0;
                            cellMatrix[i, j].TextDouble = 0;
                            cellMatrix[i, j].TextLong = 0;
                            cellMatrix[i, j].TextString = "";
                            cellMatrix[i, j].RowIndex = i;
                            cellMatrix[i, j].ColIndex = j;
                            cellMatrix[i, j].IsDirty = false;
                            cellMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                            cellMatrix[i, j].CellModelType = col.CellModelType;
                            cellMatrix[i, j].DrawText = true;

                            cellMatrix[i, j].SourceIndex = -1;

                            if (col.CurrentPosition <= 0)
                                col.CurrentPosition = j;

                            int allColumnsIndex = Columns.IndexOf(col);

                            if (allColumnsIndex >= 0 && Columns[allColumnsIndex].CurrentPosition <= 0)
                                Columns[allColumnsIndex].CurrentPosition = j;

                            if (i > rowCount - NumberOfSummaryRows - 1)
                                cellMatrix[i, j].CellModelType = CellType.Summary;
                        }

                        continue;
                    }

                    for (int j = 0; j < columnCount; j++)
                    {
                        TableInfo.TableColumn col = VisibleColumns[j];

                        if (col == null)
                            continue;

                        //
                        //Column 1 is always a Primary Column
                        //
                        if (col.IsPrimaryColumn && j != 0)
                        {
                            if (!listOfEmptyRows.Contains(i))
                            {
                                if (recordIndex < this.Grid.DataSource.Count)
                                {
                                    if (!Grid.IsColumnAlterationInProgress && r != null)
                                        this.allRecords.Add(r);
                                }

                                if (!endOfRecordsFound)
                                    recordIndex++;

                                if (recordIndex >= this.Grid.DataSource.Count)
                                {
                                    endOfRecordsFound = true;
                                }
                                else
                                {
                                    if (!Grid.IsColumnAlterationInProgress && Grid.DataSource != null)
                                        r = new Record(recordIndex, Grid.DataSource[recordIndex], this);
                                    else
                                        r = this.allRecords[recordIndex] as Record;
                                }
                            }
                        }

                        if (col.IsPrimaryColumn)
                        {
                            r.ColumnIndex = j;
                        }
                        cellMatrix[i, j].Column = col;

                        if (cellMatrix[i, j].CellModelType != CellType.Summary)
                        {
                            if (col.QueryStyle)
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                            }
                            else
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle;
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle;
                            }
                        }
                        else
                        {
                            cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                            cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                        }

                        cellMatrix[i, j].Type = col.Type;
                        col.ColumnStyle.CellValueType = col.Type;

                        col.ColumnStyle.CellValueType = VisibleColumns[j].Type;

                        cellMatrix[i, j].Style.CellValueType = col.Type;
                        cellMatrix[i, j].OriginalStyle.CellValueType = col.Type;
                        cellMatrix[i, j].IsEmpty = true;
                        cellMatrix[i, j].TextInt = 0;
                        cellMatrix[i, j].TextDouble = 0;
                        cellMatrix[i, j].TextLong = 0;
                        cellMatrix[i, j].TextString = "";
                        cellMatrix[i, j].RowIndex = i;
                        cellMatrix[i, j].ColIndex = j;
                        cellMatrix[i, j].IsDirty = false;
                        cellMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                        cellMatrix[i, j].CellModelType = col.CellModelType;
                        cellMatrix[i, j].DrawText = true;

                        if (!endOfRecordsFound && !listOfEmptyRows.Contains(i))
                        {
                            cellMatrix[i, j].SourceIndex = recordIndex;

                            cellMatrix[i, j].Record = r;
                            if (col.CurrentPosition <= 0)
                                col.CurrentPosition = j;

                            int allColumnsIndex = Columns.IndexOf(col);

                            if (allColumnsIndex >= 0 && Columns[allColumnsIndex].CurrentPosition <= 0)
                                Columns[allColumnsIndex].CurrentPosition = j;

                            if (i > rowCount - NumberOfSummaryRows - 1)
                                cellMatrix[i, j].CellModelType = CellType.Summary;

                            if (!columnDefaultStyles.ContainsKey(col.Name))
                            {
                                GridStyleInfo styleNew = new GridStyleInfo();
                                ObjectCloneHelper.Clone<GridStyleInfo>(ref styleNew, col.ColumnStyle);
                                columnDefaultStyles.Add(col.Name, styleNew);
                            }

                        }
                        else
                            cellMatrix[i, j].SourceIndex = -1;
                    }

                    if (recordIndex < this.Grid.DataSource.Count)
                    {
                        if (!Grid.IsColumnAlterationInProgress && r != null)
                            this.allRecords.Add(r);
                    }

                    if (!endOfRecordsFound)
                        recordIndex++;
                }

                if (comp == null)
                    comp = new RecordDataComparer(new SortColumnComparer(this));

                if (isSortingEnabled)
                {
                    foreach (SortColumnDescriptor col in SortedColumnDescriptors)
                    {
                        UpdateRecordSortData(col);

                        string id = col.Id;
                        bool isSameValue = false;

                        if (!string.IsNullOrEmpty(id))
                        {
                            string[] arr = id.Split(':');
                            int currentPosition = Convert.ToInt32(arr[1]);

                            if (arr.Length > 1)
                            {
                                for (int i = this.VisibleColumns.Count - 1; i >= 0; i--)
                                {
                                    if (this.VisibleColumns[i].IsPrimaryColumn && (i >= currentPosition || (col.Id == id && !isSameValue)))
                                    {
                                        col.Id = arr[0] + ":" + i;
                                        if (col.Id == id)
                                            isSameValue = true;
                                    }
                                }
                            }
                        }
                    }

                    SortSourceList();
                }

                if (rowCount > 0 && columnCount > 0)
                    currentCell = cellMatrix[0, 0];

                if (Grid.GridType == GridType.DataBound)
                    this.rowCount = rowCount;
                else
                {
                    if (Grid.GridType == GridType.MultiColumn)
                    {
                        if (allRows >= (Grid as GridGroupingControlMC).MinimumRowCount)
                            this.rowCount = allRows;
                        else
                            this.rowCount = (Grid as GridGroupingControlMC).MinimumRowCount;
                    }
                }
                this.sourceDataRowCount = recordIndex;
                this.selectedRecordIndexes.Clear();
                this.SelectedRecords.Clear();
                this.lastSelectedRow = -1;
                this.firstSelectedRow = -1;
                LastRecordIndex = allRows;
                currentRow = 0;
                currentCol = 0;
                this.TopRow = 0;

                originalRecords.Clear();

                for (int i = 0; i < allRecords.Count; i++)
                {
                    this.originalRecords.Add(allRecords[i]);
                }

                //
                //Remove unwanted rows
                //
                if (cellMatrix.GetLength(0) > LastRecordIndex && cellMatrix.GetLength(0) > (Grid as GridGroupingControlMC).MinimumRowCount)
                {
                    TableInfo.CellStruct[,] cellMatrixNew = new TableInfo.CellStruct[this.rowCount, columnCount];

                    for (int i = 0; i < cellMatrixNew.GetLength(0); i++)
                    {
                        for (int j = 0; j < cellMatrixNew.GetLength(1); j++)
                        {
                            cellMatrixNew[i, j] = cellMatrix[i, j];
                        }
                    }

                    cellMatrix = cellMatrixNew;
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                OnGridRecreated();
            }
        }

        public void RestoreMatrixForMultiColumnGrid(TableInfo.CellStruct[,] matrix, List<string> recordKeys)
        {
            try
            {
                cellMatrix = new CellStruct[matrix.GetLength(0), matrix.GetLength(1)];

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    for (int j = 0; j < matrix.GetLength(1); j++)
                    {
                        cellMatrix[i, j] = matrix[i, j];
                    }
                }

                if (this.Grid.DataSource == null)
                    return;

                int recordIndex = 0;
                int allRows = 0;
                int rowCount = matrix.GetLength(0);
                int columnCount = matrix.GetLength(1);

                Dictionary<string, string> locations = new Dictionary<string, string>();
                string[] keys = new string[3];
                string countingKey = string.Empty;

                if (recordKeys != null)
                {
                    for (int i = 0; i < recordKeys.Count; i++)
                    {
                        keys = recordKeys[i].Split(':');
                        locations.Add(keys[0] + ":" + keys[1], keys[2]);
                    }
                }

                this.allRecords.Clear();

                for (int i = 0; i < this.Grid.DataSource.Count; i++)
                {
                    Record r = new Record(recordIndex, Grid.DataSource[i], this);
                    this.allRecords.Add(r);
                }

                originalRecords.Clear();

                for (int i = 0; i < allRecords.Count; i++)
                {
                    this.originalRecords.Add(allRecords[i]);
                }

                for (int i = 0; i < rowCount; i++)
                {
                    //if (i < (Grid as GridGroupingControlMC).MinimumRowCount)
                    {
                        for (int j = 0; j < columnCount; j++)
                        {
                            if (cellMatrix[i, j].SourceIndex < this.Grid.DataSource.Count)
                            {
                                TableInfo.TableColumn col = VisibleColumns[j];

                                if (col == null)
                                    continue;

                                cellMatrix[i, j].Column = col;

                                if (cellMatrix[i, j].CellModelType != CellType.Summary)
                                {
                                    if (col.QueryStyle)
                                    {
                                        cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                        cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                                    }
                                    else
                                    {
                                        cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle;
                                        cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle;
                                    }
                                }
                                else
                                {
                                    cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                                    cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                                }

                                cellMatrix[i, j].Type = col.Type;

                                cellMatrix[i, j].Style.CellValueType = col.Type;
                                cellMatrix[i, j].OriginalStyle.CellValueType = col.Type;
                                cellMatrix[i, j].IsEmpty = true;
                                cellMatrix[i, j].TextInt = 0;
                                cellMatrix[i, j].TextDouble = 0;
                                cellMatrix[i, j].TextLong = 0;
                                cellMatrix[i, j].TextString = "";
                                cellMatrix[i, j].IsDirty = false;
                                cellMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                                cellMatrix[i, j].CellModelType = col.CellModelType;
                                cellMatrix[i, j].DrawText = true;

                                if (cellMatrix[i, j].SourceIndex > 0)
                                {
                                    cellMatrix[i, j].Record = allRecords[cellMatrix[i, j].SourceIndex] as Record;

                                    if (recordIndex < cellMatrix[i, j].SourceIndex)
                                        recordIndex = cellMatrix[i, j].SourceIndex;

                                    if (allRows < i)
                                        allRows = i;

                                    if (VisibleColumns[j].IsPrimaryColumn)
                                        (allRecords[cellMatrix[i, j].SourceIndex] as Record).ColumnIndex = j;
                                }
                                else
                                {
                                    cellMatrix[i, j].ColIndex = j;
                                    keys = cellMatrix[i, j].Key.Split(':');
                                    countingKey = keys[0] + ":" + keys[1];

                                    if (recordKeys != null && locations.Keys.Contains(countingKey))
                                    {
                                        cellMatrix[i, j].SourceIndex = Convert.ToInt32(locations[countingKey]);
                                        cellMatrix[i, j].Record = allRecords[cellMatrix[i, j].SourceIndex] as Record;

                                        if (recordIndex < cellMatrix[i, j].SourceIndex)
                                            recordIndex = cellMatrix[i, j].SourceIndex;

                                        if (allRows < i)
                                            allRows = i;

                                        if (VisibleColumns[j].IsPrimaryColumn)
                                            (allRecords[cellMatrix[i, j].SourceIndex] as Record).ColumnIndex = j;
                                    }
                                }
                            }
                            else
                            {
                                TableInfo.TableColumn col = VisibleColumns[j];

                                if (col == null)
                                    continue;

                                cellMatrix[i, j].Column = col;

                                if (cellMatrix[i, j].CellModelType != CellType.Summary)
                                {
                                    if (col.QueryStyle)
                                    {
                                        cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                        cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                                    }
                                    else
                                    {
                                        cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle;
                                        cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle;
                                    }
                                }
                                else
                                {
                                    cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                                    cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                                }

                                cellMatrix[i, j].Type = col.Type;

                                cellMatrix[i, j].Style.CellValueType = col.Type;
                                cellMatrix[i, j].OriginalStyle.CellValueType = col.Type;
                                cellMatrix[i, j].IsEmpty = true;
                                cellMatrix[i, j].TextInt = 0;
                                cellMatrix[i, j].TextDouble = 0;
                                cellMatrix[i, j].TextLong = 0;
                                cellMatrix[i, j].TextString = "";
                                cellMatrix[i, j].IsDirty = false;
                                cellMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                                cellMatrix[i, j].CellModelType = col.CellModelType;
                                cellMatrix[i, j].DrawText = true;
                                cellMatrix[i, j].SourceIndex = -1;
                                cellMatrix[i, j].Record = null;
                            }
                        }
                    }
                }

                if (comp == null)
                    comp = new RecordDataComparer(new SortColumnComparer(this));

                if (isSortingEnabled)
                {
                    foreach (SortColumnDescriptor col in SortedColumnDescriptors)
                    {
                        UpdateRecordSortData(col);
                    }

                    SortSourceList();
                }

                if (rowCount > 0 && columnCount > 0)
                    currentCell = cellMatrix[0, 0];

                this.rowCount = cellMatrix.GetLength(0);
                this.sourceDataRowCount = allRecords.Count;
                this.selectedRecordIndexes.Clear();
                this.SelectedRecords.Clear();
                this.lastSelectedRow = -1;
                this.firstSelectedRow = -1;
                LastRecordIndex = allRows;
                currentRow = 0;
                currentCol = 0;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void DeleteColumnFromMatrix(int colIndex)
        {
            try
            {
                bool isDeletingPrimaryColumn = VisibleColumns[colIndex].IsPrimaryColumn;

                int RowCount = CellMatrix.GetLength(0);

                if (isDeletingPrimaryColumn)
                {
                    for (int i = 0; i < RowCount; i++)
                    {
                        Grid.RemoveRecordFromMatrix(i, colIndex);
                    }
                }

                DeleteColumn(colIndex);

                int ColCount = CellMatrix.GetLength(1);

                CellStruct[,] CellMatrixNew = new CellStruct[RowCount, ColCount - 1];
                bool isCellFound = false;

                for (int j = ColCount - 1; j >= 0; j--)
                {
                    if (!isCellFound)
                    {
                        if (colIndex != j)
                        {
                            for (int i = 0; i < RowCount; i++)
                            {
                                CellMatrixNew[i, j - 1] = new CellStruct(CellMatrix[i, j]);
                                CellMatrixNew[i, j - 1].ColIndex = CellMatrixNew[i, j - 1].ColIndex - 1;
                                CellMatrixNew[i, j - 1].Column.CurrentPosition = CellMatrixNew[i, j - 1].Column.CurrentPosition - 1;
                                CellMatrixNew[i, j - 1].Column = VisibleColumns[j - 1];
                            }
                        }
                        else
                        {
                            isCellFound = true;

                            if (CellMatrixNew.GetLength(1) <= j)
                                continue;
                            for (int i = 0; i < RowCount; i++)
                            {
                                CellMatrixNew[i, j] = new CellStruct(CellMatrix[i, j]);

                                CellMatrixNew[i, j].Column = VisibleColumns[j];
                                TableColumn col = CellMatrixNew[i, j].Column;

                                CellMatrixNew[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                CellMatrixNew[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();

                                CellMatrixNew[i, j].Type = col.Type;
                                col.ColumnStyle.CellValueType = col.Type;

                                col.ColumnStyle.CellValueType = VisibleColumns[j].Type;

                                CellMatrixNew[i, j].Style.CellValueType = col.Type;
                                CellMatrixNew[i, j].OriginalStyle.CellValueType = col.Type;
                                CellMatrixNew[i, j].IsEmpty = true;
                                CellMatrixNew[i, j].TextInt = 0;
                                CellMatrixNew[i, j].TextDouble = 0;
                                CellMatrixNew[i, j].TextLong = 0;
                                CellMatrixNew[i, j].TextString = "";
                                CellMatrixNew[i, j].RowIndex = i;
                                CellMatrixNew[i, j].ColIndex = j;
                                CellMatrixNew[i, j].IsDirty = false;
                                CellMatrixNew[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                                CellMatrixNew[i, j].CellModelType = col.CellModelType;
                                CellMatrixNew[i, j].DrawText = true;

                                if (!col.IsPrimaryColumn)
                                {
                                    CellMatrixNew[i, j].SourceIndex = CellMatrix[i, j - 1].SourceIndex;
                                    CellMatrixNew[i, j].Record = CellMatrix[i, j - 1].Record;
                                }
                                else
                                {
                                    CellMatrixNew[i, j].SourceIndex = CellMatrix[i, j + 1].SourceIndex;
                                    CellMatrixNew[i, j].Record = CellMatrix[i, j + 1].Record;
                                }

                                if (col.CurrentPosition <= 0)
                                    col.CurrentPosition = j;

                                int allColumnsIndex = Columns.IndexOf(col);

                                if (allColumnsIndex >= 0 && Columns[allColumnsIndex].CurrentPosition <= 0)
                                    Columns[allColumnsIndex].CurrentPosition = j;
                            }

                        }
                    }
                    else
                    {
                        for (int i = 0; i < RowCount; i++)
                        {
                            CellMatrixNew[i, j] = new CellStruct(CellMatrix[i, j]);

                            CellMatrixNew[i, j].Column = VisibleColumns[j];
                            TableColumn col = CellMatrixNew[i, j].Column;

                            CellMatrixNew[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                            CellMatrixNew[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();

                            CellMatrixNew[i, j].Type = col.Type;
                            col.ColumnStyle.CellValueType = col.Type;

                            col.ColumnStyle.CellValueType = VisibleColumns[j].Type;

                            CellMatrixNew[i, j].Style.CellValueType = col.Type;
                            CellMatrixNew[i, j].OriginalStyle.CellValueType = col.Type;
                            CellMatrixNew[i, j].IsEmpty = true;
                            CellMatrixNew[i, j].TextInt = 0;
                            CellMatrixNew[i, j].TextDouble = 0;
                            CellMatrixNew[i, j].TextLong = 0;
                            CellMatrixNew[i, j].TextString = "";
                            CellMatrixNew[i, j].RowIndex = i;
                            CellMatrixNew[i, j].ColIndex = j;
                            CellMatrixNew[i, j].IsDirty = false;
                            CellMatrixNew[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                            CellMatrixNew[i, j].CellModelType = col.CellModelType;
                            CellMatrixNew[i, j].DrawText = true;

                            CellMatrixNew[i, j].SourceIndex = CellMatrix[i, j].SourceIndex;

                            CellMatrixNew[i, j].Record = CellMatrix[i, j].Record;

                            if (col.CurrentPosition <= 0)
                                col.CurrentPosition = j;

                            int allColumnsIndex = Columns.IndexOf(col);

                            if (allColumnsIndex >= 0 && Columns[allColumnsIndex].CurrentPosition <= 0)
                                Columns[allColumnsIndex].CurrentPosition = j;
                        }
                        //Columns[j] = Columns[j];
                    }
                }

                CellMatrix = CellMatrixNew;
                currentCell = cellMatrix[0, 0];
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                Grid.Invalidate();
            }
        }

        public void InsertColumnToMatrix(TableInfo.TableColumn newColumn, int colIndex, bool setRecords)
        {
            try
            {
                int oldIndex = -1;

                if (this.VisibleColumns.Contains(newColumn))
                    oldIndex = this.VisibleColumns.IndexOf(newColumn);

                if (colIndex == 0 || VisibleColumns.Count == 1)
                    VisibleColumns.Add(newColumn);
                else
                    VisibleColumns.Insert(colIndex, newColumn);

                int RowCount = CellMatrix.GetLength(0);
                int ColCount = CellMatrix.GetLength(1);
                bool isMovingPrimaryColumn = newColumn.IsPrimaryColumn;

                CellStruct[,] CellMatrixNew = new CellStruct[RowCount, ColCount + 1];
                bool isCellFound = false;

                for (int j = ColCount; j >= 0; j--)
                {
                    if (!isCellFound)
                    {
                        if (colIndex != j)
                        {
                            for (int i = 0; i < RowCount; i++)
                            {
                                CellMatrixNew[i, j] = new CellStruct(CellMatrix[i, j - 1]);
                                CellMatrixNew[i, j].ColIndex = CellMatrixNew[i, j].ColIndex + 1;
                                CellMatrixNew[i, j].Column.CurrentPosition = CellMatrixNew[i, j].Column.CurrentPosition + 1;
                                CellMatrixNew[i, j].Column = VisibleColumns[j];

                                if (colIndex == 0 && j == 1)
                                {
                                    CellMatrixNew[i, j].Column = newColumn;
                                    CellMatrixNew[i, j].Type = newColumn.Type;

                                    CellMatrixNew[i, j].Style.CellValueType = newColumn.Type;
                                    CellMatrixNew[i, j].OriginalStyle.CellValueType = newColumn.Type;
                                    CellMatrixNew[i, j].IsEmpty = true;
                                    CellMatrixNew[i, j].TextInt = 0;
                                    CellMatrixNew[i, j].TextDouble = 0;
                                    CellMatrixNew[i, j].TextLong = 0;
                                    CellMatrixNew[i, j].TextString = "";
                                    CellMatrixNew[i, j].RowIndex = i;
                                    CellMatrixNew[i, j].ColIndex = j;
                                    CellMatrixNew[i, j].IsDirty = false;
                                    CellMatrixNew[i, j].IsPushButton = newColumn.CellModelType == CellType.PushButton;
                                    CellMatrixNew[i, j].CellModelType = newColumn.CellModelType;
                                    CellMatrixNew[i, j].DrawText = true;
                                }
                            }
                        }
                        else
                        {
                            isCellFound = true;

                            if (newColumn != null)
                            {
                                for (int i = 0; i < RowCount; i++)
                                {
                                    CellMatrixNew[i, j].Column = newColumn;
                                    TableColumn col = CellMatrixNew[i, j].Column;

                                    if (string.IsNullOrEmpty(newColumn.Name))
                                    {
                                        CellMatrixNew[i, j].Style = (GridStyleInfo)this.TableStyle.Clone();
                                        CellMatrixNew[i, j].OriginalStyle = (GridStyleInfo)this.TableStyle.Clone();
                                    }
                                    else
                                    {
                                        CellMatrixNew[i, j].Style = (GridStyleInfo)newColumn.ColumnStyle.Clone();
                                        CellMatrixNew[i, j].OriginalStyle = (GridStyleInfo)newColumn.ColumnStyle.Clone();
                                    }

                                    CellMatrixNew[i, j].Type = col.Type;
                                    col.ColumnStyle.CellValueType = col.Type;

                                    col.ColumnStyle.CellValueType = col.Type;

                                    CellMatrixNew[i, j].Style.CellValueType = col.Type;
                                    CellMatrixNew[i, j].OriginalStyle.CellValueType = col.Type;
                                    CellMatrixNew[i, j].IsEmpty = true;
                                    CellMatrixNew[i, j].TextInt = 0;
                                    CellMatrixNew[i, j].TextDouble = 0;
                                    CellMatrixNew[i, j].TextLong = 0;
                                    CellMatrixNew[i, j].TextString = "";
                                    CellMatrixNew[i, j].RowIndex = i;
                                    CellMatrixNew[i, j].ColIndex = j;
                                    CellMatrixNew[i, j].IsDirty = false;
                                    CellMatrixNew[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                                    CellMatrixNew[i, j].CellModelType = col.CellModelType;
                                    CellMatrixNew[i, j].DrawText = true;

                                    if (setRecords)
                                    {
                                        if (j > 0)
                                        {
                                            CellMatrixNew[i, j].SourceIndex = CellMatrix[i, j - 1].SourceIndex;
                                            CellMatrixNew[i, j].Record = CellMatrix[i, j - 1].Record;
                                        }
                                        else
                                        {
                                            CellMatrixNew[i, j].SourceIndex = CellMatrix[i, j].SourceIndex;
                                            CellMatrixNew[i, j].Record = CellMatrix[i, j].Record;
                                        }
                                    }
                                    else
                                    {
                                        CellMatrixNew[i, j].SourceIndex = -1;
                                        CellMatrixNew[i, j].Record = null;
                                    }

                                    if (col.CurrentPosition <= 0)
                                        col.CurrentPosition = j;

                                    int allColumnsIndex = Columns.IndexOf(col);

                                    if (allColumnsIndex >= 0 && Columns[allColumnsIndex].CurrentPosition <= 0)
                                        Columns[allColumnsIndex].CurrentPosition = j;

                                    if (!columnDefaultStyles.ContainsKey(col.Name))
                                    {
                                        GridStyleInfo styleNew = new GridStyleInfo();
                                        ObjectCloneHelper.Clone<GridStyleInfo>(ref styleNew, col.ColumnStyle);
                                        columnDefaultStyles.Add(col.Name, styleNew);
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 0; i < RowCount; i++)
                                {
                                    CellMatrixNew[i, j].Column = VisibleColumns[j];
                                    TableColumn col = CellMatrixNew[i, j].Column;
                                    col.ColumnStyle = VisibleColumns[0].ColumnStyle.Clone() as GridStyleInfo;

                                    CellMatrixNew[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                    CellMatrixNew[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();


                                    CellMatrixNew[i, j].Type = col.Type;
                                    col.ColumnStyle.CellValueType = col.Type;

                                    col.ColumnStyle.CellValueType = VisibleColumns[j].Type;

                                    CellMatrixNew[i, j].Style.CellValueType = col.Type;
                                    CellMatrixNew[i, j].OriginalStyle.CellValueType = col.Type;
                                    CellMatrixNew[i, j].IsEmpty = true;
                                    CellMatrixNew[i, j].TextInt = 0;
                                    CellMatrixNew[i, j].TextDouble = 0;
                                    CellMatrixNew[i, j].TextLong = 0;
                                    CellMatrixNew[i, j].TextString = "";
                                    CellMatrixNew[i, j].RowIndex = i;
                                    CellMatrixNew[i, j].ColIndex = j;
                                    CellMatrixNew[i, j].IsDirty = false;
                                    CellMatrixNew[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                                    CellMatrixNew[i, j].CellModelType = col.CellModelType;
                                    CellMatrixNew[i, j].DrawText = true;

                                    if (j > 0)
                                    {
                                        CellMatrixNew[i, j].SourceIndex = CellMatrix[i, j - 1].SourceIndex;
                                        CellMatrixNew[i, j].Record = CellMatrix[i, j - 1].Record;
                                    }
                                    else
                                    {
                                        CellMatrixNew[i, j].SourceIndex = CellMatrix[i, j].SourceIndex;
                                        CellMatrixNew[i, j].Record = CellMatrix[i, j].Record;
                                    }

                                    if (col.CurrentPosition <= 0)
                                        col.CurrentPosition = j;

                                    int allColumnsIndex = Columns.IndexOf(col);

                                    if (allColumnsIndex >= 0 && Columns[allColumnsIndex].CurrentPosition <= 0)
                                        Columns[allColumnsIndex].CurrentPosition = j;

                                    if (!columnDefaultStyles.ContainsKey(col.Name))
                                    {
                                        GridStyleInfo styleNew = new GridStyleInfo();
                                        ObjectCloneHelper.Clone<GridStyleInfo>(ref styleNew, col.ColumnStyle);
                                        columnDefaultStyles.Add(col.Name, styleNew);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < RowCount; i++)
                        {
                            CellMatrixNew[i, j] = new CellStruct(CellMatrix[i, j]);
                        }
                    }
                }

                CellMatrix = CellMatrixNew;
                currentCell = cellMatrix[0, 0];
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                Grid.Invalidate();
            }
        }

        internal void InsertRecordToMatrix(int rowIndex, int colIndex, int newSourceIndex, int previousSourceIndex, bool replaceExistingRecord)
        {
            try
            {
                Record r = null;
                List<int> recordsToRemove = new List<int>();

                if (newSourceIndex >= 0)
                {
                    if (replaceExistingRecord)
                    {
                        for (int i = 0; i < allRecords.Count; i++)
                        {
                            bool isFound = false;

                            foreach (object item in Grid.DataSource)
                            {
                                if (item == (allRecords[i] as Record).ObjectBound)
                                {
                                    isFound = true;
                                    break;
                                }
                            }

                            if (!isFound)
                            {
                                recordsToRemove.Add(i);
                                break;
                            }

                            //if ((allRecords[i] as Record).SourceIndex == sourceIndex)
                            //{
                            //    recordsToRemove.Add(i);
                            //    break;
                            //}
                        }

                        for (int i = 0; i < recordsToRemove.Count; i++)
                        {
                            this.allRecords.RemoveAt(recordsToRemove[i]);
                        }
                    }

                    if (r == null)
                    {
                        r = new Record(previousSourceIndex, Grid.DataSource[newSourceIndex], this);

                        this.allRecords.Insert(previousSourceIndex, r);
                    }
                }
                else
                {
                    newSourceIndex = sourceDataRowCount;
                    previousSourceIndex = newSourceIndex;
                    if (r == null)
                    {
                        r = new Record(newSourceIndex, Grid.DataSource[newSourceIndex], this);
                        this.allRecords.Add(r);
                        sourceDataRowCount++;

                        if (rowIndex > LastRecordIndex)
                            LastRecordIndex = rowIndex;
                    }
                }

                for (int j = colIndex; j < this.VisibleColumns.Count; j++)
                {
                    TableColumn col = this.VisibleColumns[j];

                    if (col.IsPrimaryColumn && j != colIndex)
                        break;

                    cellMatrix[rowIndex, j].SourceIndex = previousSourceIndex;
                    cellMatrix[rowIndex, j].Record = r;
                    cellMatrix[rowIndex, j].IsDirty = true;
                }

                originalRecords.Clear();

                for (int i = 0; i < allRecords.Count; i++)
                {
                    this.originalRecords.Add(allRecords[i]);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        internal void CreateMultiColumnMatrixFromFilteredRecords(int columnCount)
        {
            try
            {
                int rowCount = filteredRecords.Count;

                if (AllowSummaryRows && rowCount > 0)
                {
                    rowCount += NumberOfSummaryRows;
                }

                rowCount += listOfEmptyRows.Count;

                if (this.Grid.DataSource == null)
                    return;

                if (rowCount < (Grid as GridGroupingControlMC).MinimumRowCount)
                    rowCount = (Grid as GridGroupingControlMC).MinimumRowCount;

                cellMatrix = new TableInfo.CellStruct[rowCount, columnCount];

                if (AllRecords.Count <= 0)
                    CreateMatrix(Grid.DataSource.Count, columnCount);

                int recordIndex = 0;
                int allRows = 0;
                bool endOfRecordsFound = false;

                for (int i = 0; i < rowCount; i++)
                {
                    Record r = null;

                    if (recordIndex >= this.filteredRecords.Count)
                    {
                        if (i < (Grid as GridGroupingControlMC).MinimumRowCount)
                        {
                            for (int j = 0; j < columnCount; j++)
                            {
                                TableInfo.TableColumn col = VisibleColumns[j];

                                if (col == null)
                                    continue;

                                cellMatrix[i, j].Column = col;

                                if (cellMatrix[i, j].CellModelType != CellType.Summary)
                                {
                                    if (col.QueryStyle)
                                    {
                                        cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                        cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                                    }
                                    else
                                    {
                                        cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle;
                                        cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle;
                                    }
                                }
                                else
                                {
                                    cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                                    cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                                }

                                cellMatrix[i, j].Type = col.Type;
                                col.ColumnStyle.CellValueType = col.Type;

                                col.ColumnStyle.CellValueType = VisibleColumns[j].Type;

                                cellMatrix[i, j].Style.CellValueType = col.Type;
                                cellMatrix[i, j].OriginalStyle.CellValueType = col.Type;
                                cellMatrix[i, j].IsEmpty = true;
                                cellMatrix[i, j].TextInt = 0;
                                cellMatrix[i, j].TextDouble = 0;
                                cellMatrix[i, j].TextLong = 0;
                                cellMatrix[i, j].TextString = "";
                                cellMatrix[i, j].RowIndex = i;
                                cellMatrix[i, j].ColIndex = j;
                                cellMatrix[i, j].IsDirty = false;
                                cellMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                                cellMatrix[i, j].CellModelType = col.CellModelType;
                                cellMatrix[i, j].DrawText = true;

                                //ashan
                                cellMatrix[i, j].SourceIndex = -1;

                                if (col.CurrentPosition <= 0)
                                    col.CurrentPosition = j;

                                int allColumnsIndex = Columns.IndexOf(col);

                                if (allColumnsIndex >= 0 && Columns[allColumnsIndex].CurrentPosition <= 0)
                                    Columns[allColumnsIndex].CurrentPosition = j;

                                if (i > rowCount - NumberOfSummaryRows - 1)
                                    cellMatrix[i, j].CellModelType = CellType.Summary;
                            }
                        }
                        continue;
                    }

                    allRows++;

                    if (!listOfEmptyRows.Contains(i))
                    {
                        if (recordIndex < this.filteredRecords.Count)
                        {
                            r = this.filteredRecords[recordIndex] as Record;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < columnCount; j++)
                        {
                            TableInfo.TableColumn col = VisibleColumns[j];

                            if (col == null)
                                continue;


                            cellMatrix[i, j].Column = col;

                            if (cellMatrix[i, j].CellModelType != CellType.Summary)
                            {
                                if (col.QueryStyle)
                                {
                                    cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                    cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                                }
                                else
                                {
                                    cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle;
                                    cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle;
                                }
                            }
                            else
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                            }

                            cellMatrix[i, j].Type = col.Type;
                            col.ColumnStyle.CellValueType = col.Type;

                            col.ColumnStyle.CellValueType = VisibleColumns[j].Type;

                            cellMatrix[i, j].Style.CellValueType = col.Type;
                            cellMatrix[i, j].OriginalStyle.CellValueType = col.Type;
                            cellMatrix[i, j].IsEmpty = true;
                            cellMatrix[i, j].TextInt = 0;
                            cellMatrix[i, j].TextDouble = 0;
                            cellMatrix[i, j].TextLong = 0;
                            cellMatrix[i, j].TextString = "";
                            cellMatrix[i, j].RowIndex = i;
                            cellMatrix[i, j].ColIndex = j;
                            cellMatrix[i, j].IsDirty = false;
                            cellMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                            cellMatrix[i, j].CellModelType = col.CellModelType;
                            cellMatrix[i, j].DrawText = true;

                            cellMatrix[i, j].SourceIndex = -1;

                            if (col.CurrentPosition <= 0)
                                col.CurrentPosition = j;

                            int allColumnsIndex = Columns.IndexOf(col);

                            if (allColumnsIndex >= 0 && Columns[allColumnsIndex].CurrentPosition <= 0)
                                Columns[allColumnsIndex].CurrentPosition = j;

                            if (i > rowCount - NumberOfSummaryRows - 1)
                                cellMatrix[i, j].CellModelType = CellType.Summary;
                        }

                        continue;
                    }

                    for (int j = 0; j < columnCount; j++)
                    {
                        TableInfo.TableColumn col = VisibleColumns[j];

                        if (col == null)
                            continue;

                        //
                        //Column 1 is always a Primary Column
                        //
                        if (col.IsPrimaryColumn && j != 0)
                        {
                            if (!listOfEmptyRows.Contains(i))
                            {
                                if (!endOfRecordsFound)
                                    recordIndex++;

                                if (recordIndex >= this.filteredRecords.Count)
                                {
                                    endOfRecordsFound = true;
                                }
                                else
                                {
                                    r = this.filteredRecords[recordIndex] as Record;
                                }
                            }
                        }

                        cellMatrix[i, j].Column = col;

                        if (cellMatrix[i, j].CellModelType != CellType.Summary)
                        {
                            if (col.QueryStyle)
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                            }
                            else
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle;
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle;
                            }
                        }
                        else
                        {
                            cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                            cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                        }

                        cellMatrix[i, j].Type = col.Type;
                        col.ColumnStyle.CellValueType = col.Type;

                        col.ColumnStyle.CellValueType = VisibleColumns[j].Type;

                        cellMatrix[i, j].Style.CellValueType = col.Type;
                        cellMatrix[i, j].OriginalStyle.CellValueType = col.Type;
                        cellMatrix[i, j].IsEmpty = true;
                        cellMatrix[i, j].TextInt = 0;
                        cellMatrix[i, j].TextDouble = 0;
                        cellMatrix[i, j].TextLong = 0;
                        cellMatrix[i, j].TextString = "";
                        cellMatrix[i, j].RowIndex = i;
                        cellMatrix[i, j].ColIndex = j;
                        cellMatrix[i, j].IsDirty = false;
                        cellMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                        cellMatrix[i, j].CellModelType = col.CellModelType;
                        cellMatrix[i, j].DrawText = true;

                        if (!endOfRecordsFound && !listOfEmptyRows.Contains(i))
                        {
                            cellMatrix[i, j].SourceIndex = recordIndex;

                            cellMatrix[i, j].Record = r;
                            cellMatrix[i, j].Record.CurrentIndex = i;
                            cellMatrix[i, j].Record.ColumnIndex = j;

                            if (col.CurrentPosition <= 0)
                                col.CurrentPosition = j;

                            int allColumnsIndex = VisibleColumns.IndexOf(col);

                            if (allColumnsIndex >= 0 && VisibleColumns[allColumnsIndex].CurrentPosition <= 0)
                                VisibleColumns[allColumnsIndex].CurrentPosition = j;

                            if (i > rowCount - NumberOfSummaryRows - 1)
                                cellMatrix[i, j].CellModelType = CellType.Summary;

                            if (!columnDefaultStyles.ContainsKey(col.Name))
                            {
                                GridStyleInfo styleNew = new GridStyleInfo();
                                ObjectCloneHelper.Clone<GridStyleInfo>(ref styleNew, col.ColumnStyle);
                                columnDefaultStyles.Add(col.Name, styleNew);
                            }

                        }
                        //ashan
                        else
                            cellMatrix[i, j].SourceIndex = -1;
                    }

                    if (!endOfRecordsFound)
                        recordIndex++;
                }

                if (comp == null)
                    comp = new RecordDataComparer(new SortColumnComparer(this));

                if (isSortingEnabled)
                {
                    foreach (SortColumnDescriptor col in SortedColumnDescriptors)
                    {
                        UpdateRecordSortData(col);

                        string id = col.Id;
                        bool isSameValue = false;

                        if (!string.IsNullOrEmpty(id))
                        {
                            string[] arr = id.Split(':');
                            int currentPosition = Convert.ToInt32(arr[1]);

                            if (arr.Length > 1)
                            {
                                for (int i = this.VisibleColumns.Count - 1; i >= 0; i--)
                                {
                                    if (this.VisibleColumns[i].IsPrimaryColumn && (i >= currentPosition || (col.Id == id && !isSameValue)))
                                    {
                                        col.Id = arr[0] + ":" + i;
                                        if (col.Id == id)
                                            isSameValue = true;
                                    }
                                }
                            }
                        }
                    }

                    SortSourceList();
                }

                if (rowCount > 0 && columnCount > 0)
                    currentCell = cellMatrix[0, 0];

                if (Grid.GridType == GridType.MultiColumn)
                {
                    if (allRows >= (Grid as GridGroupingControlMC).MinimumRowCount)
                        this.rowCount = allRows;
                    else
                        this.rowCount = (Grid as GridGroupingControlMC).MinimumRowCount;
                }

                this.sourceDataRowCount = recordIndex;
                this.selectedRecordIndexes.Clear();
                this.SelectedRecords.Clear();
                this.lastSelectedRow = -1;
                this.firstSelectedRow = -1;
                LastRecordIndex = allRows;
                currentRow = 0;
                currentCol = 0;
                this.TopRow = 0;

                //originalRecords.Clear();

                //for (int i = 0; i < allRecords.Count; i++)
                //{
                //    this.originalRecords.Add(allRecords[i]);
                //}

                //
                //Remove unwanted rows
                //
                if (cellMatrix.GetLength(0) > LastRecordIndex && cellMatrix.GetLength(0) > (Grid as GridGroupingControlMC).MinimumRowCount)
                {
                    TableInfo.CellStruct[,] cellMatrixNew = new TableInfo.CellStruct[this.rowCount, columnCount];

                    for (int i = 0; i < cellMatrixNew.GetLength(0); i++)
                    {
                        for (int j = 0; j < cellMatrixNew.GetLength(1); j++)
                        {
                            cellMatrixNew[i, j] = cellMatrix[i, j];
                        }
                    }

                    cellMatrix = cellMatrixNew;
                }

                if (filteredRecords.Count > 0 && columnCount > 0)
                {
                    CurrentRow = 0;
                }

                this.TopRow = 0;
                this.selectedRecordIndexes.Clear();
                Grid.RecreateMatrixFlag = false;

                UpdateRecordSortKeys();

                //UpdateExpressionFields(columnCount);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                OnGridRecreated();
            }
        }

        internal void CreateMatrixFromFilteredRecords(int columnCount)
        {
            try
            {
                int rowCount = filteredRecords.Count;

                if (AllowSummaryRows && rowCount > 0)
                {
                    rowCount += NumberOfSummaryRows;
                }

                if (filteredRecords.Count > 0)
                    cellMatrix = new TableInfo.CellStruct[rowCount, columnCount];
                else
                    cellMatrix = new TableInfo.CellStruct[10, 20];

                if (AllRecords.Count <= 0)
                    CreateMatrix(Grid.DataSource.Count, columnCount);

                for (int i = 0; i < rowCount; i++)
                {
                    Record r = null;

                    if (AllowSummaryRows)
                    {
                        if (i <= rowCount - NumberOfSummaryRows - 1)
                        {
                            r = filteredRecords[i] as Record;
                        }
                    }
                    else
                    {
                        r = filteredRecords[i] as Record;
                    }

                    for (int j = 0; j < columnCount; j++)
                    {
                        cellMatrix[i, j].Column = VisibleColumns[j];

                        if (cellMatrix[i, j].CellModelType != CellType.Summary)
                        {
                            if (this.VisibleColumns[j].QueryStyle)
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                            }
                            else
                            {
                                cellMatrix[i, j].Style = (GridStyleInfo)VisibleColumns[j].ColumnStyle;
                                cellMatrix[i, j].OriginalStyle = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                            }
                        }
                        else
                        {
                            cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                            cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                        }

                        VisibleColumns[j].ColumnStyle.CellValueType = VisibleColumns[j].Type;

                        TableInfo.TableColumn col = GetColumnFromName(VisibleColumns[j].Name);

                        if (col == null)
                            continue;

                        col.ColumnStyle.CellValueType = VisibleColumns[j].Type;

                        cellMatrix[i, j].Type = VisibleColumns[j].Type;
                        cellMatrix[i, j].Style.CellValueType = VisibleColumns[j].Type;
                        cellMatrix[i, j].OriginalStyle.CellValueType = VisibleColumns[j].Type;
                        cellMatrix[i, j].IsEmpty = true;
                        cellMatrix[i, j].TextInt = 0;
                        cellMatrix[i, j].TextDouble = 0;
                        cellMatrix[i, j].TextLong = 0;
                        cellMatrix[i, j].TextString = "";
                        cellMatrix[i, j].RowIndex = i;
                        cellMatrix[i, j].ColIndex = j;
                        cellMatrix[i, j].IsDirty = false;
                        cellMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                        cellMatrix[i, j].CellModelType = VisibleColumns[j].CellModelType;
                        cellMatrix[i, j].DrawText = true;

                        cellMatrix[i, j].Record = r;
                        VisibleColumns[j].CurrentPosition = j;

                        int allColumnsIndex = Columns.IndexOf(col);

                        if (allColumnsIndex >= 0 && Columns[allColumnsIndex].CurrentPosition <= 0)
                            Columns[allColumnsIndex].CurrentPosition = j;

                        if (i > rowCount - NumberOfSummaryRows - 1)
                            cellMatrix[i, j].CellModelType = CellType.Summary;

                        if (!columnDefaultStyles.ContainsKey(col.Name))
                        {
                            GridStyleInfo styleNew = new GridStyleInfo();
                            ObjectCloneHelper.Clone<GridStyleInfo>(ref styleNew, VisibleColumns[j].ColumnStyle);
                            columnDefaultStyles.Add(col.Name, styleNew);
                        }
                    }
                }

                if (comp == null)
                    comp = new RecordDataComparer(new SortColumnComparer(this));

                if (isSortingEnabled)
                {
                    foreach (SortColumnDescriptor col in SortedColumnDescriptors)
                    {
                        UpdateRecordSortData(col);

                        for (int i = 0; i < this.VisibleColumns.Count; i++)
                        {
                            if (this.VisibleColumns[i].MappingName == col.Name)
                            {
                                col.Id = this.VisibleColumns[i].MappingName + ":" + i;// this.VisibleColumns[i].CurrentPosition;
                                break;
                            }
                        }
                    }

                    Grid.QueryUnboundData();

                    SortSourceList();
                }

                if (filteredRecords.Count > 0 && columnCount > 0)
                {
                    if (CurrentRow < filteredRecords.Count
                        && CurrentRow != 0
                        && IndexRow != null)
                    {
                        bool isContainsOldRecord = false;
                        Record selectedRecord = null;

                        foreach (var item in filteredRecords)
                        {
                            Record record = item as Record;
                            if (record.GetData() == IndexRow.GetData())
                            {
                                selectedRecord = record;
                                isContainsOldRecord = true;
                                break;
                            }
                        }

                        if (isContainsOldRecord)
                            CurrentRow = filteredRecords.IndexOf(selectedRecord);
                        else
                            CurrentRow = -1;
                    }
                    else
                    {
                        CurrentRow = -1;
                    }
                }

                this.rowCount = rowCount;//this.FilteredRecords.Count;
                this.LastRecordIndex = this.FilteredRecords.Count;
                //this.TopRow = 0;

                // Changed to prevent scrolling to top when refreshing the grid
                if (this.TopRow > rowCount)
                    this.TopRow = rowCount;

                this.selectedRecordIndexes.Clear();

                Grid.RecreateMatrixFlag = false;

                UpdateRecordSortKeys();

                //UpdateExpressionFields(columnCount);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                OnGridRecreated();
            }

        }

        #region Grouping Grid Implementation

        public void ExpandAllGroups()
        {
            collapseAll = false;
            Grid.MinimisedGroupList.Clear();

            foreach (var item in Grid.GroupHeaderRowIds)
            {
                item.Collapsed = false;
            }
        }

        public void CollapseAllGroups()
        {
            collapseAll = true;
            TopRow = 0;//Reset To First Row
        }

        public delegate string QueryGroup(Record e);
        public event QueryGroup QueryGroupingText;

        internal string GetQueryGroupText(Record e)
        {
            if (QueryGroupingText != null)
            {
                return QueryGroupingText(e);
            }

            return string.Empty;
        }

        internal void PopulateGroupHeaders()
        {
            Records collection = null;

            if (IsFilterEnabled)
                collection = filteredRecords;
            else
                collection = allRecords;

            try
            {
                this.Grid.GroupHeaderRowIds.Clear();

                if (collection.Count == 0)
                    return;

                if (this.Grid.IsGroupingEnabled() && collection.Count > 0)
                {
                    for (int i = 0; i < this.Grid.GroupByNameList.Count; i++)
                    {
                        int col = 0;
                        bool isQueryMode = false;

                        if (this.Grid.GroupByNameList[i].Name != "*")
                        {
                            if (Grid.Table.GetColumnFromName(this.Grid.GroupByNameList[i].Name) != null)
                            {
                                col = Grid.Table.Columns.IndexOf(Grid.Table.Columns[this.Grid.GroupByNameList[i].Name]);
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            isQueryMode = true;
                        }

                        var lastStringValue = "----";

                        TableInfo.GroupHeaderRowLevel foundObj = null;

                        for (int j = 0; j < collection.Count; j++)
                        {
                            if (CheckGroupHeaderAvailable(j))
                            {
                                lastStringValue = "----";
                            }

                            Record curItem = collection[j] as Record;
                            //var curStringValue = curItem.GetValue(this.Grid.GroupByNameList[i]).ToString();
                            //Grid.gettingValuesForGroupingHeader = false;

                            string curStringValue = string.Empty;

                            if (!isQueryMode)
                            {
                                var pAccess = GetOrCreatePropertyAccessor(this.Grid.GroupByNameList[i].Name, curItem.GetData().GetType());
                                curStringValue = pAccess.Get(curItem.GetData()).ToString();

                                //if (CellMatrix.GetLength(0) > j && cellMatrix.GetLength(1) > col)
                                //    curStringValue = Grid.GetValueAsString(CellMatrix[j, col], CellMatrix[j, col].CellStructType);
                            }
                            else
                            {
                                if (QueryGroupingText != null)
                                {
                                    curStringValue = QueryGroupingText(curItem);
                                }
                            }

                            if (string.IsNullOrWhiteSpace(curStringValue) && !isQueryMode)
                            {
                                //Matrixs out of ragne.
                                Grid.GettingValuesForGroupingHeader = true;
                                curStringValue = curItem.GetValue(this.Grid.GroupByNameList[i].Name).ToString();
                                Grid.GettingValuesForGroupingHeader = false;
                            }

                            if (!lastStringValue.Equals(curStringValue.ToString()))
                            {
                                lastStringValue = curStringValue.ToString();
                                var insertItem = new GroupHeaderRowLevel(j, i, curStringValue, curItem);

                                this.Grid.GroupHeaderRowIds.Add(insertItem);
                                foundObj = insertItem;
                                foundObj.EndRowId = j;

                                if (i > 0)
                                {
                                    foreach (var item in this.Grid.GroupHeaderRowIds)
                                    {
                                        if (item.RowLevel == i - 1)
                                        {
                                            if (item.RowId <= j && j <= item.EndRowId)
                                            {
                                                foundObj.ParentGroup = item;
                                            }
                                        }
                                    }
                                }

                                //foreach (var item in tempGroupHeaderRowIds)
                                //{
                                //    if (item.RowLevel == insertItem.RowLevel && item.TitleText == insertItem.TitleText)
                                //    {
                                //        insertItem.Collapsed = item.Collapsed;
                                //        insertItem.CollapsedForProsessing = item.CollapsedForProsessing;
                                //    }
                                //}

                                string groupTitleText = GetNestedStrinValue(insertItem);

                                if (!IsGroupParentAllExpanded(foundObj))
                                {
                                    insertItem.Collapsed = true;
                                }
                                else if (collapseAll || Grid.MinimisedGroupList.Contains(groupTitleText))
                                {
                                    insertItem.Collapsed = true;

                                    if (!Grid.MinimisedGroupList.Contains(groupTitleText))
                                    {
                                        Grid.MinimisedGroupList.Add(groupTitleText);
                                    }
                                }
                            }
                            else
                            {
                                if (foundObj != null)
                                    foundObj.EndRowId = j;
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
            }
            finally
            {
                Grid.GroupingHeaderFilterText = string.Empty;

                foreach (var item in this.Grid.GroupHeaderRowIds)
                {
                    Grid.GroupingHeaderFilterText += item.RowId + "|" + item.EndRowId + "|" + item.Direction + "|" + item.Collapsed + "|" + item.RowLevel + "|" + item.TitleText + "||";
                }
            }
        }

        #endregion

        internal bool CheckGroupHeaderAvailable(int j)
        {
            bool isFound = false;

            for (int i = 0; i < Grid.GroupHeaderRowIds.Count; i++)
            {
                if (this.Grid.GroupHeaderRowIds[i].RowId == j)
                {
                    isFound = true;
                    break;
                }
            }
            return isFound;
        }

        internal void AddRows(int count)
        {
            try
            {
                TableInfo.CellStruct[,] newArray = new TableInfo.CellStruct[rowCount, CellMatrix.GetLength(1)];

                int offset = rowCount - 1;

                for (int k = CellMatrix.GetLength(0) - 1; k >= 0; k--)
                {
                    for (int j = 0; j < CellMatrix.GetLength(1); j++)
                    {
                        if (CellMatrix[k, j].Column == null)
                        {
                            CellMatrix[k, j].Column = VisibleColumns[j];
                        }

                        if (k < newArray.GetLength(0))
                        {
                            newArray[offset, j] = CellMatrix[k, j];
                            newArray[offset, j].RowIndex = offset;
                        }
                    }

                    offset--;
                }

                int index = 0;// rowCount - count + 1;

                if (this.Grid.GridType == GridType.Virtual)
                {
                    for (int i = 0; i < count; i++)
                    {
                        TableInfo.CellStruct structure;

                        for (int j = 0; j < this.VisibleColumns.Count; j++)
                        {
                            TableInfo.TableColumn col = VisibleColumns[j];
                            if (index >= newArray.GetLength(0) || j >= newArray.GetLength(1))
                                continue;
                            structure = newArray[index, j];
                            structure.Column = col;

                            if (this.VisibleColumns[j].QueryStyle)
                            {
                                structure.Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                structure.OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                            }
                            else
                            {
                                structure.Style = (GridStyleInfo)col.ColumnStyle;
                                structure.OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                            }

                            structure.Type = col.Type;

                            structure.Style.CellValueType = col.Type;
                            structure.OriginalStyle.CellValueType = VisibleColumns[j].Type;

                            structure.IsEmpty = true;
                            structure.TextInt = 0;
                            structure.TextDouble = 0;
                            structure.TextLong = 0;
                            structure.TextString = "";
                            structure.IsDirty = false;
                            structure.RowIndex = index;
                            structure.ColIndex = j;
                            structure.DrawText = true;

                            structure.IsPushButton = VisibleColumns[j].CellModelType == CellType.PushButton;
                            structure.CellModelType = VisibleColumns[j].CellModelType;

                            newArray[index, j] = structure;
                        }
                        index++;
                    }
                }

                CellMatrix = newArray;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        internal void AddRow(int index)
        {
            try
            {
                TableInfo.CellStruct[,] newArray = new TableInfo.CellStruct[RowCount, CellMatrix.GetLength(1)];

                bool isAddingRowFound = false;

                for (int k = 0; k < CellMatrix.GetLength(0); k++)
                {
                    if (k == index)
                    {
                        isAddingRowFound = true;
                        continue;
                    }

                    for (int j = 0; j < CellMatrix.GetLength(1); j++)
                    {
                        if (!isAddingRowFound)
                            newArray[k, j] = CellMatrix[k, j];
                        else
                            newArray[k, j] = CellMatrix[k, j];
                    }
                }

                if (this.Grid.DataSource != null)
                {
                    Record r = new Record(RowCount - 1, Grid.DataSource[index], this);
                    TableInfo.CellStruct structure;

                    for (int j = 0; j < this.VisibleColumns.Count; j++)
                    {
                        structure = newArray[index, j];
                        structure.Column = VisibleColumns[j];

                        if (this.VisibleColumns[j].QueryStyle)
                        {
                            structure.Style = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                            structure.OriginalStyle = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                        }
                        else
                        {
                            structure.Style = (GridStyleInfo)VisibleColumns[j].ColumnStyle;
                            structure.OriginalStyle = (GridStyleInfo)VisibleColumns[j].ColumnStyle.Clone();
                        }

                        structure.Type = VisibleColumns[j].Type;

                        structure.Style.CellValueType = VisibleColumns[j].Type;
                        structure.OriginalStyle.CellValueType = VisibleColumns[j].Type;

                        structure.IsEmpty = true;
                        structure.TextInt = 0;
                        structure.TextDouble = 0;
                        structure.TextLong = 0;
                        structure.TextString = "";
                        structure.IsDirty = false;
                        structure.RowIndex = RowCount - 1;
                        structure.ColIndex = j;
                        structure.DrawText = true;

                        structure.Record = r;



                        structure.IsPushButton = VisibleColumns[j].CellModelType == CellType.PushButton;
                        structure.CellModelType = VisibleColumns[j].CellModelType;

                        structure.Record = r;
                    }

                    if (r != null)
                        this.allRecords.Add(r);
                }

                CellMatrix = newArray;

                if (isSortingEnabled)
                {
                    foreach (SortColumnDescriptor col in SortedColumnDescriptors)
                    {
                        UpdateRecordSortData(col);
                    }

                    SortSourceList();
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }


        internal void RemoveRow(int index)
        {
            TableInfo.CellStruct[,] newArray = new TableInfo.CellStruct[RowCount, CellMatrix.GetLength(1)];

            bool isRemovingRowFound = false;

            for (int k = 0; k < CellMatrix.GetLength(0); k++)
            {
                if (k == index)
                {
                    isRemovingRowFound = true;
                    continue;
                }

                for (int j = 0; j < CellMatrix.GetLength(1); j++)
                {
                    if (!isRemovingRowFound)
                        newArray[k, j] = CellMatrix[k, j];
                    else
                        newArray[k - 1, j] = CellMatrix[k, j];
                }
            }

            allRecords.RemoveAt(index);

            CellMatrix = newArray;

            if (isSortingEnabled)
            {
                foreach (SortColumnDescriptor col in SortedColumnDescriptors)
                {
                    UpdateRecordSortData(col);
                }

                SortSourceList();
            }
        }

        internal bool AddToFilteredRecords(Record record)
        {
            if (!this.filteredRecords.Contains(record))
            {
                filteredRecords.Add(record);
                return true;
            }

            return false;
        }

        public Record GetRecordFromIndex(int rowIndex)
        {
            return GetRecordFromIndex(rowIndex, -1);
        }

        public Record GetRecordFromIndex(int rowIndex, int colIndex)
        {
            if (rowIndex < 0)
                return null;

            if (IsFilterEnabled)
            {
                if (filteredRecords.Count > 0 && filteredRecords.Count > rowIndex)
                {
                    if (Grid.GridType == GridType.MultiColumn)
                    {
                        int sourceIndex = 0;

                        if (rowIndex >= cellMatrix.GetLength(0))
                            return null;

                        if (colIndex >= 0)
                            sourceIndex = cellMatrix[rowIndex, colIndex].SourceIndex;
                        else
                            sourceIndex = CurrentCell.SourceIndex;

                        if (filteredRecords.Count > 0 && filteredRecords.Count > sourceIndex && sourceIndex >= 0)
                        {
                            if (colIndex >= 0)
                                cellMatrix[rowIndex, colIndex].Record = filteredRecords[sourceIndex] as Record;
                            return filteredRecords[sourceIndex] as Record;
                        }
                    }
                    else
                    {
                        return filteredRecords[rowIndex] as Record;
                    }
                }
            }
            else
            {
                if (Grid.GridType == GridType.MultiColumn)
                {
                    int sourceIndex = 0;

                    if (rowIndex >= cellMatrix.GetLength(0))
                        return null;

                    if (colIndex >= 0)
                        sourceIndex = cellMatrix[rowIndex, colIndex].SourceIndex;
                    else
                        sourceIndex = CurrentCell.SourceIndex;

                    if (allRecords.Count > 0 && allRecords.Count > sourceIndex && sourceIndex >= 0)
                    {
                        if (colIndex >= 0)
                            cellMatrix[rowIndex, colIndex].Record = allRecords[sourceIndex] as Record;
                        return allRecords[sourceIndex] as Record;
                    }
                }
                else
                {
                    if (allRecords.Count > 0 && allRecords.Count > rowIndex)
                        return allRecords[rowIndex] as Record;
                }
            }

            return null;
        }

        public void ResetSortedColumns()
        {
            SortedColumnDescriptors.Clear();
            alreadyHasHiddenSortDescriptorInCollection = HiddenDefaultSortColumnDescriptor != null && this.SortedColumnDescriptors[HiddenDefaultSortColumnDescriptor.Name] != null;

            for (int j = 0; j < this.allRecords.Count; j++)
            {
                Record rec = allRecords[j] as Record;
                if (rec != null)
                {
                    rec.SortKeys.Clear();
                    rec.UnboundColumns.Clear();
                }
            }

            isSortingEnabled = false;
        }

        //
        //Use this method when a grid parameter is changed eg: Start Date , End Date or Search Criteria.
        //This method should be called only if the grid can't detemine that grid row should be focused to zero.
        //
        public void ResetRowToFirstItem()
        {
            firstSelectedRow = 0;
            lastSelectedRow = 0;
            TopRow = 0;
            selectedRecordIndexes = new List<int>() { 0 };
            CurrentRow = 0;
        }

        //public void ResetConditionalFormats()
        //{
        //    ConditionalFormats.Clear();
        //}

        public void SetRecreateFlag(bool recreate)
        {
            this.Grid.RecreateMatrixFlag = recreate;
        }

        //
        //Use this method to add Sorting Descriptors
        //
        public void AddSortingDescriptor(string columnID, ListSortDirection direction)
        {
            AddSortingDescriptor(columnID, direction, false);
        }

        //
        //Use this method to add Sorting Descriptors
        //
        public void AddSortingDescriptor(string columnID, ListSortDirection direction, bool isUnbound)
        {
            AddSortingDescriptor(columnID, direction, isUnbound, SortComparisonMethod.Common, false);
        }

        //
        //Use this method to add Sorting Descriptors
        //
        public void AddSortingDescriptor(string columnID, ListSortDirection direction, bool isUnbound, bool setSortToColumn)
        {
            AddSortingDescriptor(columnID, direction, isUnbound, SortComparisonMethod.Common, setSortToColumn);
        }

        //
        //Use this method to add Sorting Descriptors
        //
        public void AddSortingDescriptor(string columnID, ListSortDirection direction, bool isUnbound, SortComparisonMethod sortMethod)
        {
            AddSortingDescriptor(columnID, direction, isUnbound, sortMethod, false);
        }

        //
        //Use this method to add Sorting Descriptors
        //
        public void AddSortingDescriptor(string columnID, ListSortDirection direction, bool isUnbound, SortComparisonMethod sortMethod, bool setSortToColumn)
        {
            TableInfo.TableColumn column = GetVisibleColumnFromName(columnID);

            if (column == null)
                return;

            int currentPosition = column.CurrentPosition;

            if (this.Grid.GridType != GridType.MultiColumn)
                currentPosition = this.VisibleColumns.IndexOf(column);

            AddSortingDescriptor(column.MappingName, currentPosition, direction, isUnbound, sortMethod, setSortToColumn);
        }

        //
        //Use this method to add Sorting Descriptors
        //
        public void AddSortingDescriptor(string columnID, int position, ListSortDirection direction, bool isUnbound, SortComparisonMethod sortMethod, bool setSortToColumn)
        {
            if (!this.AllowSort)
                return;

            TableInfo.TableColumn column = this.VisibleColumns[position];//GetVisibleColumnFromName(columnID);

            int columnPosition = position;

            if (column == null)
            {
                column = GetVisibleColumnFromName(columnID);
                columnPosition = column.CurrentPosition;
            }

            if (column == null)
                return;

            columnPosition = position;

            for (int i = 0; i < this.VisibleColumns.Count; i++)
            {
                if (this.VisibleColumns[i].IsPrimaryColumn)
                {
                    if (i <= columnPosition)
                        position = i;
                }
            }

            if (!setSortToColumn)
            {
                if (!column.AllowSort)
                    return;
            }
            else
            {
                column.AllowSort = true;
                this.Columns[columnID].AllowSort = true;

                if (VisibleColumns.Contains(columnID))
                    this.VisibleColumns[columnID].AllowSort = true;
            }

            //if (this.SortedColumnDescriptors[columnID] == null)
            {
                if (this.SortedColumnDescriptors.Count == 1 && !AllowMultipleColumnSort)
                    return;

                SortColumnDescriptor sortDescriptor = new SortColumnDescriptor(column.MappingName, direction, this);

                sortDescriptor.IsUnbound = isUnbound;
                sortDescriptor.ComparisonMethod = sortMethod;

                if (position < 0)
                {
                    position = this.VisibleColumns.IndexOf(column);

                    for (int i = 0; i < this.VisibleColumns.Count; i++)
                    {
                        if (this.VisibleColumns[i].IsPrimaryColumn)
                        {
                            if (i > position)
                            {
                                position = i;
                                break;
                            }
                        }
                    }
                }

                sortDescriptor.Id = column.MappingName + ":" + position;
                this.SortedColumnDescriptors.Add(sortDescriptor);
                this.isSortingEnabled = true;

                UpdateRecordSortData(column.MappingName, sortDescriptor, true);

                alreadyHasHiddenSortDescriptorInCollection = HiddenDefaultSortColumnDescriptor != null && this.SortedColumnDescriptors[HiddenDefaultSortColumnDescriptor.Name] != null;
            }
        }

        internal void UpdateRecordSortData(SortColumnDescriptor col)
        {
            bool alreadyHasHiddenSortDescriptorInCollection = HiddenDefaultSortColumnDescriptor != null && this.SortedColumnDescriptors[HiddenDefaultSortColumnDescriptor.Name] != null;

            if (!alreadyHasHiddenSortDescriptorInCollection && HiddenDefaultSortColumnDescriptor != null)
            {
                foreach (PropertyAccessor p in this.Grid.PropertyList.Values)
                {
                    if (p.Property == HiddenDefaultSortColumnDescriptor.Name)
                    {
                        HiddenDefaultSortColumnDescriptor.Accessor = p;

                        for (int j = 0; j < this.allRecords.Count; j++)
                        {
                            Record rec = allRecords[j] as Record;
                            if (rec != null && rec.SortKeys.Contains(p))
                                rec.SortKeys.Remove(p);
                        }
                    }
                }
            }

            TableInfo.TableColumn column = GetVisibleColumnFromName(col.Name);

            if (!col.IsUnbound /*&& !column.IsCustomFormulaColumn*/)
            {
                PropertyAccessor p = col.Accessor;

                if (p == null || this.Grid.dataObjectTypeChanged)
                {
                    foreach (PropertyAccessor property in this.Grid.PropertyList.Values)
                    {
                        if (property.Property == col.Name)
                        {
                            col.Accessor = property;
                            p = col.Accessor;
                            break;
                        }
                    }
                }

                if (p != null)
                {
                    for (int j = 0; j < this.allRecords.Count; j++)
                    {
                        Record rec = allRecords[j] as Record;
                        if (rec != null)// && !rec.SortKeys.Contains(p))
                            rec.SortKeys.Add(p);
                    }
                }
            }
            else
            {
                //
                //Unbound column
                //

                if (column != null)
                {
                    for (int j = 0; j < this.allRecords.Count; j++)
                    {
                        Record rec = allRecords[j] as Record;
                        if (rec != null && !rec.UnboundColumns.Contains(column))
                            rec.UnboundColumns.Add(column);
                    }
                }
            }

            if (!alreadyHasHiddenSortDescriptorInCollection && HiddenDefaultSortColumnDescriptor != null)
            {
                foreach (PropertyAccessor p in this.Grid.PropertyList.Values)
                {
                    if (p.Property == HiddenDefaultSortColumnDescriptor.Name)
                    {
                        for (int j = 0; j < this.allRecords.Count; j++)
                        {
                            Record rec = allRecords[j] as Record;
                            if (rec != null && !rec.SortKeys.Contains(p))
                                rec.SortKeys.Add(p);
                        }
                    }
                }
            }
        }

        internal void UpdateRecordSortData(string columnID, SortColumnDescriptor col, bool addSortDescriptor)
        {
            try
            {
                bool columnFound = false;
                bool alreadyHasHiddenSortDescriptorInCollection = HiddenDefaultSortColumnDescriptor != null && this.SortedColumnDescriptors[HiddenDefaultSortColumnDescriptor.Name] != null;

                if (!alreadyHasHiddenSortDescriptorInCollection && HiddenDefaultSortColumnDescriptor != null)
                {
                    foreach (PropertyAccessor p in this.Grid.PropertyList.Values)
                    {
                        if (p.Property == HiddenDefaultSortColumnDescriptor.Name)
                        {
                            HiddenDefaultSortColumnDescriptor.Accessor = p;

                            for (int j = 0; j < this.allRecords.Count; j++)
                            {
                                Record rec = allRecords[j] as Record;
                                if (rec != null && rec.SortKeys.Contains(p))
                                    rec.SortKeys.Remove(p);
                            }
                        }
                    }
                }

                if (addSortDescriptor)
                {
                    foreach (PropertyAccessor p in this.Grid.PropertyList.Values)
                    {
                        if (p.Property == columnID)
                        {
                            col.Accessor = p;

                            for (int j = 0; j < this.allRecords.Count; j++)
                            {
                                Record rec = allRecords[j] as Record;
                                if (rec != null)
                                    rec.SortKeys.Add(p);
                            }

                            columnFound = true;
                            break;
                        }
                    }

                    //
                    //Unbound column
                    //
                    if (!columnFound)
                    {
                        TableInfo.TableColumn column = GetVisibleColumnFromName(columnID);
                        if (column != null)
                        {
                            for (int j = 0; j < this.allRecords.Count; j++)
                            {
                                Record rec = allRecords[j] as Record;
                                if (rec != null && !rec.UnboundColumns.Contains(column))
                                    rec.UnboundColumns.Add(column);
                            }
                        }
                    }
                }
                else
                {
                    foreach (PropertyAccessor p in this.Grid.PropertyList.Values)
                    {
                        if (p.Property == columnID)
                        {
                            col.Accessor = p;

                            for (int j = 0; j < this.allRecords.Count; j++)
                            {
                                Record rec = allRecords[j] as Record;
                                if (rec != null && rec.SortKeys.Contains(p))
                                    rec.SortKeys.Remove(p);
                            }

                            columnFound = true;
                            break;
                        }
                    }

                    //
                    //Unbound column
                    //
                    if (!columnFound)
                    {
                        TableInfo.TableColumn column = GetVisibleColumnFromName(columnID);
                        if (column != null)
                        {
                            for (int j = 0; j < this.allRecords.Count; j++)
                            {
                                Record rec = allRecords[j] as Record;

                                bool remove = false;

                                if (rec != null)
                                {
                                    for (int i = 0; i < rec.UnboundColumns.Count; i++)
                                    {
                                        if (column.Name == rec.UnboundColumns[i].Name)
                                        {
                                            remove = true;
                                            break;
                                        }
                                    }
                                }

                                if (remove)
                                    rec.UnboundColumns.Remove(column);
                            }
                        }
                    }
                }

                if (!alreadyHasHiddenSortDescriptorInCollection && HiddenDefaultSortColumnDescriptor != null)
                {
                    foreach (PropertyAccessor p in this.Grid.PropertyList.Values)
                    {
                        if (p.Property == HiddenDefaultSortColumnDescriptor.Name)
                        {
                            for (int j = 0; j < this.allRecords.Count; j++)
                            {
                                Record rec = allRecords[j] as Record;
                                if (rec != null && !rec.SortKeys.Contains(p))
                                    rec.SortKeys.Add(p);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public TableInfo.TableColumn GetColumnFromName(string str)
        {
            for (int j = 0; j < Columns.Count; j++)
            {
                TableInfo.TableColumn col = Columns[j];

                if (col != null)
                {
                    col.Table = this;
                    if ((col.Name == str || col.MappingName == str))
                    {
                        return col;
                    }
                }
            }

            return null;
        }

        public TableInfo.TableColumn GetColumnFromNameForColumnSet(string str, int columnSetPrimaryIndex)
        {
            for (int j = 0; j < VisibleColumns.Count; j++)
            {
                TableInfo.TableColumn col = VisibleColumns[j];

                if (col != null)
                {
                    col.Table = this;

                    if ((col.Name == str || col.MappingName == str))
                    {
                        if (col.CurrentPosition >= columnSetPrimaryIndex)
                            return col;
                    }
                }
            }

            return null;
        }

        public TableInfo.TableColumn GetColumnFromDisplayName(string str)
        {
            for (int j = 0; j < Columns.Count; j++)
            {
                TableInfo.TableColumn col = Columns[j];

                if (col != null)
                {
                    col.Table = this;
                    if (col.DisplayName != null && col.DisplayName == str)
                    {
                        return col;
                    }
                }
            }

            return null;
        }

        public TableInfo.TableColumn GetColumnFromID(int id)
        {
            for (int j = 0; j < Columns.Count; j++)
            {
                TableInfo.TableColumn col = Columns[j];

                if (col != null)
                {
                    col.Table = this;
                    if (col.Id == id)
                    {
                        return col;
                    }
                }
            }

            return null;
        }

        public TableInfo.TableColumn GetVisibleColumnFromName(string str)
        {
            for (int j = 0; j < VisibleColumns.Count; j++)
            {
                TableInfo.TableColumn col = VisibleColumns[j];

                if (col != null)
                {
                    col.Table = this;
                    if (col.Name == str || col.MappingName == str)
                    {
                        return col;
                    }
                }
            }

            return null;
        }

        public TableInfo.TableColumn GetVisibleColumnFromDisplayName(string str)
        {
            for (int j = 0; j < VisibleColumns.Count; j++)
            {
                TableInfo.TableColumn col = VisibleColumns[j];

                if (col != null)
                {
                    col.Table = this;
                    if (col.DisplayName == str)
                    {
                        return col;
                    }
                }
            }

            return null;
        }

        public TableInfo.TableColumn GetVisibleColumnFromID(int id)
        {
            for (int j = 0; j < VisibleColumns.Count; j++)
            {
                TableInfo.TableColumn col = VisibleColumns[j];

                if (col != null)
                {
                    col.Table = this;
                    if (col.Id == id)
                    {
                        return col;
                    }
                }
            }

            return null;
        }

        internal void GetSortInfo(out bool isSorted, out SortColumnDescriptor[] arrayOfColumnDescriptors)
        {
            isSorted = this.isSortingEnabled;

            if (this.SortedColumnDescriptors == null)
                SortedColumnDescriptors = new SortColumnDescriptorCollection(this);

            if (HiddenDefaultSortColumnDescriptor != null && !alreadyHasHiddenSortDescriptorInCollection)
                this.SortedColumnDescriptors.Add(HiddenDefaultSortColumnDescriptor);

            arrayOfColumnDescriptors = this.SortedColumnDescriptors.ToArray<SortColumnDescriptor>();

            if (HiddenDefaultSortColumnDescriptor != null && !alreadyHasHiddenSortDescriptorInCollection)
            {
                this.SortedColumnDescriptors.Remove(HiddenDefaultSortColumnDescriptor);
                // arrayOfColumnDescriptors = this.SortedColumnDescriptors.ToArray<SortColumnDescriptor>();
            }
        }

        public virtual void SetHiddenSortDescriptor(string name, ListSortDirection direction)
        {
            try
            {
                this.HiddenDefaultSortColumnDescriptor = new SortColumnDescriptor(name, direction, this);
                UpdateRecordSortData(name, this.HiddenDefaultSortColumnDescriptor, true);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void ResetToOriginalRecords()
        {
            try
            {
                allRecords.Clear();

                for (int i = 0; i < originalRecords.Count; i++)
                {
                    this.allRecords.Add(originalRecords[i]);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void ResetSelectedRecords()
        {
            try
            {
                lastSelectedRow = -1;
                firstSelectedRow = -1;
                selectedRecordIndexes.Clear();
                selectedRecords.Clear();
                currentRow = MouseRowNo;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void ResetSelectionToFirstRecord()
        {
            try
            {
                lastSelectedRow = -1;
                firstSelectedRow = 0;
                selectedRecordIndexes.Clear();
                selectedRecordIndexes.Add(0);
                selectedRecords.Clear();
                currentRow = 0;

            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void SetSelection(int i)
        {
            try
            {
                lastSelectedRow = -1;
                firstSelectedRow = i;
                selectedRecordIndexes.Clear();
                selectedRecordIndexes.Add(i);
                selectedRecords.Clear();
                CurrentRow = i;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        internal virtual void SortSourceList()
        {
            try
            {
                if (this.isSortingEnabled)
                {
                    Record currentRecord = (GetRecordFromIndex(currentRow) as Record);
                    bool sortChangedRecords = false;

                    //
                    //Main Sorting
                    //
                    if (Grid.GridType != GridType.MultiColumn)
                    {
                        SortCollection(AllRecords, !IsFilterEnabled);

                        //AllRecords.Sort(comp);

                        if (SortApplied != null)
                        {
                            for (int i = 0; i < allRecords.Count; i++)
                            {
                                if (originalRecords.Count > i && allRecords[i] != originalRecords[i])
                                {
                                    sortChangedRecords = true;
                                    break;
                                }
                            }
                        }

                        originalRecords.Clear();

                        for (int i = 0; i < allRecords.Count; i++)
                        {
                            this.originalRecords.Add(allRecords[i]);
                        }

                        for (int i = 0; i < VisibleColumns.Count; i++)
                        {
                            if (VisibleColumns[i].IsCustomColumn)
                            {
                                for (int j = 0; j < cellMatrix.GetLength(0); j++)
                                {
                                    if (j >= cellMatrix.GetLength(0) || i >= cellMatrix.GetLength(1))
                                        continue;

                                    cellMatrix[j, i].LastUpdatedTime = 0;
                                }
                            }
                        }

                        if (sortChangedRecords && SortApplied != null)
                            SortApplied(this.Grid, null);
                    }
                    else
                    {
                        List<int> primaryColumnNumbers = new List<int>();

                        for (int i = 0; i < this.VisibleColumns.Count; i++)
                        {
                            if (this.VisibleColumns[i].IsPrimaryColumn)
                            {
                                primaryColumnNumbers.Add(i);
                            }
                        }

                        if (SortApplied != null)
                        {
                            for (int i = 0; i < allRecords.Count; i++)
                            {
                                if (originalRecords.Count > i && allRecords[i] != originalRecords[i])
                                {
                                    sortChangedRecords = true;
                                    break;
                                }
                            }
                        }

                        originalRecords.Clear();

                        Dictionary<int, Records> columnRecords = new Dictionary<int, Records>();
                        int lastPrimaryIndex = -1;

                        for (int j = 0; j < primaryColumnNumbers.Count; j++)
                        {
                            for (int i = 0; i < allRecords.Count; i++)
                            {
                                if ((AllRecords[i] as Record).ColumnIndex > lastPrimaryIndex && (AllRecords[i] as Record).ColumnIndex <= primaryColumnNumbers[j])
                                {
                                    if (!columnRecords.ContainsKey(primaryColumnNumbers[j]))
                                        columnRecords.Add(primaryColumnNumbers[j], new Records());

                                    columnRecords[primaryColumnNumbers[j]].Add((allRecords[i] as Record));
                                }
                            }

                            lastPrimaryIndex = primaryColumnNumbers[j];
                        }

                        foreach (int i in columnRecords.Keys)
                        {
                            foreach (SortColumnDescriptor item in this.SortedColumnDescriptors)
                            {
                                if (item.Id.Contains(i.ToString()))
                                {
                                    columnRecords[i].Sort(comp);
                                    break;
                                }
                            }
                        }

                        for (int i = 0; i < allRecords.Count; i++)
                        {
                            this.originalRecords.Add(allRecords[i]);
                        }

                        allRecords.Clear();

                        foreach (int i in columnRecords.Keys)
                        {
                            for (int j = 0; j < columnRecords[i].Count; j++)
                            {
                                this.allRecords.Add(columnRecords[i][j]);
                            }
                        }

                        for (int i = 0; i < originalRecords.Count; i++)
                        {
                            if (!allRecords.Contains(originalRecords[i]))
                                this.allRecords.Add(originalRecords[i]);
                        }

                        if (sortChangedRecords && SortApplied != null)
                            SortApplied(this.Grid, null);
                    }

                    if (IsFilterEnabled)
                    {
                        //filteredRecords.Sort(comp);
                        SortCollection(filteredRecords, true);
                    }

                    int currentRecordIndex = -1;

                    if (IsFilterEnabled)
                        currentRecordIndex = filteredRecords.IndexOf(currentRecord);
                    else
                        currentRecordIndex = allRecords.IndexOf(currentRecord);

                    if (ScrollToSelectedRecordOnSort)
                    {
                        if (currentRecordIndex != currentRow && (currentRow > TopRow || currentRow < TopRow))
                        {
                            TopRow += (currentRecordIndex - currentRow);

                            if (TopRow < 0)
                                TopRow = 0;

                            CurrentRow = currentRecordIndex;
                            firstSelectedRow = CurrentRow;
                            lastSelectedRow = -1;

                            Grid.VScroll.Value = currentRecordIndex;
                            Grid.VScroll.Invalidate();
                            Grid.Invalidate();
                            Grid.PrepareTables();
                        }
                    }

                    //
                    //Check this
                    //
                    if (IsDirty)
                    {
                        selectedRecordIndexes.Clear();
                        selectedRecords.Clear();
                    }

                    //
                    //Updating source indices of matrix
                    //
                    if (this.Grid.GridType == GridType.MultiColumn)
                    {
                        int primaryColumnCount = -1;
                        int recordFilledCount = 0;
                        int groupRecordCount = 0;

                        for (int j = 0; j < this.VisibleColumnCount; j++)
                        {
                            TableInfo.TableColumn col = VisibleColumns[j];

                            if (col == null)
                                continue;

                            if (col.IsPrimaryColumn)
                            {
                                primaryColumnCount++;
                                groupRecordCount = 0;
                            }

                            int index = -1;

                            for (int i = 0; i < RowCount; i++)
                            {
                                if (cellMatrix.GetLength(0) <= i || cellMatrix[i, j].Record == null)
                                    continue;

                                index++;

                                if (primaryColumnCount == 0)
                                {
                                    cellMatrix[i, j].SourceIndex = index;
                                    if (cellMatrix[i, j].Record != null)
                                    {
                                        cellMatrix[i, j].Record.SourceIndex = index;
                                    }
                                }
                                else
                                {
                                    if (col.IsPrimaryColumn)
                                    {
                                        cellMatrix[i, j].SourceIndex = recordFilledCount;
                                        if (cellMatrix[i, j].Record != null)
                                        {
                                            cellMatrix[i, j].Record.SourceIndex = recordFilledCount;
                                        }
                                        groupRecordCount++;
                                    }
                                    else
                                    {
                                        cellMatrix[i, j].SourceIndex = recordFilledCount - groupRecordCount + index;
                                        if (cellMatrix[i, j].Record != null)
                                        {
                                            cellMatrix[i, j].Record.SourceIndex = cellMatrix[i, j].SourceIndex;
                                        }
                                    }
                                }

                                if (col.IsPrimaryColumn)
                                {
                                    recordFilledCount++;
                                }

                                if (cellMatrix[i, j].SourceIndex >= allRecords.Count)
                                    cellMatrix[i, j].Record = null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                if (rowIndexSelectedBeforeSorting != null)
                {
                    CurrentRow = (int)rowIndexSelectedBeforeSorting;
                    rowIndexSelectedBeforeSorting = null;
                }
                //selectedRecordIndexes.Clear();
                //selectedRecords.Clear();

            }
        }

        private void SortCollection(Records collection, bool includeGroupSort = true)
        {
            int extraSorts = 0;

            if (Grid.SortUsingLinq == false)
            {
                //if (this.Grid.IsGroupingEnabled())
                //{
                //    for (int i = Grid.GroupByNameList.Count - 1; i > -1; i--)
                //    {
                //        string columnName = Grid.GroupByNameList[i].Name;
                //        SortedColumnDescriptors.Add(new SortColumnDescriptor(columnName, ListSortDirection.Ascending, this));
                //        extraSorts++;
                //    }
                //}

                collection.Sort(comp);

                //if (this.Grid.IsGroupingEnabled())
                //{
                //    for (int i = 0; i < extraSorts; i++)
                //    {
                //        SortedColumnDescriptors.RemoveAt(SortedColumnDescriptors.Count - 1);
                //    }
                //}
            }
            else
            {
                preFilterSort.Clear();

                try
                {
                    foreach (var item in collection)
                    {
                        preFilterSort.Add(item as Record);
                    }

                    if (this.SortedColumnDescriptors.Count > 0 || this.Grid.IsGroupingEnabled())
                    {
                        for (int i = SortedColumnDescriptors.Count - 1; i >= 0; i--)
                        {
                            string columnName = this.SortedColumnDescriptors[i].Name;
                            Type type = this.VisibleColumns[columnName].Type;

                            SortByColumn(SortedColumnDescriptors[i].SortDirection, columnName, type);
                        }

                        if (this.Grid.IsGroupingEnabled())
                        {
                            for (int i = Grid.GroupByNameList.Count - 1; i > -1; i--)
                            {
                                string columnName = Grid.GroupByNameList[i].Name;

                                if (columnName != "*")
                                {
                                    //if (this.VisibleColumns.Contains(columnName))
                                    {
                                        Type type = this.Columns[columnName].Type;
                                        SortByColumn(Grid.GroupByNameList[i].SortDirection, columnName, type);
                                    }
                                }
                                else
                                {
                                    if (Grid.GroupByNameList[i].SortDirection == ListSortDirection.Ascending)
                                        preFilterSort = preFilterSort.OrderBy(x => x.GetQueryString()).ToList();
                                    else
                                        preFilterSort = preFilterSort.OrderByDescending(x => x.GetQueryString()).ToList();
                                }
                            }
                        }

                        collection.Clear();

                        foreach (var item in preFilterSort)
                        {
                            collection.Add(item);
                        }

                        PopulateGroupHeaders();

                        //if (includeGroupSort)
                        //{
                        //    Timer tim = new Timer();
                        //    tim.Interval = 1000;
                        //    tim.Tick += (s, e) =>
                        //    {
                        //        tim.Stop();
                        //        PopulateGroupHeaders(preFilterSort);
                        //    };
                        //    tim.Start();

                        //}
                    }
                }
                catch (Exception ex)
                {
                    sortByLinqExceptionCount++;

                    if (sortByLinqExceptionCount > 20)
                        Grid.SortUsingLinq = false;

                    collection.Sort(comp);
                    ExceptionsLogger.LogError("Grid Sorting Exception : " + ex.Message);
                }
            }


        }

        private void SortByColumn(ListSortDirection direction, string columnName, Type type)
        {
            if ((type == typeof(int) || type == typeof(int?)))
            {
                if (direction == ListSortDirection.Ascending)
                    preFilterSort = preFilterSort.OrderBy(x => x.GetValueAsInt(columnName)).ToList();
                else
                    preFilterSort = preFilterSort.OrderByDescending(x => x.GetValueAsInt(columnName)).ToList();
            }
            else if (type == typeof(long) || type == typeof(long?))
            {
                if (direction == ListSortDirection.Ascending)
                    preFilterSort = preFilterSort.OrderBy(x => x.GetValueAsLong(columnName)).ToList();
                else
                    preFilterSort = preFilterSort.OrderByDescending(x => x.GetValueAsLong(columnName)).ToList();
            }
            else if (type == typeof(double) || type == typeof(double?) || type == typeof(float))
            {
                if (direction == ListSortDirection.Ascending)
                    preFilterSort = preFilterSort.OrderBy(x => x.GetValueAsDouble(columnName)).ToList();
                else
                    preFilterSort = preFilterSort.OrderByDescending(x => x.GetValueAsDouble(columnName)).ToList();
            }
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                if (direction == ListSortDirection.Ascending)
                    preFilterSort = preFilterSort.OrderBy(x => x.GetValueAsDateTime(columnName)).ToList();
                else
                    preFilterSort = preFilterSort.OrderByDescending(x => x.GetValueAsDateTime(columnName)).ToList();
            }
            else
            {
                if (preFilterSort.Count > 0)
                {
                    var pAccess = GetOrCreatePropertyAccessor(columnName, preFilterSort[0].GetData().GetType());

                    if (direction == ListSortDirection.Ascending)
                        preFilterSort = preFilterSort.OrderBy(x => pAccess.Get(x.GetData()).ToString()).ToList();
                    else
                        preFilterSort = preFilterSort.OrderByDescending(x => pAccess.Get(x.GetData()).ToString()).ToList();
                }
            }
        }

        Dictionary<string, PropertyAccessor> GroupPropertyAccessorDictionary = new Dictionary<string, PropertyAccessor>();

        public PropertyAccessor GetOrCreatePropertyAccessor(string columnName, Type type)
        {
            string mappingName = GetColumnFromName(columnName).MappingName;
            PropertyAccessor pAccess = null;

            if (GroupPropertyAccessorDictionary.ContainsKey(mappingName))
            {
                pAccess = GroupPropertyAccessorDictionary[mappingName];
            }
            else
            {
                GroupPropertyAccessorDictionary.Add(mappingName, new Accessors.PropertyAccessor(type, mappingName));
                pAccess = GroupPropertyAccessorDictionary[mappingName];
            }

            return pAccess;
        }

        public void CalculateColumnLeft(int startValue = 1)
        {
            try
            {
                if (VisibleColumns.Count <= 0)
                    return;

                bool isMirrored = Grid.IsMirrored;

                //if (!isMirrored)
                //   VisibleColumns[0].Left = 0;
                //else
                //   VisibleColumns[VisibleColumnCount - 1].Left = 5;// this.Grid.GetVisibleColumnWidth() + 5;

                if (!isMirrored)
                {
                    for (int i = startValue; i < VisibleColumns.Count; i++)
                    {
                        if (VisibleColumns[i - 1].ColumnStyle == null)
                            VisibleColumns[i - 1].ColumnStyle = Grid.Table.TableStyle;

                        VisibleColumns[i].Left = VisibleColumns[i - 1].Left + VisibleColumns[i - 1].CellWidth;//+ Grid.horizontalOffset;
                    }
                }
                else
                {
                    //VisibleColumns[0].Left = Grid.Width - VisibleColumns[0].CellWidth;

                    for (int i = VisibleColumnCount - 2; i >= 0; i--)
                    {
                        //if (i == VisibleColumnCount - 1)
                        //{
                        //    if (VisibleColumns[i].ColumnStyle == null)
                        //        VisibleColumns[i].ColumnStyle = Grid.Table.TableStyle;

                        //    VisibleColumns[i].Left = 0;
                        //}
                        //else
                        {
                            if (i >= 0)
                            {
                                //if (i == 0)
                                //{
                                //    VisibleColumns[i].Left = VisibleColumns[i + 1].Left - VisibleColumns[i + 1].CellWidth;//+ Grid.horizontalOffset;
                                //}
                                //else
                                {

                                    if (VisibleColumns[i].ColumnStyle == null)
                                        VisibleColumns[i].ColumnStyle = Grid.Table.TableStyle;

                                    VisibleColumns[i].Left = VisibleColumns[i + 1].Left + VisibleColumns[i + 1].CellWidth;//+ Grid.horizontalOffset;
                                }
                            }
                            //else if (i == 0)
                            //{
                            //    VisibleColumns[i].Left = 0;
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void AddColumn(string columnName, int id, string displayName, GridStyleInfo style, Type cellType, int columnWidth, GridHorizontalAlignment horizontalAlignment, bool isVisible)
        {
            AddColumn(columnName, id, displayName, style, cellType, columnWidth, horizontalAlignment, false, isVisible);
        }

        public void AddColumn(string columnName, int id, string displayName, GridStyleInfo style, Type cellType, int columnWidth, GridHorizontalAlignment horizontalAlignment)
        {
            AddColumn(columnName, id, displayName, style, cellType, columnWidth, horizontalAlignment, false, false);
        }

        public void AddColumn(string columnName, int id, string displayName, GridStyleInfo style, Type cellType, int columnWidth, GridHorizontalAlignment horizontalAlignment, bool queryStyle, bool isVisible)
        {
            try
            {
                TableColumn column = new TableColumn(this);
                column.Name = columnName;
                column.Id = id;

                //
                //id is considered -1 for all Unbound columns as they dont have any matching values for the datasource object
                //
                if (id < 0)
                    column.IsUnbound = true;

                if (style == null)
                    column.ColumnStyle = (GridStyleInfo)GridStyleInfo.Default.Clone();
                else
                    column.ColumnStyle = style;

                column.ColumnStyle.HorizontalAlignment = horizontalAlignment;
                column.ColumnStyle.CellValueType = cellType;
                column.Type = cellType;
                column.CellWidth = columnWidth;
                column.DesignWidth = columnWidth;
                column.DisplayName = displayName;
                column.QueryStyle = queryStyle;

                if (isVisible)
                    VisibleColumns.Add(column);

                Columns.Add(column.Clone() as TableColumn);

                if (isVisible)
                    CalculateColumnLeft();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void SetColHidden(string name, bool hide)
        {
            if (this.Columns == null || this.VisibleColumns == null)
                return;

            if (!hide)
            {
                TableInfo.TableColumn col = this.GetColumnFromName(name);

                if (col != null && !this.VisibleColumns.Contains(col))
                    this.VisibleColumns.Add(col);
            }
            else
            {
                TableInfo.TableColumn col = this.GetVisibleColumnFromName(name);

                if (col != null && this.VisibleColumns.Contains(col))
                    this.VisibleColumns.Remove(col);
            }
        }

        public void SetColWidth(string name, int width)
        {
            if (this.VisibleColumns == null)
                return;

            if (this.VisibleColumns.Contains(name))
            {
                this.VisibleColumns[name].DesignWidth = width;
                this.VisibleColumns[name].CellWidth = width;
            }
        }

        public void SetScale(Graphics g)
        {
            float scale = GetScale();
            g.ScaleTransform(scale, scale);
        }

        private float scalePercentage = 1;

        public float ScalePercentage
        {
            get
            {
                return scalePercentage;
            }
            set
            {
                scalePercentage = value;
            }
        }

        public float GetScale()
        {
            if (Grid.isPrinting)
                return 1f;

            return ScalePercentage;
        }

        internal void InsertColumn(int ColNo)
        {
            try
            {
                TableColumn column = new TableColumn(this);
                column.DisplayName = String.Empty;
                VisibleColumns.Insert(ColNo, column);
                CalculateColumnLeft();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void DeleteColumn(int ColNo)
        {
            try
            {
                VisibleColumns.RemoveAt(ColNo);
                //CalculateColumnLeft();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void DeleteColumn(string name)
        {
            try
            {
                TableColumn column = GetVisibleColumnFromName(name);
                if (column != null)
                {
                    VisibleColumns.Remove(column);
                    CalculateColumnLeft();
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void AddColumnToVisibleColumns(string name)
        {
            try
            {
                TableColumn column = GetColumnFromName(name);

                VisibleColumns.Add(column);
                CalculateColumnLeft();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void AddColumnToVisibleColumns(string name, int position)
        {
            try
            {
                TableColumn column = GetColumnFromName(name);

                VisibleColumns.Insert(position, column);
                CalculateColumnLeft();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        internal void DeleteAllColumns()
        {
            try
            {
                for (int i = 0; i < VisibleColumnCount; i++)
                {
                    VisibleColumns[i].Name = "0";
                    VisibleColumns[i].Id = -1;
                    VisibleColumns[i].DisplayName = "";
                }

                CalculateColumnLeft();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void PointToRowCol(Point location, out int row, out int col)
        {
            if (location != null)
            {
                FindCellNo(location.X, location.Y);

                row = MouseRowNo;
                col = MouseColNo;
            }
            else
            {
                row = -10;
                col = -10;
            }
        }

        public GridStyleInfo PointToTableCellStyle(Point location)
        {
            if (location != null)
            {
                FindCellNo(location.X, location.Y);

                return cellMatrix[currentRow, currentCol].Style;
            }

            return null;
        }


        public GroupHeaderRowLevel SelectedGroup = null;
        public bool OutOfGridRowsRange = false;
        public bool IsOverGroupingHeader = false;

        public void FindCellNo(float X, float Y)
        {
            OutOfGridRowsRange = false;
            SelectedGroup = null;
            bool boolResize = false;
            MouseRowNo = -10;
            MouseColNo = -10;

            try
            {
                if ((Y > 0) & (Y < HeaderHeight * GetScale()))
                    MouseRowNo = -1;
                else if (Y > HeaderHeight * GetScale())
                {
                    int lastRowBottom = (int)(HeaderHeight * GetScale());
                    double row = TopRow + (Y - (HeaderHeight * GetScale())) / (RowHeight * GetScale());

                    if (Grid.IsGroupingEnabled())
                    {
                        List<GroupHeaderRowLevel> skippedGroups = new List<GroupHeaderRowLevel>();
                        int i = TopRow;
                        int bottomRowValueTemp = BottomRow;

                        for (i = TopRow; i < BottomRow; i++)
                        {
                            List<GroupHeaderRowLevel> groupTempList = new List<GroupHeaderRowLevel>();

                            for (int j = 0; j < Grid.GroupHeaderRowIds.Count; j++)
                            {
                                if (Grid.GroupHeaderRowIds[j].RowId <= i && Grid.GroupHeaderRowIds[j].EndRowId >= i)// !Grid.GroupHeaderRowIds[j].Collapsed)
                                {
                                    groupTempList.Add(Grid.GroupHeaderRowIds[j]);
                                    //break;
                                }
                            }

                            bool doBreak = false;

                            for (int x = 0; x < groupTempList.Count; x++)
                            {
                                bool isLastItem = x == groupTempList.Count - 1;
                                GroupHeaderRowLevel group = groupTempList[x];

                                if (group != null)
                                {
                                    if (group.Collapsed)
                                    {
                                        if (!skippedGroups.Contains(group) && group.RowId >= topRow)
                                        {
                                            skippedGroups.Add(group);

                                            if (Y >= lastRowBottom && Y <= lastRowBottom + (RowHeight * GetScale()))
                                            {
                                                row = i;
                                                SelectedGroup = group;

                                                if (isLastItem)
                                                    doBreak = true;
                                            }

                                            if (group.RowId >= TopRow)
                                                lastRowBottom += (int)(RowHeight * GetScale());
                                        }
                                        break;
                                    }
                                    else
                                    {
                                        if (!skippedGroups.Contains(group) && group.RowId >= topRow)//Process Header
                                        {
                                            skippedGroups.Add(group);

                                            if (Y >= lastRowBottom && Y <= lastRowBottom + (RowHeight * GetScale()) + (RowHeight * GetScale()))
                                            {
                                                row = i;

                                                if (Y <= lastRowBottom + (RowHeight * GetScale()) && group.RowId == i)
                                                    SelectedGroup = group;

                                                if (isLastItem)
                                                    doBreak = true;
                                                //break;
                                            }

                                            if (!isLastItem || i == topRow && group.RowId < i)
                                                lastRowBottom += (int)(RowHeight * GetScale());
                                            else
                                                lastRowBottom += (int)((RowHeight * GetScale()) + (RowHeight * GetScale()));
                                        }
                                        else//Process Cells
                                        {
                                            if (Y >= lastRowBottom && Y <= lastRowBottom + (RowHeight * GetScale()))
                                            {
                                                row = i;
                                                //SelectedGroup = group;
                                                if (isLastItem)
                                                    doBreak = true;
                                                //break;
                                            }

                                            if (isLastItem)
                                                lastRowBottom += (int)(RowHeight * GetScale());
                                        }
                                    }
                                }
                            }

                            if (doBreak)
                            {
                                break;
                            }
                        }

                        if (i >= bottomRowValueTemp)
                        {
                            if (lastRowBottom < Y)
                            {
                                OutOfGridRowsRange = true;
                            }
                        }
                    }

                    int fullRow = Math.Max(RowCount, SourceDataRowCount);

                    if (row < fullRow)
                        MouseRowNo = (int)row;//(int)Math.Round(row, MidpointRounding.ToEven);

                    if ((int)row == fullRow)
                    {
                        MouseRowNo = -15;
                    }
                }

                //
                //Col adjusted according to offset.
                //
                X = (X / GetScale()) + XOffset;

                for (int i = 0; i < VisibleColumnCount; i++)
                {
                    if (VisibleColumns[i].IsFrozen)
                    {
                        float tempX = (X) - XOffset;

                        if (tempX >= VisibleColumns[i].Left && tempX < VisibleColumns[i].Left + VisibleColumns[i].CellWidth + 4)
                        {
                            MouseColNo = i;

                            if (!IsDragging)
                            {
                                if ((Y > 0 & Y < HeaderHeight) | HeaderHeight == 0)
                                {
                                    if ((X >= VisibleColumns[i].Left + XOffset + VisibleColumns[i].CellWidth - 4) & (X <= VisibleColumns[i].Left + Grid.horizontalOffset + VisibleColumns[i].CellWidth + 4))
                                    {
                                        if (!HideHeader && allowColumnMouseResize)
                                        {
                                            MouseRowNo = -2;
                                            boolResize = true;
                                        }
                                    }
                                    break;
                                }
                            }

                            break;
                        }
                    }
                    else if (Culture.TextInfo.IsRightToLeft && (X - (VisibleColumns[VisibleColumnCount - 1].Left + XOffset)) > -4 && (X - (VisibleColumns[VisibleColumnCount - 1].Left + XOffset)) < 4)
                    {
                        MouseColNo = VisibleColumnCount;

                        MouseRowNo = -2;
                        boolResize = true;

                        break;
                    }
                    else if (((X >= VisibleColumns[i].Left + XOffset) & (X < (VisibleColumns[i].Left + Grid.horizontalOffset + VisibleColumns[i].CellWidth))))
                    {


                        MouseColNo = i;

                        if (!IsDragging)
                        {
                            if ((Y > 0 & Y < HeaderHeight) | HeaderHeight == 0)
                            {
                                if ((X >= VisibleColumns[i].Left + XOffset + VisibleColumns[i].CellWidth - 4) & (X <= VisibleColumns[i].Left + Grid.horizontalOffset + VisibleColumns[i].CellWidth + 4))
                                {
                                    if (!HideHeader && allowColumnMouseResize)
                                    {
                                        MouseRowNo = -2;
                                        boolResize = true;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

                if (!IsDragging)
                {
                    if (boolResize)
                        Grid.Cursor = Cursors.SizeWE;
                    else
                        Grid.Cursor = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }


            if (IsDragging && MouseRowNo < -1 && draggingColumn != null)
                MouseRowNo = -1;

            if (IsDragging && MouseRowNo > -1 && draggingColumn != null)
                MouseRowNo = -1;
        }

        private void ChangeColumnPosition(TableColumn column, int newIndex)
        {
            try
            {
                if (Grid.GridType == GridType.MultiColumn)
                {
                    int oldIndex = this.VisibleColumns.IndexOf(column);

                    if (oldIndex == newIndex)
                        return;

                    Grid.BeginUpdate();

                    if (oldIndex > newIndex)
                    {
                        this.InsertColumnToMatrix(column, newIndex, true);
                        this.DeleteColumnFromMatrix(oldIndex + 1);
                    }
                    else
                    {
                        this.InsertColumnToMatrix(column, newIndex + 1, true);
                        this.DeleteColumnFromMatrix(oldIndex);
                    }
                }
                else
                {
                    int currentColumnIndex = Grid.Table.VisibleColumns.IndexOf(column);

                    if (currentColumnIndex != newIndex)
                    {
                        Grid.Table.VisibleColumns.RemoveAt(currentColumnIndex);

                        Grid.Table.VisibleColumns.Insert(newIndex, column);

                        for (int j = 0; j < Grid.Table.VisibleColumns.Count; j++)
                        {
                            Grid.Table.VisibleColumns[j].CurrentPosition = j;
                        }

                        draggingColumn.CurrentPosition = newIndex;
                        TableColumn col = GetColumnFromName(column.Name);
                        if (col != null)
                        {
                            col.CurrentPosition = newIndex;
                        }

                        Grid.Table.CreateMatrix(RowCount, VisibleColumns.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                if (Grid.GridType == GridType.MultiColumn)
                    Grid.EndUpdate(false);
            }
        }

        internal bool HasCurrentCellAt(int rowIndex, int colIndex)
        {
            if (currentCell.RowIndex == rowIndex && currentCell.ColIndex == colIndex)
                return true;

            return false;
        }

        #endregion

        #region Mouse and Keyboard Methods

        public void ProcessMouseMove(MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.None)
                {
                    int currentRowIndex = MouseRowNo;
                    int currentColIndex = MouseColNo;

                    FindCellNo(e.X, e.Y);

                    if (currentRowIndex != MouseRowNo ||
                        currentColIndex != MouseColNo)
                    {
                        Grid.toolTip.Hide(this.Grid);
                        Grid.toolTip.RemoveAll();

                        if (MouseRowNo < this.cellMatrix.GetLength(0) && MouseColNo < this.cellMatrix.GetLength(1))
                        {
                            if (currentColIndex >= 0)
                            {
                                if (this.VisibleColumns.Count > currentColIndex && this.VisibleColumns[currentColIndex].AllowCellMouseHoverEvents)
                                {
                                    RaiseMouseLeaveEvent(e.Location, currentRowIndex, currentColIndex);                                   
                                }
                            }

                            if (Grid.GridType == GridType.MultiColumn)
                            {
                                string toolTip = string.Empty;

                                if (MouseColNo >= 0 && !string.IsNullOrEmpty(VisibleColumns[MouseColNo].Name))
                                    toolTip = GetColumnFromName(VisibleColumns[MouseColNo].Name).HeaderTooltipValue;

                                if (MouseRowNo == -1 && MouseColNo >= 0 && !string.IsNullOrEmpty(toolTip))
                                {
                                    Grid.toolTip.RemoveAll();
                                    Grid.toolTip.Hide(this.Grid);
                                    Grid.toolTip.ShowAlways = true;
                                    Grid.toolTip.Active = true;
                                    Grid.toolTip.SetToolTip(this.Grid, toolTip);

                                }
                                else if (MouseRowNo >= 0 && MouseColNo >= 0 && cellMatrix[MouseRowNo, MouseColNo].ToolTipTextValue != null)
                                {
                                    Grid.toolTip.RemoveAll();
                                    Grid.toolTip.Hide(this.Grid);
                                    Grid.toolTip.ShowAlways = true;
                                    Grid.toolTip.Active = true;
                                    Grid.toolTip.SetToolTip(this.Grid, cellMatrix[MouseRowNo, MouseColNo].ToolTipTextValue);
                                }
                                else
                                {
                                    Grid.toolTip.Hide(this.Grid);
                                    Grid.toolTip.RemoveAll();
                                }
                            }
                            else
                            {
                                if (MouseRowNo == -1 && MouseColNo >= 0)
                                {
                                    Grid.toolTip.RemoveAll();
                                    Grid.toolTip.Hide(this.Grid);
                                    Grid.toolTip.ShowAlways = true;
                                    Grid.toolTip.Active = true;

                                    if (!string.IsNullOrEmpty(VisibleColumns[MouseColNo].HeaderTooltipValue))
                                        Grid.toolTip.SetToolTip(this.Grid, VisibleColumns[MouseColNo].HeaderTooltipValue);
                                    else
                                        Grid.toolTip.SetToolTip(this.Grid, VisibleColumns[MouseColNo].DisplayName);
                                }
                                else if (MouseRowNo >= 0 && MouseColNo >= 0 && cellMatrix[MouseRowNo, MouseColNo].ToolTipTextValue != null)
                                {
                                    Grid.toolTip.RemoveAll();
                                    Grid.toolTip.Hide(this.Grid);
                                    Grid.toolTip.ShowAlways = true;
                                    Grid.toolTip.Active = true;
                                    Grid.toolTip.SetToolTip(this.Grid, cellMatrix[MouseRowNo, MouseColNo].ToolTipTextValue, cellMatrix[MouseRowNo, MouseColNo].ToolTipHeaderTextValue);
                                }
                                else
                                {
                                    Grid.toolTip.Hide(this.Grid);
                                    Grid.toolTip.RemoveAll();
                                }
                            }
                        }
                    }

                    if (MouseColNo >= 0)
                    {
                        if (this.VisibleColumns[MouseColNo].AllowCellMouseHoverEvents)
                        {
                            string result = RaiseMouseMoveEventNewForToolTip(e.Location, MouseRowNo, MouseColNo, e.Button);

                            if (!string.IsNullOrEmpty(result))
                            {
                                Grid.toolTip.ShowAlways = true;
                                Grid.toolTip.Active = true;
                                Grid.toolTip.SetToolTip(this.Grid, result, string.Empty);
                            }
                            else
                            {
                                Grid.toolTip.RemoveAll();
                                Grid.toolTip.Hide(this.Grid);
                            }              
                        }
                    }

                    try
                    {
                        if (MouseRowNo >= 0 && MouseColNo >= 0 && VisibleColumns[MouseColNo].ShowButtons && this.VisibleColumns[MouseColNo].AllowCellMouseHoverEvents)
                        {
                            string cellModel = Grid.GetCellStructure(MouseRowNo, MouseColNo).CellModelType;

                            Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers.CellRendererBase renderer = Grid.CellRenderers[cellModel];

                            if (renderer != null && renderer.Buttons.Count > 0)
                            {
                                Rectangle cellRectangle = new Rectangle();

                                cellRectangle.X = VisibleColumns[MouseColNo].Left;// +2;
                                cellRectangle.Y = HeaderHeight + ((MouseRowNo - TopRow) * RowHeight);
                                cellRectangle.Width = VisibleColumns[MouseColNo].CellWidth;
                                cellRectangle.Height = RowHeight;

                                Rectangle[] buttonsBounds = new Rectangle[renderer.Buttons.Count];

                                renderer.PerformCellLayout(MouseRowNo, MouseColNo, cellMatrix[MouseRowNo, MouseColNo].Style, cellRectangle);
                                renderer.OnLayout(MouseRowNo, MouseColNo, cellMatrix[MouseRowNo, MouseColNo].Style, cellRectangle, buttonsBounds);

                                bool toolTipAssigned = false;

                                for (int i = 0; i < renderer.Buttons.Count; i++)
                                {
                                    if (renderer.GetButton(i).Visible)
                                    {
                                        renderer.GetButton(i).Bounds = buttonsBounds[i];
                                        if ((renderer.Buttons[i] as GridCellButton).Bounds.Contains(new Point(e.Location.X + 2, e.Location.Y + 2)))
                                        {
                                            if (string.IsNullOrEmpty(Grid.toolTip.GetToolTip(Grid)))
                                            {
                                                Grid.toolTip.ShowAlways = true;
                                                Grid.toolTip.Active = true;
                                                Grid.toolTip.SetToolTip(this.Grid, (renderer.Buttons[i] as GridCellButton).ToolTip);
                                                toolTipAssigned = true;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (!toolTipAssigned)
                                {
                                    Grid.toolTip.Hide(this.Grid);
                                    Grid.toolTip.RemoveAll();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionsLogger.LogError(ex);
                    }
                }
                else if (e.Button == MouseButtons.Left & MouseRowNo == -2)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        if (MouseRowNo == -2 && allowColumnMouseResize)
                        {
                            bool isMirrored = Culture.TextInfo.IsRightToLeft;

                            if (!isMirrored)
                            {
                                IsDragging = true;
                                VisibleColumns[MouseColNo].CellWidth = (int)((e.X / GetScale()) - (VisibleColumns[MouseColNo].Left));
                            }
                            else
                            {
                                if (MouseColNo < 1)
                                    return;
                                IsDragging = true;

                                int initialLeft = VisibleColumns[MouseColNo - 1].Left;
                                VisibleColumns[MouseColNo - 1].Left = (int)(e.X / Grid.Table.GetScale());

                                VisibleColumns[MouseColNo - 1].CellWidth += initialLeft - VisibleColumns[MouseColNo - 1].Left;

                                if (MouseColNo < VisibleColumns.Count)
                                {
                                    VisibleColumns[MouseColNo].Left = VisibleColumns[MouseColNo - 1].Left - VisibleColumns[MouseColNo].CellWidth;

                                    for (int i = MouseColNo + 1; i < VisibleColumnCount; i++)
                                    {
                                        VisibleColumns[i].Left -= initialLeft - VisibleColumns[MouseColNo - 1].Left;
                                    }
                                }

                            }

                            int leftColNumber = MouseColNo;

                            if (leftColNumber < 1)
                                leftColNumber = 1;

                            CalculateColumnLeft(leftColNumber);
                            Grid.Invalidate();
                        }
                    }
                }
                else
                {
                    if (allowdrag && isHeaderMouseDown)
                    {

                        if (MouseRowNo == -1)
                        {
                            dragPoint2 = e.Location;

                            if (Math.Abs(dragPoint1.X - dragPoint2.X) > 5 || Math.Abs(dragPoint1.Y - dragPoint2.Y) > 5)
                            {
                                FindCellNo(e.X, e.Y);
                                if (e.Button == MouseButtons.Left)
                                    IsDragging = true;
                                Grid.Refresh();
                            }
                        }
                    }
                    else
                    {
                        int currentRowIndex = MouseRowNo;
                        int currentColIndex = MouseColNo;

                        FindCellNo(e.X, e.Y);

                        if (currentRowIndex != MouseRowNo ||
                            currentColIndex != MouseColNo)
                        {
                            if (currentColIndex >= 0)
                            {
                                //if (this.VisibleColumns[currentColIndex].AllowCellMouseHoverEvents)
                                //{
                                RaiseMouseLeaveEvent(e.Location, currentRowIndex, currentColIndex);
                                //}
                            }
                        }

                        if (MouseColNo >= 0)
                        {
                            dragPoint2 = e.Location;
                            if (Math.Abs(dragPoint1.X - dragPoint2.X) > 2 || Math.Abs(dragPoint1.Y - dragPoint2.Y) > 2)
                            {
                                if (!RaiseMouseMoveEvent(e.Location, MouseRowNo, MouseColNo, e.Button))
                                    return;
                            }
                        }

                        if (CurrentRow != MouseRowNo || this.Grid.GridType == GridType.Virtual)
                        {
                            if (MouseRowNo >= 0)
                            {
                                dragPoint2 = e.Location;
                                CurrentRow = MouseRowNo;

                                if (RowCelectionType == RowCelectionType.Multiple && isMouseDown
                                    && dragPoint1 != Point.Empty && dragPoint1 != dragPoint2)
                                {
                                    lastSelectedRow = currentRow;
                                    selectedRecordIndexes.Clear();

                                    if (lastSelectedRow > firstSelectedRow)
                                    {
                                        for (int i = firstSelectedRow; i <= lastSelectedRow; i++)
                                        {
                                            selectedRecordIndexes.Add(i);
                                        }
                                    }
                                    else if (lastSelectedRow < firstSelectedRow)
                                    {
                                        for (int i = lastSelectedRow; i <= firstSelectedRow; i++)
                                        {
                                            selectedRecordIndexes.Add(i);
                                        }
                                    }
                                    else
                                    {
                                        selectedRecordIndexes.Add(currentRow);
                                    }
                                }
                            }

                            Grid.Invalidate();
                        }
                    }
                }

                //if (this.VisibleColumns[CurrentCol].AllowCellMouseEvents)
                //{
                //    RaiseMouseMoveEvent(e.Location, MouseRowNo, MouseColNo);
                //}
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }


        public void ProcessMouseDown(long DataCount, MouseEventArgs e)
        {
            try
            {
                FindCellNo(e.X, e.Y);
                this.Grid.Focus();

                if (MouseRowNo < 0 && MouseRowNo != -10)
                    isHeaderMouseDown = true;

                if (isMouseDownOnMultiSelect)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        firstSelectedRow = MouseRowNo;
                        lastSelectedRow = -1;
                        selectedRecordIndexes.Clear();

                        Grid.Invalidate();
                    }
                }

                isMouseDownOnMultiSelect = false;

                if (MouseRowNo >= -1 & MouseRowNo < DataCount && MouseColNo >= -1 & MouseColNo < VisibleColumns.Count)
                {
                    if (ProcessClickOnGroupHeader(e.X, e.Y, e.Button, MouseRowNo))
                        IsOverGroupingHeader = true;
                    else
                        IsOverGroupingHeader = false;

                    if (MouseRowNo >= 0)
                    {
                        int currentColIndex = CurrentCol;

                        if (Grid.GridType != GridType.Virtual) //For now disabled for Virtual, eventually removed for all grid types
                        {
                            if (MouseRowNo == CurrentRow && e.Button == MouseButtons.Left)
                            {
                                isMouseDown = true;
                                CurrentCol = MouseColNo;

                                if (this.VisibleColumns[MouseColNo].AllowCellMouseEvents)
                                {
                                    RaiseMouseDownEvent(e.Location, MouseRowNo, MouseColNo, e.Button);
                                }

                                //this.selectedRecordIndexes.Clear();
                                if (!this.selectedRecordIndexes.Contains(MouseRowNo))
                                    this.selectedRecordIndexes.Add(MouseRowNo);

                                this.lastSelectedRow = -1;
                                this.firstSelectedRow = MouseRowNo;
                                dragPoint1 = e.Location;

                                Grid.Focus();

                                if (this.VisibleColumns[MouseColNo].IsEditColumn)
                                {
                                    if (e.Button == MouseButtons.Left)
                                    {
                                        Grid.DisplayEditTextBox(MouseRowNo, MouseColNo, e.Location);
                                        Grid.Invalidate();
                                    }
                                }
                                else
                                {
                                    Grid.HideEditTextBox();
                                    Grid.Invalidate();
                                }

                                return;
                            }
                        }

                        if (Grid.GridType == GridType.MultiColumn && currentColIndex != MouseColNo)
                        {
                            firstSelectedRow = -1;
                            lastSelectedRow = -1;
                            this.selectedRecordIndexes.Clear();
                            Grid.Invalidate();
                        }

                        if (this.VisibleColumns[MouseColNo].IsEditColumn)
                        {
                            if (e.Button == MouseButtons.Left)
                            {
                                Grid.DisplayEditTextBox(MouseRowNo, MouseColNo, e.Location);
                                Grid.Invalidate();
                            }
                        }
                        else
                            Grid.HideEditTextBox();


                        bool isMultiChoice = false;

                        if (RowCelectionType == RowCelectionType.Multiple && (e.Button == MouseButtons.Right || Grid.GridType == GridType.DataBound))
                        {
                            if (lastSelectedRow == -1)
                                isMultiChoice = MouseRowNo == firstSelectedRow || selectedRecordIndexes.Contains(MouseRowNo);
                            else
                                isMultiChoice = (MouseRowNo >= firstSelectedRow && MouseRowNo <= lastSelectedRow) || (MouseRowNo <= firstSelectedRow && MouseRowNo >= lastSelectedRow);
                        }


                        if (!isMultiChoice)
                        {
                            isMouseDown = true;
                            dragPoint1 = e.Location;

                            if (this.VisibleColumns[CurrentCol].AllowCellMouseEvents)
                            {
                                RaiseMouseDownEvent(e.Location, MouseRowNo, MouseColNo, e.Button);
                            }

                            CurrentRow = MouseRowNo;

                            isShiftKeyPressed = (Control.ModifierKeys & Keys.Shift) != 0;
                            isCtrlKeyPressed = (Control.ModifierKeys & Keys.Control) != 0;

                            if (e.Button == MouseButtons.Left)
                            {
                                if (!isShiftKeyPressed && !isCtrlKeyPressed)
                                {
                                    firstSelectedRow = MouseRowNo;
                                    lastSelectedRow = -1;
                                    selectedRecordIndexes.Clear();
                                    selectedRecordIndexes.Add(firstSelectedRow);
                                }
                            }
                            else if (e.Button == MouseButtons.Right)
                            {
                                if (isCtrlKeyPressed)
                                {
                                    if (!this.selectedRecordIndexes.Contains(currentRow))
                                        this.selectedRecordIndexes.Add(currentRow);
                                    else
                                    {
                                        if (e.Button != MouseButtons.Right)
                                        {
                                            this.selectedRecordIndexes.Remove(currentRow);
                                            if (firstSelectedRow == currentRow)
                                                firstSelectedRow = this.selectedRecordIndexes[this.selectedRecordIndexes.Count - 1];
                                            currentRow = this.selectedRecordIndexes[this.selectedRecordIndexes.Count - 1];
                                        }
                                    }
                                }
                                else if (isShiftKeyPressed)
                                {
                                    lastSelectedRow = CurrentRow;

                                    selectedRecordIndexes.Clear();

                                    if (lastSelectedRow > firstSelectedRow)
                                    {
                                        for (int i = firstSelectedRow; i < lastSelectedRow; i++)
                                        {
                                            selectedRecordIndexes.Add(i);
                                        }
                                    }
                                    else if (lastSelectedRow < firstSelectedRow)
                                    {
                                        for (int i = lastSelectedRow; i < firstSelectedRow; i++)
                                        {
                                            selectedRecordIndexes.Add(i);
                                        }
                                    }
                                    else
                                    {
                                        selectedRecordIndexes.Add(currentRow);
                                    }
                                }
                                else
                                {
                                    firstSelectedRow = MouseRowNo;
                                    lastSelectedRow = -1;
                                    selectedRecordIndexes.Clear();
                                    selectedRecordIndexes.Add(firstSelectedRow);
                                }
                                Grid.Invalidate();
                            }
                        }
                        else
                        {
                            if (this.Grid.GridType == GridType.Virtual)
                            {
                                dragPoint1 = e.Location;

                                if (e.Button == MouseButtons.Left)
                                    isMouseDown = true;
                                else
                                    isMouseDown = false;
                            }
                            else
                            {
                                isMouseDown = true;
                            }

                            if (this.VisibleColumns[CurrentCol].AllowCellMouseEvents)
                            {
                                RaiseMouseDownEvent(e.Location, MouseRowNo, MouseColNo, e.Button);
                            }

                            if (e.Button == MouseButtons.Left)
                                isMouseDownOnMultiSelect = true;
                            CurrentRow = MouseRowNo;
                        }
                    }
                    else
                    {
                        Grid.HideEditTextBox();
                        if (e.Button == MouseButtons.Left)
                        {
                            if (MouseColNo >= 0)
                            {
                                if (allowdrag)
                                {
                                    int frozenColumnIndex = -1;

                                    //
                                    //TODO : Enable this once frozen columns are implemeted.
                                    //Also check freeze column sort once done
                                    //
                                    //if (!string.IsNullOrEmpty(FrozenColumn))
                                    //{
                                    //    frozenColumnIndex = GetVisibleColumnFromName(FrozenColumn).CurrentPosition + 1;
                                    //}

                                    TableColumn col = VisibleColumns[MouseColNo];

                                    if (this.AllowDrag && col.CurrentPosition >= frozenColumnIndex)
                                    {
                                        if (Grid.GridType == GridType.MultiColumn)
                                        {
                                            if (MouseColNo > 0)
                                                draggingColumn = col;

                                            isHeaderMouseDown = true;
                                            dragPoint1 = e.Location;
                                        }
                                        else
                                        {
                                            draggingColumn = col;
                                            isHeaderMouseDown = true;
                                            dragPoint1 = e.Location;
                                        }
                                    }
                                }

                                if (this.VisibleColumns[CurrentCol].AllowCellMouseEvents)
                                {
                                    RaiseMouseDownEvent(e.Location, MouseRowNo, MouseColNo, e.Button);
                                }
                            }
                        }
                        else
                        {
                            if (e.Button == MouseButtons.Right)
                                OnHeaderRightClick(e);
                        }
                    }

                    if (MouseColNo >= 0 && MouseColNo < VisibleColumns.Count - 1)
                        CurrentCol = MouseColNo;

                    //if (this.VisibleColumns[CurrentCol].AllowCellMouseEvents)
                    //{
                    //    RaiseMouseDownEvent(e.Location, MouseRowNo, MouseColNo, e.Button);
                    //}

                    //this.Grid.SetGridDirty(true);
                    //GricControl.Refresh();
                }
                else
                    Grid.HideEditTextBox();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public TableInfo.GroupHeaderRowLevel GetRootGroup(int rowValue)
        {
            TableInfo.GroupHeaderRowLevel lastValue = null;

            foreach (var item in Grid.GroupHeaderRowIds)
            {
                if (item.RowId <= rowValue && item.EndRowId >= rowValue)
                    lastValue = item;
            }

            return lastValue;
        }

        internal bool IsGroupParentAllExpanded(TableInfo.GroupHeaderRowLevel item)
        {
            bool isAddToList = true;
            var curGroup = item;

            while (curGroup.ParentGroup != null)
            {
                if (curGroup.ParentGroup.Collapsed)
                {
                    isAddToList = false;
                    break;
                }

                curGroup = curGroup.ParentGroup;
            }
            return isAddToList;
        }

        internal TableInfo.GroupHeaderRowLevel GetLastCollapsedGroup(TableInfo.GroupHeaderRowLevel item)
        {
            var curGroup = item;

            while (curGroup.ParentGroup != null)
            {
                if (!curGroup.ParentGroup.Collapsed)
                {
                    return curGroup;
                }

                curGroup = curGroup.ParentGroup;
            }

            return curGroup;
        }

        private bool ProcessClickOnGroupHeader(float X, float Y, MouseButtons mouseButton, int rowIndex = -1, bool doChanges = true)
        {
            if (Grid.GridType == GridType.Virtual)
            {
                if (Grid.VirtualGroupHeaderRowIds.Count > 0)
                {
                    if (Grid.VirtualGroupHeaderRowIds.ContainsKey(rowIndex))
                    {

                        if (mouseButton == MouseButtons.Left)
                        {
                            var curHeader = Grid.VirtualGroupHeaderRowIds[rowIndex];
                            curHeader.IsCollapsed = !curHeader.IsCollapsed;

                            Grid.OnVirtualGroupHeaderChanged();
                        }

                        return true;
                    }
                }
            }

            if (!Grid.IsGroupingEnabled())
                return false;

            if (SelectedGroup != null)
            {
                if (doChanges && mouseButton == MouseButtons.Left)
                {
                    collapseAll = false;
                    SelectedGroup.Collapsed = !SelectedGroup.Collapsed;

                    string titleText = GetNestedStrinValue(SelectedGroup);

                    if (SelectedGroup.Collapsed == true)
                    {
                        if (!Grid.MinimisedGroupList.Contains(titleText))
                            Grid.MinimisedGroupList.Add(titleText);
                    }
                    else
                    {
                        if (Grid.MinimisedGroupList.Contains(titleText))
                            Grid.MinimisedGroupList.Remove(titleText);
                    }
                }

                return true;
            }

            return false;
        }

        private string GetNestedStrinValue(GroupHeaderRowLevel item)
        {
            if (item == null)
                return string.Empty;

            string stringValue = item.TitleText;

            while (item != null && item.ParentGroup != null)
            {
                stringValue += item.ParentGroup.TitleText + "//";
                item = item.ParentGroup;
            }

            return stringValue;
        }


        private void OnHeaderRightClick(MouseEventArgs e)
        {
            try
            {
                if (HeaderRightClicked != null)
                {
                    GridHeaderRightClickEventArgs args = new GridHeaderRightClickEventArgs(e.Location, MouseRowNo, MouseColNo);
                    HeaderRightClicked(this, args);

                    if (!args.Cancel)
                    {
                        //
                        //TODO : Grid Header Menu
                        //
                    }
                }
                else
                {
                    //
                    //TODO : Grid Header Menu
                    //
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void OnGridRecreated()
        {
            try
            {
                if (GridRecreated != null)
                {
                    GridRecreated(this, null);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ProcessMouseLeave()
        {
            if (IsDragging)
            {
                isHeaderMouseDown = false;
                IsDragging = false;
                draggingColumn = null;
                dragPoint1 = Point.Empty;
                dragPoint2 = Point.Empty;

                isMouseDown = false;

                if (MouseRowNo < 0)//Row number will be always below 0. as No Change is done..
                {
                    IsDirty = true;
                    Grid.Invalidate();
                }
            }

        }

        internal bool ChangedSorting = false;

        public void ProcessMouseUp(long DataCount, MouseEventArgs e, bool skipInvalidation = false)
        {
            try
            {
                FindCellNo(e.X, e.Y);

                if (Grid.GridType == GridType.MultiColumn)
                    DataCount = LastRecordIndex + 1;

                if (MouseRowNo >= -1 & MouseRowNo < DataCount && MouseColNo >= -1 & MouseColNo < VisibleColumns.Count)
                {
                    if (MouseRowNo >= 0)
                    {
                        isShiftKeyPressed = (Control.ModifierKeys & Keys.Shift) != 0;
                        isCtrlKeyPressed = (Control.ModifierKeys & Keys.Control) != 0;

                        CurrentRow = MouseRowNo;

                        if (isCtrlKeyPressed)
                        {
                            if (!this.selectedRecordIndexes.Contains(currentRow))
                                this.selectedRecordIndexes.Add(currentRow);
                            else
                            {
                                if (e.Button != MouseButtons.Right)
                                {
                                    this.selectedRecordIndexes.Remove(currentRow);
                                    if (firstSelectedRow == currentRow)
                                        firstSelectedRow = this.selectedRecordIndexes[this.selectedRecordIndexes.Count - 1];
                                    currentRow = this.selectedRecordIndexes[this.selectedRecordIndexes.Count - 1];
                                }
                            }
                        }
                        else if (isShiftKeyPressed)
                        {
                            lastSelectedRow = CurrentRow;

                            selectedRecordIndexes.Clear();

                            if (lastSelectedRow > firstSelectedRow)
                            {
                                for (int i = firstSelectedRow; i < lastSelectedRow; i++)
                                {
                                    selectedRecordIndexes.Add(i);
                                }
                            }
                            else if (lastSelectedRow < firstSelectedRow)
                            {
                                for (int i = lastSelectedRow; i < firstSelectedRow; i++)
                                {
                                    selectedRecordIndexes.Add(i);
                                }
                            }
                            else
                            {
                                selectedRecordIndexes.Add(currentRow);
                            }
                        }
                    }
                    else
                    {
                        if (!IsDragging)
                        {
                            if (e.Button == MouseButtons.Left)
                            {
                                if (MouseColNo >= 0)
                                {
                                    TableColumn col = VisibleColumns[MouseColNo];

                                    if (col.AllowSort)
                                    {
                                        isCtrlKeyPressed = (Control.ModifierKeys & Keys.Control) != 0;

                                        if (isCtrlKeyPressed)
                                        {
                                            if (allowMultipleColumnSort)
                                            {
                                                if (this.Grid.GridType != GridType.MultiColumn)
                                                {
                                                    if (SortedColumnDescriptors[col.MappingName] == null)
                                                    {
                                                        ListSortDirection direction = ListSortDirection.Descending;

                                                        if (col.Type == typeof(string))
                                                            direction = ListSortDirection.Ascending;

                                                        AddSortingDescriptor(col.MappingName, direction, col.IsUnbound, col.ComparisonMethod);
                                                    }
                                                    else
                                                    {
                                                        if (SortedColumnDescriptors[col.MappingName].SortDirection == ListSortDirection.Descending)
                                                            SortedColumnDescriptors[col.MappingName].SortDirection = ListSortDirection.Ascending;
                                                        else if (SortedColumnDescriptors[col.MappingName].SortDirection == ListSortDirection.Ascending)
                                                            SortedColumnDescriptors[col.MappingName].SortDirection = ListSortDirection.Descending;
                                                    }
                                                }
                                                else
                                                {
                                                    int position = MouseColNo;

                                                    for (int i = this.VisibleColumns.Count - 1; i >= 0; i--)
                                                    {
                                                        if (this.VisibleColumns[i].IsPrimaryColumn)
                                                        {
                                                            if (i <= MouseColNo)
                                                            {
                                                                position = i;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    SortColumnDescriptor sortDesc = null;
                                                    bool isSortColumn = false;

                                                    for (int i = SortedColumnDescriptors.Count - 1; i >= 0; i--)
                                                    {
                                                        if (SortedColumnDescriptors[i].Id == col.MappingName + ":" + position)
                                                        {
                                                            isSortColumn = true;
                                                            sortDesc = SortedColumnDescriptors[i];
                                                            break;
                                                        }
                                                    }

                                                    if (isSortColumn)
                                                    {
                                                        if (sortDesc.SortDirection == ListSortDirection.Descending)
                                                            sortDesc.SortDirection = ListSortDirection.Ascending;
                                                        else if (sortDesc.SortDirection == ListSortDirection.Ascending)
                                                            sortDesc.SortDirection = ListSortDirection.Descending;
                                                    }
                                                    else
                                                    {
                                                        ListSortDirection direction = ListSortDirection.Descending;

                                                        if (col.Type == typeof(string))
                                                            direction = ListSortDirection.Ascending;

                                                        AddSortingDescriptor(col.MappingName, col.CurrentPosition, direction, col.IsUnbound, col.ComparisonMethod, false);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ChangedSorting = true;

                                            if (SortedColumnDescriptors[col.MappingName] == null || SortedColumnDescriptors.Count > 1)
                                            {
                                                if (this.Grid.GridType != GridType.MultiColumn)
                                                {
                                                    //if (SortedColumnDescriptors[col.MappingName] != null)
                                                    //{
                                                    //    if (SortedColumnDescriptors[col.MappingName].SortDirection == ListSortDirection.Descending)
                                                    //        SortedColumnDescriptors[col.MappingName].SortDirection = ListSortDirection.Ascending;
                                                    //    else if (SortedColumnDescriptors[col.MappingName].SortDirection == ListSortDirection.Ascending)
                                                    //        SortedColumnDescriptors[col.MappingName].SortDirection = ListSortDirection.Descending;
                                                    //}
                                                    //else
                                                    //{
                                                    if (ChangeSortOnMouseHeaderClick)
                                                    {
                                                        ResetSortedColumns();

                                                        ListSortDirection direction = ListSortDirection.Descending;

                                                        if (col.Type == typeof(string))
                                                            direction = ListSortDirection.Ascending;

                                                        AddSortingDescriptor(col.MappingName, direction, col.IsUnbound, col.ComparisonMethod, false);
                                                        //}
                                                    }
                                                }
                                                else
                                                {
                                                    //ResetSortedColumns();
                                                    if (ChangeSortOnMouseHeaderClick)
                                                    {
                                                        int columnPosition = col.CurrentPosition;
                                                        bool columnFound = false;

                                                        for (int i = this.VisibleColumns.Count - 1; i >= 0; i--)
                                                        {
                                                            if (i <= columnPosition)
                                                                columnFound = true;

                                                            if (columnFound && this.VisibleColumns[i].IsPrimaryColumn)
                                                            {
                                                                columnPosition = i;
                                                                break;
                                                            }
                                                        }

                                                        SortColumnDescriptor sortedColumn = null;
                                                        List<SortColumnDescriptor> toRemove = new List<SortColumnDescriptor>();

                                                        for (int i = 0; i < SortedColumnDescriptors.Count; i++)
                                                        {
                                                            string id = SortedColumnDescriptors[i].Id;

                                                            if (!string.IsNullOrEmpty(id))
                                                            {
                                                                string[] arr = id.Split(':');
                                                                if (arr.Length > 1 && arr[0] != col.MappingName && Convert.ToInt32(arr[1]) >= columnPosition && Convert.ToInt32(arr[1]) <= col.CurrentPosition)
                                                                    toRemove.Add(SortedColumnDescriptors[i]);

                                                                if (arr.Length > 1 && arr[0] == col.MappingName && Convert.ToInt32(arr[1]) >= columnPosition && Convert.ToInt32(arr[1]) <= col.CurrentPosition)
                                                                    sortedColumn = SortedColumnDescriptors[i];
                                                            }
                                                        }

                                                        if (sortedColumn != null)
                                                        {
                                                            if (sortedColumn.SortDirection == ListSortDirection.Descending)
                                                                sortedColumn.SortDirection = ListSortDirection.Ascending;
                                                            else if (sortedColumn.SortDirection == ListSortDirection.Ascending)
                                                                sortedColumn.SortDirection = ListSortDirection.Descending;
                                                        }
                                                        else
                                                        {
                                                            for (int i = 0; i < toRemove.Count; i++)
                                                            {
                                                                for (int j = 0; j < this.allRecords.Count; j++)
                                                                {
                                                                    Record rec = allRecords[j] as Record;
                                                                    if (rec != null && rec.SortKeys.Contains(toRemove[i].Accessor))
                                                                    {
                                                                        rec.SortKeys.Remove(toRemove[i].Accessor);
                                                                        //rec.UnboundColumn = null;
                                                                    }
                                                                }

                                                                this.SortedColumnDescriptors.Remove(toRemove[i]);
                                                                alreadyHasHiddenSortDescriptorInCollection = HiddenDefaultSortColumnDescriptor != null && this.SortedColumnDescriptors[HiddenDefaultSortColumnDescriptor.Name] != null;
                                                            }

                                                            ListSortDirection direction = ListSortDirection.Descending;

                                                            if (col.Type == typeof(string))
                                                                direction = ListSortDirection.Ascending;

                                                            AddSortingDescriptor(col.MappingName, col.CurrentPosition, direction, col.IsUnbound, col.ComparisonMethod, false);
                                                            //AddSortingDescriptor(col.MappingName, direction, col.IsUnbound, col.ComparisonMethod);
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                int position = MouseColNo;

                                                for (int i = 0; i < this.VisibleColumns.Count; i++)
                                                {
                                                    if (this.VisibleColumns[i].IsPrimaryColumn)
                                                    {
                                                        if (i <= MouseColNo)
                                                            position = i;
                                                    }
                                                }

                                                if (ChangeSortOnMouseHeaderClick)
                                                {
                                                    if (SortedColumnDescriptors[col.MappingName].Id.Contains(position.ToString()))
                                                    {
                                                        if (SortedColumnDescriptors[col.MappingName].SortDirection == ListSortDirection.Descending)
                                                            SortedColumnDescriptors[col.MappingName].SortDirection = ListSortDirection.Ascending;
                                                        else if (SortedColumnDescriptors[col.MappingName].SortDirection == ListSortDirection.Ascending)
                                                            SortedColumnDescriptors[col.MappingName].SortDirection = ListSortDirection.Descending;
                                                    }
                                                    else
                                                    {
                                                        ListSortDirection direction = ListSortDirection.Descending;

                                                        if (col.Type == typeof(string))
                                                            direction = ListSortDirection.Ascending;

                                                        AddSortingDescriptor(col.MappingName, col.CurrentPosition, direction, col.IsUnbound, col.ComparisonMethod, false);
                                                        //SortedColumnDescriptors[col.MappingName].Id = col.MappingName + ":" + position;
                                                    }
                                                }
                                            }
                                        }

                                        this.Grid.SetGridDirty(true);
                                        rowIndexSelectedBeforeSorting = CurrentRow;
                                    }
                                    else
                                    {
                                        skipInvalidation = true;
                                    }
                                }
                            }
                            else if (e.Button == MouseButtons.Middle)
                            {
                                ResetSortedColumns();
                            }
                            else if (e.Button == MouseButtons.Right)
                            {
                                skipInvalidation = true;
                            }
                        }
                        else
                        {
                            if (allowdrag && draggingColumn != null)
                            {
                                if (Grid.GridType == GridType.MultiColumn)
                                {
                                    if (MouseColNo != 0 && !draggingColumn.IsPrimaryColumn)
                                    {
                                        int currentIndex = VisibleColumns.IndexOf(draggingColumn);
                                        int primaryColumn = -1;
                                        bool proceedWithColumnChange = true;

                                        for (int i = VisibleColumns.Count - 1; i >= 0; i--)
                                        {
                                            if (VisibleColumns[i].IsPrimaryColumn)
                                            {
                                                primaryColumn = i;

                                                if (currentIndex > MouseColNo)
                                                {
                                                    if (i >= MouseColNo && i <= currentIndex)
                                                    {
                                                        proceedWithColumnChange = false;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    if (i <= MouseColNo && i >= currentIndex)
                                                    {
                                                        proceedWithColumnChange = false;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        if (proceedWithColumnChange)
                                            ChangeColumnPosition(draggingColumn, MouseColNo);
                                    }
                                }
                                else
                                {
                                    bool proceedWithColumnChange = true;

                                    if (!string.IsNullOrEmpty(FrozenColumn))
                                    {
                                        if (draggingColumn.Name == this.FrozenColumn)
                                            proceedWithColumnChange = false;

                                        int currentIndex = VisibleColumns.IndexOf(draggingColumn);
                                        int freezeColumnIndex = GetVisibleColumnFromName(this.FrozenColumn).CurrentPosition;
                                        int droppingIndex = MouseColNo;

                                        if (freezeColumnIndex > currentIndex)
                                        {
                                            if (droppingIndex >= freezeColumnIndex)
                                                proceedWithColumnChange = false;
                                        }
                                        else
                                        {
                                            if (freezeColumnIndex >= droppingIndex)
                                                proceedWithColumnChange = false;
                                        }
                                    }

                                    if (proceedWithColumnChange)
                                        ChangeColumnPosition(draggingColumn, MouseColNo);
                                }

                                isHeaderMouseDown = false;
                                IsDragging = false;
                                draggingColumn = null;

                                this.Grid.SetGridDirty(true);
                                Grid.Refresh();
                            }
                        }

                        this.selectedRecordIndexes.Clear();
                        this.selectedRecords.Clear();
                    }

                    CurrentCol = MouseColNo;

                    if (isMouseDownOnMultiSelect)
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            if (MouseRowNo > firstSelectedRow && this.Grid.GridType == GridType.Virtual)
                            {
                                lastSelectedRow = MouseRowNo;
                            }
                            else if (firstSelectedRow > MouseRowNo && this.Grid.GridType == GridType.Virtual)
                            {
                                lastSelectedRow = firstSelectedRow;
                                firstSelectedRow = MouseRowNo;
                            }
                            else
                            {
                                firstSelectedRow = MouseRowNo;
                                lastSelectedRow = -1;
                            }
                            selectedRecordIndexes.Clear();
                            isMouseDownOnMultiSelect = false;

                            if (!skipInvalidation)
                                Grid.Invalidate();
                        }
                    }

                    if (this.VisibleColumns[CurrentCol].AllowCellMouseEvents)
                    {
                        RaiseMouseUpEvent(e.Location, MouseRowNo, MouseColNo, e.Button);
                    }

                    if (!skipInvalidation)
                        Grid.Invalidate();
                }
                else
                {
                    //this.selectedRecordIndexes.Clear();
                    //this.SelectedRecords.Clear();
                    //lastSelectedRow = -1;
                    //firstSelectedRow = -1;
                    //Grid.Invalidate();
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                isHeaderMouseDown = false;
                IsDragging = false;
                draggingColumn = null;
                dragPoint1 = Point.Empty;
                dragPoint2 = Point.Empty;

                isMouseDown = false;

                if (!skipInvalidation)
                {
                    if (MouseRowNo < 0 && MouseRowNo != -15)
                    {
                        IsDirty = true;
                        Grid.Invalidate();
                    }
                }
            }
        }

        public void ProcessMouseClick(MouseEventArgs e)
        {
            try
            {
                FindCellNo(e.X, e.Y);

                if (!this.VisibleColumns[CurrentCol].AllowCellMouseEvents)
                    return;

                if (MouseRowNo < 0 || MouseColNo < 0)
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        if (MouseRowNo == -1)
                        {
                            //Do Nothing
                        }
                        else
                        {
                            if (MouseOuterRightClick != null)
                            {
                                MouseOuterRightClick(this, null);
                            }
                        }
                    }

                    return;
                }

                if (this.Grid.GridType == GridType.DataBound)
                    currentCell = this.cellMatrix[currentRow, currentCol];

                RaiseMouseClickEvent(e.Location, currentRow, currentCol, e.Button);

                if (this.Grid.GridType == GridType.Virtual)
                    return;

                if (CurrentCell.IsPushButton)
                {
                    CellStruct structure = this.cellMatrix[MouseRowNo, MouseColNo];

                    if (structure.Record != null)
                        RaiseCellButtonClickEvent(e.Location, MouseRowNo, MouseColNo, null, e.Button);
                }
                else if (this.VisibleColumns[CurrentCol].ShowButtons)
                {
                    string cellModel = string.Empty;

                    if (cellMatrix.GetLength(0) > MouseRowNo)
                        cellModel = cellMatrix[MouseRowNo, MouseColNo].CellModelType;
                    else if (cellMatrix.GetLength(0) > 0)
                        cellModel = cellMatrix[0, MouseColNo].CellModelType;

                    Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers.CellRendererBase renderer = Grid.CellRenderers[cellModel];

                    if (renderer != null && renderer.Buttons.Count > 0)
                    {
                        Rectangle cellRectangle = new Rectangle();

                        cellRectangle.X = VisibleColumns[MouseColNo].Left;// +2;
                        cellRectangle.Y = HeaderHeight + ((MouseRowNo - TopRow) * RowHeight);
                        cellRectangle.Width = VisibleColumns[MouseColNo].CellWidth;
                        cellRectangle.Height = RowHeight;

                        Rectangle[] buttonsBounds = new Rectangle[renderer.Buttons.Count];

                        renderer.PerformCellLayout(MouseRowNo, MouseColNo, cellMatrix[MouseRowNo, MouseColNo].Style, cellRectangle);
                        renderer.OnLayout(MouseRowNo, MouseColNo, cellMatrix[MouseRowNo, MouseColNo].Style, cellRectangle, buttonsBounds);

                        for (int i = 0; i < renderer.Buttons.Count; i++)
                        {
                            if (renderer.GetButton(i).Visible)
                            {
                                renderer.GetButton(i).Bounds = buttonsBounds[i];
                                if ((renderer.Buttons[i] as GridCellButton).Bounds.Contains(e.Location))
                                {
                                    RaiseCellButtonClickEvent(e.Location, currentRow, currentCol, (renderer.Buttons[i] as GridCellButton), e.Button);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ProcessMouseDoubleClick(MouseEventArgs e)
        {
            try
            {
                FindCellNo(e.X, e.Y);

                if (!this.VisibleColumns[CurrentCol].AllowCellMouseEvents)
                    return;

                if (MouseColNo < 0 || MouseRowNo < 0)
                    return;

                if (ProcessClickOnGroupHeader(e.X, e.Y, e.Button, MouseRowNo, false))
                    return;

                RaiseMouseDoubleClickEvent(e.Location, MouseRowNo, MouseColNo, e.Button);

                //if (this.VisibleColumns[CurrentCol].ShowButtons)
                //{
                //    string cellModel = cellMatrix[MouseRowNo, MouseColNo].CellModelType;

                //    Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers.CellRendererBase renderer = Grid.CellRenderers[cellModel];

                //    if (renderer != null && renderer.Buttons.Count > 0)
                //    {
                //        Rectangle cellRectangle = new Rectangle();

                //        cellRectangle.X = VisibleColumns[MouseColNo].Left;// +2;
                //        cellRectangle.Y = HeaderHeight + ((MouseRowNo - TopRow) * RowHeight);
                //        cellRectangle.Width = VisibleColumns[MouseColNo].CellWidth;
                //        cellRectangle.Height = RowHeight;

                //        Rectangle[] buttonsBounds = new Rectangle[renderer.Buttons.Count];

                //        renderer.PerformCellLayout(MouseRowNo, MouseColNo, cellMatrix[MouseRowNo, MouseColNo].Style, cellRectangle);
                //        renderer.OnLayout(MouseRowNo, MouseColNo, cellMatrix[MouseRowNo, MouseColNo].Style, cellRectangle, buttonsBounds);

                //        bool continueRaise = true;

                //        for (int i = 0; i < renderer.Buttons.Count; i++)
                //        {
                //            if (renderer.GetButton(i).Visible)
                //            {
                //                renderer.GetButton(i).Bounds = buttonsBounds[i];
                //                if ((renderer.Buttons[i] as GridCellButton).Bounds.Contains(e.Location))
                //                {
                //                    continueRaise = false;
                //                }
                //            }
                //        }

                //        if (continueRaise)
                //            RaiseMouseDoubleClickEvent(e.Location, MouseRowNo, MouseColNo, e.Button);
                //    }
                //}
                //else
                //{
                //    RaiseMouseDoubleClickEvent(e.Location, MouseRowNo, MouseColNo, e.Button);
                //}
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ProcessKeyUp(KeyEventArgs e)
        {
            isCtrlKeyPressed = false;
            isShiftKeyPressed = false;
        }

        public void ProcessKeyDown(KeyEventArgs e)
        {
            if (e.Control)
                isCtrlKeyPressed = true;
        }

        public bool ProcessMouseWheel(int DataCount, MouseEventArgs e)
        {
            bool Refresh = false;

            if (Grid.GridType == GridType.Virtual)
            {
                DataCount = SourceDataRowCount;
            }

            try
            {
                if (DataCount > 0)
                {
                    if (e.Delta < 0)
                    {
                        if (TopRow < DataCount - DisplayRows)
                            TopRow = TopRow + 1;
                    }
                    else if (e.Delta > 0)
                    {
                        if (TopRow > 0)
                        {
                            TopRow -= 1;
                            if (TopRow == 0)
                                Refresh = true;
                        }
                    }
                }
                return Refresh;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
                return Refresh;
            }
        }

        private void RaiseMouseDownEvent(Point location, int rowIndex, int colIndex, MouseButtons button)
        {
            if (!listenToMouseEvents)
                return;

            try
            {
                if (MouseDown != null)
                    MouseDown(this, new GridCellMouseDownEventArgs(location, rowIndex, colIndex, button));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void RaiseMouseUpEvent(Point location, int rowIndex, int colIndex, MouseButtons button)
        {
            if (!listenToMouseEvents)
                return;

            try
            {
                if (MouseUp != null)
                    MouseUp(this, new GridCellMouseUpEventArgs(location, rowIndex, colIndex, button));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void RaiseMouseLeaveEvent(Point location, int rowIndex, int colIndex)
        {
            if (!listenToMouseEvents)
                return;

            try
            {
                if (MouseHoverLeave != null)
                    MouseHoverLeave(this, new GridCellMouseHoverLeaveEventArgs(location, rowIndex, colIndex));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private string RaiseMouseMoveEventNewForToolTip(Point location, int rowIndex, int colIndex, MouseButtons button, string toolTipText = null)
        {
            if (!listenToMouseEvents)
                return string.Empty;

            try
            {
                GridCellMouseHoverEventArgs args = new GridCellMouseHoverEventArgs(location, rowIndex, colIndex, button);

                if (MouseHover != null)
                    MouseHover(this, args);

                return args.ToolTipString;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return string.Empty;
        }

        private bool RaiseMouseMoveEvent(Point location, int rowIndex, int colIndex, MouseButtons button, string toolTipText = null)
        {
            if (!listenToMouseEvents)
                return false;

            try
            {
                GridCellMouseHoverEventArgs args = new GridCellMouseHoverEventArgs(location, rowIndex, colIndex, button);

                if (MouseHover != null)
                    MouseHover(this, args);

                return !args.Cancel;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return false;
        }

        private void RaiseMouseDoubleClickEvent(Point location, int rowIndex, int colIndex, MouseButtons button)
        {
            if (!listenToMouseEvents)
                return;

            try
            {
                if (MouseDoubleClick != null)
                    MouseDoubleClick(this, new GridCellMouseDoubleClickEventArgs(location, rowIndex, colIndex, button));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void RaiseMouseClickEvent(Point location, int rowIndex, int colIndex, MouseButtons button)
        {
            if (!listenToMouseEvents)
                return;

            try
            {
                if (MouseClick != null)
                    MouseClick(this, new GridCellMouseClickEventArgs(location, rowIndex, colIndex, button));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void RaiseCellButtonClickEvent(Point location, int rowIndex, int colIndex, GridCellButton button, MouseButtons mouseButton)
        {
            if (!listenToMouseEvents)
                return;

            try
            {
                if (CellButtonClick != null)
                    CellButtonClick(this, new GridCellButtonClickEventArgs(location, rowIndex, colIndex, button, mouseButton));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void RaiseCurrentCellChangingEvent(int rowIndex, int colIndex, TableInfo.CellStruct cell)
        {
            if (!listenToSelectionChangedEvents)
                return;

            try
            {
                if (CellChanging != null)
                    CellChanging(this, new GridCurrentCellChangingEventArgs(rowIndex, colIndex, cell));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void RaiseCurrentCellChangedEvent(int rowIndex, int colIndex, TableInfo.CellStruct cell)
        {
            if (!listenToSelectionChangedEvents || Grid.RecreateMatrixFlag)
                return;

            try
            {
                if (CellChanged != null)
                    CellChanged(this, new GridCurrentCellChangedEventArgs(rowIndex, colIndex, cell));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void RaiseSelectedRecordChangingEvent(int rowIndex, Record rec)
        {
            if (!listenToSelectionChangedEvents)
                return;

            try
            {
                if (SelectedRecordChanging != null)
                    SelectedRecordChanging(this, new GridSelectedRecordChangingEventArgs(rowIndex, rec));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void RaiseSelectedRecordChangedEvent(int rowIndex, Record rec)
        {
            if (!listenToSelectionChangedEvents)
                return;

            try
            {
                if (SelectedRecordChanged != null)
                    SelectedRecordChanged(this, new GridSelectedRecordChangedEventArgs(rowIndex, rec));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        #endregion

        #region Inner Classes

        public class GridGroup
        {
            public ListSortDirection SortDirection = ListSortDirection.Ascending;
            public string Name = string.Empty;
        }

        public class VirtualGroupHeader
        {
            public string Title { get; set; }
            public bool IsCollapsed { get; set; }
        }

        public class GroupHeaderRowLevel
        {
            public int RowId { get; set; }
            public int EndRowId { get; set; }
            public int RowLevel { get; set; }
            public string TitleText { get; set; }
            public GroupHeaderRowLevel ParentGroup;

            private bool collapsed;

            public bool Collapsed
            {
                get { return collapsed; }
                set
                {
                    collapsed = value;
                    CollapsedForProsessing = value;
                }
            }

            internal bool CollapsedForProsessing { get; set; }//Only used for Processing
            public Record FirstRecord { get; set; }
            private ListSortDirection direction = ListSortDirection.Ascending;

            public ListSortDirection Direction
            {
                get { return direction; }
                set { direction = value; }
            }

            public GroupHeaderRowLevel(int id, int level, string titleText, Record firstRecord)
            {
                RowId = id;
                RowLevel = level;
                TitleText = titleText;
                FirstRecord = firstRecord;
            }
        }

        [Serializable]
        public class TableColumn : ICloneable
        {
            public int MINIMUM_WIDTH = 20;
            private string name = string.Empty;

            public string Name
            {
                get
                {
                    if (!string.IsNullOrEmpty(name))
                        return name;
                    else
                        return mappingName;
                }
                set
                {
                    name = value;
                }
            }

            private object tag = null;

            public object Tag
            {
                get { return tag; }
                set { tag = value; }
            }

            private String headerTooltipValue = String.Empty;

            public String HeaderTooltipValue
            {
                get { return headerTooltipValue; }
                set { headerTooltipValue = value; }
            }

            private bool allowBlink = false;

            public bool AllowBlink
            {
                get { return allowBlink; }
                set
                {
                    allowBlink = value;
                }
            }

            private bool allowDrag = false;

            public bool AllowDrag
            {
                get { return allowDrag; }
                set
                {
                    allowDrag = value;
                }
            }

            private bool allowCellMouseEvents = true;

            public bool AllowCellMouseEvents
            {
                get { return allowCellMouseEvents; }
                set
                {
                    allowCellMouseEvents = value;
                }
            }

            private bool allowCellMouseHoverEvents = false;

            public bool AllowCellMouseHoverEvents
            {
                get { return allowCellMouseHoverEvents && AllowCellMouseEvents; }
                set
                {
                    allowCellMouseHoverEvents = value;
                }
            }

            private bool isFrozen = false;

            public bool IsFrozen
            {
                get { return isFrozen; }
                set
                {
                    isFrozen = value;
                }
            }

            private bool isAutoResized = true;

            public bool IsAutoResized
            {
                get { return isAutoResized; }
                set
                {
                    isAutoResized = value;
                }
            }

            private bool allowEdit = false;

            public bool AllowEdit
            {
                get { return allowEdit; }
                set
                {
                    allowEdit = value;
                }
            }

            private bool isEditColumn = false;

            public bool IsEditColumn
            {
                get { return isEditColumn && allowEdit; }
                set
                {
                    isEditColumn = value;

                    if (isEditColumn)
                        allowEdit = value;
                }
            }

            private bool allowEqualValueBlink = false;

            public bool AllowEqualValueBlink
            {
                get { return allowEqualValueBlink && this.allowBlink; }
                set
                {
                    allowEqualValueBlink = value;
                }
            }

            private bool allowSort = true;

            public bool AllowSort
            {
                get
                {
                    if (allowSort)
                    {
                        if (!this.Table.AllowSort)
                            return false;

                        if (this.type == null)
                            return false;
                        else
                            return allowSort;
                    }
                    else
                    {
                        return false;
                    }
                }
                set
                {
                    allowSort = value;
                }
            }

            private bool isCustomColumn = false;

            public bool IsCustomColumn
            {
                get { return isCustomColumn || (this.cellType != "Static" && this.cellType != "PushButton"); }
                set
                {
                    isCustomColumn = value;
                }
            }

            //private bool isCustomFormulaColumn = false;

            //public bool IsCustomFormulaColumn
            //{
            //    get { return isCustomFormulaColumn; }
            //    set
            //    {
            //        isCustomFormulaColumn = value;
            //    }
            //}

            private bool checkMinimumWidth = true;

            public bool CheckMinimumWidth
            {
                get { return checkMinimumWidth; }
                set
                {
                    checkMinimumWidth = value;
                }
            }

            private Dictionary<int, MergeCellInfo> mergedCells;

            public Dictionary<int, MergeCellInfo> MergedCells
            {
                get
                {
                    if (mergedCells == null)
                        mergedCells = new Dictionary<int, MergeCellInfo>();

                    return mergedCells;
                }
                set { mergedCells = value; }
            }

            private bool isPrimaryColumn = false;

            public bool IsPrimaryColumn
            {
                get { return isPrimaryColumn; }
                set
                {
                    isPrimaryColumn = value;
                }
            }

            private int id = -2;

            public int Id
            {
                get { return id; }
                set
                {
                    id = value;

                    if (id == -1 && !isUnbound)
                        IsUnbound = true;
                }
            }

            private int currentPosition = -1;

            public int CurrentPosition
            {
                get { return currentPosition; }
                set
                {
                    currentPosition = value;
                }
            }

            public int left = 0;

            public int Left
            {
                get { return left; }
                set
                {
                    left = value;
                }
            }

            private GridStyleInfo columnStyle;

            public GridStyleInfo ColumnStyle
            {
                get
                {
                    if (columnStyle == null)
                        columnStyle = GridStyleInfo.GetDefaultStyle();
                    return columnStyle;
                }
                set
                {
                    columnStyle = value;
                }
            }

            public CellStructType CellType;

            private Type type = typeof(string);

            public Type Type
            {
                get { return type; }
                set
                {
                    type = value;

                    CellType = new CellStructType();

                    if (type == typeof(double) || type == typeof(double?) || type == typeof(float))
                        CellType |= CellStructType.Double;
                    else if (type == typeof(long) || type == typeof(long?))
                        CellType |= CellStructType.Long;
                    else if (type == typeof(int) || type == typeof(int?))
                        CellType |= CellStructType.Integer;
                    else if (type == typeof(DateTime) || type == typeof(DateTime?))
                        CellType |= CellStructType.DateTime;
                    else if (type == typeof(String))
                        CellType |= CellStructType.String;
                    else
                        CellType |= CellStructType.Style;

                    if (this.ColumnStyle != null && type != null)
                    {
                        if (type == typeof(string))
                        {
                            if (this.columnStyle.HorizontalAlignment == GridHorizontalAlignment.Right)
                                this.columnStyle.HorizontalAlignment = GridHorizontalAlignment.Left;
                        }
                        else if (this.columnStyle.HorizontalAlignment == GridHorizontalAlignment.Left)
                        {
                            this.columnStyle.HorizontalAlignment = GridHorizontalAlignment.Right;
                        }
                    }
                }
            }

            private string cellType = Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes.CellType.Static;

            public string CellModelType
            {
                get { return cellType; }
                set
                {
                    cellType = value;

                    if (this.Table != null)
                    {
                        for (int i = 0; i < this.Table.CellMatrix.GetLength(0); i++)
                        {
                            this.Table.Grid.SetPushButton(i, this.currentPosition, cellType == Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes.CellType.PushButton);
                        }
                    }
                }
            }

            private string mappingName = string.Empty;

            public string MappingName
            {
                get
                {
                    if (!string.IsNullOrEmpty(mappingName))
                        return mappingName;
                    return name;
                }
                set
                {
                    mappingName = value;
                }
            }

            private string displayName = string.Empty;

            public string DisplayName
            {
                get
                {
                    if (string.IsNullOrEmpty(displayName))
                        return Name;

                    return displayName;
                }
                set { displayName = value; }
            }

            private int cellWidth = 100;

            public int CellWidth
            {
                get
                {
                    return cellWidth;
                }
                set
                {
                    if (value < MINIMUM_WIDTH && checkMinimumWidth)
                    {
                        cellWidth = MINIMUM_WIDTH;
                    }
                    else
                        cellWidth = value;
                }
            }

            private int designWidth = 0;

            public int DesignWidth
            {
                get
                {
                    if (designWidth <= 0)
                        return cellWidth;

                    return designWidth;
                }
                set
                {
                    designWidth = value;
                }
            }

            private bool isUnbound = false;

            public bool IsUnbound
            {
                get { return isUnbound; }
                set
                {
                    isUnbound = value;
                }
            }

            private bool queryStyle = false;

            public bool QueryStyle
            {
                get { return queryStyle; }
                set
                {
                    if (queryStyle != value)
                    {
                        queryStyle = value;
                    }
                }
            }

            private bool showButtons = false;

            public bool ShowButtons
            {
                get { return showButtons; }
                set { showButtons = value; }
            }

            private RightToLeft rightToLeft = RightToLeft.No;

            public RightToLeft RightToLeft
            {
                get { return rightToLeft; }
                set { rightToLeft = value; }
            }

            private bool isHighlightEnable = false;

            public bool IsHighlightEnable
            {
                get { return isHighlightEnable; }
                set { isHighlightEnable = value; }
            }

            private Dictionary<string, Color> highlightTextCollection;

            public Dictionary<string, Color> HighlightTextCollection
            {
                get { return highlightTextCollection; }
                set { highlightTextCollection = value; }
            }

            public TableInfo Table = null;

            public SortComparisonMethod ComparisonMethod = SortComparisonMethod.Common;

            public TableColumn()
            {
            }

            public TableColumn(TableInfo tableInfo)
            {
                this.Table = tableInfo;
            }

            public TableColumn(TableInfo tableInfo, string name, string mappingName, string displayName, int width)
            {
                this.Table = tableInfo;
                this.name = name;
                this.mappingName = mappingName;
                this.displayName = displayName;
                this.CellWidth = width;
            }

            public void RecreateColumnStyle()
            {
                columnStyle = GridStyleInfo.GetDefaultStyle(true);
            }

            #region ICloneable Members

            public object Clone()
            {
                //
                //Using this method is costly. It uses Reflection. Instead used Manual Clone.
                //
                //TableColumn tc = ObjectCloneHelper.Clone<TableColumn>(this);

                //return tc;

                TableColumn tc = new TableColumn(Table)
                {
                    name = this.name,
                    allowBlink = this.allowBlink,
                    AllowEqualValueBlink = this.AllowEqualValueBlink,
                    allowDrag = this.allowDrag,
                    allowEdit = this.allowEdit,
                    isFrozen = this.isFrozen,
                    Table = this.Table,
                    queryStyle = this.queryStyle,
                    isUnbound = this.isUnbound,
                    designWidth = this.designWidth,
                    cellWidth = this.cellWidth,
                    MappingName = this.MappingName,
                    DisplayName = this.DisplayName,
                    HeaderTooltipValue = this.HeaderTooltipValue,
                    IsHighlightEnable = this.IsHighlightEnable,
                    HighlightTextCollection = this.HighlightTextCollection,
                    cellType = this.cellType,
                    type = this.type,
                    columnStyle = this.columnStyle,
                    Left = this.Left,
                    currentPosition = this.currentPosition,
                    id = this.id,
                    isCustomColumn = this.isCustomColumn,
                    allowSort = this.allowSort,
                    allowEqualValueBlink = this.allowEqualValueBlink,
                    isAutoResized = this.isAutoResized,
                    showButtons = this.ShowButtons,
                    ComparisonMethod = this.ComparisonMethod,
                    AllowCellMouseEvents = this.AllowCellMouseEvents,
                    AllowCellMouseHoverEvents = this.AllowCellMouseHoverEvents,
                    //IsCustomFormulaColumn = this.IsCustomFormulaColumn,
                    Tag = this.Tag
                };

                return tc;
            }

            #endregion
        }

        [Serializable]
        public struct CellStruct
        {
            private string txt;

            public string TextString
            {
                get
                {
                    return txt;
                }
                set
                {
                    if (isEmpty || !string.Equals(txt, value))
                    {
                        txt = value;

                        if (this.Column != null && this.Column.AllowBlink && LastUpdatedTime > 0)
                        {
                            //
                            //Temp Comment
                            //
                            //if (this.Column.AllowEqualValueBlink)
                            //{
                            //    this.isBlinkCell = true;
                            //    this.BlinkType = BlinkType.Equal;
                            //}
                        }

                        IsDirty = true;
                    }
                }
            }

            private int integer;

            public int TextInt
            {
                get
                {
                    return integer;
                }
                set
                {
                    if (isEmpty || integer != value)
                    {
                        IsDirty = true;

                        if (this.Column.AllowBlink && (LastUpdatedTime > 0 || (LastUpdatedTime >= 0 && !isEmpty)))
                        {
                            this.isBlinkCell = true;

                            if (BlinkType == GridGrouping.BlinkType.Equal && this.Column.AllowEqualValueBlink)
                            {
                                //
                                //No need to set blink type here for existing equal type
                                //
                            }
                            else
                            {
                                if (integer < value)
                                    this.BlinkType = BlinkType.Up;
                                else if (integer > value)
                                    this.BlinkType = BlinkType.Down;
                                else
                                {
                                    //
                                    //NOTE : May need to check for Allowequalvalueblink here
                                    //
                                    this.BlinkType = BlinkType.None;
                                }
                            }
                        }

                        integer = value;

                    }
                }
            }

            private double doubleValue;

            public double TextDouble
            {
                get
                {
                    return doubleValue;
                }
                set
                {
                    if (isEmpty || doubleValue != value)
                    {
                        IsDirty = true;

                        if (this.Column.AllowBlink && (LastUpdatedTime > 0 || (LastUpdatedTime >= 0 && !isEmpty)))
                        {
                            this.isBlinkCell = true;

                            if (BlinkType == GridGrouping.BlinkType.Equal && this.Column.AllowEqualValueBlink)
                            {
                                //
                                //No need to set blink type here for existing equal type
                                //
                            }
                            else
                            {
                                if (doubleValue < value)
                                {
                                    this.BlinkType = BlinkType.Up;
                                }
                                else if (doubleValue > value)
                                {
                                    this.BlinkType = BlinkType.Down;
                                }
                                else
                                {
                                    //
                                    //NOTE : May need to check for Allowequalvalueblink here
                                    //
                                    this.BlinkType = BlinkType.None;
                                }
                            }
                        }

                        doubleValue = value;

                    }
                }
            }

            private long longValue;

            public long TextLong
            {
                get
                {
                    return longValue;
                }
                set
                {
                    if (isEmpty || longValue != value)
                    {
                        IsDirty = true;

                        if (this.Column.AllowBlink && (LastUpdatedTime > 0 || (LastUpdatedTime >= 0 && !isEmpty)))
                        {
                            this.isBlinkCell = true;

                            if (BlinkType == GridGrouping.BlinkType.Equal && this.Column.AllowEqualValueBlink)
                            {
                                //
                                //No need to set blink type here for existing equal type
                                //
                            }
                            else
                            {
                                if (longValue < value)
                                    this.BlinkType = BlinkType.Up;
                                else if (longValue > value)
                                    this.BlinkType = BlinkType.Down;
                                else
                                {
                                    //
                                    //NOTE : May need to check for Allowequalvalueblink here
                                    //
                                    this.BlinkType = BlinkType.None;
                                }
                            }
                        }

                        longValue = value;
                    }
                }
            }

            private decimal decimalValue;

            public decimal TextDecimal
            {
                get
                {
                    return decimalValue;
                }
                set
                {
                    if (isEmpty || decimalValue != value)
                    {
                        IsDirty = true;

                        if (this.Column.AllowBlink && (LastUpdatedTime > 0 || (LastUpdatedTime >= 0 && !isEmpty)))
                        {
                            this.isBlinkCell = true;

                            if (BlinkType == GridGrouping.BlinkType.Equal && this.Column.AllowEqualValueBlink)
                            {
                                //
                                //No need to set blink type here for existing equal type
                                //
                            }
                            else
                            {
                                if (decimalValue < value)
                                    this.BlinkType = BlinkType.Up;
                                else if (decimalValue > value)
                                    this.BlinkType = BlinkType.Down;
                                else
                                {
                                    //
                                    //NOTE : May need to check for Allowequalvalueblink here
                                    //
                                    this.BlinkType = BlinkType.None;
                                }
                            }
                        }

                        decimalValue = value;
                    }
                }
            }

            private DateTime dateTimeValue;

            public DateTime TextDateTime
            {
                get
                {
                    return dateTimeValue;
                }
                set
                {
                    if (isEmpty || dateTimeValue == null || dateTimeValue.Ticks != value.Ticks)
                    {
                        dateTimeValue = value;
                        IsDirty = true;
                    }
                }
            }

            public CellStructType CellStructType;

            public BlinkType BlinkType;

            private Type type;

            public Type Type
            {
                get { return type; }
                set
                {
                    type = value;

                    CellStructType = new CellStructType();

                    if (type == typeof(double) || type == typeof(double?))
                        CellStructType |= CellStructType.Double;
                    else if (type == typeof(long) || type == typeof(long?))
                        CellStructType |= CellStructType.Long;
                    else if (type == typeof(int) || type == typeof(int?))
                        CellStructType |= CellStructType.Integer;
                    else if (type == typeof(decimal) || type == typeof(decimal?))
                        CellStructType |= CellStructType.Decimal;
                    else if (type == typeof(DateTime) || type == typeof(DateTime?))
                    {
                        CellStructType |= CellStructType.DateTime;
                        isNumeric = false;
                    }
                    else if (type == typeof(String))
                    {
                        CellStructType |= CellStructType.String;
                        isNumeric = false;
                    }
                    else
                        CellStructType |= CellStructType.Style;
                }
            }

            private bool isDirty;

            public bool IsDirty
            {
                get { return isDirty || this.cellModelType == CellType.Summary; }
                set
                {
                    isDirty = value;

                    if (Record != null)
                    {
                        Record.IsDirty = value;

                        //if (Column.Table != null && Column.Table.RecordFilters.Count > 0)
                        //{
                        //    if (RowNo <= Column.Table.BottomRow && Column.Table.RecordFilters.FilterList.Contains(Column.Name))
                        //    {
                        //        Record.checkFiltering = true;
                        //    }
                        //}
                    }
                }
            }

            private bool suspendDraw;

            public bool SuspendDraw
            {
                get { return suspendDraw; }
                set
                {
                    suspendDraw = value;
                }
            }

            private bool strikeThrough;

            public bool StrikeThrough
            {
                get { return strikeThrough; }
                set
                {
                    strikeThrough = value;
                }
            }

            private bool isBold;

            public bool IsBold
            {
                get { return isBold; }
                set { isBold = value; }
            }


            private bool drawText;

            public bool DrawText
            {
                get { return drawText; }
                set
                {
                    drawText = value;
                }
            }

            private bool updateCustomFormula;

            public bool UpdateCustomFormula
            {
                get { return updateCustomFormula; }
                set
                {
                    updateCustomFormula = value;
                }
            }

            private string toolTipTextValue;

            public string ToolTipTextValue
            {
                get
                {
                    return toolTipTextValue;
                }
                set
                {
                    toolTipTextValue = value;
                }
            }

            private string toolTipHeaderTextValue;

            public string ToolTipHeaderTextValue
            {
                get { return toolTipHeaderTextValue; }
                set { toolTipHeaderTextValue = value; }
            }

            private bool isEmpty;

            public bool IsEmpty
            {
                get { return isEmpty; }
                set
                {
                    isEmpty = value;
                }
            }

            private bool isNumeric;

            public bool IsNumeric
            {
                get { return isNumeric; }
                set
                {
                    isNumeric = value;
                }
            }

            private bool isFormattedOnCondition;

            public bool IsFormattedOnCondition
            {
                get { return isFormattedOnCondition; }
                set
                {
                    isFormattedOnCondition = value;
                }
            }

            private bool isPushButton;

            public bool IsPushButton
            {
                get
                {
                    return isPushButton;
                }
                set
                {
                    isPushButton = value;
                }
            }

            private bool isBlinkCell;

            public bool IsBlinkCell
            {
                get { return isBlinkCell; }
                set
                {
                    isBlinkCell = value;
                }
            }

            private string cellModelType;

            public string CellModelType
            {
                get { return cellModelType; }
                set
                {
                    cellModelType = value;
                }
            }

            private bool underline;

            public bool Underline
            {
                get { return underline; }
                set
                {
                    underline = value;
                }
            }

            private GridStyleInfo style;

            public GridStyleInfo Style
            {
                get
                {
                    if (style == null)
                    {
                        if (Column != null)
                            style = (GridStyleInfo)Column.ColumnStyle.Clone();
                    }
                    return style;
                }
                set
                {
                    style = value;
                }
            }

            private GridStyleInfo originalStyle;

            public GridStyleInfo OriginalStyle
            {
                get
                {
                    return originalStyle;
                }
                set
                {
                    originalStyle = value;// (GridStyleInfo)value.Clone();
                }
            }

            private bool isChecked;

            public bool IsChecked
            {
                get { return isChecked; }
                set { isChecked = value; }
            }

            public string Key
            {
                get { return this.RowIndex.ToString() + ":" + this.ColIndex.ToString() + ":" + this.SourceIndex.ToString(); }
            }



            public double LastUpdatedTime;
            public TableColumn Column;
            public Record Record;
            //
            //This holds the source index of the unbound data collection to get data when grid is in virtual mode
            //
            public int SourceIndex;
            public int RowIndex;
            public int ColIndex;

            #region Constructors

            public CellStruct(int rowIndex, int colIndex, Record rec, TableColumn column, Type type)
            {
                this.RowIndex = rowIndex;
                this.ColIndex = colIndex;
                this.SourceIndex = -1;
                this.Record = rec;
                this.Column = column;
                this.isBlinkCell = false;
                this.isDirty = false;
                this.isEmpty = true;
                this.txt = "";
                this.doubleValue = 0;
                this.longValue = 0;
                this.integer = 0;
                this.decimalValue = 0;
                this.dateTimeValue = DateTime.MinValue;
                if (column.QueryStyle)
                    this.style = (GridStyleInfo)column.ColumnStyle.Clone();
                else
                    this.style = column.ColumnStyle;
                if (column.QueryStyle)
                    this.originalStyle = column.ColumnStyle;
                else
                    this.originalStyle = column.ColumnStyle;
                this.CellStructType = new CellStructType();
                this.CellStructType |= CellStructType.None;
                this.type = type;
                this.BlinkType = BlinkType.None;
                this.LastUpdatedTime = 0;
                this.isNumeric = true;
                this.isPushButton = false;
                this.cellModelType = CellType.Static;
                this.suspendDraw = false;
                this.drawText = true;
                this.toolTipTextValue = string.Empty;
                this.isFormattedOnCondition = false;
                this.updateCustomFormula = true;
                this.underline = false;
                this.isChecked = false;
                this.toolTipHeaderTextValue = string.Empty;
                this.strikeThrough = false;
                this.isBold = false;
                this.Type = column.Type;                

            }

            public CellStruct(int rowIndex, int colIndex, int sourceIndex, Record rec, TableColumn column, Type type)
            {
                this.RowIndex = rowIndex;
                this.ColIndex = colIndex;
                this.SourceIndex = sourceIndex;
                this.Record = rec;
                this.Column = column;
                this.isBlinkCell = false;
                this.isDirty = false;
                this.isEmpty = true;
                this.txt = "";
                this.doubleValue = 0;
                this.longValue = 0;
                this.integer = 0;
                this.decimalValue = 0;
                this.dateTimeValue = DateTime.MinValue;
                if (column.QueryStyle)
                    this.style = (GridStyleInfo)column.ColumnStyle.Clone();
                else
                    this.style = column.ColumnStyle;
                if (column.QueryStyle)
                    this.originalStyle = column.ColumnStyle;
                else
                    this.originalStyle = column.ColumnStyle;
                this.CellStructType = new CellStructType();
                this.CellStructType |= CellStructType.None;
                this.type = type;
                this.BlinkType = BlinkType.None;
                this.LastUpdatedTime = 0;
                this.isNumeric = true;
                this.isPushButton = false;
                this.cellModelType = CellType.Static;
                this.suspendDraw = false;
                this.drawText = true;
                this.toolTipTextValue = string.Empty;
                this.isFormattedOnCondition = false;
                this.updateCustomFormula = true;
                this.underline = false;
                this.toolTipHeaderTextValue = string.Empty;
                this.isChecked = false;
                this.strikeThrough = false;
                this.isBold = false;
                this.Type = column.Type;
            }

            public CellStruct(int rowIndex, int colIndex, Record rec, TableColumn column, Type type, GridStyleInfo style)
            {
                this.RowIndex = rowIndex;
                this.ColIndex = colIndex;
                this.SourceIndex = -1;
                this.Record = rec;
                this.Column = column;
                this.isBlinkCell = false;
                this.isDirty = false;
                this.isEmpty = true;
                this.txt = "";
                this.doubleValue = 0;
                this.longValue = 0;
                this.integer = 0;
                this.decimalValue = 0;
                this.dateTimeValue = DateTime.MinValue;
                this.style = style;
                this.originalStyle = style;
                this.type = type;
                this.CellStructType = new CellStructType();
                CellStructType |= CellStructType.None;
                this.BlinkType = BlinkType.None;
                this.LastUpdatedTime = 0;
                this.isNumeric = true;
                this.isPushButton = false;
                this.cellModelType = CellType.Static;
                this.suspendDraw = false;
                this.toolTipTextValue = string.Empty;
                this.isFormattedOnCondition = false;
                this.updateCustomFormula = true;
                this.underline = false;
                this.isChecked = false;
                this.drawText = true;
                this.strikeThrough = false;
                this.toolTipHeaderTextValue = string.Empty;
                this.isBold = false;
            }

            public CellStruct(TableInfo.CellStruct structNew)
            {
                this.RowIndex = structNew.RowIndex;
                this.ColIndex = structNew.ColIndex;
                this.SourceIndex = structNew.SourceIndex;
                this.Record = structNew.Record;
                this.Column = structNew.Column;
                this.isBlinkCell = structNew.IsBlinkCell;
                this.isDirty = structNew.IsDirty;
                this.isEmpty = structNew.IsEmpty;
                this.txt = structNew.TextString;
                this.decimalValue = structNew.TextDecimal;
                this.doubleValue = structNew.TextDouble;
                this.longValue = structNew.TextLong;
                this.integer = structNew.TextInt;
                this.dateTimeValue = structNew.TextDateTime;
                this.style = structNew.Style.Clone() as GridStyleInfo;
                this.originalStyle = structNew.OriginalStyle.Clone() as GridStyleInfo;
                this.type = structNew.Type;
                this.CellStructType = new CellStructType();
                CellStructType |= structNew.CellStructType;
                this.BlinkType = structNew.BlinkType;
                this.LastUpdatedTime = structNew.LastUpdatedTime;
                this.isNumeric = structNew.IsNumeric;
                this.isPushButton = structNew.IsPushButton;
                this.cellModelType = structNew.CellModelType;
                this.suspendDraw = structNew.SuspendDraw;
                this.toolTipTextValue = structNew.toolTipTextValue;
                this.drawText = structNew.DrawText;
                this.underline = structNew.underline;
                this.updateCustomFormula = structNew.UpdateCustomFormula;
                this.isFormattedOnCondition = structNew.IsFormattedOnCondition;
                this.isChecked = structNew.IsChecked;
                this.strikeThrough = structNew.StrikeThrough;
                this.toolTipHeaderTextValue = structNew.toolTipHeaderTextValue;
                this.isBold = false;
            }

            #endregion

            public void SetStyleCellValue(object obj)
            {
                if (isDirty || this.style.CellValue != obj)
                {
                    this.style.CellValue = obj;
                    isDirty = true;
                }
            }
        }

        [Flags]
        public enum CellStructType
        {
            None = 0, //(0000)
            String = 1, //(0001)
            Integer = 2, //(0010)
            Long = 4, //(0100)
            Double = 5, //(0101)
            DateTime = 6, //(0110)
            Decimal = 7,//(0111)
            Style = 8, //(1000)
        }

        public enum HeaderOrientation
        {
            Bottom,
            Top
        }

        public class MergedColumnCell
        {
            internal int StartingIndex = -1;
            internal int EndIndex = -1;
            internal int RowIndex = -1;

            public MergedColumnCell(int colIndex, int start, int end)
            {
                this.RowIndex = colIndex;
                this.StartingIndex = start;
                this.EndIndex = end;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (this.VisibleColumns != null)
            {
                this.VisibleColumns.Changing -= new ListPropertyChangedEventHandler(VisibleColumns_Changing);
                this.VisibleColumns.Changed -= new ListPropertyChangedEventHandler(VisibleColumns_Changed);
            }

            //if (expressionFields != null)
            //    expressionFields.Dispose();
        }

        #endregion
    }

    [Serializable]
    public enum BlinkType
    {
        None = 0,
        Up = 1,
        Down = 2,
        Equal = 3
    }

    public enum GridRefreshMode
    {
        AutoRefresh,
        UserDefined
    }

    public class MergeCellInfo
    {
        /// <summary>
        /// Number of items to Merge, 2 for marging 2 cells.
        /// </summary>
        public int MergeCellCount;

        public MergeCellInfo(int mergeCellCount)
        {
            this.MergeCellCount = mergeCellCount;
        }
    }

    public class BlinkInfo
    {
        public BlinkType Type = BlinkType.None;
        public double Ticks = 0;
        public Record Record;
        public int ColIndex;
        public int RowIndex;
        public TableInfo.CellStruct Cell;

        public BlinkInfo(TableInfo.CellStruct cell)
        {
            this.Cell = cell;
            this.Type = cell.BlinkType;
            this.Ticks = DateTime.Now.TimeOfDay.TotalMilliseconds;
            this.Record = cell.Record;
            this.ColIndex = cell.ColIndex;
            this.RowIndex = cell.RowIndex;
        }
    }

    public class Records : ArrayList
    {
        internal Dictionary<object, Record> RecordMapByObject = new Dictionary<object, Record>();

        public new void Add(object obj)
        {
            base.Add(obj);

            Record rec = obj as Record;

            if (rec != null && rec.ObjectBound != null)
            {
                //
                // CurrentIndex should be reset when adding to the collection. Otherwise CurrentIndex property and 
                // actual index inside the collection would not match, causing filtering issues, scrolling issues etc.
                //
                rec.CurrentIndex = this.Count - 1;

                if (!this.RecordMapByObject.ContainsKey(rec.ObjectBound))
                    this.RecordMapByObject.Add(rec.ObjectBound, rec);

            }
        }

        internal Record GetRecordFromObject(object obj)
        {
            Record rec = null;

            if (obj != null)
                RecordMapByObject.TryGetValue(obj, out rec);

            return rec;
        }
    }

    [Serializable]
    public class Record
    {
        #region Fields & Properties

        private List<PropertyAccessor> sortKeys;

        internal List<PropertyAccessor> SortKeys
        {
            get
            {
                if (sortKeys == null)
                    sortKeys = new List<PropertyAccessor>();

                return sortKeys;
            }
            set { sortKeys = value; }
        }

        private Dictionary<string, Dictionary<TableInfo.TableColumn, object>> customFormulaKeys;

        internal Dictionary<string, Dictionary<TableInfo.TableColumn, object>> CustomFormulaKeys
        {
            get
            {
                if (customFormulaKeys == null)
                    customFormulaKeys = new Dictionary<string, Dictionary<TableInfo.TableColumn, object>>();

                return customFormulaKeys;
            }
            set { customFormulaKeys = value; }
        }

        private List<TableInfo.TableColumn> unboundColumns;

        public List<TableInfo.TableColumn> UnboundColumns
        {
            get
            {
                if (unboundColumns == null)
                    unboundColumns = new List<TableInfo.TableColumn>();

                return unboundColumns;
            }
            set { unboundColumns = value; }
        }

        internal TableInfo tableInfo;
        internal object ObjectBound = null;
        internal bool checkFiltering = false;

        private bool isDirty = false;

        internal bool IsDirty
        {
            get
            {
                return isDirty;
            }
            set
            {
                isDirty = value;
            }
        }

        private bool filterReset = true;

        internal bool FilterReset
        {
            get
            {
                return filterReset;
            }
            set
            {
                filterReset = value;
            }
        }

        int sourceIndex = -1;

        public int SourceIndex
        {
            get
            {
                return sourceIndex;
            }
            set
            {
                sourceIndex = value;
            }
        }

        int currentIndex = -1;

        public int CurrentIndex
        {
            get
            {
                return currentIndex;
            }
            set
            {
                currentIndex = value;
            }
        }

        int columnIndex = -1;

        public int ColumnIndex
        {
            get
            {
                return columnIndex;
            }
            set
            {
                columnIndex = value;
            }
        }

        #endregion

        #region Constructor

        public Record(int row, object obj, TableInfo table)
        {
            this.sourceIndex = row;
            this.currentIndex = row;
            this.ObjectBound = obj;
            this.tableInfo = table;
        }

        #endregion

        #region public Methods

        public object GetData()
        {
            return ObjectBound;
        }

        public bool MeetsFilterCriteria(bool isApplyFilterInProgress)
        {
            //return true;
            if (!tableInfo.IsFilterEnabled)
                return false;
            else if (!isApplyFilterInProgress && !checkFiltering)
                return false;

            if (tableInfo.FilterManager.Compare(this))
            {
                if (tableInfo != null)
                {
                    if (tableInfo.AddToFilteredRecords(this))
                    {
                        if (!isApplyFilterInProgress)
                        {
                            tableInfo.SetRecreateFlag(true);
                        }
                    }
                }

                return true;
            }
            else
            {
                this.checkFiltering = false;

                if (tableInfo.FilteredRecords.Contains(this))
                {
                    tableInfo.FilteredRecords.Remove(this);

                    if (!isApplyFilterInProgress)
                    {
                        tableInfo.SetRecreateFlag(true);
                    }
                }
            }

            return false;
        }

        public object GetValue(TableInfo.TableColumn column)
        {
            if (tableInfo != null)
            {
                //
                //Used more reliable method
                //
                if (tableInfo != null && tableInfo.Grid != null && tableInfo.Grid.GridType == GridType.MultiColumn)
                    return GetValue(column.Name, column.CurrentPosition);
                else
                    return GetValue(column.Name);
                //return tableInfo.Grid.GetRecordColumnValue(this, column);
            }
            return null;
        }

        #region Sorting Values

        public double GetValueAsDouble(string columnName)
        {
            double value = 0;
            object objectValue = GetValue(columnName, -1, true);

            if (objectValue != null)
            {
                if (!double.TryParse(objectValue.ToString(), out value))
                {
                    if (objectValue is double)
                    {
                        value = (double)objectValue;
                    }
                }
            }

            return value;
        }

        public int GetValueAsInt(string columnName)
        {
            int value = 0;
            object objectValue = GetValue(columnName, -1, true);

            if (objectValue != null)
                int.TryParse(objectValue.ToString(), out value);

            return value;
        }

        public long GetValueAsLong(string columnName)
        {
            long value = 0;
            object objectValue = GetValue(columnName, -1, true);

            if (objectValue != null)
                long.TryParse(objectValue.ToString(), out value);

            return value;
        }

        public DateTime GetValueAsDateTime(string columnName)
        {
            DateTime value = DateTime.MinValue;
            object valueObject = GetValue(columnName, -1, true);

            if (valueObject != null)
                DateTime.TryParse(valueObject.ToString(), out value);

            return value;
        }

        public string GetValueAsString(string columnName)
        {
            string value = string.Empty;
            object valueObject = GetValue(columnName, -1, true);

            if (valueObject != null)
                value = Convert.ToString(valueObject);

            return value;
        }

        public string GetQueryString()
        {
            return tableInfo.GetQueryGroupText(this);
        }

        #endregion

        public object GetValue(string columnName)
        {
            return GetValue(columnName, -1, true);
        }

        public object GetValue(string columnName, int currentPosition)
        {
            return GetValue(columnName, currentPosition, true);
        }

        public object GetValue(string columnName, bool checkInVisibleColumns)
        {
            return GetValue(columnName, -1, checkInVisibleColumns);
        }

        public object GetValue(string columnName, int currentPosition, bool checkInVisibleColumns)
        {
            TableInfo.TableColumn column = tableInfo.GetVisibleColumnFromName(columnName);

            if (currentPosition >= 0)
                column = tableInfo.VisibleColumns[currentPosition];

            if (checkInVisibleColumns && tableInfo != null && column != null)
            {
                if (FilterReset)
                {
                    if (column.Id == -1)
                        return tableInfo.Grid.GetCellMatrixValue(this.currentIndex, column.CurrentPosition);
                    else
                        return tableInfo.Grid.GetRecordColumnValue(this, column);
                }
                else
                    return tableInfo.Grid.GetRecordColumnValue(this, column);
            }
            else if (this.tableInfo.Grid.propertyNames != null && this.tableInfo.Grid.propertyNames.Contains(columnName))
            {
                int progressiveID = this.tableInfo.Grid.lastProgressiveColumnID;

                if (this.tableInfo.Grid.allProperties.ContainsKey(columnName))
                {
                    //if (!this.tableInfo.Grid.PropertiesMapBayClass.ContainsKey(this.tableInfo.Grid.BoundObjectType))
                    //    PropertiesMapBayClass.Add(type, new Dictionary<string, PropertyAccessor>());

                    PropertyAccessor newAccessor = null;
                    int mappingID = (int)this.tableInfo.Grid.propertyNames[columnName];

                    if (!GridGroupingControl.PropertiesMapBayClass[this.tableInfo.Grid.BoundObjectType].ContainsKey(columnName))
                    {
                        newAccessor = new PropertyAccessor(this.tableInfo.Grid.BoundObjectType, columnName);
                        GridGroupingControl.PropertiesMapBayClass[this.tableInfo.Grid.BoundObjectType].Add(columnName, newAccessor);
                        progressiveID++;
                        this.tableInfo.Grid.lastProgressiveColumnID = progressiveID;
                        this.tableInfo.Grid.PropertyList.Add(progressiveID, newAccessor);
                    }
                    else
                    {
                        newAccessor = GridGroupingControl.PropertiesMapBayClass[this.tableInfo.Grid.BoundObjectType][columnName];
                        int currentCount = GridGroupingControl.PropertiesMapBayClass[this.tableInfo.Grid.BoundObjectType].Count;

                        bool isInCollection = false;

                        for (int i = 0; i < this.tableInfo.Grid.PropertyList.Values.Count; i++)
                        {
                            if (this.tableInfo.Grid.PropertyList.ContainsKey(i))
                            {
                                PropertyAccessor accessor = this.tableInfo.Grid.PropertyList[i];

                                if (accessor.Property == columnName)
                                {
                                    isInCollection = true;
                                    progressiveID = i;
                                }
                            }
                        }

                        if (!isInCollection)
                        {
                            progressiveID++;
                            this.tableInfo.Grid.lastProgressiveColumnID = progressiveID;

                            if (!this.tableInfo.Grid.PropertyList.ContainsKey(progressiveID))
                                this.tableInfo.Grid.PropertyList.Add(progressiveID, newAccessor);
                        }
                    }

                    //this.tableInfo.Grid.allProperties.Add(columnName, null);

                    if (!this.tableInfo.Grid.PropertyList.ContainsKey(progressiveID))
                    {
                        this.tableInfo.Grid.PropertyList.Add(progressiveID, newAccessor);

                        this.tableInfo.Grid.lastProgressiveColumnID++;

                        return this.tableInfo.Grid.PropertyList[progressiveID].Get(this.ObjectBound);
                    }
                    else
                    {
                        return this.tableInfo.Grid.PropertyList[progressiveID].Get(this.ObjectBound);
                    }
                }
                else
                {
                    int mappingID = (int)this.tableInfo.Grid.propertyNames[columnName];

                    if (this.tableInfo.Grid.PropertyList.ContainsKey(progressiveID))
                        return this.tableInfo.Grid.PropertyList[progressiveID].Get(this.ObjectBound);
                }
            }
            else
            {
                try
                {
                    column = tableInfo.GetColumnFromName(columnName);

                    object o = null;

                    if (tableInfo.Grid.PropertyList.ContainsKey(column.Id))
                        o = tableInfo.Grid.PropertyList[column.Id].Get((this).ObjectBound);

                    if (o != null)
                        return o;

                    if (column != null)
                    {
                        TableInfo.CellStruct structure = new TableInfo.CellStruct();
                        structure.Column = column;
                        structure.Record = this;
                        structure.RowIndex = this.currentIndex;
                        structure.ColIndex = 0;// column.CurrentPosition;

                        if (column.Type == typeof(double) || column.Type == typeof(double?))
                            structure.CellStructType |= TableInfo.CellStructType.Double;
                        else if (column.Type == typeof(long) || column.Type == typeof(long?))
                            structure.CellStructType |= TableInfo.CellStructType.Long;
                        else if (column.Type == typeof(int) || column.Type == typeof(int?))
                            structure.CellStructType |= TableInfo.CellStructType.Integer;
                        else if (column.Type == typeof(decimal) || column.Type == typeof(decimal?))
                            structure.CellStructType |= TableInfo.CellStructType.Decimal;
                        else if (column.Type == typeof(DateTime) || column.Type == typeof(DateTime?))
                        {
                            structure.CellStructType |= TableInfo.CellStructType.DateTime;
                            structure.IsNumeric = false;
                        }
                        else if (column.Type == typeof(String))
                        {
                            structure.CellStructType |= TableInfo.CellStructType.String;
                            structure.IsNumeric = false;
                        }
                        else
                            structure.CellStructType |= TableInfo.CellStructType.Style;

                        this.tableInfo.Grid.cellsToExtractTempData.Clear();
                        this.tableInfo.Grid.cellsToExtractTempData.Add(structure);

                        tableInfo.Grid.IsValueExtractMode = true;
                        tableInfo.Grid.OnQueryGridCells(this.tableInfo.Grid.cellsToExtractTempData, false);

                        o = tableInfo.Grid.GetValue(this.tableInfo.Grid.cellsToExtractTempData[0], structure.CellStructType);

                        if (o == null)
                            return string.Empty;
                        else
                            return o;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionsLogger.LogError(ex);
                }
                finally
                {
                    tableInfo.Grid.IsValueExtractMode = false;
                }
            }

            return null;
        }

        public object GetExactValueOfObjectBound(string propertyName)
        {
            if (this.tableInfo.Grid.propertyNames != null && this.tableInfo.Grid.propertyNames.Contains(propertyName))
            {
                int progressiveID = this.tableInfo.Grid.lastProgressiveColumnID;

                if (this.tableInfo.Grid.allProperties.ContainsKey(propertyName))
                {
                    //if (!this.tableInfo.Grid.PropertiesMapBayClass.ContainsKey(this.tableInfo.Grid.BoundObjectType))
                    //    PropertiesMapBayClass.Add(type, new Dictionary<string, PropertyAccessor>());

                    PropertyAccessor newAccessor = null;
                    int mappingID = (int)this.tableInfo.Grid.propertyNames[propertyName];

                    if (!GridGroupingControl.PropertiesMapBayClass[this.tableInfo.Grid.BoundObjectType].ContainsKey(propertyName))
                    {
                        newAccessor = new PropertyAccessor(this.tableInfo.Grid.BoundObjectType, propertyName);
                        GridGroupingControl.PropertiesMapBayClass[this.tableInfo.Grid.BoundObjectType].Add(propertyName, newAccessor);
                        progressiveID++;
                        this.tableInfo.Grid.lastProgressiveColumnID = progressiveID;
                        this.tableInfo.Grid.PropertyList.Add(progressiveID, newAccessor);
                    }
                    else
                    {
                        newAccessor = GridGroupingControl.PropertiesMapBayClass[this.tableInfo.Grid.BoundObjectType][propertyName];
                        int currentCount = GridGroupingControl.PropertiesMapBayClass[this.tableInfo.Grid.BoundObjectType].Count;

                        bool isInCollection = false;

                        for (int i = 0; i < this.tableInfo.Grid.PropertyList.Values.Count; i++)
                        {
                            if (this.tableInfo.Grid.PropertyList.ContainsKey(i))
                            {
                                PropertyAccessor accessor = this.tableInfo.Grid.PropertyList[i];

                                if (accessor.Property == propertyName)
                                {
                                    isInCollection = true;
                                    progressiveID = i;
                                }
                            }
                        }

                        if (!isInCollection)
                        {
                            progressiveID++;
                            this.tableInfo.Grid.lastProgressiveColumnID = progressiveID;

                            if (!this.tableInfo.Grid.PropertyList.ContainsKey(progressiveID))
                                this.tableInfo.Grid.PropertyList.Add(progressiveID, newAccessor);
                        }
                    }

                    //this.tableInfo.Grid.allProperties.Add(columnName, null);

                    if (!this.tableInfo.Grid.PropertyList.ContainsKey(progressiveID))
                    {
                        this.tableInfo.Grid.PropertyList.Add(progressiveID, newAccessor);

                        this.tableInfo.Grid.lastProgressiveColumnID++;

                        return this.tableInfo.Grid.PropertyList[progressiveID].Get(this.ObjectBound);
                    }
                    else
                    {
                        return this.tableInfo.Grid.PropertyList[progressiveID].Get(this.ObjectBound);
                    }
                }
                else
                {
                    int mappingID = (int)this.tableInfo.Grid.propertyNames[propertyName];

                    if (this.tableInfo.Grid.PropertyList.ContainsKey(progressiveID))
                        return this.tableInfo.Grid.PropertyList[progressiveID].Get(this.ObjectBound);
                }
            }

            return null;
        }

        #endregion
    }

    public class TableColumnCollection : IList
    {
        private ArrayList inner = new ArrayList();
        public TableInfo Table;

        public event ListPropertyChangedEventHandler Changed;
        public event ListPropertyChangedEventHandler Changing;

        public virtual TableInfo.TableColumn this[string key]
        {
            get
            {
                foreach (TableInfo.TableColumn column in this)
                {
                    if (column.Name == key || column.MappingName == key)
                        return column;
                }

                return null;
            }
            set
            {
                TableInfo.TableColumn col = null;

                foreach (TableInfo.TableColumn column in this)
                {
                    if (column.Name == key || column.MappingName == key)
                        col = column;
                }

                if (col != null)
                    col = value;
            }
        }

        public TableColumnCollection(TableInfo info)
        {
            this.Table = info;
        }

        /// <summary>
        /// Raises the <see cref="Changed"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ListPropertyChangedEventArgs" /> that contains the event data.</param>
        protected virtual void OnChanged(ListPropertyChangedEventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        /// <summary>
        /// Raises the <see cref="Changing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ListPropertyChangedEventArgs" /> that contains the event data.</param>
        protected virtual void OnChanging(ListPropertyChangedEventArgs e)
        {
            if (Changing != null)
                Changing(this, e);
        }

        public bool Contains(string key)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Name == key || this[i].MappingName == key)
                    return true;
            }

            return false;
        }

        public void Remove(string key)
        {
            TableInfo.TableColumn column = this[key];

            int index = this.inner.IndexOf(column);

            if (index >= 0)
            {
                OnChanging(new ListPropertyChangedEventArgs(ListPropertyChangedType.Remove, index, column, column.Name));

                if (column != null)
                    this.Remove(column);

                OnChanged(new ListPropertyChangedEventArgs(ListPropertyChangedType.Remove, index, column, column.Name));
            }

            if (Table != null && column != null && column.Name == Table.FrozenColumn)
                Table.FrozenColumn = string.Empty;
        }

        public void Insert(int index, string key)
        {
            Insert(index, key, false);
        }

        public void Insert(int index, string key, bool getFromOriginalColumnList)
        {
            TableInfo.TableColumn col = this[key];

            if (getFromOriginalColumnList)
            {
                col = (TableInfo.TableColumn)Table.GetColumnFromName(key).Clone();

                TableInfo.TableColumn temp = null;

                for (int i = 0; i < this.Count; i++)
                {
                    temp = this[i];

                    if (temp != null && temp.IsPrimaryColumn)
                    {
                        if (key == temp.Name)
                            col.IsPrimaryColumn = true;
                    }
                    if (temp != null && temp.IsEditColumn)
                    {
                        if (key == temp.Name)
                            col.IsEditColumn = true;
                    }
                }
            }
            else if (col == null)
                col = Table.GetColumnFromName(key);

            if (col != null)
            {
                TableInfo.TableColumn colExisting = this.Table.GetColumnFromName(key);

                if (colExisting != null && !getFromOriginalColumnList)
                {
                    GridStyleInfo style = colExisting.ColumnStyle;
                    col.ColumnStyle.HorizontalAlignment = style.HorizontalAlignment;
                    col.ColumnStyle.VerticalAlignment = style.VerticalAlignment;
                    col.ColumnStyle.Format = style.Format;

                    col.ColumnStyle.BackColor = col.ColumnStyle.BackColor;
                    col.ColumnStyle.TextColor = col.ColumnStyle.TextColor;
                    col.ColumnStyle.BackColorAlt = col.ColumnStyle.BackColorAlt;
                    col.ColumnStyle.Font = col.ColumnStyle.Font;
                    col.ColumnStyle.Font.ResetFont();

                    col.ColumnStyle.ImageList = col.ColumnStyle.ImageList;
                    col.ColumnStyle.ImageIndex = col.ColumnStyle.ImageIndex;
                }
                else
                {
                    if (Table.Grid.StopPaint && Table.Grid.GridType == GridType.MultiColumn)
                    {
                        GridStyleInfo style = colExisting.ColumnStyle;
                        col.ColumnStyle.HorizontalAlignment = style.HorizontalAlignment;
                        col.ColumnStyle.VerticalAlignment = style.VerticalAlignment;
                        col.ColumnStyle.Format = style.Format;

                        col.ColumnStyle.BackColor = col.ColumnStyle.BackColor;
                        col.ColumnStyle.TextColor = col.ColumnStyle.TextColor;
                        col.ColumnStyle.BackColorAlt = col.ColumnStyle.BackColorAlt;
                        col.ColumnStyle.Font = col.ColumnStyle.Font;
                        col.ColumnStyle.Font.ResetFont();

                        col.ColumnStyle.ImageList = col.ColumnStyle.ImageList;
                        col.ColumnStyle.ImageIndex = col.ColumnStyle.ImageIndex;

                        int colIndex = index;

                        for (int i = 0; i < Table.RowCount; i++)
                        {
                            Table.CellMatrix[i, colIndex].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                            Table.CellMatrix[i, colIndex].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();

                            Table.CellMatrix[i, colIndex].Column.QueryStyle = true;
                            Table.CellMatrix[i, colIndex].Column.MappingName = colExisting.MappingName;
                            Table.CellMatrix[i, colIndex].Column.Name = colExisting.Name;
                            Table.CellMatrix[i, colIndex].Column.Id = colExisting.Id;
                            Table.CellMatrix[i, colIndex].Column.IsEditColumn = colExisting.IsEditColumn;
                            Table.CellMatrix[i, colIndex].Column.IsPrimaryColumn = colExisting.IsPrimaryColumn;
                            Table.CellMatrix[i, colIndex].Column.DisplayName = colExisting.DisplayName;
                            Table.CellMatrix[i, colIndex].IsPushButton = colExisting.CellModelType == CellType.PushButton;
                            Table.CellMatrix[i, colIndex].CellModelType = colExisting.CellModelType;
                            Table.CellMatrix[i, colIndex].Column.CellModelType = colExisting.CellModelType;
                            Table.CellMatrix[i, colIndex].Style.CellValueType = colExisting.Type;
                            Table.CellMatrix[i, colIndex].Type = colExisting.Type;
                        }

                        if (!Table.columnDefaultStyles.ContainsKey(col.Name))
                        {
                            GridStyleInfo styleNew = new GridStyleInfo();
                            ObjectCloneHelper.Clone<GridStyleInfo>(ref styleNew, col.ColumnStyle);
                            Table.columnDefaultStyles.Add(col.Name, styleNew);
                        }
                    }
                }

                OnChanging(new ListPropertyChangedEventArgs(ListPropertyChangedType.Add, index, col, col.Name));

                this.Insert(index, col);

                OnChanged(new ListPropertyChangedEventArgs(ListPropertyChangedType.Add, this.IndexOf(col), col, col.Name));

                //this.Table.Grid.CreateColumnPropertyAccessor(col.MappingName, -1, col.Type);
            }
        }

        #region IList Members

        public int Add(object value)
        {
            return Add(value, false);
        }

        public int Add(object value, bool getFromOriginalColumnList)
        {
            int ret = 0;

            TableInfo.TableColumn col = value as TableInfo.TableColumn;

            if (this.Table == null)
            {
                this.inner.Add(value);
                return 0;
            }

            if (col != null)
            {
                if (inner.Contains(value))
                    return 0;

                TableInfo.TableColumn colExisting = this.Table.GetColumnFromName(col.Name);

                if (colExisting != null)
                {
                    GridStyleInfo style = colExisting.ColumnStyle;
                    col.ColumnStyle.HorizontalAlignment = style.HorizontalAlignment;
                    col.ColumnStyle.VerticalAlignment = style.VerticalAlignment;
                    col.ColumnStyle.Format = style.Format;

                    col.ColumnStyle.BackColor = col.ColumnStyle.BackColor;
                    col.ColumnStyle.TextColor = col.ColumnStyle.TextColor;
                    col.ColumnStyle.BackColorAlt = col.ColumnStyle.BackColorAlt;
                    col.ColumnStyle.Font = col.ColumnStyle.Font;
                    col.ColumnStyle.Font.ResetFont();

                    col.ColumnStyle.ImageList = col.ColumnStyle.ImageList;
                    col.ColumnStyle.ImageIndex = col.ColumnStyle.ImageIndex;
                }

                OnChanging(new ListPropertyChangedEventArgs(ListPropertyChangedType.Add, this.IndexOf(col), col, col.Name));

                ret = this.inner.Add(value);

                OnChanged(new ListPropertyChangedEventArgs(ListPropertyChangedType.Add, this.IndexOf(col), col, col.Name));

                this.Table.Grid.CreateColumnPropertyAccessor(col.MappingName, -1, col.Type);
            }
            else
            {
                string key = value.ToString();

                if (getFromOriginalColumnList)
                {
                    col = (TableInfo.TableColumn)Table.GetColumnFromName(key).Clone();

                    TableInfo.TableColumn temp = null;

                    for (int i = 0; i < this.Count; i++)
                    {
                        temp = this[i];

                        if (temp != null && temp.IsPrimaryColumn)
                        {
                            if (key == temp.Name)
                                col.IsPrimaryColumn = true;
                        }

                        if (temp != null && temp.IsEditColumn)
                        {
                            if (key == temp.Name)
                                col.IsEditColumn = true;
                        }
                    }
                }
                else
                {
                    col = this.Table.GetColumnFromName(value.ToString());
                }
                if (col != null)
                {
                    if (inner.Contains(col))
                        return 0;

                    TableInfo.TableColumn colExisting = this.Table.GetColumnFromName(value.ToString());

                    if (colExisting != null)
                    {
                        GridStyleInfo style = colExisting.ColumnStyle;
                        col.ColumnStyle.HorizontalAlignment = style.HorizontalAlignment;
                        col.ColumnStyle.VerticalAlignment = style.VerticalAlignment;
                        col.ColumnStyle.Format = style.Format;

                        col.ColumnStyle.BackColor = col.ColumnStyle.BackColor;
                        col.ColumnStyle.TextColor = col.ColumnStyle.TextColor;
                        col.ColumnStyle.BackColorAlt = col.ColumnStyle.BackColorAlt;
                        col.ColumnStyle.Font = col.ColumnStyle.Font;
                        col.ColumnStyle.Font.ResetFont();

                        col.ColumnStyle.ImageList = col.ColumnStyle.ImageList;
                        col.ColumnStyle.ImageIndex = col.ColumnStyle.ImageIndex;
                    }

                    ret = this.inner.Add(col);

                    OnChanged(new ListPropertyChangedEventArgs(ListPropertyChangedType.Add, this.IndexOf(col), col, col.Name));

                    this.Table.Grid.CreateColumnPropertyAccessor(col.MappingName, -1, col.Type);
                }
            }

            return ret;
        }

        public void Clear()
        {
            inner.Clear();
            Table.SetRecreateFlag(true);
        }

        public bool Contains(object value)
        {
            if (value == null)
                return false;

            return inner.Contains(value);
        }

        public int IndexOf(object value)
        {
            int index = inner.IndexOf(value);

            if (index < 0)
            {
                TableInfo.TableColumn col = this.Table.GetColumnFromName(value.ToString());

                if (col != null)
                    index = inner.IndexOf(col);
            }

            return index;
        }

        public void Insert(int index, object value)
        {
            inner.Insert(index, value);

            TableInfo.TableColumn col = value as TableInfo.TableColumn;

            if (col != null)
            {
                this.Table.Grid.CreateColumnPropertyAccessor(col.MappingName, -1, col.Type);
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Remove(object value)
        {
            int index = this.inner.IndexOf(value);
            TableInfo.TableColumn col = value as TableInfo.TableColumn;
            OnChanging(new ListPropertyChangedEventArgs(ListPropertyChangedType.Remove, index, col, col.Name));
            inner.Remove(value);
            OnChanged(new ListPropertyChangedEventArgs(ListPropertyChangedType.Remove, index, col, col.Name));

            if (Table != null && col != null && col.Name == Table.FrozenColumn)
                Table.FrozenColumn = string.Empty;
        }

        public void RemoveAt(int index)
        {
            object value = inner[index];
            TableInfo.TableColumn col = value as TableInfo.TableColumn;
            OnChanging(new ListPropertyChangedEventArgs(ListPropertyChangedType.Remove, index, col, col.Name));
            inner.RemoveAt(index);
            OnChanged(new ListPropertyChangedEventArgs(ListPropertyChangedType.Remove, index, col, col.Name));

            if (Table != null && col != null && col.Name == Table.FrozenColumn)
                Table.FrozenColumn = string.Empty;
        }

        public TableInfo.TableColumn this[int index]
        {
            get
            {
                if (index < 0)
                    return null;

                if (inner.Count > index)
                    return (TableInfo.TableColumn)inner[index];
                return null;
            }
            set
            {
                if (inner[index] != value)
                {
                    inner[index] = value;

                }
            }
        }

        object IList.this[int index]
        {
            get
            {
                return (TableInfo.TableColumn)inner[index];
            }
            set
            {
                if (inner[index] != value)
                {
                    inner[index] = value;

                }
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            //
            //TODO
            //
            //int count = Count;
            //for (int n = 0; n < count; n++)
            //    array[index + n]  = this[n];
        }

        public int Count
        {
            get
            {
                return inner.Count;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public void AddRange(TableColumnCollection columns)
        {
            foreach (TableInfo.TableColumn cd in columns)
                this.Add(cd);
        }

        public void AddRange(string[] columns)
        {
            foreach (string str in columns)
            {
                TableInfo.TableColumn cd = Table.GetColumnFromName(str);

                if (cd != null)
                    this.Add(cd);
            }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return new TableColumnCollectionEnumerator(this);
        }

        #endregion
    }

    public class TableColumnCollectionEnumerator : IEnumerator
    {
        int _cursor = -1, _next = -1;
        TableColumnCollection _coll;

        /// <summary>
        /// Initalizes the enumerator and attaches it to the collection.
        /// </summary>
        /// <param name="parentCollection">The parent collection to enumerate.</param>
        public TableColumnCollectionEnumerator(TableColumnCollection parentCollection)
        {
            _coll = parentCollection;
            _next = _coll.Count > 0 ? 0 : -1;
        }

        #region IEnumerator Members

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public virtual void Reset()
        {
            _cursor = -1;
            _next = _coll.Count > 0 ? 0 : -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        /// <summary>
        /// Gets the current element in the collection.
        /// </summary>
        public TableInfo.TableColumn Current
        {
            get
            {
                return _coll[_cursor];
            }
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns>
        /// True if the enumerator was successfully advanced to the next element; False if the enumerator has passed the end of the collection.
        /// </returns>
        public bool MoveNext()
        {
            if (_next == -1)
                return false;

            _cursor = _next;

            _next++;
            if (_next >= _coll.Count)
                _next = -1;

            return _cursor != -1;
        }
        #endregion


    }

    /// <summary>
    /// Specifies the change in the ListProperty. Used by OnChanging and OnChanged events of strong typed collections.
    /// </summary>
    public enum ListPropertyChangedType
    {
        /// <summary>
        /// An item is appended.
        /// </summary>
        Add,
        /// <summary>
        /// An item is removed.
        /// </summary>
        Remove,
        /// <summary>
        /// An item is inserted.
        /// </summary>
        Insert,
        /// <summary>
        /// An item is moved.
        /// </summary>
        Move,
        /// <summary>
        /// The whole collection is changed.
        /// </summary>
        Refresh,
        /// <summary>
        /// An item is replaced.
        /// </summary>
        ItemChanged,
        /// <summary>
        /// A nested property of an item is changed.
        /// </summary>
        ItemPropertyChanged
    }

    /// <summary>
    /// Used by OnChanging and OnChanged events of strong typed collections.
    /// </summary>
    public sealed class ListPropertyChangedEventArgs
    {
        ListPropertyChangedType listChangedType;
        int index;
        object item;
        string property;
        object tag;

        /// <summary>
        /// Initializes the ListPropertyChangedEventArgs.
        /// </summary>
        /// <param name="listChangedType"></param>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <param name="property"></param>
        public ListPropertyChangedEventArgs(ListPropertyChangedType listChangedType, int index, object item, string property)
        {
            this.listChangedType = listChangedType;
            this.index = index;
            this.property = property;
            this.item = item;
        }

        /// <summary>
        /// Initializes the ListPropertyChangedEventArgs.
        /// </summary>
        /// <param name="listChangedType"></param>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <param name="property"></param>
        /// <param name="tag"></param>
        public ListPropertyChangedEventArgs(ListPropertyChangedType listChangedType, int index, object item, string property, object tag)
        {
            this.listChangedType = listChangedType;
            this.index = index;
            this.property = property;
            this.item = item;
            this.tag = tag;
        }

        /// <summary>
        /// Returns the type in which the list changed.
        /// </summary>
        public ListPropertyChangedType Action
        {
            get
            {
                return listChangedType;
            }
        }

        /// <summary>
        /// Returns the index of the item that is changed.
        /// </summary>
        public int Index
        {
            get
            {
                return index;
            }
        }

        /// <summary>
        /// Returns a reference to the affected item.
        /// </summary>
        public object Item
        {
            get
            {
                return item;
            }
        }


        /// <summary>
        /// Returns the names of the affected property.
        /// </summary>
        public string Property
        {
            get
            {
                return property;
            }
        }

        /// <summary>
        /// If tag is EventArgs, then it returns the Tag casted to EventArgs.
        /// </summary>
        public EventArgs Inner
        {
            get
            {
                return tag as EventArgs;
            }
        }

        /// <summary>
        /// Gets / sets a tag.
        /// </summary>
        public object Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = value;
            }
        }
    }

    /// <summary>
    /// Used by OnChanging and OnChanged events of strong typed collections.
    /// </summary>
    public delegate void ListPropertyChangedEventHandler(object sender, ListPropertyChangedEventArgs e);
}
