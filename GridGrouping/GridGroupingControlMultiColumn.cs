using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Reflection;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Accessors;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Controls;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping
{
    //
    //Added by Ashan D
    //
    public partial class GridGroupingControlMC : GridGroupingControl
    {
        #region Fields

        private int minimumRowCount = 20;

        #endregion

        #region Events

        #endregion

        #region Properties

        public int MinimumRowCount
        {
            get { return minimumRowCount; }
            set { minimumRowCount = value; }
        }

        #endregion

        #region Constructors

        public GridGroupingControlMC()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //SetStyle(ControlStyles.DoubleBuffer, true);

            this.DoubleBuffered = true;

            Table.Grid = this;

            AddBasicCellModels(true);

            CreateRefreshTimer();

            CreateCellMatrix(this.tableInfo.RowCount, this.tableInfo.VisibleColumnCount);

            LoadSettings();
        }

        #endregion

        #region Helper Methods

        public override void GridColumnResize(GridGroupingControl grid, bool isSetDesignWidth)
        {
            base.GridColumnResize(grid, isSetDesignWidth);

            if (this.Width > tableInfo.TotalVisibleColumnWidth)
                this.HScroll.IsVisible = false;
            else
                this.HScroll.IsVisible = true;
        }

        public override void AddNewEmptyRowToMatrix(int index)
        {
            AddNewEmptyRowToMatrix(index, -1);
        }

        public void AddNewEmptyRowToMatrix(int index, int partialAddPrimaryColumnIndex)
        {
            try
            {
                //
                //Temp Removed
                //
                //if (!this.tableInfo.listOfEmptyRows.Contains(index))
                //    this.tableInfo.listOfEmptyRows.Add(index);
                //else
                //{
                //    int indexOf = this.tableInfo.listOfEmptyRows.IndexOf(index);

                //    for (int i = indexOf; i < this.tableInfo.listOfEmptyRows.Count; i++)
                //    {
                //        this.tableInfo.listOfEmptyRows[i]++;
                //    }
                //    this.tableInfo.listOfEmptyRows.Add(index);
                //}

                //this.tableInfo.listOfEmptyRows.Sort();

                bool isPartialAdding = partialAddPrimaryColumnIndex >= 0;
                int nextPrimaryIndex = -1;

                if (isPartialAdding)
                {
                    for (int i = partialAddPrimaryColumnIndex; i < tableInfo.VisibleColumns.Count; i++)
                    {
                        if (i != partialAddPrimaryColumnIndex && tableInfo.VisibleColumns[i].IsPrimaryColumn)
                        {
                            nextPrimaryIndex = i;
                            break;
                        }
                    }
                }

                TableInfo.CellStruct[,] cellMatrixNew = new TableInfo.CellStruct[this.Table.rowCount + 1, this.Table.VisibleColumns.Count];

                for (int i = 0; i < Table.CellMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < Table.CellMatrix.GetLength(1); j++)
                    {
                        cellMatrixNew[i, j] = Table.CellMatrix[i, j];
                    }
                }

                Table.CellMatrix = cellMatrixNew;
                this.Table.rowCount++;

                for (int i = Table.CellMatrix.GetLength(0) - 1; i >= index + 1; i--)
                {
                    for (int j = 0; j < Table.CellMatrix.GetLength(1); j++)
                    {
                        if (i == Table.CellMatrix.GetLength(0) - 1)
                        {
                            TableInfo.TableColumn col = Table.VisibleColumns[j];

                            if (col.QueryStyle)
                            {
                                Table.CellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                Table.CellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                            }
                            else
                            {
                                Table.CellMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle;
                                Table.CellMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle;
                            }
                            Table.CellMatrix[i, j].Column = col;

                            Table.CellMatrix[i, j].Type = col.Type;

                            Table.CellMatrix[i, j].Style.CellValueType = col.Type;
                            Table.CellMatrix[i, j].OriginalStyle.CellValueType = col.Type;
                            Table.CellMatrix[i, j].IsEmpty = true;
                            Table.CellMatrix[i, j].TextInt = 0;
                            Table.CellMatrix[i, j].TextDouble = 0;
                            Table.CellMatrix[i, j].TextLong = 0;
                            Table.CellMatrix[i, j].TextString = "";
                            Table.CellMatrix[i, j].IsDirty = false;
                            Table.CellMatrix[i, j].RowIndex = i;
                            Table.CellMatrix[i, j].ColIndex = j;
                            Table.CellMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                            Table.CellMatrix[i, j].CellModelType = col.CellModelType;
                            Table.CellMatrix[i, j].DrawText = true;
                            Table.CellMatrix[i, j].SourceIndex = -1;
                        }

                        if (isPartialAdding)
                        {
                            if (j >= partialAddPrimaryColumnIndex)
                            {
                                if (j >= partialAddPrimaryColumnIndex)
                                {
                                    if (nextPrimaryIndex >= 0)
                                    {
                                        if (j < nextPrimaryIndex)
                                        {
                                            Table.CellMatrix[i, j].Record = Table.CellMatrix[i - 1, j].Record;
                                            Table.CellMatrix[i, j].SourceIndex = Table.CellMatrix[i - 1, j].SourceIndex;
                                        }
                                    }
                                    else
                                    {
                                        Table.CellMatrix[i, j].Record = Table.CellMatrix[i - 1, j].Record;
                                        Table.CellMatrix[i, j].SourceIndex = Table.CellMatrix[i - 1, j].SourceIndex;
                                    }
                                }
                            }

                            if (i > Table.LastRecordIndex)
                                Table.LastRecordIndex = i;
                        }
                        else
                        {
                            Table.CellMatrix[i, j].Record = Table.CellMatrix[i - 1, j].Record;
                            Table.CellMatrix[i, j].SourceIndex = Table.CellMatrix[i - 1, j].SourceIndex;
                        }

                        if (Table.CellMatrix[i, j].Record != null)
                        {
                            if (i > Table.LastRecordIndex)
                                Table.LastRecordIndex = i;
                        }

                        if (!isPartialAdding)
                        {
                            Table.CellMatrix[i - 1, j].Record = null;
                            Table.CellMatrix[i - 1, j].SourceIndex = -1;
                            Table.CellMatrix[i - 1, j].TextString = "";
                        }
                    }
                }

                for (int j = 0; j < Table.CellMatrix.GetLength(1); j++)
                {
                    Table.CellMatrix[index, j].Style = Table.CellMatrix[index, j].OriginalStyle.Clone() as GridStyleInfo;
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public override void RemoveRecordFromMatrix(int rowIndex, int colIndex)
        {
            try
            {
                int recordStartColumnIndex = -1;

                for (int j = 0; j < Table.CellMatrix.GetLength(1); j++)
                {
                    if (j > colIndex && Table.VisibleColumns[j].IsPrimaryColumn)
                    {
                        break;
                    }

                    if (Table.VisibleColumns[j].IsPrimaryColumn)
                    {
                        recordStartColumnIndex = j;
                    }
                }

                int removingSourceIndex = -1;

                for (int j = recordStartColumnIndex; j < Table.CellMatrix.GetLength(1); j++)
                {
                    if (j > recordStartColumnIndex && Table.VisibleColumns[j].IsPrimaryColumn)
                    {
                        break;
                    }

                    Record r = Table.CellMatrix[rowIndex, j].Record;

                    if (r != null && this.Table.AllRecords.Contains(r))
                    {
                        this.Table.AllRecords.Remove(r);
                        Table.sourceDataRowCount--;
                    }

                    if (r != null && this.Table.OriginalRecords.Contains(r))
                    {
                        this.Table.OriginalRecords.Remove(r);
                    }

                    Table.CellMatrix[rowIndex, j].Record = null;

                    if (removingSourceIndex == -1)
                        removingSourceIndex = Table.CellMatrix[rowIndex, j].SourceIndex;

                    Table.CellMatrix[rowIndex, j].SourceIndex = -1;
                    Table.CellMatrix[rowIndex, j].TextString = "";

                    Table.CellMatrix[rowIndex, j].Style = Table.CellMatrix[rowIndex, j].OriginalStyle.Clone() as GridStyleInfo;
                }

                for (int i = 0; i < Table.CellMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < Table.CellMatrix.GetLength(1); j++)
                    {
                        if (Table.CellMatrix[i, j].SourceIndex > removingSourceIndex && removingSourceIndex >= 0)
                            Table.CellMatrix[i, j].SourceIndex--;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public override void RemoveEmptyRowFromMatrix(int index)
        {
            try
            {
                //this.tableInfo.listOfEmptyRows.Clear();


                if (this.tableInfo.listOfEmptyRows.Contains(index))
                {
                    //
                    //Temp Removed
                    //
                    //indexOf = this.tableInfo.listOfEmptyRows.IndexOf(index);
                    //this.tableInfo.listOfEmptyRows.Remove(index);

                    //if (this.tableInfo.listOfEmptyRows.Count > 0)
                    //{
                    //    for (int i = indexOf; i < this.tableInfo.listOfEmptyRows.Count; i++)
                    //    {
                    //        this.tableInfo.listOfEmptyRows[i]--;
                    //        if (this.tableInfo.listOfEmptyRows[i] < 0)
                    //            this.tableInfo.listOfEmptyRows[i] = 0;
                    //    }
                    //}

                    //this.tableInfo.listOfEmptyRows.Sort();
                }
                else
                {
                    int removedRecordCount = 0;

                    for (int i = index; i < Table.CellMatrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < Table.CellMatrix.GetLength(1); j++)
                        {
                            if (index == i)
                            {
                                if (Table.VisibleColumns[j].IsPrimaryColumn)
                                {
                                    Record r = Table.CellMatrix[i, j].Record;

                                    if (r != null && this.Table.AllRecords.Contains(r))
                                    {
                                        this.Table.AllRecords.Remove(r);
                                        Table.sourceDataRowCount--;
                                        removedRecordCount++;
                                    }
                                }
                            }

                            Table.CellMatrix[i, j].Style = Table.CellMatrix[i, j].OriginalStyle.Clone() as GridStyleInfo;

                            if (i + 1 < Table.CellMatrix.GetLength(0))
                            {
                                if (index != i)
                                {
                                    Table.CellMatrix[i, j].SourceIndex = Table.CellMatrix[i + 1, j].SourceIndex;
                                    Table.CellMatrix[i, j].SourceIndex = Table.CellMatrix[i, j].SourceIndex - removedRecordCount;
                                }

                                if (index == i && (Table.CellMatrix[i, j].Record == null || Table.CellMatrix[i + 1, j].Record == null))
                                {
                                    Table.CellMatrix[i, j].SourceIndex = Table.CellMatrix[i + 1, j].SourceIndex;
                                }

                                Table.CellMatrix[i, j].Record = Table.CellMatrix[i + 1, j].Record;

                                Table.CellMatrix[i + 1, j].Record = null;
                                Table.CellMatrix[i + 1, j].SourceIndex = -1;
                                Table.CellMatrix[i + 1, j].TextString = "";
                            }
                            else
                            {
                                Table.CellMatrix[i, j].Record = null;
                                Table.CellMatrix[i, j].SourceIndex = -1;
                                Table.CellMatrix[i, j].TextString = "";
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

        public TableInfo.CellStruct[,] GetCellMatrixFrame(ref List<string> recordKeys, ref List<Record> itemRecordsInOrder)
        {
            TableInfo.CellStruct[,] frameMatrix = new TableInfo.CellStruct[Table.CellMatrix.GetLength(0), Table.CellMatrix.GetLength(1)];

            if (itemRecordsInOrder != null)
                itemRecordsInOrder.Clear();

            if (recordKeys != null)
                recordKeys.Clear();

            Dictionary<int, Record> recordDic = new Dictionary<int, Record>();
            List<int> entries = new List<int>();

            for (int i = 0; i < frameMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < frameMatrix.GetLength(1); j++)
                {
                    frameMatrix[i, j].RowIndex = Table.CellMatrix[i, j].RowIndex;
                    frameMatrix[i, j].ColIndex = Table.CellMatrix[i, j].ColIndex;
                    frameMatrix[i, j].SourceIndex = Table.CellMatrix[i, j].SourceIndex;

                    if (Table.CellMatrix[i, j].Column != null && Table.CellMatrix[i, j].Record != null && Table.CellMatrix[i, j].Column.IsPrimaryColumn)
                    {
                        if (!recordDic.ContainsKey(Table.CellMatrix[i, j].SourceIndex))
                        {
                            entries.Add(Table.CellMatrix[i, j].SourceIndex);
                            recordDic.Add(Table.CellMatrix[i, j].SourceIndex, Table.CellMatrix[i, j].Record);
                        }
                    }

                    if (recordKeys != null && Table.CellMatrix[i, j].Record != null && !recordKeys.Contains(Table.CellMatrix[i, j].Key)
                        && frameMatrix[i, j].SourceIndex >= 0)
                        recordKeys.Add(Table.CellMatrix[i, j].Key);
                }
            }

            entries.Sort();

            if (itemRecordsInOrder == null)
                itemRecordsInOrder = new List<Record>();

            for (int i = 0; i < entries.Count; i++)
            {
                if (recordDic.ContainsKey(entries[i]))
                    itemRecordsInOrder.Add(recordDic[entries[i]]);
            }

            return frameMatrix;
        }

        public override void BeginUpdate()
        {
            base.BeginUpdate();
        }

        public override void EndUpdate(bool setGridDirty)
        {
            base.EndUpdate(setGridDirty);
        }

        public override void RecreateMatrix()
        {
            if (tableInfo.IsFilterEnabled)
            {
                if (tableInfo.FilteredRecords.Count == 0 && tableInfo.AllowFilter)
                    ApplyFilter(false);

                if (tableInfo.FilteredRecords.Count > 0)
                {
                    //this.Table.AllRecords.Clear();

                    //for (int i = 0; i < this.dataSource.Count; i++)
                    //{
                    //    Record r = new Record(i, this.DataSource[i], this.Table);
                    //    this.Table.AllRecords.Add(r);
                    //}

                    if (tableInfo.AllowFilter)
                        ApplyFilter(false);

                    tableInfo.CreateMultiColumnMatrixFromFilteredRecords(tableInfo.VisibleColumnCount);
                }
                else
                {
                    CreateCellMatrix(this.dataSource.Count, this.Table.VisibleColumnCount);

                    if (tableInfo.AllowFilter)
                        ApplyFilter();
                }
            }
            else
            {
                if (dataSource != null)
                    CreateCellMatrix(this.dataSource.Count, this.Table.VisibleColumnCount);
                else
                {
                    if (GridType == GridGrouping.GridType.DataBound)
                        CreateCellMatrix(this.Table.RowCount, this.Table.VisibleColumnCount);
                    else if (GridType == GridGrouping.GridType.Virtual)
                        CreateCellMatrix(this.Table.BottomRow, this.Table.VisibleColumnCount);
                }
            }

            IsColumnAlterationInProgress = false;
        }

        protected override void CreateCellMatrix(int rowCount, int columnCount)
        {
            try
            {
                this.stopPaint = true;

                tableInfo.CreateMatrixForMultiColumnGrid(rowCount, columnCount);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                this.stopPaint = false;
                this.resetGrid = true;
            }
        }

        public void InsertRecordToMatrix(int rowIndex, int colIndex, int newSourceIndex, int previousSourceIndex, bool replaceExisting)
        {
            try
            {
                tableInfo.InsertRecordToMatrix(rowIndex, colIndex, newSourceIndex, previousSourceIndex, replaceExisting);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        internal override void CreateColumnPropertyAccessor(string columnName, int columnID, Type type)
        {
            if (BoundObjectType == null)
                return;

            //if (columnsMapByName.ContainsKey(columnName))
            //    return;

            bool isCustom = false;

            //
            //Add New Class Type To Map 
            //
            if (!PropertiesMapBayClass.ContainsKey(BoundObjectType))
                PropertiesMapBayClass.Add(BoundObjectType, new Dictionary<string, PropertyAccessor>());

            if (columnID == -1)
            {
                isCustom = true;
            }

            try
            {
                if (properties == null)
                {
                    properties = BoundObjectType.GetProperties();

                    propertyTypes = new System.Collections.Specialized.OrderedDictionary();
                    propertyNames = new System.Collections.Specialized.OrderedDictionary();

                    for (int i = 0; i < properties.Length; i++)
                    {
                        string info = properties[i].Name;

                        if (!propertyNames.Contains(info))
                            propertyNames.Add(info, i);
                        if (!propertyTypes.Contains(info))
                            propertyTypes.Add(info, properties[i].PropertyType);

                        if (!allProperties.ContainsKey(info))
                            allProperties.Add(info, properties[i]);
                    }
                }

                int identityID = -2;
                int progressiveIdentityID = 0;

                progressiveIdentityID = lastProgressiveColumnID + 1;
                lastProgressiveColumnID = progressiveIdentityID;

                string str = columnName;

                for (int i = 0; i < this.tableInfo.VisibleColumns.Count; i++)
                {
                    TableInfo.TableColumn col = this.tableInfo.VisibleColumns[i];

                    if (col != null && col.MappingName == columnName)
                    {
                        TableInfo.TableColumn colVisible = tableInfo.GetVisibleColumnFromName(str);

                        col.Table = tableInfo;

                        if (colVisible != null)
                            colVisible.Table = tableInfo;

                        if (col != null && !col.IsUnbound)
                        {
                            if (propertyNames.Contains(str))
                            {
                                if (!isCustom)
                                    identityID = columnID;
                                else
                                    identityID = progressiveIdentityID;

                                int mappingID = (int)propertyNames[str];

                                try
                                {
                                    //
                                    //NOTE : Need to check this error when Instrument Type is used
                                    //

                                    if (properties[mappingID].Name != "InstrumentType" && !PropertyList.ContainsKey(identityID))
                                    {
                                        PropertyAccessor newAccessor = null;

                                        if (!PropertiesMapBayClass[BoundObjectType].ContainsKey(str))
                                        {
                                            newAccessor = new PropertyAccessor(BoundObjectType, properties[mappingID].Name);
                                            PropertiesMapBayClass[BoundObjectType].Add(str, newAccessor);
                                        }
                                        else
                                            newAccessor = PropertiesMapBayClass[BoundObjectType][str];

                                        PropertyList.Add(identityID, newAccessor);
                                    }
                                    else
                                    {
                                        //
                                        //Making the switch to user defined ID
                                        //
                                        col.Id = identityID;

                                        //
                                        //Making the correct property type
                                        //
                                        col.Type = (Type)propertyTypes[str];

                                        //
                                        //Set Mapping Name
                                        //
                                        col.MappingName = str;

                                        if (!columnsMapByID.ContainsKey(identityID))
                                            columnsMapByID.Add(identityID, col);

                                        if (!columnsMapByName.ContainsKey(str))
                                            columnsMapByName.Add(str, col);

                                        identityID = col.Id;
                                        str = col.MappingName;
                                    }

                                    if (colVisible != null && !colVisible.IsUnbound)
                                    {
                                        //
                                        //Making the switch to user defined ID
                                        //
                                        colVisible.Id = identityID;

                                        //
                                        //Making the correct property type
                                        //
                                        colVisible.Type = (Type)propertyTypes[str];

                                        //
                                        //Set Mapping Name
                                        //
                                        colVisible.MappingName = str;

                                        if (!designTimeWidths.ContainsKey(str))
                                            designTimeWidths.Add(str, colVisible.CellWidth);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ExceptionsLogger.LogError(ex);
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


        private void CheckForDataSourceTypeChange(Type type)
        {
            if (type == BoundObjectType)
                return;

            if (tableInfo == null || tableInfo.VisibleColumns == null)
                return;

            string[] columnNames = new string[this.tableInfo.VisibleColumns.Count];

            for (int i = 0; i < this.tableInfo.VisibleColumns.Count; i++)
            {
                columnNames[i] = this.tableInfo.VisibleColumns[i].MappingName;
            }

            if (BoundObjectType != null)
            {
                this.tableInfo.SortedColumnDescriptors.Clear();
                //this.tableInfo.RecordFilters.Clear();
            }

            GenerateColumnsAndProperties(columnNames, null, type, null);

            BoundObjectType = type;
        }

        private new void AssignColumnPropertiesToVisibleColumns()
        {
            TableInfo.TableColumn col = null;

            for (int i = 0; i < tableInfo.VisibleColumns.Count; i++)
            {
                col = tableInfo.GetColumnFromName(tableInfo.VisibleColumns[i].Name);

                if (col == null)
                    continue;

                GridStyleInfo style = col.ColumnStyle;
                tableInfo.VisibleColumns[i].ColumnStyle.HorizontalAlignment = style.HorizontalAlignment;
                tableInfo.VisibleColumns[i].ColumnStyle.VerticalAlignment = style.VerticalAlignment;
                tableInfo.VisibleColumns[i].ColumnStyle.Format = style.Format;

                tableInfo.VisibleColumns[i].ColumnStyle.BackColor = col.ColumnStyle.BackColor;
                tableInfo.VisibleColumns[i].ColumnStyle.TextColor = col.ColumnStyle.TextColor;
                tableInfo.VisibleColumns[i].ColumnStyle.BackColorAlt = col.ColumnStyle.BackColorAlt;
                tableInfo.VisibleColumns[i].ColumnStyle.Font = col.ColumnStyle.Font;
                tableInfo.VisibleColumns[i].ColumnStyle.Font.ResetFont();

                tableInfo.VisibleColumns[i].ColumnStyle.ImageList = col.ColumnStyle.ImageList;
                tableInfo.VisibleColumns[i].ColumnStyle.ImageIndex = col.ColumnStyle.ImageIndex;

            }
        }

        protected override void ProcessBlinkRegistry()
        {
            if (!isGridUpdatedOnce || resetGrid)
                blinkRegistry.Clear();

            List<string> itemsToRemove = null;

            if (tableInfo.AllowBlink)
            {
                foreach (BlinkInfo info in blinkRegistry.Values)
                {
                    if (info.Cell.BlinkType == BlinkType.None
                        || (DateTime.Now.TimeOfDay.TotalMilliseconds - info.Ticks) > this.blinkTime)// && info.Ticks + this.blinkTime > DateTime.Now.TimeOfDay.TotalMilliseconds)
                    {
                        this.cellsToUpdate.Add(info.Cell);
                        //}
                        //else
                        //{
                        if (itemsToRemove == null)
                            itemsToRemove = new List<string>();

                        itemsToRemove.Add(info.Cell.Key);
                    }
                }

                if (itemsToRemove != null)
                {
                    foreach (string item in itemsToRemove)
                    {
                        if (!blinkRegistry.ContainsKey(item))
                            continue;

                        BlinkInfo info = blinkRegistry[item];

                        if (tableInfo.CellMatrix.GetLength(0) > info.Cell.RowIndex)
                        {
                            tableInfo.CellMatrix[info.Cell.RowIndex, info.Cell.ColIndex].IsBlinkCell = false;
                            tableInfo.CellMatrix[info.Cell.RowIndex, info.Cell.ColIndex].IsDirty = true;
                            tableInfo.CellMatrix[info.Cell.RowIndex, info.Cell.ColIndex].BlinkType = BlinkType.None;
                        }

                        blinkRegistry.Remove(item);
                    }
                }
            }
        }

        protected override void InvalidateGrid()
        {
            if (isMainPaint)
                return;

            if (RecreateMatrixFlag && isGridPaintedOnce)
            {
                RecreateMatrix();
                RecreateMatrixFlag = false;
                this.Refresh();
                return;
            }

            if (StopPaint)
                return;

            tableInfo.BottomRow = tableInfo.TopRow + tableInfo.DisplayRows;

            //
            //Main Data Handling
            //
            DoDataCalculations();

            if (grafx == null)
                return;

            lock (paintLock)
            {
                PaintGridData(null, grafx.Graphics);
                grafx.Render(Graphics.FromHwnd(this.Handle));
            }
        }

        private void DoDataCalculations()
        {
            try
            {
                this.cellsToUpdate.Clear();
                this.cellsUnboundToUpdate.Clear();
                this.cellsSummaryToUpdate.Clear();
                this.cellsNestedHeaders.Clear();
                this.cellsRowHeaders.Clear();
                this.cellsToDraw.Clear();
                this.cellsUnboundToDraw.Clear();

                if (!tableInfo.IsDirty)
                    ProcessBlinkRegistry();

                CheckCellsToUpdate(false);

                CheckFilterUpdates();

                if (tableInfo.IsDirty)
                    blinkRegistry.Clear();

                RaiseCommonQueryCellEvents();

                SetDrawingCells();

                if (isSortingRequired)
                    tableInfo.SortSourceList();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void SetDrawingCells()
        {
            this.cellsToDraw.Clear();
            this.cellsUnboundToDraw.Clear();

            if (cellsToUpdate.Count == 0 && cellsUnboundToUpdate.Count == 0)
                return;

            if (dataSource != null)
            {
                for (int i = 0; i < cellsToUpdate.Count; i++)
                {
                    TableInfo.CellStruct structure = cellsToUpdate[i];
                    if (/*tableInfo.CellMatrix[structure.RowNo, structure.ColNo].IsDirty  && */ structure.RowIndex <= tableInfo.BottomRow)
                    {
                        if (!tableInfo.VisibleColumns[structure.ColIndex].IsUnbound)
                            cellsToDraw.Add(structure);
                        else
                        {
                            // Check this 
                            if (tableInfo.CellMatrix[structure.RowIndex, structure.ColIndex].IsDirty)
                                cellsUnboundToDraw.Add(structure);
                        }
                    }
                }

                for (int i = 0; i < cellsUnboundToUpdate.Count; i++)
                {
                    TableInfo.CellStruct structure = cellsUnboundToUpdate[i];
                    if (/*tableInfo.CellMatrix[structure.RowNo, structure.ColNo].IsDirty &&  */ structure.RowIndex <= tableInfo.BottomRow)
                        cellsUnboundToDraw.Add(structure);
                }
            }
            else
            {
                for (int i = 0; i < cellsUnboundToUpdate.Count; i++)
                {
                    TableInfo.CellStruct structure = cellsUnboundToUpdate[i];
                    if (structure.RowIndex < tableInfo.RowCount)
                        cellsUnboundToDraw.Add(structure);
                }
            }
        }

        private void SetRowStyleText(int row, Color backColor, Color backColorAlt, Color textColor, GridFontInfo gridFontInfo)
        {
            try
            {
                if (row >= this.tableInfo.RowCount || row >= tableInfo.CellMatrix.GetLength(0))
                    return;

                for (int i = 0; i < tableInfo.CellMatrix.GetLength(1); i++)
                {
                    if (tableInfo.CellMatrix[row, i].Style != null && !tableInfo.CellMatrix[row, i].Style.IsCustomized)
                    {
                        tableInfo.CellMatrix[row, i].Style.BackColor = backColor;
                        tableInfo.CellMatrix[row, i].Style.BackColorAlt = backColorAlt;
                        tableInfo.CellMatrix[row, i].Style.TextColor = textColor;
                        tableInfo.CellMatrix[row, i].Style.Font = gridFontInfo;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        #endregion

        #region Painting Method

        public override void PaintCell(int rowIndexToPaint, int colIndexToPaint, int rowIndex, int colIndex, Graphics g, bool updateBeforePainting)
        {
            bool refreshRow = colIndex == -1;
            bool isSelected = false;

            if (refreshRow)
            {
                string strText = "";

                if (dataSource == null || rowIndex < dataSource.Count)
                {
                    if (g == null)
                        g = this.CreateGraphics();

                    TableInfo.CellStruct cell;

                    for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
                    {
                        if (updateBeforePainting && dataSource != null)
                            UpdateCell(rowIndex, j, true);

                        cell = tableInfo.CellMatrix[rowIndex, j];

                        if (cell.SuspendDraw)
                            continue;

                        GridStyleInfo style = cell.Style;

                        strText = GetValueAsString(cell, cell.CellStructType, style);

                        Rect.X = tableInfo.VisibleColumns[j].Left;
                        if (gridType == GridGrouping.GridType.DataBound)
                            Rect.Y = Table.HeaderHeight + ((rowIndexToPaint - tableInfo.TopRow) * Table.RowHeight);
                        else
                            Rect.Y = Table.HeaderHeight + ((rowIndexToPaint) * Table.RowHeight);
                        Rect.Width = tableInfo.VisibleColumns[j].CellWidth;
                        Rect.Height = Table.RowHeight;

                        if (tableInfo.CellSelectionType == CellSelectionType.Row)
                        {
                            if (tableInfo.RowCelectionType == RowCelectionType.Single)
                                GridPainter.PaintCell(g, Rect, rowIndexToPaint, colIndex, cell, strText, style, rowIndexToPaint == tableInfo.CurrentRow);
                            else
                            {
                                if (tableInfo.RowCelectionType == RowCelectionType.Single)
                                {
                                    isSelected = rowIndex == tableInfo.CurrentRow;
                                }
                                else if (tableInfo.RowCelectionType == RowCelectionType.Multiple)
                                {
                                    if (tableInfo.lastSelectedRow == -1)
                                    {
                                        isSelected = rowIndexToPaint == tableInfo.firstSelectedRow;

                                        if (!isSelected && tableInfo.selectedRecordIndexes.Contains(rowIndexToPaint))
                                            isSelected = true;
                                    }
                                    else
                                        isSelected = (rowIndexToPaint >= tableInfo.firstSelectedRow && rowIndexToPaint <= tableInfo.lastSelectedRow) || (rowIndexToPaint <= tableInfo.firstSelectedRow && rowIndexToPaint >= tableInfo.lastSelectedRow);
                                }

                                GridPainter.PaintCell(g, Rect, rowIndexToPaint, colIndex, cell, strText, style, isSelected);
                            }
                        }
                        else
                        {
                            GridPainter.PaintCell(g, Rect, rowIndexToPaint, j, cell, strText, style, (rowIndexToPaint == tableInfo.CurrentRow && j == tableInfo.CurrentCol));
                        }

                        CleanCell(rowIndex, j);
                    }
                }
            }
            else
            {
                //
                //Draw Cell
                //
                string strText = "";

                if ((rowIndex >= 0 && rowIndex < this.tableInfo.RowCount)
                    || (rowIndex >= 0 && rowIndex < this.tableInfo.RowCount && tableInfo.CellMatrix[rowIndex, colIndex].CellModelType == CellType.Summary))
                {
                    if (g == null)
                        g = this.CreateGraphics();

                    TableInfo.CellStruct cell = tableInfo.CellMatrix[rowIndex, colIndex];

                    if (cell.Record == null)
                    {
                        tableInfo.CellMatrix[rowIndex, colIndex].DrawText = false;
                        cell.DrawText = false;
                    }
                    else
                    {
                        tableInfo.CellMatrix[rowIndex, colIndex].DrawText = true;
                        cell.DrawText = true;
                    }

                    if (cell.SuspendDraw)
                        return;

                    if (updateBeforePainting && dataSource != null)
                        UpdateCell(rowIndex, colIndex, true);

                    GridStyleInfo style = cell.Style;

                    strText = GetValueAsString(cell, cell.CellStructType, style);

                    Rect.X = tableInfo.VisibleColumns[colIndex].Left;
                    if (gridType != GridGrouping.GridType.Virtual)
                        Rect.Y = Table.HeaderHeight + ((rowIndexToPaint - tableInfo.TopRow) * Table.RowHeight);
                    else
                        Rect.Y = Table.HeaderHeight + ((rowIndex) * Table.RowHeight);
                    Rect.Width = tableInfo.VisibleColumns[colIndex].CellWidth;
                    Rect.Height = Table.RowHeight;

                    if (tableInfo.CellSelectionType == CellSelectionType.Row)
                    {
                        if (tableInfo.RowCelectionType == RowCelectionType.Single)
                            GridPainter.PaintCell(g, Rect, rowIndexToPaint, colIndex, cell, strText, style, rowIndexToPaint == tableInfo.CurrentRow);
                        else
                        {
                            if (tableInfo.RowCelectionType == RowCelectionType.Single)
                            {
                                isSelected = rowIndexToPaint == tableInfo.CurrentRow;
                            }
                            else if (tableInfo.RowCelectionType == RowCelectionType.Multiple)
                            {
                                if (tableInfo.lastSelectedRow == -1)
                                {
                                    isSelected = rowIndexToPaint == tableInfo.firstSelectedRow;

                                    if (!isSelected && tableInfo.selectedRecordIndexes.Contains(rowIndexToPaint))
                                        isSelected = true;
                                }
                                else
                                    isSelected = (rowIndexToPaint >= tableInfo.firstSelectedRow && rowIndexToPaint <= tableInfo.lastSelectedRow) || (rowIndexToPaint <= tableInfo.firstSelectedRow && rowIndexToPaint >= tableInfo.lastSelectedRow);
                            }

                            GridPainter.PaintCell(g, Rect, rowIndexToPaint, colIndex, cell, strText, style, isSelected);
                        }
                    }
                    else
                    {
                        if (tableInfo.RowCelectionType == RowCelectionType.Single)
                            GridPainter.PaintCell(g, Rect, rowIndexToPaint, colIndex, cell, strText, style, rowIndexToPaint == tableInfo.CurrentRow);
                        else
                        {
                            if (tableInfo.RowCelectionType == RowCelectionType.Single)
                            {
                                if (rowIndexToPaint <= tableInfo.LastRecordIndex)
                                    isSelected = rowIndexToPaint == tableInfo.CurrentRow && colIndex == tableInfo.CurrentCol;
                            }
                            else if (tableInfo.RowCelectionType == RowCelectionType.Multiple)
                            {
                                if (tableInfo.lastSelectedRow == -1)
                                {
                                    if (rowIndexToPaint <= tableInfo.LastRecordIndex)
                                        isSelected = rowIndexToPaint == tableInfo.firstSelectedRow && colIndex == tableInfo.CurrentCol;

                                    if ((rowIndexToPaint <= tableInfo.LastRecordIndex) && !isSelected && tableInfo.selectedRecordIndexes.Contains(rowIndexToPaint) && colIndex == tableInfo.CurrentCol)
                                        isSelected = true;
                                }
                                else
                                {
                                    if (rowIndexToPaint <= tableInfo.LastRecordIndex)
                                        isSelected = ((rowIndexToPaint >= tableInfo.firstSelectedRow && rowIndexToPaint <= tableInfo.lastSelectedRow)
                                            || (rowIndexToPaint <= tableInfo.firstSelectedRow && rowIndexToPaint >= tableInfo.lastSelectedRow)) && colIndex == tableInfo.CurrentCol;
                                }
                            }

                            GridPainter.PaintCell(g, Rect, rowIndexToPaint, colIndex, cell, strText, style, isSelected);
                        }
                    }

                    CleanCell(rowIndex, colIndex);
                }
            }
        }

        protected override void PaintGrid(object sender, Graphics g)
        {
            try
            {
                if (StopPaint)
                    return;

                if (RecreateMatrixFlag && this.tableInfo.IsDirty)
                {
                    RecreateMatrix();
                    RecreateMatrixFlag = false;
                    this.Refresh();
                    return;
                }

                //
                //Use this if loading time should be improved
                //
                //if (isGridPaintedOnce)
                //    stopPaint = true;
                isMainPaint = true;

                SetColumnPositions();

                Rectangle rect = new Rectangle(this.Location.X, this.Location.Y, this.Width - VScroll.Width, this.Height);

                //
                //Draw Header
                //
                if (!isGridPaintedOnce || this.tableInfo.IsDirty
                    || this.tableInfo.AllowRowHeaders || this.tableInfo.AllowNestedHeaders
                    || startColumnLocation != tableInfo.VisibleColumns[0].Left)
                    PaintHeaders(g, rect);

                Table.DisplayRows = (int)Math.Round(((float)(this.Height - Table.HeaderHeight) / (float)Table.RowHeight));

                this.TotalRecordCount = tableInfo.RowCount;
                this.VScroll.Maximum = tableInfo.RowCount - tableInfo.DisplayRows;
                this.VScroll.Value = tableInfo.TopRow;
                int totalWidth = GetVisibleColumnWidth();
                this.HScroll.Maximum = (totalWidth > this.Width) ? ((this.IsMirrored) ? totalWidth - this.Width : totalWidth - this.Width) : 0;
                this.HScroll.Update();

                if (isGridPaintedOnce || this.tableInfo.IsDirty)
                {
                    if (this.tableInfo.IsDirty || isSortingRequired)
                        tableInfo.SortSourceList();

                    this.cellsToUpdate.Clear();
                    this.cellsSummaryToUpdate.Clear();
                    this.cellsUnboundToUpdate.Clear();
                    this.cellsNestedHeaders.Clear();
                    this.cellsRowHeaders.Clear();
                    this.cellsToDraw.Clear();
                    this.cellsUnboundToDraw.Clear();

                    CheckCellsToUpdate(true);

                    CheckFilterUpdates();

                    if (tableInfo.IsDirty)
                        blinkRegistry.Clear();

                    RaiseCommonQueryCellEvents();
                    SetDrawingCells();
                    if (this.tableInfo.IsDirty || isSortingRequired)
                        tableInfo.SortSourceList();
                }
                else
                    tableInfo.SortSourceList();

                if (this.dataSource != null)
                    PaintGridWithDataSource(g, false);
                else
                    PaintGridWithMatrix(g, false);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                isGridPaintedOnce = true;
                isMainPaint = false;
                resetGrid = false;
                this.tableInfo.IsDirty = false;
                isSortingRequired = false;
                stopPaint = false;
            }
        }

        internal override void SetColumnPositions()
        {
            if (tableInfo.isHeaderMouseDown)
                return;

            if (tableInfo.VisibleColumns.Count <= 0)
                return;

            bool isMirrored = IsMirrored;
            int intLeftPos = 0;
            int initialLeft = 0;

            int frozenColumnIndex = -1;

            if (!string.IsNullOrEmpty(Table.FrozenColumn))
            {
                frozenColumnIndex = Table.GetVisibleColumnFromName(Table.FrozenColumn).CurrentPosition;
            }

            if (!isMirrored)
            {
                if (frozenColumnIndex < 0)
                {
                    tableInfo.VisibleColumns[0].Left = 0 + tableInfo.RowHeaderWidth - horizontalOffset;
                }
                else
                {
                    tableInfo.VisibleColumns[0].Left = 0 + tableInfo.RowHeaderWidth;
                }

                if (tableInfo.VisibleColumns.Count > 0)
                {
                    if (frozenColumnIndex < 0)
                    {
                        initialLeft = tableInfo.VisibleColumns[0].Left + tableInfo.VisibleColumns[0].CellWidth;
                    }
                    else
                    {
                        initialLeft = tableInfo.VisibleColumns[0].CellWidth - horizontalOffset;
                    }
                }

                intLeftPos = initialLeft;
                startColumnLocation = intLeftPos;
                tableInfo.BottomRow = tableInfo.TopRow + tableInfo.DisplayRows;

                //
                //Set Column Left Positions
                //
                for (int i = 1; i < tableInfo.VisibleColumns.Count; i++)
                {
                    if (frozenColumnIndex > i)
                    {
                        tableInfo.VisibleColumns[i].Left = tableInfo.VisibleColumns[i - 1].Left + tableInfo.VisibleColumns[i - 1].CellWidth;
                        intLeftPos = tableInfo.VisibleColumns[i].Left;
                    }
                    else
                    {
                        tableInfo.VisibleColumns[i].Left = intLeftPos;
                        intLeftPos = intLeftPos + tableInfo.VisibleColumns[i].CellWidth;
                    }
                }
            }
            else
            {
                if (frozenColumnIndex < 0)
                {
                    tableInfo.VisibleColumns[0].Left = this.tableInfo.TotalVisibleColumnWidth + 5 - tableInfo.RowHeaderWidth - tableInfo.VisibleColumns[0].CellWidth - horizontalOffset;
                }
                else
                {
                    tableInfo.VisibleColumns[0].Left = this.tableInfo.TotalVisibleColumnWidth + 5 - tableInfo.RowHeaderWidth;
                }

                if (tableInfo.VisibleColumns.Count > 0)
                {
                    if (frozenColumnIndex < 0)
                    {
                        initialLeft = tableInfo.VisibleColumns[0].Left;
                    }
                    else
                    {
                        initialLeft = tableInfo.VisibleColumns[0].Left + horizontalOffset;
                    }
                }

                intLeftPos = initialLeft;
                startColumnLocation = intLeftPos;
                tableInfo.BottomRow = tableInfo.TopRow + tableInfo.DisplayRows;

                //
                //Set Column Left Positions
                //
                for (int i = 1; i < tableInfo.VisibleColumns.Count; i++)
                {
                    if (frozenColumnIndex > i)
                    {
                        tableInfo.VisibleColumns[i].Left = tableInfo.VisibleColumns[i - 1].Left - tableInfo.VisibleColumns[i - 1].CellWidth;
                        intLeftPos = tableInfo.VisibleColumns[i].Left + horizontalOffset;
                    }
                    else
                    {
                        tableInfo.VisibleColumns[i].Left = intLeftPos - tableInfo.VisibleColumns[i].CellWidth;
                        intLeftPos = tableInfo.VisibleColumns[i].Left;// -horizontalOffset;
                    }
                }
            }

            if (this.Width > tableInfo.TotalVisibleColumnWidth)
                this.HScroll.IsVisible = false;
            else
                this.HScroll.IsVisible = true;
        }

        protected override void PaintGridWithMatrixOnUpdate(Graphics g, bool paintNew)
        {
            try
            {
                if (tableInfo.CellMatrix == null)
                    return;

                TotalRecordCount = this.Table.RowCount;

                //
                //Paint Updated unbound fields
                //
                for (int i = 0; i < this.cellsUnboundToUpdate.Count; i++)
                {
                    TableInfo.CellStruct structure = this.cellsUnboundToUpdate[i];

                    if (!tableInfo.VisibleColumns[structure.ColIndex].IsUnbound)
                    {
                        if (tableInfo.VisibleColumns[structure.ColIndex].Left > this.Width)
                            continue;

                        PaintCell(structure.RowIndex, structure.ColIndex, g);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void PaintGridWithMatrix(Graphics g, bool paintNew)
        {
            try
            {
                if (tableInfo.RowCount > tableInfo.sourceDataRowCount || tableInfo.CellMatrix == null || (gridType == GridGrouping.GridType.Virtual && tableInfo.sourceDataRowCount == -1))
                    return;

                TotalRecordCount = this.Table.RowCount;
                bool isDirty = false;

                //
                //Draw Data
                //
                for (int i = 0; i < tableInfo.RowCount; i++)
                {
                    if (tableInfo.CellMatrix.GetLength(0) > i)
                    {
                        for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
                        {
                            if (tableInfo.VisibleColumns[j].Left > this.Width)
                                continue;

                            isDirty = tableInfo.CellMatrix[i, j].IsDirty;

                            if (!isDirty && paintNew)
                            {
                                continue;
                            }

                            PaintCell(tableInfo.CellMatrix[i, j].SourceIndex, j, i, j, g, false);
                        }
                    }
                }

                ////
                ////Draw Data
                ////
                //for (int i = tableInfo.TopRow; i < tableInfo.BottomRow; i++)
                //{
                //    if (i < this.tableInfo.rowCount && tableInfo.CellMatrix.GetLength(0) > i)
                //    {
                //        for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
                //        {
                //            if (tableInfo.VisibleColumns[j].Left > this.Width)
                //                continue;

                //            isDirty = tableInfo.CellMatrix[i, j].IsDirty;

                //            if (!isDirty && paintNew)
                //            {
                //                continue;
                //            }

                //            PaintCell(i, j, g);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        /// <summary>
        /// Draws All Cells
        /// </summary>
        /// <param name="g"></param>
        /// <param name="drawAll"></param>
        protected override void PaintGridWithDataSource(Graphics g, bool drawAll)
        {
            try
            {
                bool isDirty = false;
                int colID = -1; ;
                TotalRecordCount = dataSource.Count;

                int index = tableInfo.TopRow;
                int lastIndexToPaint = tableInfo.BottomRow;

                if (tableInfo.IsDirty)
                    lastIndexToPaint = Table.LastRecordIndex;

                //
                //Draw Data
                //
                for (int i = tableInfo.TopRow; i <= lastIndexToPaint; i++)
                {
                    if (i <= Table.LastRecordIndex && i < tableInfo.CellMatrix.GetLength(0) || i < this.MinimumRowCount)
                    {
                        for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
                        {
                            if (tableInfo.VisibleColumns[j].Left > this.Width)
                                continue;

                            if (tableInfo.CellMatrix.GetLength(1) <= j)
                                continue;

                            if (tableInfo.CellMatrix.GetLength(0) <= i)
                                continue;

                            isDirty = tableInfo.CellMatrix[i, j].IsDirty;

                            if (!tableInfo.VisibleColumns[j].IsUnbound)
                            {
                                colID = tableInfo.VisibleColumns[j].Id;

                                if (colID >= 0 && (!isDirty || !drawAll))
                                {
                                    if (!isDirty && drawAll)
                                    {
                                        continue;
                                    }
                                }
                            }
                            else
                            {
                                if (!isDirty && drawAll)
                                {
                                    continue;
                                }
                            }

                            PaintCell(index, j, i, j, g, false);
                        }
                    }

                    index++;
                }

                if (tableInfo.AllowSummaryRows)
                {
                    for (int i = tableInfo.CellMatrix.GetLength(0) - 1; i > tableInfo.CellMatrix.GetLength(0) - tableInfo.NumberOfSummaryRows - 1; i--)
                    {
                        for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
                        {
                            PaintCell(i, j, i, j, g, false);
                        }
                    }
                }

                //
                //Temp
                //
                //if (!string.IsNullOrEmpty(tableInfo.FrozenColumn))
                //{
                //    int frozenColumnIndex = tableInfo.GetVisibleColumnFromName(tableInfo.FrozenColumn).CurrentPosition + 1;

                //    for (int j = 0; j < frozenColumnIndex; j++)
                //    {
                //        for (int i = tableInfo.TopRow; i <= tableInfo.BottomRow; i++)
                //        {
                //            if (i < dataSource.Count)
                //            {
                //                PaintCell(i, j, g);
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void PaintGridWithDataSourceOnUpdate(Graphics g, bool paintNew)
        {
            try
            {
                TotalRecordCount = dataSource.Count;

                //
                //Paint Normal Columns
                //
                for (int i = 0; i < this.cellsToDraw.Count; i++)
                {
                    TableInfo.CellStruct structure = this.cellsToDraw[i];

                    if (!tableInfo.VisibleColumns[structure.ColIndex].IsUnbound)
                    {
                        if (structure.RowIndex <= Table.LastRecordIndex || structure.CellModelType == CellType.Summary)
                        {
                            if (tableInfo.VisibleColumns[structure.ColIndex].Left > this.Width)
                                continue;

                            PaintCell(structure.RowIndex, structure.ColIndex, g);
                        }
                    }
                }

                //System.Diagnostics.Debug.WriteLine(a.ToString());

                //
                //Paint Unbound Columns
                //
                for (int i = 0; i < this.cellsUnboundToDraw.Count; i++)
                {
                    TableInfo.CellStruct structure = this.cellsUnboundToDraw[i];

                    if (tableInfo.VisibleColumns[structure.ColIndex].IsUnbound)
                    {
                        if (structure.RowIndex <= Table.LastRecordIndex && structure.RowIndex >= tableInfo.TopRow)
                        {
                            if (tableInfo.VisibleColumns[structure.ColIndex].Left > this.Width)
                                continue;

                            PaintCell(structure.RowIndex, structure.ColIndex, g);
                        }
                    }
                }

                //
                //Temp
                //
                //if (!string.IsNullOrEmpty(tableInfo.FrozenColumn))
                //{
                //    int frozenColumnIndex = tableInfo.GetVisibleColumnFromName(tableInfo.FrozenColumn).CurrentPosition + 1;

                //    for (int j = 0; j < frozenColumnIndex; j++)
                //    {
                //        for (int i = tableInfo.TopRow; i <= tableInfo.BottomRow; i++)
                //        {
                //            if (i < dataSource.Count)
                //            {
                //                PaintCell(i, j, g);
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void CleanCell(int rowIndex, int colIndex)
        {
            if (tableInfo.CellMatrix[rowIndex, colIndex].IsBlinkCell)
            {
                if (blinkRegistry.ContainsKey(tableInfo.CellMatrix[rowIndex, colIndex].Key))
                {
                    BlinkInfo info = blinkRegistry[tableInfo.CellMatrix[rowIndex, colIndex].Key];

                    //
                    //Check Blink Time here
                    //
                    if ((DateTime.Now.TimeOfDay.TotalMilliseconds - info.Ticks) > this.blinkTime)
                    {
                        tableInfo.CellMatrix[rowIndex, colIndex].IsBlinkCell = false;
                        tableInfo.CellMatrix[rowIndex, colIndex].BlinkType = BlinkType.None;

                        blinkRegistry.Remove(tableInfo.CellMatrix[rowIndex, colIndex].Key);
                    }
                }
                else
                {
                    tableInfo.CellMatrix[rowIndex, colIndex].IsBlinkCell = false;
                    tableInfo.CellMatrix[rowIndex, colIndex].BlinkType = BlinkType.None;
                }
            }
            else
            {
                tableInfo.CellMatrix[rowIndex, colIndex].IsDirty = false;
                tableInfo.CellMatrix[rowIndex, colIndex].IsEmpty = false;
                tableInfo.CellMatrix[rowIndex, colIndex].IsFormattedOnCondition = false;
                tableInfo.CellMatrix[rowIndex, colIndex].StrikeThrough = false;

                //if (!this.tableInfo.VisibleColumns[colIndex].QueryStyle)
                //{
                //    tableInfo.CellMatrix[rowIndex, colIndex].Style = tableInfo.columnDefaultStyles[tableInfo.CellMatrix[rowIndex, colIndex].Column.Name];//tableInfo.CellMatrix[rowIndex, colIndex].OriginalStyle;
                //}
                //else
                //{
                //    tableInfo.CellMatrix[rowIndex, colIndex].Style = tableInfo.CellMatrix[rowIndex, colIndex].OriginalStyle;
                //}
            }

            if (!isMainPaint || this.gridType == GridGrouping.GridType.Virtual && !tableInfo.VisibleColumns[colIndex].IsCustomColumn)
                tableInfo.CellMatrix[rowIndex, colIndex].LastUpdatedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            tableInfo.CellMatrix[rowIndex, colIndex].UpdateCustomFormula = true;
        }

        private bool UpdateCell(int rowNo, int colNo, bool mainRefresh)
        {
            //return false;

            bool isDirty = false;
            int colID = -1;

            if (rowNo < tableInfo.RowCount)
            {
                if (tableInfo.VisibleColumns[colNo].Left > this.Width)
                    return isDirty;

                isDirty = tableInfo.CellMatrix[rowNo, colNo].IsDirty;

                if (!tableInfo.VisibleColumns[colNo].IsUnbound)
                {
                    colID = tableInfo.VisibleColumns[colNo].Id;

                    if (colID >= 0)
                    {
                        if ((!isDirty || resetGrid || mainRefresh))
                        {
                            SetValue(ref tableInfo.CellMatrix[rowNo, colNo], tableInfo.CellMatrix[rowNo, colNo].CellStructType, colID);

                            isDirty = tableInfo.CellMatrix[rowNo, colNo].IsDirty || mainRefresh;

                            if (isDirty && tableInfo.IsSortingEnabled)
                            {
                                //
                                //Need to test this
                                //
                                if (tableInfo.SortedColumnDescriptors[tableInfo.VisibleColumns[colNo].MappingName] != null || this.tableInfo.IsDirty) // && !mainRefresh
                                    isSortingRequired = true;
                            }
                        }
                        else
                        {
                            if (tableInfo.AllowBlink && tableInfo.CellMatrix[rowNo, colNo].Column.AllowBlink && tableInfo.CellMatrix[rowNo, colNo].CellModelType != CellType.Summary)
                            {
                                SetValue(ref tableInfo.CellMatrix[rowNo, colNo], tableInfo.CellMatrix[rowNo, colNo].CellStructType, colID);
                                isDirty = tableInfo.CellMatrix[rowNo, colNo].IsDirty || mainRefresh;
                            }
                        }
                    }
                }
            }

            return isDirty;
        }

        internal override object GetRecordColumnValue(Record rec, TableInfo.TableColumn column)
        {
            if (column == null || rec == null || column.IsUnbound || rec.ColumnIndex < 0)
                return null;

            try
            {
                TableInfo.CellStruct structure = Table.CellMatrix[rec.CurrentIndex, rec.ColumnIndex];

                if (structure.SourceIndex < 0)
                    return null;

                switch (column.CellType)
                {
                    case TableInfo.CellStructType.String:
                        return GetStringValueUsingProperty(column.Id, structure.SourceIndex);
                    case TableInfo.CellStructType.Double:
                        return GetDoubleValueUsingProperty(column.Id, structure.SourceIndex);
                    case TableInfo.CellStructType.Integer:
                        return GetIntValueUsingProperty(column.Id, structure.SourceIndex);
                    case TableInfo.CellStructType.DateTime:
                        return GetDateTimeValueUsingProperty(column.Id, structure.SourceIndex);
                    case TableInfo.CellStructType.Long:
                        return GetLongValueUsingProperty(column.Id, structure.SourceIndex);
                    case TableInfo.CellStructType.Style:
                        return GetStyleUsingProperty(column.Id, structure.SourceIndex);
                    case TableInfo.CellStructType.Decimal:
                        return base.GetDecimalValueUsingProperty(column.Id, structure.SourceIndex);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return null;
        }

        protected override void SetValue(ref TableInfo.CellStruct cell, TableInfo.CellStructType type, int colID)
        {
            SetValue(ref cell, type, colID, null);
        }

        protected override void SetValue(ref TableInfo.CellStruct cell, TableInfo.CellStructType type, int colID, object value)
        {
            if (type == TableInfo.CellStructType.None)
                return;

            if (cell.SourceIndex < 0)
                return;

            if (value == null)
            {
                switch (type)
                {
                    case TableInfo.CellStructType.String:
                        cell.TextString = GetStringValueUsingProperty(colID, cell.SourceIndex, false);
                        break;
                    case TableInfo.CellStructType.Decimal:

                        //if (cell.Column.IsCustomFormulaColumn)
                        //{
                        //    //value = Convert.ToDecimal(cell.Column.Table.ExpressionFields[cell.Column.Name].GetValue(tableInfo.CellMatrix[cell.RowIndex, cell.ColIndex].Record));
                        //}
                        //else
                        value = GetDecimalValueUsingProperty(colID, cell.SourceIndex, false);

                        //
                        //TODO : Check this
                        //
                        if (decimal.TryParse(value.ToString(), out returnDecimal) || string.IsNullOrEmpty(cell.TextString))
                        {
                            cell.IsNumeric = true;
                            cell.TextDecimal = (decimal)value;
                            cell.TextString = string.Empty;
                        }
                        else
                        {
                            cell.IsNumeric = false;
                            cell.TextString = value.ToString();
                        }

                        if (cell.IsNumeric && cell.Type == typeof(decimal?) && returnDecimal == 0)
                            cell.DrawText = false;
                        break;
                    case TableInfo.CellStructType.Double:
                        //cell.TextDouble = GetDoubleValueUsingProperty(colID, cell.RowNo, false);

                        //if (cell.Column.IsCustomFormulaColumn)
                        //{
                        //    //value = Convert.ToDouble(cell.Column.Table.ExpressionFields[cell.Column.Name].GetValue(tableInfo.CellMatrix[cell.RowIndex, cell.ColIndex].Record));
                        //}
                        //else
                        value = GetDoubleValueUsingProperty(colID, cell.SourceIndex, false);

                        //
                        //TODO : Check this
                        //
                        if (double.TryParse(value.ToString(), out returnDouble) || string.IsNullOrEmpty(cell.TextString))
                        {
                            cell.IsNumeric = true;
                            cell.TextDouble = (double)value;
                            cell.TextString = string.Empty;
                        }
                        else
                        {
                            cell.IsNumeric = false;
                            cell.TextString = value.ToString();
                        }
                        if (cell.IsNumeric && cell.Type == typeof(double?) && returnDouble == 0)
                            cell.DrawText = false;
                        break;
                    case TableInfo.CellStructType.Integer:
                        value = GetIntValueUsingProperty(colID, cell.SourceIndex, false);

                        if (int.TryParse(value.ToString(), out returnInt))
                        {
                            cell.IsNumeric = true;
                            cell.TextInt = (int)value;
                            cell.TextString = string.Empty;
                        }
                        else
                        {
                            cell.IsNumeric = false;
                            cell.TextString = value.ToString();
                        }
                        if (cell.IsNumeric && cell.Type == typeof(int?) && returnInt == 0)
                            cell.DrawText = false;
                        break;
                    case TableInfo.CellStructType.DateTime:
                        cell.TextDateTime = GetDateTimeValueUsingProperty(colID, cell.SourceIndex, false);
                        break;
                    case TableInfo.CellStructType.Long:
                        value = GetLongValueUsingProperty(colID, cell.SourceIndex, false);

                        if (long.TryParse(value.ToString(), out returnLong))
                        {
                            cell.IsNumeric = true;
                            cell.TextLong = (long)value;
                            cell.TextString = string.Empty;
                        }
                        else
                        {
                            cell.IsNumeric = false;
                            cell.TextString = value.ToString();
                        }
                        if (cell.IsNumeric && cell.Type == typeof(long?) && returnLong == 0)
                            cell.DrawText = false;
                        break;
                    case TableInfo.CellStructType.Style:
                        cell.SetStyleCellValue(GetStyleUsingProperty(colID, cell.SourceIndex));
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case TableInfo.CellStructType.String:
                        //cell.TextString = value.ToString();

                        if (double.TryParse(value.ToString(), out returnDouble))
                        {
                            cell.IsNumeric = true;
                            cell.TextDouble = returnDouble;
                            cell.TextString = string.Empty;
                            cell.CellStructType |= TableInfo.CellStructType.Double;
                        }
                        else
                        {
                            if (long.TryParse(value.ToString(), out returnLong))
                            {
                                if (int.TryParse(value.ToString(), out returnInt))
                                {
                                    cell.IsNumeric = true;
                                    cell.TextInt = returnInt;
                                    cell.TextString = string.Empty;
                                    cell.CellStructType |= TableInfo.CellStructType.Integer;
                                }
                                else
                                {
                                    cell.IsNumeric = true;
                                    cell.TextLong = returnLong;
                                    cell.TextString = string.Empty;
                                    cell.CellStructType |= TableInfo.CellStructType.Long;
                                }
                            }
                            else
                            {
                                cell.IsNumeric = false;
                                cell.TextString = value.ToString();
                            }
                        }
                        break;
                    case TableInfo.CellStructType.Double:
                        //if (cell.IsNumeric)
                        //{
                        //    cell.TextDouble = (double)value;
                        //    cell.TextString = string.Empty;
                        //}
                        //else
                        //{
                        //    cell.TextString = value.ToString();
                        //}
                        if (double.TryParse(value.ToString(), out returnDouble))
                        {
                            cell.IsNumeric = true;
                            cell.TextDouble = returnDouble;
                            cell.TextString = string.Empty;
                        }
                        else
                        {
                            cell.IsNumeric = false;
                            cell.TextString = value.ToString();
                        }
                        break;
                    case TableInfo.CellStructType.Integer:
                        //if (cell.IsNumeric)
                        //{
                        //    cell.TextInt = (int)value;
                        //    cell.TextString = string.Empty;
                        //}
                        //else
                        //{
                        //    cell.TextString = value.ToString();
                        //}
                        if (int.TryParse(value.ToString(), out returnInt))
                        {
                            cell.IsNumeric = true;
                            if (long.TryParse(value.ToString(), out returnLong))
                            {
                                cell.TextInt = (int)returnLong;// (long)value;
                            }
                            else
                                cell.TextInt = (int)value;
                            cell.TextString = string.Empty;
                        }
                        else
                        {
                            cell.IsNumeric = false;
                            cell.TextString = value.ToString();
                        }
                        break;
                    case TableInfo.CellStructType.DateTime:
                        if (value is DateTime)
                        {
                            //if (DateTime.TryParseExact((value as DateTime).ToString(), "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out returnDateTime))
                            //{
                            cell.IsNumeric = true;
                            cell.TextDateTime = (DateTime)value;
                            cell.TextString = string.Empty;
                            //}
                            //else
                            //{
                            //    cell.IsNumeric = false;
                            //    cell.TextString = value.ToString();
                            //}
                        }
                        else
                        {
                            cell.IsNumeric = false;
                            cell.TextString = value.ToString();
                        }
                        break;
                    case TableInfo.CellStructType.Long:
                        //if (cell.IsNumeric)
                        //{
                        //    cell.TextLong = (long)value;
                        //    cell.TextString = string.Empty;
                        //}
                        //else
                        //{
                        //    cell.TextString = value.ToString();
                        //}
                        if (long.TryParse(value.ToString(), out returnLong))
                        {
                            cell.IsNumeric = true;

                            if (int.TryParse(value.ToString(), out returnInt))
                            {
                                cell.TextLong = (long)returnInt;// (int)value;
                            }
                            else
                                cell.TextLong = (long)value;

                            cell.TextString = string.Empty;
                        }
                        else
                        {
                            cell.IsNumeric = false;
                            cell.TextString = value.ToString();
                        }
                        break;
                    case TableInfo.CellStructType.Style:
                        cell.SetStyleCellValue(value);
                        break;
                }
            }
        }

        public int GetGroupedLastSourceIndexToInsert(int rowIndex, int colIndex)
        {
            int sourceIndex = -1;

            for (int i = 0; i < tableInfo.RowCount; i++)
            {
                if (tableInfo.CellMatrix[i, colIndex].Record == null)
                    continue;

                if (rowIndex < i)
                    sourceIndex = tableInfo.CellMatrix[i, colIndex].SourceIndex;
            }

            if (sourceIndex == -1)
            {
                //for (int j = colIndex + 1; j < this.Table.VisibleColumns.Count; j++)
                //{
                //    TableInfo.TableColumn col = this.Table.VisibleColumns[j];

                //    if (col.IsPrimaryColumn)
                //    {
                //        colIndex = j;
                //        break;
                //    }
                //}

                //for (int i = 0; i < tableInfo.RowCount; i++)
                //{
                //    if (tableInfo.CellMatrix[i, colIndex].Record == null)
                //        continue;

                //    sourceIndex = tableInfo.CellMatrix[i, colIndex].SourceIndex;
                //    break;
                //}
            }

            return sourceIndex;
        }

        private string GetValueAsString(TableInfo.CellStruct cell, TableInfo.CellStructType type, GridStyleInfo style)
        {
            returnString = string.Empty;
            string format = string.Empty;

            if (style != null)
                format = style.Format;

            switch (type)
            {
                case TableInfo.CellStructType.String:
                    returnString = cell.TextString;
                    break;
                case TableInfo.CellStructType.Double:
                    if (!cell.IsNumeric && !string.IsNullOrEmpty(cell.TextString))
                        returnString = cell.TextString;
                    else
                        returnString = cell.TextDouble.ToString(format);
                    break;
                case TableInfo.CellStructType.Decimal:
                    if (!cell.IsNumeric && !string.IsNullOrEmpty(cell.TextString))
                        returnString = cell.TextString;
                    else
                        returnString = cell.TextDecimal.ToString(format);
                    break;
                case TableInfo.CellStructType.Integer:
                    if (!cell.IsNumeric && !string.IsNullOrEmpty(cell.TextString))
                        returnString = cell.TextString;
                    else
                        returnString = cell.TextInt.ToString(format);
                    break;
                case TableInfo.CellStructType.DateTime:
                    if (!cell.IsNumeric && !string.IsNullOrEmpty(cell.TextString) && cell.TextDateTime == DateTime.MinValue)
                        returnString = cell.TextString;
                    else
                        returnString = cell.TextDateTime.ToString(format);
                    break;
                case TableInfo.CellStructType.Long:
                    if (!cell.IsNumeric && !string.IsNullOrEmpty(cell.TextString))
                        returnString = cell.TextString;
                    else
                        returnString = cell.TextLong.ToString(format);
                    break;
                case TableInfo.CellStructType.Style:
                    if (cell.Style.CellValue != null)
                        returnString = cell.Style.CellValue.ToString();
                    break;
            }

            return returnString;
        }

        private string GetStringValueUsingProperty(int colID, int i)
        {
            return GetStringValueUsingProperty(colID, i, true);
        }

        private new string GetStringValueUsingProperty(int colID, int i, bool lookInOriginalRecordList)
        {
            returnString = string.Empty;

            try
            {
                if (tableInfo.IsFilterEnabled && !lookInOriginalRecordList)
                {
                    if (tableInfo.FilteredRecords != null && i < tableInfo.FilteredRecords.Count)
                    {
                        returnbject = PropertyList[colID].Get((tableInfo.FilteredRecords[i] as Record).ObjectBound);

                        if (returnbject != null)
                            returnString = returnbject.ToString();
                    }
                    else
                    {
                        if (tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                        {
                            returnbject = PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound);

                            if (returnbject != null)
                                returnString = returnbject.ToString();
                        }
                    }
                }
                else
                {
                    if (tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                    {
                        returnbject = PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound);

                        if (returnbject != null)
                            returnString = returnbject.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return returnString;
        }

        private int GetIntValueUsingProperty(int colID, int i)
        {
            return GetIntValueUsingProperty(colID, i, true);
        }

        private int GetIntValueUsingProperty(int colID, int i, bool lookInOriginalRecordList)
        {
            returnInt = 0;

            try
            {
                if (tableInfo.IsFilterEnabled && !lookInOriginalRecordList)
                {
                    if (tableInfo.FilteredRecords != null && i < tableInfo.FilteredRecords.Count)
                        returnInt = Convert.ToInt32(PropertyList[colID].Get((tableInfo.FilteredRecords[i] as Record).ObjectBound));

                    else
                    {
                        if (tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                        {
                            returnInt = Convert.ToInt32(PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound));
                        }
                    }
                }
                else
                {
                    if (tableInfo.AllRecords.Count <= i)
                        return returnInt;

                    returnbject = PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound);
                    if (returnbject != null && tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                        returnInt = Convert.ToInt32(returnbject);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return returnInt;
        }

        private long GetLongValueUsingProperty(int colID, int i)
        {
            return GetLongValueUsingProperty(colID, i, true);
        }

        private long GetLongValueUsingProperty(int colID, int i, bool lookInOriginalRecordList)
        {
            returnLong = 0;

            try
            {
                if (tableInfo.IsFilterEnabled && !lookInOriginalRecordList)
                {
                    if (tableInfo.FilteredRecords != null && i < tableInfo.FilteredRecords.Count)
                        returnLong = Convert.ToInt64(PropertyList[colID].Get((tableInfo.FilteredRecords[i] as Record).ObjectBound));

                    else
                    {
                        if (tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                        {
                            returnLong = Convert.ToInt64(PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound));
                        }
                    }
                }
                else
                {
                    if (tableInfo.AllRecords.Count <= i)
                        return returnLong;

                    returnbject = PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound);
                    if (returnbject != null && tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                        returnLong = Convert.ToInt64(returnbject);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return returnLong;
        }

        private object GetStyleUsingProperty(int colID, int i)
        {
            return GetStyleUsingProperty(colID, i, true);
        }

        private object GetStyleUsingProperty(int colID, int i, bool lookInOriginalRecordList)
        {
            returnbject = 0;

            try
            {
                if (tableInfo.IsFilterEnabled && !lookInOriginalRecordList)
                {
                    if (tableInfo.FilteredRecords != null && i < tableInfo.FilteredRecords.Count)
                        returnbject = PropertyList[colID].Get((tableInfo.FilteredRecords[i] as Record).ObjectBound);

                    else
                    {
                        if (tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                        {
                            returnbject = PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound);
                        }
                    }
                }
                else
                {
                    if (tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                        returnbject = PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return returnbject;
        }

        private double GetDoubleValueUsingProperty(int colID, int i)
        {
            return GetDoubleValueUsingProperty(colID, i, true);
        }

        private double GetDoubleValueUsingProperty(int colID, int i, bool lookInOriginalRecordList)
        {
            returnDouble = 0;

            try
            {
                if (tableInfo.IsFilterEnabled && !lookInOriginalRecordList)
                {
                    if (tableInfo.FilteredRecords != null && i < tableInfo.FilteredRecords.Count)
                        returnDouble = Convert.ToDouble(PropertyList[colID].Get((tableInfo.FilteredRecords[i] as Record).ObjectBound));

                    else
                    {
                        if (tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                        {
                            returnDouble = Convert.ToDouble(PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound));
                        }
                    }
                }
                else
                {
                    if (tableInfo.AllRecords.Count <= i)
                        return returnDouble;

                    if (tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                        returnDouble = Convert.ToDouble(PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound));
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return returnDouble;
        }

        private DateTime GetDateTimeValueUsingProperty(int colID, int i)
        {
            return GetDateTimeValueUsingProperty(colID, i, true);
        }

        private DateTime GetDateTimeValueUsingProperty(int colID, int i, bool lookInOriginalRecordList)
        {
            returnDateTime = DateTime.MinValue;

            try
            {
                if (tableInfo.IsFilterEnabled && !lookInOriginalRecordList)
                {
                    if (tableInfo.FilteredRecords != null && i < tableInfo.FilteredRecords.Count)
                        returnDateTime = Convert.ToDateTime(PropertyList[colID].Get((tableInfo.FilteredRecords[i] as Record).ObjectBound));

                    else
                    {
                        if (tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                        {
                            returnDateTime = Convert.ToDateTime(PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound));
                        }
                    }
                }
                else
                {
                    if (tableInfo.AllRecords != null && i < tableInfo.AllRecords.Count)
                        returnDateTime = Convert.ToDateTime(PropertyList[colID].Get((tableInfo.AllRecords[i] as Record).ObjectBound));
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return returnDateTime;
        }

        protected override void PaintGridData(object sender, Graphics g)
        {
            if (StopPaint)
                return;

            try
            {
                if (cellsToDraw.Count <= 0 && cellsUnboundToDraw.Count <= 0)
                    return;

                g.CompositingQuality = CompositingQuality.HighSpeed;

                if (isGridPaintedOnce)
                    stopPaint = true;

                Rectangle rect = new Rectangle(this.Location.X, this.Location.Y, this.Width - VScroll.Width, this.Height);

                if (!isGridPaintedOnce || this.tableInfo.IsDirty)
                    PaintHeaders(grafx.Graphics, rect);

                if (this.dataSource != null)
                    PaintGridWithDataSourceOnUpdate(grafx.Graphics, true);
                else
                    PaintGridWithMatrixOnUpdate(grafx.Graphics, true);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                isGridUpdatedOnce = true;
                isSortingRequired = false;
                stopPaint = false;
                resetGrid = false;
            }
        }

        private new void PaintHeaders(Graphics g, Rectangle rect)
        {
            if (tableInfo.VisibleColumns.Count == 0)
                return;

            if (tableInfo.AllowNestedHeaders)
            {
                if (tableInfo.MergedHeaderColumns.Count > 0)
                {
                    for (int i = 0; i < this.tableInfo.MergedHeaderColumns.Count; i++)
                    {
                        this.cellsNestedHeaders.Add(new TableInfo.CellStruct(this.tableInfo.MergedHeaderColumns[i].RowIndex, this.tableInfo.MergedHeaderColumns[i].EndIndex, null, tableInfo.VisibleColumns[this.tableInfo.MergedHeaderColumns[i].EndIndex], typeof(string)));
                    }
                }
                else
                {
                    for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
                    {
                        for (int i = 0; i < this.tableInfo.NumberOfNestedHeaders; i++)
                        {
                            this.cellsNestedHeaders.Add(new TableInfo.CellStruct(i, j, null, tableInfo.VisibleColumns[j], typeof(string)));
                        }
                    }
                }

                OnQueryNestedHeaderGridCells();
            }

            if (tableInfo.AllowRowHeaders)
            {
                int top = this.tableInfo.TopRow + this.Table.NumberOfNestedHeaders;
                int bottom = this.tableInfo.BottomRow + this.Table.NumberOfNestedHeaders;

                for (int i = top; i < bottom; i++)
                {
                    if (i <= tableInfo.RowCount)
                    {
                        TableInfo.CellStruct structure = new TableInfo.CellStruct(i, -1, null, tableInfo.VisibleColumns[0], typeof(string));
                        structure.TextString = i.ToString();
                        this.cellsRowHeaders.Add(structure);
                    }
                }

                OnQueryRowHeaderGridCells();
            }

            if (!tableInfo.HideHeader)
                GridPainter.PaintHeaders(g, rect);
        }

        protected override void RaiseCommonQueryCellEvents()
        {
            //if (StopPaint)
            //    return;

            //
            //Query Cells
            //
            OnQueryGridCells();

            OnQuerySummaryGridCells();
        }

        protected override void CheckCellsToUpdate(bool mainRefresh)
        {
            if ((tableInfo.IsFilterEnabled && tableInfo.FilteredRecords.Count <= 0) || StopPaint || tableInfo.VisibleColumns.Count == 0)
                return;

            if (dataSource == null)
            {
                OnQueryGridRowCount();
            }

            int bottomRow = this.tableInfo.BottomRow;
            TableInfo.TableColumn currentColumn = null;

            for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
            {
                if (tableInfo.VisibleColumns[j].Left > this.Width)
                    continue;

                if (tableInfo.CellMatrix.GetLength(1) <= j)
                    continue;

                currentColumn = tableInfo.VisibleColumns[j];
                tableInfo.VisibleColumns[j].CurrentPosition = j;

                if (this.dataSource != null)
                {
                    for (int i = this.tableInfo.TopRow; i < bottomRow + 1; i++)
                    {
                        if (tableInfo.CellMatrix.GetLength(0) > i)
                            if (tableInfo.CellMatrix[i, j].CellModelType == CellType.Summary)
                                this.cellsSummaryToUpdate.Add(tableInfo.CellMatrix[i, j]);
                    }

                    if (!tableInfo.VisibleColumns[j].IsCustomColumn)
                    {
                        if (tableInfo.VisibleColumns[j].IsUnbound)
                        {
                            int recordIndex = -1;

                            for (int i = 0; i < tableInfo.RowCount; i++)
                            {
                                if (tableInfo.CellMatrix[i, j].Record == null)
                                    continue;

                                recordIndex++;

                                if (tableInfo.AllRecords.Count <= recordIndex)
                                    continue;

                                if (tableInfo.VisibleColumns[j].IsPrimaryColumn)
                                {
                                    if (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] != null)
                                    {
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).CurrentIndex = i;
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).ColumnIndex = j;
                                    }
                                }

                                if (UpdateRecordIndex(i, j) && !tableInfo.FilterManager.IsFilterAvailable(tableInfo.VisibleColumns[j].Name))
                                {
                                    this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                                }
                            }
                        }
                        else if (tableInfo.VisibleColumns[j].QueryStyle)
                        {
                            int recordIndex = -1;

                            for (int i = 0; i < tableInfo.RowCount; i++)
                            {
                                if (tableInfo.CellMatrix.GetLength(0) <= i || tableInfo.CellMatrix.GetLength(1) <= j)
                                    continue;

                                if (tableInfo.CellMatrix[i, j].Record == null)
                                    continue;

                                recordIndex++;

                                if (tableInfo.AllRecords.Count <= recordIndex)
                                    continue;

                                if (tableInfo.VisibleColumns[j].IsPrimaryColumn)
                                {
                                    if (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] != null)
                                    {
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).CurrentIndex = i;
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).ColumnIndex = j;
                                    }
                                }

                                if (i >= tableInfo.TopRow && i < bottomRow + 1)
                                {
                                    if (UpdateRecordIndex(recordIndex, j))
                                    {
                                        if (UpdateCell(i, j, mainRefresh))
                                        {
                                            this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);

                                            if (tableInfo.AllowBlink && isGridUpdatedOnce)
                                                AddToBlinkRegistry(i, j);
                                        }

                                        if (!isGridUpdatedOnce || mainRefresh || this.tableInfo.IsDirty)
                                            tableInfo.CellMatrix[i, j].IsBlinkCell = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            int recordIndex = -1;

                            for (int i = 0; i < tableInfo.RowCount; i++)
                            {
                                if (tableInfo.CellMatrix.GetLength(0) <= i || tableInfo.CellMatrix.GetLength(1) <= j)
                                    continue;

                                if (tableInfo.CellMatrix[i, j].Record == null)
                                    continue;

                                recordIndex++;

                                if (tableInfo.AllRecords.Count <= recordIndex)
                                    continue;

                                if (tableInfo.VisibleColumns[j].IsPrimaryColumn)
                                {
                                    if (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] != null)
                                    {
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).CurrentIndex = i;
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).ColumnIndex = j;
                                    }
                                }

                                if (i >= tableInfo.TopRow && i < bottomRow + 1)
                                {

                                    if (UpdateRecordIndex(recordIndex, j))
                                    {
                                        if (UpdateCell(i, j, mainRefresh))
                                        {
                                            this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);

                                            if (tableInfo.AllowBlink && isGridUpdatedOnce)
                                                AddToBlinkRegistry(i, j);
                                        }

                                        if (!isGridUpdatedOnce || mainRefresh || this.tableInfo.IsDirty)
                                            tableInfo.CellMatrix[i, j].IsBlinkCell = false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int recordIndex = -1;
                        //
                        //Custom Columns
                        //
                        for (int i = 0; i < tableInfo.RowCount; i++)
                        {
                            if (tableInfo.CellMatrix.GetLength(0) <= i || tableInfo.CellMatrix.GetLength(1) <= j)
                                continue;

                            if (tableInfo.CellMatrix[i, j].Record == null)
                                continue;

                            recordIndex++;

                            if (tableInfo.AllRecords.Count <= recordIndex)
                                continue;

                            if (tableInfo.VisibleColumns[j].IsPrimaryColumn)
                            {
                                if (tableInfo.CellMatrix[i, j].SourceIndex >= tableInfo.AllRecords.Count)
                                    tableInfo.CellMatrix[i, j].SourceIndex = tableInfo.AllRecords.Count - 1;

                                if (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] != null)
                                {
                                    (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).CurrentIndex = i;
                                    (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).ColumnIndex = j;
                                }
                            }

                            if (i >= tableInfo.TopRow && i < bottomRow + 1)
                            {
                                if (UpdateRecordIndex(recordIndex, j))
                                {
                                    if (!tableInfo.VisibleColumns[j].IsUnbound)
                                    {
                                        if (UpdateCell(i, j, mainRefresh))
                                            this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                                    }
                                    else
                                    {
                                        this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                                    }
                                    if (tableInfo.AllowBlink && isGridUpdatedOnce)
                                        AddToBlinkRegistry(i, j);

                                    if (!isGridUpdatedOnce || mainRefresh || this.tableInfo.IsDirty)
                                        tableInfo.CellMatrix[i, j].IsBlinkCell = false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (resetGrid || mainRefresh)
                    {
                        int index = 0;

                        for (int i = tableInfo.TopRow; i < bottomRow + 1; i++)
                        {
                            //if (i < tableInfo.RowCount)
                            if (tableInfo.sourceDataRowCount == -1 || i < tableInfo.sourceDataRowCount)
                                this.cellsUnboundToUpdate.Add(new TableInfo.CellStruct(index, j, i, null, tableInfo.VisibleColumns[j], null));
                            if (this.tableInfo.CellMatrix.GetLength(0) > index)
                                this.tableInfo.CellMatrix[index, j].SourceIndex = i;
                            index++;
                        }
                    }

                    //if (resetGrid || mainRefresh)
                    //{
                    //    for (int i = tableInfo.TopRow; i < this.tableInfo.BottomRow + 1; i++)
                    //    {
                    //        if (i < tableInfo.RowCount)
                    //            this.cellsUnboundToUpdate.Add(new TableInfo.CellStruct(i, j, null, tableInfo.VisibleColumns[j], null));
                    //    }
                    //}
                }
            }

            //
            //Checking Custom Formula Columns
            //
            if (Table.AllowCustomFormulas)
            {
                for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
                {
                    currentColumn = tableInfo.VisibleColumns[j];

                    if (currentColumn.Left > this.Width)
                        continue;

                    if (tableInfo.CellMatrix.GetLength(1) <= j)
                        continue;

                    int recordIndex = -1;

                    //if (currentColumn.IsCustomFormulaColumn)
                    //{
                    //    for (int i = 0; i < tableInfo.RowCount; i++)
                    //    {
                    //        if (tableInfo.CellMatrix[i, j].Record == null)
                    //            continue;

                    //        recordIndex++;

                    //        if (i >= tableInfo.TopRow)
                    //        {
                    //            if (UpdateRecordIndex(recordIndex, j))
                    //            {
                    //                if (currentColumn.Table.ExpressionFields[currentColumn.Name] != null && (mainRefresh || tableInfo.CellMatrix[i, j].UpdateCustomFormula || tableInfo.CellMatrix[i, j].IsEmpty))
                    //                {
                    //                    //
                    //                    //TODO : Need to recheck this method and do optimizations
                    //                    //
                    //                    //if (resetGrid || mainRefresh || ShouldUpdateValue(i, j, currentColumn.Name))
                    //                    {
                    //                        SetValue(ref tableInfo.CellMatrix[i, j], tableInfo.CellMatrix[i, j].CellStructType, -2);

                    //                        if (tableInfo.CellMatrix[i, j].IsDirty || resetGrid || mainRefresh && tableInfo.IsSortingEnabled)
                    //                        {
                    //                            if (tableInfo.SortedColumnDescriptors[currentColumn.MappingName] != null || this.tableInfo.IsDirty)
                    //                                isSortingRequired = true;
                    //                        }

                    //                        this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                    //                        tableInfo.CellMatrix[i, j].UpdateCustomFormula = false;

                    //                        if (tableInfo.AllowBlink && isGridUpdatedOnce)
                    //                            AddToBlinkRegistry(i, j);
                    //                    }

                    //                    if (!isGridUpdatedOnce || mainRefresh || this.tableInfo.IsDirty)
                    //                        tableInfo.CellMatrix[i, j].IsBlinkCell = false;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
        }

        public override void PopulateAllCellValues(bool isCustom = false)
        {
            if ((tableInfo.IsFilterEnabled && tableInfo.FilteredRecords.Count <= 0) || StopPaint || tableInfo.VisibleColumns.Count == 0)
                return;

            bool mainRefresh = true;

            if (dataSource == null)
            {
                OnQueryGridRowCount();
            }

            int bottomRow = tableInfo.BottomRow;

            for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
            {
                //if (tableInfo.VisibleColumns[j].Left > this.Width)
                //    continue;

                if (tableInfo.CellMatrix.GetLength(1) <= j)
                    continue;

                if (this.dataSource != null)
                {
                    if (!tableInfo.VisibleColumns[j].IsCustomColumn)
                    {
                        if (tableInfo.VisibleColumns[j].IsUnbound)
                        {
                            int recordIndex = -1;

                            for (int i = 0; i < tableInfo.RowCount; i++)
                            {
                                if (tableInfo.CellMatrix[i, j].Record == null)
                                    continue;

                                recordIndex++;

                                if (tableInfo.AllRecords.Count <= recordIndex)
                                    continue;

                                if (tableInfo.VisibleColumns[j].IsPrimaryColumn)
                                {
                                    if (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] != null)
                                    {
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).CurrentIndex = i;
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).ColumnIndex = j;
                                    }
                                }

                                if (UpdateRecordIndex(i, j) && !tableInfo.FilterManager.IsFilterAvailable(tableInfo.VisibleColumns[j].Name))
                                {
                                    this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                                }
                            }
                        }
                        else if (tableInfo.VisibleColumns[j].QueryStyle)
                        {
                            int recordIndex = -1;

                            for (int i = 0; i < tableInfo.RowCount; i++)
                            {
                                if (tableInfo.CellMatrix[i, j].Record == null)
                                    continue;

                                recordIndex++;

                                if (tableInfo.AllRecords.Count <= recordIndex)
                                    continue;

                                if (tableInfo.VisibleColumns[j].IsPrimaryColumn)
                                {
                                    if (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] != null)
                                    {
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).CurrentIndex = i;
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).ColumnIndex = j;
                                    }
                                }

                                if (UpdateRecordIndex(recordIndex, j))
                                {
                                    if (UpdateCell(i, j, mainRefresh, true))
                                    {
                                        this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                                    }
                                }
                            }
                        }
                        else
                        {
                            int recordIndex = -1;

                            for (int i = 0; i < tableInfo.RowCount; i++)
                            {
                                if (tableInfo.CellMatrix[i, j].Record == null)
                                    continue;

                                recordIndex++;

                                if (tableInfo.AllRecords.Count <= recordIndex)
                                    continue;

                                if (tableInfo.VisibleColumns[j].IsPrimaryColumn)
                                {
                                    if (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] != null)
                                    {
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).CurrentIndex = i;
                                        (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).ColumnIndex = j;
                                    }
                                }

                                if (UpdateRecordIndex(recordIndex, j))
                                {
                                    if (UpdateCell(i, j, mainRefresh, true))
                                    {
                                        this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int recordIndex = -1;
                        //
                        //Custom Columns
                        //
                        for (int i = 0; i < tableInfo.RowCount; i++)
                        {
                            if (tableInfo.CellMatrix[i, j].Record == null)
                                continue;

                            recordIndex++;

                            if (tableInfo.AllRecords.Count <= recordIndex)
                                continue;

                            if (tableInfo.VisibleColumns[j].IsPrimaryColumn)
                            {
                                if (tableInfo.CellMatrix[i, j].SourceIndex >= tableInfo.AllRecords.Count)
                                    tableInfo.CellMatrix[i, j].SourceIndex = tableInfo.AllRecords.Count - 1;

                                if (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] != null)
                                {
                                    (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).CurrentIndex = i;
                                    (tableInfo.AllRecords[tableInfo.CellMatrix[i, j].SourceIndex] as Record).ColumnIndex = j;
                                }
                            }

                            if (UpdateRecordIndex(recordIndex, j))
                            {
                                if (!tableInfo.VisibleColumns[j].IsUnbound)
                                {
                                    if (UpdateCell(i, j, mainRefresh, true))
                                        this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                                }
                                else
                                {
                                    this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (resetGrid || mainRefresh)
                    {
                        int index = 0;

                        for (int i = tableInfo.TopRow; i < bottomRow + 1; i++)
                        {
                            //if (i < tableInfo.RowCount)
                            if (tableInfo.sourceDataRowCount == -1 || i < tableInfo.sourceDataRowCount)
                                this.cellsUnboundToUpdate.Add(new TableInfo.CellStruct(index, j, i, null, tableInfo.VisibleColumns[j], null));
                            if (this.tableInfo.CellMatrix.GetLength(0) > index)
                                this.tableInfo.CellMatrix[index, j].SourceIndex = i;
                            index++;
                        }
                    }
                }
            }

            RaiseCommonQueryCellEvents();
        }

        protected override bool UpdateRecordIndex(int row, int column)
        {
            if (tableInfo.IsFilterEnabled)
            {
                if (tableInfo.FilteredRecords.Count <= row || tableInfo.CellMatrix.GetLength(0) <= row)
                    return false;

                tableInfo.CellMatrix[row, column].Record = tableInfo.FilteredRecords[row] as Record;
                (tableInfo.FilteredRecords[row] as Record).SourceIndex = row;
            }
            else
            {
                if (tableInfo.CellMatrix.GetLength(0) <= row)
                    return false;

                //tableInfo.CellMatrix[row, column].Record = tableInfo.AllRecords[row] as Record;
                if (tableInfo.AllRecords[row] != null)
                    (tableInfo.AllRecords[row] as Record).SourceIndex = row;
            }

            return true;
        }

        #endregion

        #region UI Events

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //base.OnPaintBackground(e);

            try
            {
                GridPainter.DrawMainGridBackGround(grafx.Graphics, this.BackColor, e.ClipRectangle);

                Rectangle rect = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width - VScroll.Width, e.ClipRectangle.Height);
                //
                //Draw Header
                //
                if (!tableInfo.HideHeader)
                    GridPainter.PaintHeaders(grafx.Graphics, rect);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            #region Prev

            //return;
            //base.OnPaint(e);

            //e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;


            //e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;


            //e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            //e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;

            //lock (paintLock)
            //{
            //    PaintGrid(null, e);
            //}

            #endregion

            grafx.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

            lock (paintLock)
            {
                PaintGrid(null, grafx.Graphics);

                if (grafx != null)
                    grafx.Render(e.Graphics);
            }
        }

        private void ProcessMouseDown(MouseEventArgs e)
        {
            try
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left && !tableInfo.IsDragging)
                {
                    WindowsApiClass.ReleaseCapture();

                    // left
                    if (e.X <= 2 & e.Y <= 8)
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitTopLeft, 0);
                    else if (e.X <= 2 & (e.Y > 8 & e.Y < this.Height - 8))
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitLeft, 0);
                    else if (e.X <= 2 & (e.Y >= this.Height - 8))
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitBottomLeft, 0);

                    // right
                    else if (e.X >= this.Width - 2 & e.Y <= 8)
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitTopRight, 0);
                    else if (e.X >= this.Width - 2 & (e.Y > 8 & e.Y < this.Height - 8))
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitRight, 0);
                    else if (e.X >= this.Width - 2 & (e.Y >= this.Height - 8))
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitBottomRight, 0);

                    // top
                    else if (e.Y <= 2 & e.X <= 8)
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitTopLeft, 0);
                    else if (e.Y <= 2 & (e.X > 8 & e.X < this.Width - 8))
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitTop, 0);
                    else if (e.Y <= 2 & (e.X >= this.Width - 8))
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitTopRight, 0);

                    // bottom
                    else if (e.Y >= this.Height - 2 & e.X <= 8)
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitBottomLeft, 0);
                    else if (e.Y >= this.Height - 2 & (e.X > 8 & e.X < this.Width - 8))
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitBottom, 0);
                    else if (e.Y >= this.Height - 2 & (e.X >= this.Width - 8))
                        WindowsApiClass.SendMessage(this.Handle, WindowsApiClass.WmNonClientLeftButtonDown, WindowsApiClass.HitBottomRight, 0);

                    // caption
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void GridGroupingControl_MouseDown(object sender, MouseEventArgs e)
        {
            ProcessMouseDown(e);

            try
            {
                if (this.gridType == GridGrouping.GridType.Virtual)
                    Table.ProcessMouseDown(tableInfo.SourceDataRowCount, e);
                else if (this.gridType == GridGrouping.GridType.MultiColumn)
                    Table.ProcessMouseDown(tableInfo.RowCount, e);
                else
                    Table.ProcessMouseDown(TotalRecordCount, e);
                VScroll.ShowScrollBar();
                HScroll.ShowScrollBar();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void GridGroupingControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (this.gridType == GridGrouping.GridType.Virtual)
                    Table.ProcessMouseUp(tableInfo.SourceDataRowCount, e);
                else
                    Table.ProcessMouseUp(TotalRecordCount, e);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void GridGroupingControl_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                Table.ProcessMouseMove(e);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void GridGroupingControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                if (this.gridType == GridGrouping.GridType.Virtual)
                    this.Refresh();

                Table.ProcessMouseClick(e);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void GridGroupingControl_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                Table.ProcessMouseDoubleClick(e);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void GridGroupingControl_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Table.ProcessKeyUp(e);
        }

        protected override void GridGroupingControl_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Table.ProcessKeyDown(e);
        }

        public override void SetEditTextBox(TextBox textBox)
        {
            this.editTextBox = textBox;
            if (!this.Controls.Contains(EditTextBox))
                this.Controls.Add(EditTextBox);
            this.EditTextBox.Visible = false;
        }

        internal override void DisplayEditTextBox(int rowIndex, int colIndex, Point location)
        {
            if (!isEditable)
                return;

            try
            {
                if (colIndex < 0 || rowIndex < 0)
                    return;

                if (location != Point.Empty && !OnDisplayTextBox(rowIndex, colIndex, location))
                {
                    HideEditTextBox();
                    return;
                }

                if (!this.Table.VisibleColumns[colIndex].IsEditColumn)
                    return;

                if (this.Table.VisibleColumns[colIndex].ShowButtons)
                {
                    string cellModel = Table.CellMatrix[rowIndex, colIndex].CellModelType;

                    Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers.CellRendererBase renderer = CellRenderers[cellModel];

                    if (renderer != null && renderer.Buttons.Count > 0
                        && !string.IsNullOrEmpty(Table.CellMatrix[rowIndex, colIndex].TextString))
                    {
                        Rectangle cellRectangle = new Rectangle();

                        cellRectangle.X = Table.VisibleColumns[colIndex].Left;// +2;
                        cellRectangle.Y = Table.HeaderHeight + ((rowIndex - Table.TopRow) * Table.RowHeight);
                        cellRectangle.Width = Table.VisibleColumns[colIndex].CellWidth;
                        cellRectangle.Height = Table.RowHeight;

                        Rectangle[] buttonsBounds = new Rectangle[renderer.Buttons.Count];

                        renderer.PerformCellLayout(rowIndex, colIndex, Table.CellMatrix[rowIndex, colIndex].Style, cellRectangle);
                        renderer.OnLayout(rowIndex, colIndex, Table.CellMatrix[rowIndex, colIndex].Style, cellRectangle, buttonsBounds);

                        for (int i = 0; i < renderer.Buttons.Count; i++)
                        {
                            if (renderer.GetButton(i).Visible)
                            {
                                renderer.GetButton(i).Bounds = buttonsBounds[i];
                                if ((renderer.Buttons[i] as GridCellButton).Bounds.Contains(location))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }

                //editTextBox.ShowDropDown = false;
                Rect.X = tableInfo.VisibleColumns[colIndex].Left;
                Rect.Y = tableInfo.HeaderHeight + (rowIndex - tableInfo.TopRow) * tableInfo.RowHeight;
                Rect.Width = tableInfo.VisibleColumns[colIndex].CellWidth + 2;
                Rect.Height = tableInfo.RowHeight - 2;
                EditTextBox.SetBounds(Rect.X, Rect.Y, Rect.Width, Rect.Height);

                TableInfo.CellStruct cell = tableInfo.CellMatrix[rowIndex, colIndex];

                GridStyleInfo style = cell.Style;

                string strText = GetValueAsString(cell, cell.CellStructType, style);

                EditTextBox.Text = strText;
                //editTextBox.Font = FontData;
                //editTextBox.ShowSearchImage();
                EditTextBox.Visible = true;
                EditTextBox.Select(0, EditTextBox.Text.Length);
                EditTextBox.Focus();
                EditTextBox.BringToFront();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                //txtSymbol.ShowDropDown = true;
            }
        }

        internal override void HideEditTextBox()
        {
            if (!isEditable)
                return;

            try
            {
                EditTextBox.Visible = false;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                //txtSymbol.ShowDropDown = true;
            }
        }

        private void DrawToBuffer(Graphics g)
        {
            if (StopPaint)
                return;

            try
            {
                g.CompositingQuality = CompositingQuality.HighSpeed;

                if (this.dataSource != null)
                    PaintGridWithDataSource(g, true);
                else
                    PaintGridWithMatrix(g, true);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            HideEditTextBox();
            sizeChanging = true;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (EnableAutoResizeGridColumns && TurnOnAutoResizeGridColumns)
                isGridColumnResizeInProgress = true;

            base.OnSizeChanged(e);

            if (TurnOnAutoResizeGridColumns && EnableAutoResizeGridColumns && isGridColumnResizeInProgress)
            {
                isGridColumnResizeInProgress = false;
            }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            Form f = FindForm();

            if (f != null && !f.IsDisposed)
            {
                VScroll.AttachControl = this;
                HScroll.AttachControl = this;
            }

            base.OnParentChanged(e);
        }

        private void OptimizedGrid_SizeChanged(object sender, EventArgs e)
        {
            try
            {
                sizeChanging = false;

                if (this.isFormLoaded == false)
                    return;

                OnBufferedDataInsert();

                this.Invalidate();

                PrepareTables();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            OnBufferedDataInsert();

            try
            {
                if (TotalRecordCount > 0)
                {
                    int vScrollBarValue = VScroll.Value;

                    if (vScrollBarValue >= 0)
                        Table.TopRow = vScrollBarValue;// -((vScrollBarValue > Table.DisplayRows) ? Table.DisplayRows : 0);
                    else
                        Table.TopRow = 0;
                    queryVirtualData = true;
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            OnBufferedDataInsert();

            try
            {
                if (!this.tableInfo.IsFilterEnabled)
                    Table.ProcessMouseWheel(TotalRecordCount, e);
                else
                    Table.ProcessMouseWheel(tableInfo.FilteredRecords.Count, e);

                VScroll.Value = Table.TopRow;
                Invalidate();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                if (editTextBox != null && editTextBox.Visible)
                {
                    if (keyData == Keys.Tab)
                        return false;
                    return base.ProcessCmdKey(ref msg, keyData);
                }

                OnBufferedDataInsert();
                bool blnProcess = false;

                //
                // key up
                //
                if (keyData == Keys.Up || keyData == (Keys.Up | Keys.Shift))
                {
                    if (TotalRecordCount > 0)
                    {
                        if (Table.CurrentRow > 0)
                        {
                            if (keyData == (Keys.Up | Keys.Shift))
                            {
                                tableInfo.CurrentRow -= 1;
                                tableInfo.lastSelectedRow = tableInfo.CurrentRow;

                                tableInfo.selectedRecordIndexes.Clear();

                                if (tableInfo.lastSelectedRow > tableInfo.firstSelectedRow)
                                {
                                    for (int i = tableInfo.firstSelectedRow; i < tableInfo.lastSelectedRow; i++)
                                    {
                                        tableInfo.selectedRecordIndexes.Add(i);
                                    }
                                }
                                else if (tableInfo.lastSelectedRow < tableInfo.firstSelectedRow)
                                {
                                    for (int i = tableInfo.lastSelectedRow; i < tableInfo.firstSelectedRow; i++)
                                    {
                                        tableInfo.selectedRecordIndexes.Add(i);
                                    }
                                }
                                else
                                {
                                    tableInfo.selectedRecordIndexes.Add(tableInfo.CurrentRow);
                                }
                            }
                            else
                            {
                                Table.selectedRecordIndexes.Clear();
                                Table.CurrentRow -= 1;
                                Table.firstSelectedRow = Table.CurrentRow;
                            }

                            if (Table.CurrentRow < Table.TopRow)
                                Table.TopRow -= 1;
                            VScroll.Value = Table.TopRow;
                            this.Invalidate(this.Bounds);
                        }
                    }
                    blnProcess = true;
                }
                //
                // key down
                //
                else if (keyData == Keys.Down || keyData == (Keys.Down | Keys.Shift))
                {
                    if (TotalRecordCount > 0)
                    {
                        int totalCount = TotalRecordCount;

                        if (this.gridType == GridGrouping.GridType.Virtual)
                            totalCount = tableInfo.SourceDataRowCount;

                        if (Table.CurrentRow < totalCount - 1)
                        {
                            if (keyData == (Keys.Down | Keys.Shift))
                            {
                                tableInfo.CurrentRow += 1;
                                tableInfo.lastSelectedRow = tableInfo.CurrentRow;

                                tableInfo.selectedRecordIndexes.Clear();

                                if (tableInfo.lastSelectedRow > tableInfo.firstSelectedRow)
                                {
                                    for (int i = tableInfo.firstSelectedRow; i < tableInfo.lastSelectedRow; i++)
                                    {
                                        tableInfo.selectedRecordIndexes.Add(i);
                                    }
                                }
                                else if (tableInfo.lastSelectedRow < tableInfo.firstSelectedRow)
                                {
                                    for (int i = tableInfo.lastSelectedRow; i < tableInfo.firstSelectedRow; i++)
                                    {
                                        tableInfo.selectedRecordIndexes.Add(i);
                                    }
                                }
                                else
                                {
                                    tableInfo.selectedRecordIndexes.Add(tableInfo.CurrentRow);
                                }
                            }
                            else
                            {
                                Table.selectedRecordIndexes.Clear();
                                Table.CurrentRow += 1;
                                Table.firstSelectedRow = Table.CurrentRow;
                            }
                            if (Table.CurrentRow > Table.BottomRow - 1)
                                Table.TopRow += 1;
                            VScroll.Value = tableInfo.TopRow;
                            this.Invalidate(this.Bounds);
                        }
                    }
                    blnProcess = true;
                }
                else if (keyData == (Keys.Control | Keys.A))
                {
                    tableInfo.lastSelectedRow = tableInfo.RowCount;
                    tableInfo.firstSelectedRow = 0;

                    tableInfo.selectedRecordIndexes.Clear();

                    for (int i = tableInfo.firstSelectedRow; i < tableInfo.lastSelectedRow; i++)
                    {
                        tableInfo.selectedRecordIndexes.Add(i);
                    }

                    this.Invalidate(this.Bounds);
                }
                //
                // end
                //
                else if (keyData == Keys.End)
                {
                    if (TotalRecordCount > 0)
                    {
                        if (TotalRecordCount >= Table.DisplayRows)
                        {
                            Table.TopRow = TotalRecordCount - Table.DisplayRows + 1;
                            Table.CurrentRow = TotalRecordCount - 1;
                            Table.firstSelectedRow = Table.CurrentRow;
                            VScroll.Value = Table.TopRow;
                            this.Invalidate();
                        }
                    }
                    blnProcess = true;
                }

                //
                // home
                //
                else if (keyData == Keys.Home)
                {
                    if (TotalRecordCount > 0)
                    {
                        Table.TopRow = 0;
                        Table.CurrentRow = 0;
                        VScroll.Value = Table.TopRow;
                        Table.firstSelectedRow = 0;
                        this.Invalidate();
                    }
                    blnProcess = true;
                }

                //
                // page down
                //
                else if (keyData == Keys.PageDown)
                {
                    if (TotalRecordCount > 0)
                    {
                        if (Table.TopRow < TotalRecordCount - Table.DisplayRows)
                        {
                            Table.TopRow = Table.TopRow + Table.DisplayRows;
                            Table.CurrentRow = Table.CurrentRow + Table.DisplayRows;
                            if (Table.TopRow >= TotalRecordCount)
                                Table.TopRow = TotalRecordCount - 1;
                            if (Table.CurrentRow >= TotalRecordCount)
                                Table.CurrentRow = TotalRecordCount - 1;
                            VScroll.Value = Table.TopRow;
                            Table.firstSelectedRow = Table.CurrentRow;
                            this.Invalidate();
                        }
                    }
                    blnProcess = true;
                }

                //
                // page up
                //
                else if (keyData == Keys.PageUp)
                {
                    if (TotalRecordCount > 0)
                    {
                        Table.TopRow = Table.TopRow - Table.DisplayRows;
                        Table.CurrentRow = Table.CurrentRow - Table.DisplayRows;
                        if (Table.TopRow < 0)
                            Table.TopRow = 0;
                        if (Table.CurrentRow < 0)
                            Table.CurrentRow = 0;
                        VScroll.Value = Table.TopRow;
                        Table.firstSelectedRow = Table.CurrentRow;
                        this.Invalidate();
                    }
                    blnProcess = true;
                }
                else if (keyData == Keys.Enter)
                {
                    blnProcess = true;
                }
                else if (keyData == Keys.Left)
                {
                    HScroll.Value -= 20;
                    blnProcess = true;
                }
                else if (keyData == Keys.Right)
                {
                    HScroll.Value += 20;
                    blnProcess = true;
                }
                else if (keyData == Keys.Delete)
                {
                    //blnProcess = true;
                }
                else if (keyData == Keys.Insert)
                {
                    blnProcess = true;
                }

                if (blnProcess == true)
                    return true;
                else
                    return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
                return true;
            }
        }

        #endregion

        #region Scroller Methods

        private void HorizontalScroll_ValueChanged(object sender, EventArgs e)
        {
            OnBufferedDataInsert();

            try
            {
                stopPaint = true;
                //if (VScroll.Maximum == 0)
                {
                    this.horizontalOffset = HScroll.Value;
                    this.Table.XOffset = HScroll.Value;
                }

            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                stopPaint = false;
                this.Refresh();
            }
        }

        private bool IsHScrollNeeded()
        {
            int totalLength = GetVisibleColumnWidth();
            if (totalLength > GetAvailableWidth())
            {
                this.HScroll.Maximum = totalLength - Width + 10;//+ vScrollBar1.Width;
                this.HScroll.Visible = true;
                return true;
            }
            this.horizontalOffset = 0;
            this.Table.XOffset = 0;
            this.HScroll.Visible = false;
            return false;
        }

        private bool IsVScrollNeeded()
        {
            int totalLength = GetTotalHeight();
            if (totalLength > GetAvailableWidth())
            {

                this.VScroll.Visible = true;
                return true;
            }

            this.VScroll.Visible = false;
            return false;
        }

        private int GetAvailableWidth()
        {
            return this.Width;
        }

        private int GetAvailableLength()
        {
            return this.Height;

        }

        private int GetTotalHeight()
        {
            return this.Table.RowHeight * VScroll.Maximum;
        }

        #endregion
    }
}
