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
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Comparer;
using System.Collections.Specialized;
using Mubasher.ClientTradingPlatform.Infrastructure.Module.Helpers;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping
{
    //
    //Added by Ashan D
    //
    public partial class GridGroupingControl : Control
    {
        #region Fields

        protected TableInfo tableInfo;
        protected GridRefreshMode refreshMode = GridRefreshMode.AutoRefresh;
        protected GridPaint gridPainter;
        public int TotalRecordCount;
        protected bool isFormLoaded = false;
        protected bool isSortingRequired = false;
        public Font MainFont;
        protected bool stopPaint = false;
        protected bool suspendAllPaint = false;
        internal bool dataObjectTypeChanged = false;
        private List<SortColumnDescriptor> defaultSortColumnDescriptors = new List<SortColumnDescriptor>();
        private List<TableInfo.GridGroup> groupByNameList = new List<TableInfo.GridGroup>();

        public delegate SizeF QueryRenderCellWidthDelegate(TableInfo.CellStruct cell, BufferedGraphics gfx, string value);
        public event QueryRenderCellWidthDelegate QueryRenderCellWidth;

        public List<TableInfo.GridGroup> GroupByNameList
        {
            get { return groupByNameList; }
        }
        //internal List<List<string>> groupingGridSkipValues = new List<List<string>>();
        public bool SortUsingLinq = false;

        protected List<TableInfo.CellStruct> cellsToUpdate = new List<TableInfo.CellStruct>();
        protected List<TableInfo.CellStruct> cellsUnboundToUpdate = new List<TableInfo.CellStruct>();
        protected List<TableInfo.CellStruct> cellsToDraw = new List<TableInfo.CellStruct>();
        protected List<TableInfo.CellStruct> cellsUnboundToDraw = new List<TableInfo.CellStruct>();
        protected List<TableInfo.CellStruct> cellsSummaryToUpdate = new List<TableInfo.CellStruct>();
        internal List<TableInfo.CellStruct> cellsNestedHeaders = new List<TableInfo.CellStruct>();
        internal List<TableInfo.CellStruct> cellsRowHeaders = new List<TableInfo.CellStruct>();
        internal List<TableInfo.CellStruct> cellsToExtractTempData = new List<TableInfo.CellStruct>();
        internal List<Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.TableInfo.GroupHeaderRowLevel> GroupHeaderRowIds = new List<TableInfo.GroupHeaderRowLevel>();
        public Dictionary<int, TableInfo.VirtualGroupHeader> VirtualGroupHeaderRowIds = new Dictionary<int, TableInfo.VirtualGroupHeader>();
        internal List<string> MinimisedGroupList = new List<string>();

        public event EventHandler VirtualGroupHeaderChanged;

        internal string GroupingHeaderFilterText = string.Empty;
        private string groupingHeaderFilterTextLast = string.Empty;
        internal bool GettingValuesForGroupingHeader = false;

        internal Dictionary<string, BlinkInfo> blinkRegistry = new Dictionary<string, BlinkInfo>();
        protected int designTimeTotalWidth = 0;
        protected Dictionary<string, int> designTimeWidths = new Dictionary<string, int>();
        protected int autoResizeMinimumWidth = 300;
        protected bool isGridColumnResizeInProgress = false;
        protected double exactWidthOfAllColumns = 0;
        protected Dictionary<string, double> exactColumnWidths = new Dictionary<string, double>();
        protected int lastGridResizedWidth = 0;
        protected bool isResizedOnce = false;
        protected int resizeLimit = 100;
        protected int totalColumnWidth = 0;
        protected bool isVisibleColumnRemoved = false;
        internal bool sizeChanging = false;
        protected bool isMainPaint = false;
        protected bool isGridPaintedOnce = false;
        protected volatile object paintLock = new object();
        internal bool RecreateMatrixFlag = false;
        public bool DoVeryFullRefresh = false;
        protected PropertyInfo[] properties;
        protected Dictionary<string, PropertyInfo[]> secondaryProperties;
        internal int lastProgressiveColumnID = -1;
        internal TextBox editTextBox = null;
        public bool IsColumnAlterationInProgress = false;
        public Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.ToolTipAdv.ToolTipAdv toolTip = new ToolTipAdv.ToolTipAdv();
        private int longConvertExceptionCount = 0;
        private int intConvertExceptionCount = 0;
        private int doubleConvertExceptionCount = 0;
        private int stringConvertExceptionCount = 0;
        private int styleConvertExceptionCount = 0;
        private int dateTimeConvertExceptionCount = 0;
        private int decimalConvertExceptionCount = 0;
        private double lastIntLoggedTime = 0;
        private double lastDoubleLoggedTime = 0;
        private double lastDecimalLoggedTime = 0;
        private double lastLongLoggedTime = 0;
        private double lastStyleLoggedTime = 0;
        private double lastDateTimeLoggedTime = 0;
        private double lastStringLoggedTime = 0;

        protected bool isEditable = false;
        internal bool IsValueExtractMode = false;

        //Use to store Table wide Style
        public GridStyleInfo TableStyle = GridStyleInfo.Default;

        protected GridCellRendererCollection cellRenderers;
        protected Rectangle Rect;
        protected double refreshInterval = 1000;

        private int _horizontalOffset;

        public int horizontalOffset
        {
            get { return (int)(_horizontalOffset * tableInfo.GetScale()); }
            set { _horizontalOffset = value; }
        }

        protected int startColumnLocation = 0;
        protected object returnbject = null;
        protected string returnString = string.Empty;
        protected int returnInt = 0;
        protected long returnLong = 0;
        protected double returnDouble = 0;
        protected decimal returnDecimal = 0;
        protected DateTime returnDateTime = DateTime.MinValue;
        protected int blinkTime = 1000;

        public Type ForcedObjectType = null;
        public Type BoundObjectType = null;
        public bool isPrinting = false;
        public Dictionary<int, PropertyAccessor> PropertyList = new Dictionary<int, PropertyAccessor>();
        protected Timer tmrRefresh;
        protected System.Collections.IList dataSource = null;
        internal bool resetGrid = false;
        internal bool queryVirtualData = false;
        protected bool listenToListChangedEvents = false;
        protected bool recreateMatrixOnDataSourceChanged = false;
        internal System.Collections.Specialized.OrderedDictionary propertyNames;
        protected System.Collections.Specialized.OrderedDictionary propertyTypes;
        internal Dictionary<string, System.Collections.Specialized.OrderedDictionary> secondaryPropertyNames;
        protected Dictionary<string, System.Collections.Specialized.OrderedDictionary> secondaryPropertyTypes;
        protected bool isGridUpdatedOnce = false;
        internal Dictionary<string, PropertyInfo> allProperties = new Dictionary<string, PropertyInfo>();
        protected Dictionary<int, Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.TableInfo.TableColumn> columnsMapByID = new Dictionary<int, TableInfo.TableColumn>();
        internal Dictionary<string, Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.TableInfo.TableColumn> columnsMapByName = new Dictionary<string, TableInfo.TableColumn>();
        protected Dictionary<string, GridCellModelBase> cellModels = new Dictionary<string, GridCellModelBase>();

        internal static Dictionary<Type, Dictionary<string, PropertyAccessor>> PropertiesMapBayClass = new Dictionary<Type, Dictionary<string, PropertyAccessor>>();
        internal static GlobalMouseHandler MouseHandler = null;

        protected System.Windows.Forms.Timer resizeTimer;

        //Thread dataThread = null;

        //private Bitmap bitmap = null;
        //private Graphics bitmapGraphic = null;

        protected BufferedGraphicsContext context;
        protected BufferedGraphics grafx;

        internal Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.TableInfo.CellStruct[,] cellFullMatrix;

        private bool editingMode = false;

        public bool EditingMode
        {
            get { return editingMode; }
            set { editingMode = value; }
        }

        public TextBox EditTextBox
        {
            get { return editTextBox; }
        }

        internal Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.TableInfo.CellStruct[,] CellFullMatrix
        {
            get { return cellFullMatrix; }
            set { cellFullMatrix = value; }
        }

        #endregion

        #region Events

        public event GridQueryCellsEventHandler QueryGridCells;
        public event GridQueryCellEventHandler QueryGridCell;
        public event GridQueryCellsEventHandler QuerySummaryGridCells;
        public event GridQueryCellsEventHandler2 QueryNestedHeaderGridCells;
        public event GridQueryCellsEventHandler2 QueryRowHeaderGridCells;
        public event EventHandler ExportAllToExcel;
        public event EventHandler ExportAllToPDF;
        public event EventHandler InsertBufferData;
        public event EventHandler AfterAutoResizingColumns;
        public event EventHandler QueryGridRowCount;
        public event GridDrawCellButtonBackgroundEventHandler DrawCellButtonBackground;
        public event GridCellButtonClickedEventHandler CellButtonClicked;
        public event GridDrawCellButtonEventHandler DrawCellButton;
        public event DisplayTextBoxEventHandler DisplayTextBox;
        public event PaintEventHandler PaintCustomHeaders;
        public event KeyPressEventHandler TextSearchKeyPressed;
        public event EventHandler BeforeApplyFilter;
        public event GridFilteredRecordsHandler AfterApplyFilter;

        #endregion

        #region Properties

        public int BlinkTime
        {
            get { return blinkTime; }
            set { blinkTime = value; }
        }

        public GridRefreshMode RefreshMode
        {
            get { return refreshMode; }
            set
            {
                refreshMode = value;
                this.blinkRegistry.Clear();

                if (refreshMode == GridRefreshMode.AutoRefresh)
                {
                    if (tmrRefresh == null)
                        CreateRefreshTimer();

                    tmrRefresh.Start();
                }
            }
        }

        public double RefreshInterval
        {
            get
            {
                return refreshInterval;
            }
            set
            {
                if (refreshInterval != value)
                {
                    refreshInterval = value;

                    if (this.refreshMode == GridRefreshMode.AutoRefresh)
                    {
                        //if (tmrRefresh == null)
                        CreateRefreshTimer();

                        tmrRefresh.Start();
                    }
                }
            }
        }

        protected GridType gridType = GridType.DataBound;

        public GridType GridType
        {
            get { return gridType; }
            set { gridType = value; }
        }

        public int AutoResizeMinimumWidth
        {
            get { return autoResizeMinimumWidth; }
            set { autoResizeMinimumWidth = value; }
        }

        //
        //Set this as false if grid should not listen to datasorce list changed events
        //
        public bool ListenToListChangedEvents
        {
            get { return listenToListChangedEvents; }
            set
            {
                if (listenToListChangedEvents != value)
                {
                    listenToListChangedEvents = value;

                    if (listenToListChangedEvents)
                    {
                        if (dataSource != null)
                        {
                            if (dataSource is INotifyCollectionChanged)
                            {
                                (dataSource as INotifyCollectionChanged).CollectionChanged -= OnDataSource_CollectionChanged;
                                (dataSource as INotifyCollectionChanged).CollectionChanged += OnDataSource_CollectionChanged;
                                isListChangedDataBound = true;
                            }
                        }
                    }
                    else
                    {
                        if (dataSource != null)
                        {
                            if (dataSource is INotifyCollectionChanged)
                            {
                                (dataSource as INotifyCollectionChanged).CollectionChanged -= OnDataSource_CollectionChanged;
                                isListChangedDataBound = false;
                            }
                        }
                    }
                }
            }
        }

        protected bool isListChangedDataBound = false;

        public bool IsListChangedDataBound
        {
            get
            {
                return isListChangedDataBound;
            }
        }

        //
        //Set this as false if grid should not listen to datasorce list changed events
        //
        public bool RecreateMatrixOnDataSourceChanged
        {
            get { return recreateMatrixOnDataSourceChanged; }
            set
            {
                recreateMatrixOnDataSourceChanged = value;
            }
        }

        public bool StopPaint
        {
            get { return (stopPaint || suspendAllPaint) && IsGridInitialized; }
            set
            {
                stopPaint = value;

                if (!stopPaint)
                    this.InvalidateGrid();
            }
        }

        public bool SuspendAllPaint
        {
            get { return suspendAllPaint; }
            set
            {
                suspendAllPaint = value;

                if (!suspendAllPaint)
                {
                    this.Refresh();
                }
            }
        }

        private bool showGroupingControl = false;

        public bool ShowGroupingControl
        {
            get { return showGroupingControl; }
            set { showGroupingControl = value; }
        }

        protected static bool turnOnAutoResizeGridColumns = false;

        public static bool TurnOnAutoResizeGridColumns
        {
            get { return turnOnAutoResizeGridColumns; }
            set
            {
                turnOnAutoResizeGridColumns = value;
            }
        }

        protected static bool suspendAutoResizeColumns = false;

        public static bool SuspendAutoResizeColumns
        {
            get { return suspendAutoResizeColumns; }
            set { suspendAutoResizeColumns = value; }
        }

        protected bool enableAutoResizeGridColumns = true;

        public bool EnableAutoResizeGridColumns
        {
            get { return enableAutoResizeGridColumns && TurnOnAutoResizeGridColumns; }
            set
            {
                enableAutoResizeGridColumns = value;

                InitializeColumnResize(enableAutoResizeGridColumns);
            }
        }

        public bool IsEditable
        {
            get { return isEditable; }
            set { isEditable = value; }
        }

        protected bool isMirrored = false;

        public new bool IsMirrored
        {
            get { return isMirrored || this.RightToLeft == System.Windows.Forms.RightToLeft.Yes || tableInfo.Culture.TextInfo.IsRightToLeft; }
        }

        protected bool isGridInitialized = false;

        public bool IsGridInitialized
        {
            get { return isGridInitialized || gridType == GridGrouping.GridType.Virtual; }
        }

        protected bool checkDerivedClassTypesInDataSource = false;

        public bool CheckDerivedClassTypesInDataSource
        {
            get { return checkDerivedClassTypesInDataSource; }
            set { checkDerivedClassTypesInDataSource = value; }
        }

        protected bool getBaseClassProperties = false;

        public bool GetBaseClassProperties
        {
            get { return getBaseClassProperties; }
            set { getBaseClassProperties = value; }
        }

        public TableInfo Table
        {
            get
            {
                if (tableInfo == null)
                    tableInfo = new TableInfo(this);

                return tableInfo;
            }
        }

        public Dictionary<int, Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.TableInfo.TableColumn> ColumnsMapByID
        {
            get { return columnsMapByID; }
            set
            {
                columnsMapByID = value;

                if (columnsMapByID == null)
                    columnsMapByID = new Dictionary<int, TableInfo.TableColumn>();
            }
        }

        public Dictionary<string, GridCellModelBase> CellModels
        {
            get { return cellModels; }
            set { cellModels = value; }
        }

        public GridPaint GridPainter
        {
            get
            {
                if (gridPainter == null)
                    gridPainter = new GridPaint(this);

                return gridPainter;
            }
            set { gridPainter = value; }
        }

        internal bool isDataSourceChangeInProgress = false;
        private bool needAlign = false;

        public System.Collections.IList DataSource
        {
            get
            {
                return dataSource;
            }
            set
            {
                try
                {
                    if (!this.IsDisposed)
                    {
                        isDataSourceChangeInProgress = true;
                        tmrRefresh.Stop();

                        if (dataSource != null)
                        {
                            if (dataSource is INotifyCollectionChanged)
                            {
                                (dataSource as INotifyCollectionChanged).CollectionChanged -= OnDataSource_CollectionChanged;
                                isListChangedDataBound = false;
                            }
                        }

                        dataSource = value;

                        if (dataSource != null)
                        {
                            if (dataSource.Count > 0)
                                CheckForDataSourceTypeChange(dataSource[0].GetType());

                            CreateCellMatrix(dataSource.Count, tableInfo.VisibleColumns.Count);

                            if (listenToListChangedEvents)
                            {
                                if (dataSource is INotifyCollectionChanged)
                                {
                                    (dataSource as INotifyCollectionChanged).CollectionChanged -= OnDataSource_CollectionChanged;
                                    (dataSource as INotifyCollectionChanged).CollectionChanged += OnDataSource_CollectionChanged;
                                    isListChangedDataBound = true;
                                }
                            }

                            RestoreSortAndFilterOptionsAfterDataSourceTypeChanged();

                            //
                            //Starting auto refresh timer
                            //
                            if (this.refreshMode == GridRefreshMode.AutoRefresh)
                                tmrRefresh.Start();
                        }

                        if (dataSource != null && tableInfo.AllowFilter && tableInfo.FilterManager.IsFilterAvailable())
                            ApplyFilter();

                        if (InvokeRequired)
                        {
                            this.BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate ()
                            {
                                Refresh();
                            }));
                        }
                        else
                        {
                            this.Refresh();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionsLogger.LogError(ex);
                }
                finally
                {
                    isDataSourceChangeInProgress = false;
                    dataObjectTypeChanged = false;
                }
            }
        }

        private void RestoreSortAndFilterOptionsAfterDataSourceTypeChanged()
        {
            try
            {
                if (dataObjectTypeChanged)
                {
                    this.tableInfo.ResetSortedColumns();

                    if (this.defaultSortColumnDescriptors.Count > 0)
                    {
                        this.tableInfo.isSortingEnabled = true;
                        this.tableInfo.AllowSort = true;
                    }

                    for (int i = 0; i < this.defaultSortColumnDescriptors.Count; i++)
                    {
                        SortColumnDescriptor sort = this.defaultSortColumnDescriptors[i];

                        for (int j = 0; j < this.tableInfo.VisibleColumns.Count; j++)
                        {
                            if (this.tableInfo.VisibleColumns[j].Name == sort.Name
                                || this.tableInfo.VisibleColumns[j].MappingName == sort.Name)
                            {
                                this.tableInfo.UpdateRecordSortData(sort);

                                if (sort.Accessor != null)
                                    this.tableInfo.SortedColumnDescriptors.Add(this.defaultSortColumnDescriptors[i]);
                                else if (sort.IsUnbound /*|| this.tableInfo.VisibleColumns[j].IsCustomFormulaColumn*/)
                                    this.tableInfo.SortedColumnDescriptors.Add(this.defaultSortColumnDescriptors[i]);

                                break;
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

        public GridCellRendererCollection CellRenderers
        {
            get
            {
                if (cellRenderers == null)
                    cellRenderers = new GridCellRendererCollection(this);
                return cellRenderers;
            }
        }

        public Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers.CellRendererBase CustomHeaderCellRenderer;

        #endregion

        #region New Scrollbar

        private bool isNewScrollbarCreated = false;

        private bool isNewScrollBar;

        public bool IsNewScrollBar
        {
            get { return isNewScrollBar; }
            set
            {
                isNewScrollBar = value;

                if (value == true)
                {
                    if (isNewScrollbarCreated == false)
                    {
                        this.NewHScroll = new ScrollDataBounds();
                        //this.NewHScroll.Visible = false;
                        //this.NewHScroll.Dock = DockStyle.Bottom;
                        //this.NewHScroll.Height = 15;
                        //this.NewHScroll.LargeChange = 1;
                        //this.Controls.Add(this.NewHScroll);
                        this.NewHScroll.ValueChanged += new System.EventHandler(NewHScroll_ValueChanged);

                        this.NewVScroll = new ScrollDataBounds();
                        ///this.NewVScroll.Visible = false;
                        //this.NewVScroll.Width = 15;
                        //this.NewVScroll.Dock = DockStyle.Right;
                        //this.NewVScroll.LargeChange = 1;
                        //this.Controls.Add(this.NewVScroll);
                        this.NewVScroll.ValueChanged += new System.EventHandler(NewVScroll_ValueChanged);
                        isNewScrollbarCreated = true;
                    }

                    VScroll.IsObsolute = true;
                    HScroll.OnMaximumValueChanges -= new Action<int>(HScroll_OnMaximumValueChanges);
                    HScroll.OnMaximumValueChanges += new Action<int>(HScroll_OnMaximumValueChanges);

                    VScroll.OnMaximumValueChanges -= new Action<int>(VScroll_OnMaximumValueChanges);
                    VScroll.OnMaximumValueChanges += new Action<int>(VScroll_OnMaximumValueChanges);
                    HScroll.IsObsolute = true;
                }
                else
                {
                    if (isNewScrollbarCreated)
                    {
                        this.NewHScroll.ValueChanged -= new System.EventHandler(NewHScroll_ValueChanged);
                        this.NewVScroll.ValueChanged -= new System.EventHandler(NewVScroll_ValueChanged);
                    }

                    VScroll.IsObsolute = false;
                    HScroll.IsObsolute = false;
                    HScroll.OnMaximumValueChanges -= new Action<int>(HScroll_OnMaximumValueChanges);
                    VScroll.OnMaximumValueChanges -= new Action<int>(VScroll_OnMaximumValueChanges);
                }

            }
        }

        public new int Width
        {
            get
            {
                //if (IsNewScrollBar)
                //    return base.Width - 100;// -NewVScroll.Width;

                if (DPIHelper.DPIScale < 1.1 && base.Width <= 200 && this.Table.Culture.TextInfo.IsRightToLeft)
                {
                    if (NewVScroll.Visibility == System.Windows.Visibility.Visible)
                    {
                        return base.Width - (int)(40);
                    }
                    else
                    {
                        return base.Width - (int)(20);
                    }
                }

                return base.Width;
            }
            set { base.Width = value; }
        }

        public new int Height
        {
            get
            {
                if (IsNewScrollBar)
                    return base.Height;// -NewHScroll.Height;

                return base.Height;
            }
            set { base.Height = value; }
        }

        #endregion

        #region Constructors

        public GridGroupingControl()
        {
            SetUpMouseHandler();
            InitializeComponent();

            this.LocationChanged += GridGroupingControl_LocationChanged;


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

        private void GridGroupingControl_LocationChanged(object sender, EventArgs e)
        {

        }

        private static void SetUpMouseHandler()
        {
            if (MouseHandler == null)
            {
                MouseHandler = new GlobalMouseHandler();
                Application.AddMessageFilter(MouseHandler);
            }
        }

        //
        //This is normally the ShellForm
        //
        internal static Form mainForm = null;
        private int trasformValue = 0;

        public static void SetMainForm(Form ctrl)
        {
            try
            {
                mainForm = ctrl;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected virtual void AddBasicCellModels(bool reset)
        {
            if (reset)
                cellModels.Clear();

            this.cellModels.Add(CellType.Header, new GridHeaderCellModel());
            this.cellModels.Add(CellType.Static, new GridStaticCellModel());
            this.cellModels.Add(CellType.Summary, new GridSummaryCellModel());
            this.cellModels.Add(CellType.PushButton, new GridPushButtonCellModel());
            this.cellModels.Add(CellType.ProgressView, new ProgressViewCellModel());
            this.cellModels.Add(CellType.CheckBox, new GridCheckBoxCellModel());
        }

        #endregion

        #region Grouping Grid Implementation

        public void OnVirtualGroupHeaderChanged()
        {
            if (VirtualGroupHeaderChanged != null)
                VirtualGroupHeaderChanged(null, null);
        }

        internal void ClearGroupHeaders()
        {
            this.GroupByNameList.Clear();
            this.GroupHeaderRowIds.Clear();
            this.tableInfo.TopRow = 0;
            //this.groupingGridSkipValues.Clear();

        }

        public void RemoveGroupHeaders(bool clearPrevious, params string[] columnNames)
        {
            MinimisedGroupList.Clear();

            foreach (var item in columnNames)
            {
                if (GroupByNameList.Any(x => x.Name == item))
                {
                    this.GroupByNameList.Remove(GroupByNameList.First(x => x.Name == item));
                }
            }

            Table.ChangedSorting = true;

            if (columnNames.Length == 0)
                this.Refresh();

            tableInfo.PopulateGroupHeaders();
        }

        public void SetGroupHeaders(bool clearPrevious, params string[] columnNames)
        {
            if (clearPrevious)
                ClearGroupHeaders();

            MinimisedGroupList.Clear();

            foreach (var item in columnNames)
            {
                if (!string.IsNullOrWhiteSpace(item) && tableInfo.VisibleColumns.Contains(item))
                {
                    this.GroupByNameList.Add(new TableInfo.GridGroup() { Name = item });
                }
            }

            Table.ChangedSorting = true;

            if (columnNames.Length == 0)
                this.Refresh();

            tableInfo.PopulateGroupHeaders();
        }

        #endregion

        #region Helper Methods

        public virtual void BeginUpdate()
        {
            this.stopPaint = true;

            if (!this.IsMirrored)
            {
                for (int i = 0; i < tableInfo.VisibleColumns.Count; i++)
                {
                    tableInfo.VisibleColumns[i].Left = 0;
                }

                for (int i = 0; i < tableInfo.Columns.Count; i++)
                {
                    tableInfo.Columns[i].Left = 0;
                }
            }
        }

        public virtual void EndUpdate(bool setGridDirty)
        {
            try
            {
                this.Table.SetRecreateFlag(setGridDirty);
                this.Table.CurrentCol = 0;
                this.Table.CurrentRow = 0;
                this.SetGridDirty(setGridDirty);
                PrepareTables();
                this.GridColumnResize(this, false);
                tableInfo.isHeaderMouseDown = false;
                SetColumnPositions(setGridDirty);
                tableInfo.CalculateColumnLeft();
                this.queryVirtualData = true;
                this.StopPaint = false;
                tableInfo.SelectedRecords.Clear();
                tableInfo.selectedRecordIndexes.Clear();
                tableInfo.lastSelectedRow = -1;
                tableInfo.firstSelectedRow = -1;
                this.GridColumnResize(this, false);

                needAlign = true;

                //if (!editingMode)
                //    RTLAwarePosition(0);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError("Error in Grid End Update");
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void SetGridDirty(bool isDirty)
        {
            this.tableInfo.IsDirty = isDirty;
        }

        public virtual void RecreateMatrix()
        {
            if (tableInfo.IsFilterEnabled)
            {
                if (tableInfo.FilteredRecords.Count == 0 && tableInfo.AllowFilter)
                    ApplyFilter(false);

                if (tableInfo.FilteredRecords.Count > 0)
                {
                    this.Table.AllRecords.Clear();

                    if (this.dataSource != null)
                    {
                        for (int i = 0; i < this.dataSource.Count; i++)
                        {
                            Record r = new Record(i, this.DataSource[i], this.Table);
                            this.Table.AllRecords.Add(r);
                        }
                    }

                    if (tableInfo.AllowFilter)
                        ApplyFilter(false);

                    tableInfo.CreateMatrixFromFilteredRecords(this.Table.VisibleColumnCount);
                }
                else
                {
                    if (this.dataSource != null)
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

        }

        public void CreateCellMatrixForVirtualGrid()
        {
            CreateCellMatrix(this.Table.BottomRow, this.Table.VisibleColumnCount);
        }

        protected virtual void CreateCellMatrix(int rowCount, int columnCount)
        {
            try
            {
                this.stopPaint = true;
                //tableInfo.RowCount = rowCount;
                tableInfo.CreateMatrix(rowCount, columnCount);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                this.stopPaint = false;
                this.resetGrid = true;

                //if (this.IsHandleCreated && !this.IsDisposed)
                //    this.Invoke(new MethodInvoker(Refresh));
            }
        }

        protected void OnDataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            //TODOVB Copy  OnDataSource_ListChanged here

            try
            {
                if (this.IsHandleCreated && listenToListChangedEvents && !this.IsDisposed)
                {
                    if (dataSource != null)
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:

                                if (recreateMatrixOnDataSourceChanged)
                                {
                                    if (dataSource.Count == 1)
                                        CheckForDataSourceTypeChange(dataSource[0].GetType());
                                    RecreateMatrixFlag = true;
                                }
                                else
                                    AddNewRowToMatrix(e.NewStartingIndex);

                                break;
                            case NotifyCollectionChangedAction.Remove:

                                if (recreateMatrixOnDataSourceChanged)
                                    RecreateMatrixFlag = true;
                                else
                                    RemoveRowFromMatrix(e.NewStartingIndex);

                                break;
                            case NotifyCollectionChangedAction.Reset:
                                RecreateMatrixFlag = true;
                                break;
                            case NotifyCollectionChangedAction.Replace:
                                if (!recreateMatrixOnDataSourceChanged)
                                {
                                    int rowIndex = e.NewStartingIndex;
                                    PaintCell(rowIndex, -1, null);
                                }
                                else
                                {
                                    RecreateMatrixFlag = true;
                                }
                                break;
                            case NotifyCollectionChangedAction.Move:
                                break;
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
                stopPaint = false;
            }

        }

        protected virtual void OnDataSource_ListChanged(object sender, ListChangedEventArgs e)
        {
            try
            {
                if (this.IsHandleCreated && listenToListChangedEvents && !this.IsDisposed)
                {
                    if (dataSource != null)
                    {
                        switch (e.ListChangedType)
                        {
                            case ListChangedType.ItemAdded:

                                if (recreateMatrixOnDataSourceChanged)
                                {
                                    if (dataSource.Count == 1)
                                        CheckForDataSourceTypeChange(dataSource[0].GetType());
                                    RecreateMatrixFlag = true;
                                }
                                else
                                    AddNewRowToMatrix(e.NewIndex);

                                break;
                            case ListChangedType.ItemDeleted:
                                if (recreateMatrixOnDataSourceChanged)
                                    RecreateMatrixFlag = true;
                                else
                                    RemoveRowFromMatrix(e.NewIndex);

                                break;
                            case ListChangedType.Reset:
                                break;
                            case ListChangedType.ItemChanged:
                                if (!recreateMatrixOnDataSourceChanged)
                                {
                                    int rowIndex = e.NewIndex;
                                    PaintCell(rowIndex, -1, null);
                                }
                                break;
                            case ListChangedType.ItemMoved:
                                break;
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
                stopPaint = false;
            }
        }

        protected virtual void AddNewRowToMatrix(int index)
        {
            try
            {
                stopPaint = true;
                tableInfo.RowCount++;

                tableInfo.AddRow(index);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                resetGrid = true;
                stopPaint = false;
                if (this.IsHandleCreated && !this.IsDisposed)
                    this.Invoke(new System.Windows.Forms.MethodInvoker(Refresh));
            }
        }

        public virtual void AddNewEmptyRowToMatrix(int index)
        {
            try
            {
                //
                //Not Applicable to main grid
                //
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void RemoveEmptyRowFromMatrix(int index)
        {
            try
            {
                //
                //Not Applicable to main grid
                //
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void RemoveRecordFromMatrix(int rowIndex, int colIndex)
        {
            try
            {
                //
                //Not Applicable to main grid
                //
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected virtual void RemoveRowFromMatrix(int index)
        {
            try
            {
                stopPaint = true;
                tableInfo.RowCount--;

                tableInfo.RemoveRow(index);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                resetGrid = true;
                stopPaint = false;
                if (this.IsHandleCreated && !this.IsDisposed)
                    this.Invoke(new System.Windows.Forms.MethodInvoker(Refresh));
            }
        }

        internal virtual void CreateColumnPropertyAccessor(string columnName, int columnID, Type type)
        {
            if (BoundObjectType == null)
                return;

            if (columnsMapByName.ContainsKey(columnName))
                return;

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

                TableInfo.TableColumn col = tableInfo.GetColumnFromName(str);

                if (col != null && !columnsMapByID.ContainsKey(identityID))
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
                                }
                                else
                                {
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
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void CheckForDataSourceTypeChange(Type type)
        {
            if (ForcedObjectType != null)
                type = ForcedObjectType;

            try
            {
                Type baseType = type.BaseType;

                if (type == BoundObjectType || (baseType != null && baseType == BoundObjectType))
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
                    if (this.tableInfo.SortedColumnDescriptors != null)
                    {
                        this.defaultSortColumnDescriptors.Clear();

                        for (int i = 0; i < this.tableInfo.SortedColumnDescriptors.Count; i++)
                        {
                            defaultSortColumnDescriptors.Add(this.tableInfo.SortedColumnDescriptors[i]);
                        }

                        this.tableInfo.SortedColumnDescriptors.Clear();
                    }

                    //if (this.tableInfo.RecordFilters != null)
                    //{
                    //    this.tableInfo.RecordFilters.Clear();
                    //}
                }

                if (BoundObjectType != null)
                    dataObjectTypeChanged = true;

                bool isDerived = (checkDerivedClassTypesInDataSource && type != null && type.BaseType != null && type.BaseType == BoundObjectType) ? true : false;
                GenerateColumnsAndProperties(columnNames, null, type, null, isDerived);

                if (this.tableInfo.SortedColumnDescriptors.Count > 0)
                    tableInfo.UpdateRecordSortKeys();

                baseType = type.BaseType;

                if (getBaseClassProperties)
                {
                    if ((baseType != null && baseType != BoundObjectType))
                        BoundObjectType = type;
                }
                else
                    BoundObjectType = type;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void GenerateColumnsAndProperties(string[] columnNames, int[] columnIDs, Type type, List<Type> secondaryTypes)
        {
            GenerateColumnsAndProperties(columnNames, columnIDs, type, secondaryTypes, false);
        }

        public virtual void GenerateColumnsAndProperties(string[] columnNames, int[] columnIDs, Type type, List<Type> secondaryTypes, bool isDerivedClassFound)
        {
            try
            {
                Type originalType = type;

                if (getBaseClassProperties)
                {
                    type = type.BaseType;

                    if (type == null)
                        type = originalType;
                }

                if (type == null)
                    return;

                bool isCustom = false;

                this.BoundObjectType = type;

                if (type == null)
                {
                    isGridInitialized = true;
                    return;
                }

                //
                //Add New Class Type To Map 
                //
                if (!PropertiesMapBayClass.ContainsKey(type))
                    PropertiesMapBayClass.Add(type, new Dictionary<string, PropertyAccessor>());

                if ((columnIDs == null && columnNames != null) || (columnNames != null && columnNames.Length != columnIDs.Length))
                {
                    //
                    //Create new columnID array if mismatching parameters are passed.
                    //
                    columnIDs = new int[columnNames.Length];
                    isCustom = true;
                    //ExceptionsLogger.LogError("Mismatching lengths of columnIDs and column Names found. Creating new array for automatic column generation");

                    //throw new ApplicationException("Number of column names should be equal to number of column ids");
                }

                properties = type.GetProperties();

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

                if (getBaseClassProperties)
                {
                    properties = originalType.GetProperties();

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

                if (secondaryTypes != null && secondaryTypes.Count > 0)
                {
                    secondaryProperties = new Dictionary<string, PropertyInfo[]>();
                    secondaryPropertyNames = new Dictionary<string, System.Collections.Specialized.OrderedDictionary>();
                    secondaryPropertyTypes = new Dictionary<string, System.Collections.Specialized.OrderedDictionary>();

                    for (int i = 0; i < secondaryTypes.Count; i++)
                    {
                        Type secondaryType = secondaryTypes[i];

                        if (secondaryType != null)
                        {
                            secondaryProperties.Add(secondaryType.FullName, secondaryType.GetProperties());

                            secondaryPropertyTypes.Add(secondaryType.FullName, new System.Collections.Specialized.OrderedDictionary());
                            secondaryPropertyNames.Add(secondaryType.FullName, new System.Collections.Specialized.OrderedDictionary());

                            for (int j = 0; j < secondaryProperties[secondaryType.FullName].Length; j++)
                            {
                                string info = secondaryProperties[secondaryType.FullName][j].Name;

                                if (!secondaryPropertyNames[secondaryType.FullName].Contains(info))
                                    secondaryPropertyNames[secondaryType.FullName].Add(info, j);
                                if (!secondaryPropertyTypes[secondaryType.FullName].Contains(info))
                                    secondaryPropertyTypes[secondaryType.FullName].Add(info, secondaryProperties[secondaryType.FullName][j].PropertyType);
                            }
                        }
                    }
                }

                PropertyList.Clear();
                columnsMapByName.Clear();
                lastProgressiveColumnID = 0;
                int identityID = -2;
                int progressiveIdentityID = 0;

                if (columnNames != null)
                {
                    for (int i = 0; i < columnNames.Length; i++)
                    {
                        string str = columnNames[i];
                        string mainPropertyName = str;
                        string secondaryPropertyName = str;

                        if (str.Contains("."))
                        {
                            mainPropertyName = str.Split('.')[0];
                            secondaryPropertyName = str.Split('.')[1];
                        }

                        TableInfo.TableColumn col = tableInfo.GetColumnFromName(str);
                        TableInfo.TableColumn colVisible = tableInfo.GetVisibleColumnFromName(str);

                        if (this is GridGroupingControlMC && this.tableInfo.VisibleColumns.Count > i)
                        {
                            TableInfo.TableColumn colDummy = this.tableInfo.VisibleColumns[i];

                            if (colDummy != null && colDummy.MappingName == col.MappingName)
                            {
                                col = colDummy;
                                colVisible = col;
                            }
                        }

                        if (col != null)
                            col.Table = tableInfo;

                        if (colVisible != null)
                            colVisible.Table = tableInfo;

                        if (col != null && !col.IsUnbound)
                        {
                            if (propertyNames.Contains(str))
                            {
                                if (!isCustom)
                                    identityID = columnIDs[i];
                                else
                                    identityID = progressiveIdentityID;

                                lastProgressiveColumnID = identityID;

                                progressiveIdentityID++;

                                int mappingID = (int)propertyNames[str];

                                try
                                {
                                    //
                                    //NOTE : Need to check this error when Instrument Type is used
                                    //

                                    if (properties[mappingID].Name != "InstrumentType" && !PropertyList.ContainsKey(identityID))
                                    {
                                        PropertyAccessor newAccessor = null;

                                        if (!PropertiesMapBayClass.ContainsKey(type))
                                            PropertiesMapBayClass.Add(type, new Dictionary<string, PropertyAccessor>());

                                        if (!PropertiesMapBayClass[type].ContainsKey(str))
                                        {
                                            newAccessor = new PropertyAccessor(type, properties[mappingID].Name, isDerivedClassFound);
                                            PropertiesMapBayClass[type].Add(str, newAccessor);
                                        }
                                        else
                                        {
                                            newAccessor = PropertiesMapBayClass[type][str];

                                            if (newAccessor != null)
                                                newAccessor.IsDerivedClassFound = isDerivedClassFound;
                                        }

                                        PropertyList.Add(identityID, newAccessor);
                                    }

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
                            else
                            {
                                if (secondaryPropertyNames != null)
                                {
                                    foreach (string s in secondaryPropertyNames.Keys)
                                    {
                                        if (secondaryPropertyNames[s].Contains(secondaryPropertyName))
                                        {
                                            Type secondaryType = null;

                                            for (int k = 0; k < secondaryTypes.Count; k++)
                                            {
                                                if (secondaryTypes[k].FullName == s)
                                                {
                                                    secondaryType = secondaryTypes[k];
                                                    break;
                                                }
                                            }

                                            if (secondaryType == null)
                                                continue;

                                            if (!isCustom)
                                                identityID = columnIDs[i];
                                            else
                                                identityID = progressiveIdentityID;

                                            lastProgressiveColumnID = identityID;
                                            progressiveIdentityID++;

                                            int mappingID = (int)secondaryPropertyNames[s][secondaryPropertyName];

                                            try
                                            {
                                                //
                                                //NOTE : Need to check this error when Instrument Type is used
                                                //

                                                if (secondaryProperties[s][mappingID].Name != "InstrumentType" && !PropertyList.ContainsKey(identityID))
                                                {
                                                    PropertyAccessor newAccessor = null;

                                                    if (!PropertiesMapBayClass.ContainsKey(secondaryType))
                                                        PropertiesMapBayClass.Add(secondaryType, new Dictionary<string, PropertyAccessor>());

                                                    if (!PropertiesMapBayClass[secondaryType].ContainsKey(str))
                                                    {
                                                        newAccessor = new PropertyAccessor(type, secondaryType, str/*secondaryProperties[s][mappingID].Name*/, isDerivedClassFound);
                                                        PropertiesMapBayClass[secondaryType].Add(str, newAccessor);
                                                    }
                                                    else
                                                    {
                                                        newAccessor = PropertiesMapBayClass[secondaryType][str];

                                                        if (newAccessor != null)
                                                            newAccessor.IsDerivedClassFound = isDerivedClassFound;
                                                    }

                                                    PropertyList.Add(identityID, newAccessor);
                                                }

                                                //
                                                //Making the switch to user defined ID
                                                //
                                                col.Id = identityID;

                                                //
                                                //Making the correct property type
                                                //
                                                col.Type = (Type)secondaryPropertyTypes[s][str];

                                                //
                                                //Set Mapping Name
                                                //
                                                col.MappingName = str;

                                                if (!columnsMapByID.ContainsKey(identityID))
                                                    columnsMapByID.Add(identityID, col);

                                                if (!columnsMapByName.ContainsKey(str))
                                                    columnsMapByName.Add(str, col);

                                                if (colVisible != null && !colVisible.IsUnbound)
                                                {
                                                    //
                                                    //Making the switch to user defined ID
                                                    //
                                                    colVisible.Id = identityID;

                                                    //
                                                    //Making the correct property type
                                                    //
                                                    colVisible.Type = (Type)secondaryPropertyTypes[s][str];

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
                    }
                }

                AssignColumnPropertiesToVisibleColumns();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                isGridInitialized = true;
            }
        }

        public void AssignColumnPropertiesToVisibleColumns()
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

        protected virtual void CreateRefreshTimer()
        {
            if (tmrRefresh != null)
                tmrRefresh.Dispose();

            this.tmrRefresh = new System.Windows.Forms.Timer();
            this.tmrRefresh.Interval = (int)refreshInterval;
            this.tmrRefresh.Tick += tmrRefresh_Tick;
        }

        public void DestructAll()
        {
            UnbindEvents();

            if (tmrRefresh != null)
                tmrRefresh.Stop();

            if (resizeTimer != null)
                resizeTimer.Stop();

            if (resumeLayoutTimer != null)
                resumeLayoutTimer.Stop();

            if (tmrUpdate != null)
                tmrUpdate.Stop();
        }

        public void UnbindEvents()
        {
            try
            {
                if (tmrRefresh != null)
                {
                    this.tmrRefresh.Tick -= tmrRefresh_Tick;
                    this.tmrRefresh.Stop();
                    this.tmrRefresh.Dispose();
                }

                if (dataSource != null)
                {
                    if (dataSource is INotifyCollectionChanged)
                    {
                        (dataSource as INotifyCollectionChanged).CollectionChanged -= OnDataSource_CollectionChanged;
                    }
                }

                this.Resize -= new System.EventHandler(OnGridResize);

                if (VScroll != null && !VScroll.IsDisposed)
                    VScroll.Close();

                if (HScroll != null && !HScroll.IsDisposed)
                    HScroll.Close();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ResetColumnStyles(bool preserveColumnSettings)
        {
            try
            {
                Table.SetTableLevelStyleForAllColumns(preserveColumnSettings);
                this.RefreshGrid();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ResetCellStyles()
        {
            try
            {
                Table.SetColumnStylesToCells();
                this.RefreshGrid();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected virtual GridStyleInfo GetViewStyleInfo(int rowIndex, int colIndex)
        {
            return this.Table.VisibleColumns[colIndex].ColumnStyle;
        }

        public int DefaultMinimumWidth = 200;

        internal virtual void PrepareTables()
        {
            try
            {
                //stopPaint = true;
                resumeLayoutTimer.Stop();
                resumeLayoutTimer.Start();
                int pictureTop = 1;
                int pictureWidth = this.ClientSize.Width - 1;
                int pictureHeight = 0;

                if (Table.IsCustomHeader)
                {
                    if (this.Width > 0)
                        this.MinimumSize = new Size(this.Width, 10);
                    else
                        this.MinimumSize = new Size(DefaultMinimumWidth, pictureTop + Table.HeaderHeight + Table.RowHeight);
                }
                else
                {
                    this.MinimumSize = new Size(DefaultMinimumWidth, pictureTop + Table.HeaderHeight + Table.RowHeight);
                }
                if (Table.HideHeader)
                    pictureTop += 1;
                pictureHeight = this.ClientSize.Height - pictureTop - 2;

                //VScrollBar1.Left = this.ClientSize.Width - 2 - VScrollBar1.Width;
                //VScrollBar1.Top = pictureTop + 2;
                //VScrollBar1.Height = pictureHeight - 2;

                Table.DisplayRows = (int)Math.Floor(((float)(this.Height - (Table.HeaderHeight * tableInfo.GetScale())) / (float)(Table.RowHeight * tableInfo.GetScale())));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void OnBufferedDataInsert()
        {
            //
            //NOTE : May come useful later
            //
            //if (InsertBufferData != null)
            //{
            //    EventArgs args = new EventArgs();
            //    InsertBufferData(this, args);
            //}
        }

        public bool IsGroupingEnabled()
        {
            if (GroupByNameList != null && GroupByNameList.Count > 0)
                return true;

            return false;
        }

        protected virtual void InvalidateGrid()
        {
            if (isMainPaint)
                return;

            if (PropertyList.Count == 0)
            {
                if (dataSource != null && dataSource.Count > 0)
                {
                    CheckForDataSourceTypeChange(dataSource[0].GetType());
                }
            }

            if (IsGroupingEnabled())
                tableInfo.PopulateGroupHeaders(); //Do Check before executing

            if ((RecreateMatrixFlag && isGridPaintedOnce) || GroupingHeaderFilterText != groupingHeaderFilterTextLast || DoVeryFullRefresh)
            {
                if (GroupingHeaderFilterText != groupingHeaderFilterTextLast)
                    groupingHeaderFilterTextLast = GroupingHeaderFilterText;

                RecreateMatrix();

                if (GridType == GridType.Virtual)
                    CreateCellMatrixForVirtualGrid();

                RecreateMatrixFlag = false;
                this.Refresh();

                if (!DoVeryFullRefresh)
                    return;
                else
                    DoVeryFullRefresh = false;
            }

            if (StopPaint)
                return;

            tableInfo.BottomRow = tableInfo.TopRow + tableInfo.DisplayRows + 1;

            //
            //Main Data Handling
            //
            DoDataCalculations();

            if (grafx == null)
                return;

            lock (paintLock)
            {
                PaintGridData(null, grafx.Graphics);

                if (cellsToDraw.Count <= 0 && cellsUnboundToDraw.Count <= 0)
                {

                }
                else
                {
                    grafx.Render(Graphics.FromHwnd(this.Handle));
                }
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

                if (editingMode)
                    return;

                if (IsGroupingEnabled())
                    isSortingRequired = true;

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
                    if (i == 120)
                    {
                    }

                    TableInfo.CellStruct structure = cellsToUpdate[i];
                    //if (/*tableInfo.CellMatrix[structure.RowNo, structure.ColNo].IsDirty  && */ structure.RowIndex <= tableInfo.BottomRow)
                    {
                        // if (structure.ColIndex < tableInfo.VisibleColumns.Count)
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
                }

                for (int i = 0; i < cellsUnboundToUpdate.Count; i++)
                {
                    TableInfo.CellStruct structure = cellsUnboundToUpdate[i];
                    if (/*tableInfo.CellMatrix[structure.RowNo, structure.ColNo].IsDirty &&  */ structure.RowIndex <= tableInfo.BottomRow)
                        cellsUnboundToDraw.Add(structure);
                }

                for (int i = 0; i < cellsSummaryToUpdate.Count; i++)
                {
                    TableInfo.CellStruct structure = cellsSummaryToUpdate[i];

                    if (/*tableInfo.CellMatrix[structure.RowNo, structure.ColNo].IsDirty &&  */ structure.RowIndex <= tableInfo.BottomRow)
                        cellsToDraw.Add(structure);
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

        #region Seperate Data Handling - TODO

        //private System.Timers.Timer timerCalc;

        //private void DoDataCalculationsNew()
        //{
        //    try
        //    {
        //        while (true)
        //        {
        //            timerCalc_Elapsed(null, null);
        //            Thread.Sleep(1000);
        //        }

        //        //if (timerCalc == null)
        //        //{
        //        //    timerCalc = new System.Timers.Timer();
        //        //    timerCalc.Interval = 500;// this.RefreshInterval;
        //        //    timerCalc_Elapsed(null, null);
        //        //    timerCalc.Elapsed += new System.Timers.ElapsedEventHandler(timerCalc_Elapsed);
        //        //    timerCalc.Start();
        //        //}

        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionsLogger.LogError(ex);
        //    }
        //}

        //void timerCalc_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        this.cellsToUpdate.Clear();
        //        this.cellsUnboundToUpdate.Clear();
        //        this.cellsSummaryToUpdate.Clear();
        //        this.cellsNestedHeaders.Clear();
        //        this.cellsRowHeaders.Clear();

        //        if (tableInfo.IsDirty)
        //            blinkRegistry.Clear();

        //        if (!tableInfo.IsDirty)
        //            ProcessBlinkRegistry();

        //        CheckCellsToUpdate(false);

        //        CheckFilterUpdates();

        //        if (tableInfo.IsDirty)
        //            blinkRegistry.Clear();

        //        RaiseCommonQueryCellEvents();

        //        if (isSortingRequired)
        //            tableInfo.SortSourceList();
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionsLogger.LogError(ex);
        //    }
        //}

        #endregion

        protected void CheckFilterUpdates()
        {
            int bottomRow = tableInfo.BottomRow;

            if (tableInfo.IsFilterEnabled)
            {
                for (int i = 0; i < tableInfo.allRecords.Count; i++)
                {
                    if (tableInfo.FilterManager.IsFilterAvailable())
                    {
                        for (int j = 0; j < tableInfo.Columns.Count; j++)
                        {
                            if (tableInfo.FilterManager.IsFilterAvailable(tableInfo.Columns[j].Name))
                            //|| tableInfo.RecordFilters.FilterList.Contains(tableInfo.Columns[j].Name))
                            {
                                if (!tableInfo.Columns[j].IsUnbound)
                                {
                                    (tableInfo.allRecords[i] as Record).checkFiltering = true;
                                    break;
                                }
                                else
                                {
                                    (tableInfo.allRecords[i] as Record).checkFiltering = true;
                                    this.cellsToUpdate.Add(new TableInfo.CellStruct(i, j, tableInfo.allRecords[i] as Record, tableInfo.Columns[j], tableInfo.Columns[j].Type));
                                }
                            }
                        }
                    }

                    (tableInfo.allRecords[i] as Record).MeetsFilterCriteria(false);
                }
            }

            //if (tableInfo.ConditionalFormats.Count > 0)
            //{
            //    if (!tableInfo.IsFilterEnabled)
            //    {
            //        for (int i = 0; i < tableInfo.allRecords.Count; i++)
            //        {
            //            if ((i >= tableInfo.TopRow && i < bottomRow + 1) || isMainPaint)
            //            {
            //                foreach (Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Filters.GridConditionalFormatDescriptor conditionalFormat in tableInfo.ConditionalFormats)
            //                {
            //                    if ((i < tableInfo.CellMatrix.GetLength(0) && tableInfo.CellMatrix[i, 0].IsDirty) || isMainPaint)
            //                    {
            //                        if (conditionalFormat.CompareRecord((tableInfo.allRecords[i] as Record)))
            //                        {
            //                            SetRowStyleText((tableInfo.allRecords[i] as Record).CurrentIndex, conditionalFormat.Style.BackColor, conditionalFormat.Style.BackColorAlt, conditionalFormat.Style.TextColor, conditionalFormat.Style.Font);
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        for (int i = 0; i < tableInfo.filteredRecords.Count; i++)
            //        {
            //            if ((i >= tableInfo.TopRow && i < bottomRow + 1) || isMainPaint)
            //            {
            //                foreach (Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Filters.GridConditionalFormatDescriptor conditionalFormat in tableInfo.ConditionalFormats)
            //                {
            //                    if ((i < tableInfo.CellMatrix.GetLength(0) && tableInfo.CellMatrix[i, 0].IsDirty) || isMainPaint)
            //                    {
            //                        if (conditionalFormat.CompareRecord((tableInfo.filteredRecords[i] as Record)))
            //                        {
            //                            SetRowStyleText(i, conditionalFormat.Style.BackColor, conditionalFormat.Style.BackColorAlt, conditionalFormat.Style.TextColor, conditionalFormat.Style.Font);
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
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
                        tableInfo.CellMatrix[row, i].IsFormattedOnCondition = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected virtual void tmrRefresh_Tick(object sender, EventArgs e)
        {
            //return;
            if (refreshMode == GridRefreshMode.AutoRefresh)
            {
                if (dataSource != null)
                {
                    this.TotalRecordCount = dataSource.Count;
                    this.VScroll.Maximum = tableInfo.RowCount - tableInfo.DisplayRows > 0 ? tableInfo.RowCount - tableInfo.DisplayRows : 0;
                }
                else if (tableInfo != null)
                {
                    this.TotalRecordCount = tableInfo.RowCount;
                    //if(gridType == GridGrouping.GridType.Virtual)
                    //    this.VScroll.Maximum = tableInfo.SourceDataRowCount - tableInfo.DisplayRows > 0 ? tableInfo.SourceDataRowCount - tableInfo.DisplayRows : 0;
                    //else
                    this.VScroll.Maximum = tableInfo.RowCount - tableInfo.DisplayRows > 0 ? tableInfo.RowCount - tableInfo.DisplayRows : 0;
                }


                RefreshGrid();

                //tableInfo.PopulateGroupHeaders();
            }
        }

        //
        //Use this method to refresh the grid from other forms
        //
        public virtual void RefreshGrid()
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                if (InvokeRequired)
                {
                    this.BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate ()
                    {
                        RefreshGrid();
                    }));
                }
                else
                {
                    this.InvalidateGrid();
                }
            }
        }

        public virtual void SetCellMatrixValue(int rowNumber, int colNumber, object obj)
        {
            SetCellMatrixValue(rowNumber, colNumber, obj, false);
        }

        public virtual void SetCellMatrixValue(int rowNumber, int colNumber, object obj, bool addToUpdateCells)
        {
            try
            {
                if (rowNumber >= 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0))
                {
                    SetValue(ref tableInfo.CellMatrix[rowNumber, colNumber], tableInfo.CellMatrix[rowNumber, colNumber].CellStructType, colNumber, obj);

                    //
                    //Adding to updated columns to be drawn
                    //
                    if (tableInfo.CellMatrix[rowNumber, colNumber].IsDirty)
                    {
                        //
                        //NOTE : Checking Contains causes performance issues
                        //
                        if (addToUpdateCells && !this.cellsToUpdate.Contains(tableInfo.CellMatrix[rowNumber, colNumber]))
                            this.cellsToUpdate.Add(tableInfo.CellMatrix[rowNumber, colNumber]);

                        tableInfo.CellMatrix[rowNumber, colNumber].SuspendDraw = false;
                        tableInfo.CellMatrix[rowNumber, colNumber].DrawText = true;

                        if (tableInfo.AllowBlink)
                            AddToBlinkRegistry(rowNumber, colNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void SetCellMatrixTempValueToCollection(int rowNumber, int colNumber, object obj)
        {
            try
            {
                if (rowNumber >= 0)
                {
                    TableInfo.CellStruct itemToUpdate = new TableInfo.CellStruct();
                    int index = 0;

                    foreach (TableInfo.CellStruct item in this.cellsToExtractTempData)
                    {
                        if (item.RowIndex == rowNumber && item.ColIndex == colNumber)
                        {
                            itemToUpdate = new TableInfo.CellStruct(item);
                            index = cellsToExtractTempData.IndexOf(item);
                            break;
                        }
                    }

                    SetValue(ref itemToUpdate, itemToUpdate.CellStructType, -1, obj);
                    if (index >= 0)
                        cellsToExtractTempData[index] = itemToUpdate;
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }
        public virtual void SuspendCellDraw(int rowNumber, int colNumber, bool suspend, bool suspendAllPaint)
        {
            if (this.tableInfo.CellMatrix.GetLength(0) > 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0))
            {
                if (suspendAllPaint)
                    tableInfo.CellMatrix[rowNumber, colNumber].SuspendDraw = suspend;
                else
                    tableInfo.CellMatrix[rowNumber, colNumber].DrawText = !suspend;
            }
        }

        public virtual void SetCellMatrixUnboundValue(int rowNumber, int colNumer, object obj)
        {
            SetCellMatrixUnboundValue(rowNumber, colNumer, obj, false);
        }

        public virtual void SetCellMatrixUnboundValue(int rowNumber, int colNumer, object obj, bool addToUnboundUpdateCells)
        {
            if (obj == null)
                return;

            if (editingMode)
            {
                if (this.cellFullMatrix.GetLength(0) > 0 && rowNumber < this.cellFullMatrix.GetLength(0))
                {
                    SetValue(ref cellFullMatrix[rowNumber, colNumer], cellFullMatrix[rowNumber, colNumer].CellStructType, colNumer, obj);
                }
            }
            else if (IsValueExtractMode)
            {
                SetCellMatrixTempValueToCollection(rowNumber, colNumer, obj);
            }
            else
            {
                try
                {
                    if (tableInfo.IsFilterEnabled)
                    {
                        if (this.tableInfo.CellMatrix.GetLength(0) > 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0))
                        // && !tableInfo.RecordFilters.FilterList.Contains(tableInfo.VisibleColumns[colNumer].Name))
                        {
                            int rowCounts = tableInfo.CellMatrix.GetLength(0);
                            int colCounts = tableInfo.CellMatrix.GetLength(1);

                            //if (rowCounts > rowNumber && colCounts > colNumer)
                            {
                                SetValue(ref tableInfo.CellMatrix[rowNumber, colNumer], tableInfo.CellMatrix[rowNumber, colNumer].CellStructType, colNumer, obj);

                                //
                                //Adding to unbound columns to be drawn
                                //
                                if (tableInfo.CellMatrix[rowNumber, colNumer].IsDirty)
                                {
                                    //
                                    //NOTE : Checking Contains causes performance issues
                                    //
                                    if (addToUnboundUpdateCells)// && !this.cellsUnboundToUpdate.Contains(tableInfo.CellMatrix[rowNumber, colNumer]))
                                        this.cellsUnboundToUpdate.Add(tableInfo.CellMatrix[rowNumber, colNumer]);

                                    if (tableInfo.IsSortingEnabled && tableInfo.SortedColumnDescriptors[tableInfo.VisibleColumns[colNumer].MappingName] != null)
                                        isSortingRequired = true;

                                    tableInfo.CellMatrix[rowNumber, colNumer].SuspendDraw = false;
                                    tableInfo.CellMatrix[rowNumber, colNumer].DrawText = true;

                                    if (tableInfo.AllowBlink)
                                        AddToBlinkRegistry(rowNumber, colNumer);
                                }
                            }
                        }
                        else
                        {
                            TableInfo.TableColumn col = tableInfo.VisibleColumns[colNumer];

                            if (col != null && col.IsUnbound)
                            {
                                Record rec = tableInfo.allRecords[rowNumber] as Record;
                                if (rec != null)
                                {
                                    rec.checkFiltering = true;
                                    //
                                    //TODO
                                    //
                                    if (rec.MeetsFilterCriteria(false))
                                    {

                                    }
                                    rec.FilterReset = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (rowNumber >= 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0))
                        {
                            SetValue(ref tableInfo.CellMatrix[rowNumber, colNumer], tableInfo.CellMatrix[rowNumber, colNumer].CellStructType, colNumer, obj);

                            //
                            //Adding to unbound columns to be drawn
                            //
                            if (tableInfo.CellMatrix[rowNumber, colNumer].IsDirty)
                            {
                                if (addToUnboundUpdateCells)// && !this.cellsUnboundToUpdate.Contains(tableInfo.CellMatrix[rowNumber, colNumer]))
                                    this.cellsUnboundToUpdate.Add(tableInfo.CellMatrix[rowNumber, colNumer]);

                                if (tableInfo.IsSortingEnabled && tableInfo.SortedColumnDescriptors[tableInfo.VisibleColumns[colNumer].MappingName] != null)
                                    isSortingRequired = true;

                                tableInfo.CellMatrix[rowNumber, colNumer].SuspendDraw = false;
                                tableInfo.CellMatrix[rowNumber, colNumer].DrawText = true;

                                if (tableInfo.AllowBlink)
                                    AddToBlinkRegistry(rowNumber, colNumer);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionsLogger.LogError(ex);
                }
            }
        }

        private void CreateMatrixForPrinting(int rowCount, int columnCount)
        {
            try
            {
                cellFullMatrix = new TableInfo.CellStruct[rowCount, columnCount];

                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        TableInfo.TableColumn col = this.tableInfo.VisibleColumns[j];
                        cellFullMatrix[i, j].Column = col;  // VisibleColumns[j];

                        if (col == null)
                            continue;

                        if (cellFullMatrix[i, j].CellModelType != CellType.Summary)
                        {
                            if (col.QueryStyle)
                            {
                                cellFullMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle.Clone();
                                cellFullMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle.Clone();
                            }
                            else
                            {
                                cellFullMatrix[i, j].Style = (GridStyleInfo)col.ColumnStyle;
                                cellFullMatrix[i, j].OriginalStyle = (GridStyleInfo)col.ColumnStyle;
                            }
                        }
                        else
                        {
                            //  cellMatrix[i, j].Style = (GridStyleInfo)SummaryStyle.Clone();
                            //  cellMatrix[i, j].OriginalStyle = (GridStyleInfo)SummaryStyle.Clone();
                        }

                        cellFullMatrix[i, j].Type = col.Type;
                        col.ColumnStyle.CellValueType = col.Type;

                        col.ColumnStyle.CellValueType = this.Table.VisibleColumns[j].Type;

                        cellFullMatrix[i, j].Style.CellValueType = col.Type;
                        cellFullMatrix[i, j].OriginalStyle.CellValueType = col.Type;
                        cellFullMatrix[i, j].IsEmpty = true;
                        cellFullMatrix[i, j].TextInt = 0;
                        cellFullMatrix[i, j].TextDouble = 0;
                        cellFullMatrix[i, j].TextLong = 0;
                        cellFullMatrix[i, j].TextString = "";
                        cellFullMatrix[i, j].RowIndex = i;
                        cellFullMatrix[i, j].ColIndex = j;
                        cellFullMatrix[i, j].IsDirty = false;
                        cellFullMatrix[i, j].IsPushButton = col.CellModelType == CellType.PushButton;
                        cellFullMatrix[i, j].CellModelType = col.CellModelType;
                        cellFullMatrix[i, j].DrawText = true;

                        if (this.cellFullMatrix.GetLength(0) > i)
                            this.cellFullMatrix[i, j].SourceIndex = i;

                        // cellMatrix[i, j].Record = r;
                        if (col.CurrentPosition <= 0)
                            col.CurrentPosition = j;
                        if (this.Table.Columns[j].CurrentPosition <= 0)
                            this.Table.Columns[j].CurrentPosition = j;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ExportVirtualToExcelHelper()
        {
            try
            {
                editingMode = true;
                CreateMatrixForPrinting(this.tableInfo.sourceDataRowCount, this.Table.VisibleColumns.Count);

                for (int i = 0; i < this.tableInfo.sourceDataRowCount; i++)
                {
                    for (int j = 0; j < this.Table.VisibleColumns.Count; j++)
                    {
                        cellsUnboundToUpdate.Add(cellFullMatrix[i, j]);
                    }
                }

                RaiseCommonQueryCellEvents();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                editingMode = false;
            }
        }

        public virtual void SetCellMatrixRowHeaderTest(int rowNumber, string text)
        {
            try
            {
                if (this.tableInfo.CellMatrix.GetLength(0) > 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0))
                {

                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void SetCellMatrixStyle(int rowNumber, int colNumer, GridStyleInfo style)
        {
            try
            {
                if (rowNumber >= 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0) && colNumer < this.tableInfo.CellMatrix.GetLength(1))
                {
                    this.tableInfo.CellMatrix[rowNumber, colNumer].Style = style;

                    if (tableInfo.columnDefaultStyles.Count > 0)//&& tableInfo.columnDefaultStyles.ContainsKey(this.tableInfo.VisibleColumns[colNumer].Name))
                        this.tableInfo.CellMatrix[rowNumber, colNumer].OriginalStyle = tableInfo.columnDefaultStyles[this.tableInfo.VisibleColumns[colNumer].Name];
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void SetCellMatrixStructure(int rowNumber, int colNumer, ref TableInfo.CellStruct cell)
        {
            try
            {
                if (rowNumber >= 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0))
                {
                    this.tableInfo.CellMatrix[rowNumber, colNumer] = cell;
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void SetPushButton(int rowNumber, int colNumer, bool isPushButton)
        {
            if (rowNumber < 0 || colNumer < 0)
                return;

            try
            {
                if (this.tableInfo.CellMatrix.GetLength(0) > 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0)
                    && colNumer < this.tableInfo.CellMatrix.GetLength(1))
                {
                    this.tableInfo.CellMatrix[rowNumber, colNumer].IsPushButton = isPushButton;
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual GridStyleInfo GetCellStyle(int rowNumber, int colNumer)
        {
            GridStyleInfo info = null;

            try
            {
                if (rowNumber >= 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0))
                {
                    info = this.tableInfo.CellMatrix[rowNumber, colNumer].Style;



                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return info;
        }

        public BlinkType GetBlinkType(string key)
        {
            if (blinkRegistry.ContainsKey(key))
                return blinkRegistry[key].Cell.BlinkType;

            return BlinkType.None;
        }

        internal virtual object GetCellMatrixObject(int rowNumber, int colNumer)
        {
            object info = null;

            try
            {
                info = GetValueAsString(tableInfo.CellMatrix[rowNumber, colNumer], tableInfo.CellMatrix[rowNumber, colNumer].CellStructType, tableInfo.CellMatrix[rowNumber, colNumer].Style);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return info;
        }

        public virtual object GetFullCellMatrixObject(int rowNumber, int colNumer)
        {
            object info = null;

            try
            {
                if (cellFullMatrix == null)
                    return null;

                if (cellFullMatrix.GetLength(0) > rowNumber && cellFullMatrix.GetLength(1) > colNumer)
                    info = GetValueAsString(cellFullMatrix[rowNumber, colNumer], cellFullMatrix[rowNumber, colNumer].CellStructType, cellFullMatrix[rowNumber, colNumer].Style);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return info;
        }

        public virtual object GetFullCellMatrixValue(int rowNumber, int colNumer)
        {
            object info = null;

            try
            {
                if (rowNumber >= 0 && rowNumber < this.cellFullMatrix.GetLength(0))
                {
                    info = GetValue(cellFullMatrix[rowNumber, colNumer], cellFullMatrix[rowNumber, colNumer].CellStructType);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return info;
        }

        public virtual object GetCellMatrixValue(int rowNumber, int colNumer)
        {
            object info = null;

            try
            {
                if (rowNumber >= 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0))
                {
                    info = GetValue(tableInfo.CellMatrix[rowNumber, colNumer], tableInfo.CellMatrix[rowNumber, colNumer].CellStructType);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return info;
        }

        public virtual TableInfo.CellStruct GetCellStructure(int rowNumber, int colNumer)
        {
            try
            {
                if (rowNumber >= 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0) && colNumer < this.tableInfo.CellMatrix.GetLength(1))
                {
                    return tableInfo.CellMatrix[rowNumber, colNumer];
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return new TableInfo.CellStruct();
        }

        public virtual TableInfo.CellStruct GetFullMatrixCellStructure(int rowNumber, int colNumer)
        {
            try
            {
                if (rowNumber >= 0 && rowNumber < cellFullMatrix.GetLength(0))
                {
                    return cellFullMatrix[rowNumber, colNumer];
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return new TableInfo.CellStruct();
        }

        internal Control GetWindow()
        {
            if (this.TopLevelControl != null && !this.TopLevelControl.IsDisposed)
                return this.TopLevelControl;
            else if (this.Parent != null && !this.Parent.IsDisposed)
                return this.Parent;

            IntPtr c = WindowsApiClass.GetFocus();

            Control focusedControl = Control.FromHandle(c);

            if (focusedControl != null && !focusedControl.IsDisposed)
                return focusedControl;

            return this;
        }

        public virtual Record GetCurrentRecord()
        {
            Record rec = null;

            if (dataSource != null)
            {
                if (tableInfo.IsFilterEnabled)
                {
                    if (Table.filteredRecords != null && Table.filteredRecords.Count > Table.CurrentRow)
                        rec = this.Table.filteredRecords[Table.CurrentRow] as Record;
                }
                else
                {
                    if (Table.allRecords != null && Table.allRecords.Count > Table.CurrentRow)
                        rec = this.Table.allRecords[Table.CurrentRow] as Record;
                }
            }
            return rec;
        }

        public void RaiseDrawCellButtonBackground(GridDrawCellButtonBackgroundEventArgs e)
        {
            OnDrawCellButtonBackground(e);
        }

        protected virtual void OnDrawCellButtonBackground(GridDrawCellButtonBackgroundEventArgs e)
        {
            if (DrawCellButtonBackground != null)
                DrawCellButtonBackground(this, e);
        }

        public bool RaiseCellButtonClicked(int rowIndex, int colIndex, int index, GridCellButton button)
        {
            GridCellButtonClickedEventArgs e = new GridCellButtonClickedEventArgs(rowIndex, colIndex, index, button);
            try
            {
                OnCellButtonClicked(e);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
                e.Cancel = true;
            }
            return !e.Cancel;
        }

        public void RaiseDrawCellButton(GridDrawCellButtonEventArgs e)
        {
            OnDrawCellButton(e);
        }

        protected virtual void OnDrawCellButton(GridDrawCellButtonEventArgs e)
        {
            if (DrawCellButton != null)
                DrawCellButton(this, e);
        }

        protected virtual void OnCellButtonClicked(GridCellButtonClickedEventArgs e)
        {
            if (CellButtonClicked != null)
                CellButtonClicked(this, e);
        }

        #endregion

        #region Painting Method

        public virtual void PaintCell(int rowIndex, int colIndex, Graphics g)
        {
            PaintCell(rowIndex, colIndex, g, false);
        }

        public virtual void PaintCell(int rowIndex, int colIndex, Graphics g, bool updateBeforePainting)
        {
            PaintCell(rowIndex, colIndex, rowIndex, colIndex, g, updateBeforePainting);
        }

        public virtual void PaintCellForPrinter(int rowIndexToPaint, int colIndexToPaint, Graphics g, bool updateBeforePainting)
        {
            int rowIndex = rowIndexToPaint;
            int colIndex = colIndexToPaint;

            bool refreshRow = colIndex == -1;
            bool isSelected = false;

            //
            //Draw Cell
            //
            string strText = "";

            int sourceRowCount = 0;

            if (this.gridType == GridGrouping.GridType.DataBound)
            {
                sourceRowCount = this.Table.rowCount;
            }
            else
            {
                sourceRowCount = this.tableInfo.sourceDataRowCount;
            }

            if ((rowIndex >= 0 && rowIndex < sourceRowCount))
            //   || (rowIndex >= 0 && rowIndex < this.tableInfo.RowCount && tableInfo.CellMatrix[rowIndex, colIndex].CellModelType == CellType.Summary))
            {
                if (g == null)
                    g = this.CreateGraphics();


                TableInfo.CellStruct cell;
                if (this.gridType != GridGrouping.GridType.Virtual)
                {
                    cell = tableInfo.CellMatrix[rowIndex, colIndex];
                }
                else
                {
                    cell = cellFullMatrix[rowIndex, colIndex];
                }

                if (cell.SuspendDraw)
                    return;

                if (updateBeforePainting && dataSource != null)
                    UpdateCell(rowIndex, colIndex, true);

                GridStyleInfo style = cell.Style;

                strText = GetValueAsString(cell, cell.CellStructType, style);

                Rect.X = this.horizontalOffset + tableInfo.VisibleColumns[colIndex].Left; // +2;
                if (gridType == GridGrouping.GridType.DataBound)
                    Rect.Y = Table.HeaderHeight + ((rowIndexToPaint - 0) * Table.RowHeight);
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
                                if (tableInfo.SelectedRecords != null && tableInfo.SelectedRecords.Count == 1)
                                    isSelected = rowIndexToPaint == tableInfo.CurrentRow;
                                else
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
                    GridPainter.PaintCell(g, Rect, rowIndexToPaint, colIndex, cell, strText, style, (rowIndexToPaint == tableInfo.CurrentRow && colIndex == tableInfo.CurrentCol));
                }
            }
        }


        public virtual void PaintCell(int rowIndexToPaint, int colIndexToPaint, int rowIndex, int colIndex, Graphics g, bool updateBeforePainting)
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
                            Rect.Y = Table.HeaderHeight + ((rowIndexToPaint - tableInfo.TopRow) * Table.RowHeight) + trasformValue;
                        else
                            Rect.Y = Table.HeaderHeight + ((rowIndexToPaint) * Table.RowHeight) + trasformValue;

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
                                        if (tableInfo.SelectedRecords != null && tableInfo.SelectedRecords.Count == 1)
                                            isSelected = rowIndexToPaint == tableInfo.CurrentRow;
                                        else
                                            isSelected = rowIndexToPaint == tableInfo.firstSelectedRow;

                                        if (!isSelected && tableInfo.selectedRecordIndexes.Contains(rowIndexToPaint))
                                            isSelected = true;
                                    }
                                    else
                                        isSelected = (rowIndexToPaint >= tableInfo.firstSelectedRow && rowIndexToPaint <= tableInfo.lastSelectedRow) || (rowIndexToPaint <= tableInfo.firstSelectedRow && rowIndexToPaint >= tableInfo.lastSelectedRow);
                                }
                                else if (tableInfo.RowCelectionType == RowCelectionType.None)
                                {
                                    isSelected = false;
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

                    if (rowIndex >= tableInfo.CellMatrix.GetLength(0) || colIndex >= tableInfo.CellMatrix.GetLength(1))
                        return;

                    TableInfo.CellStruct cell = tableInfo.CellMatrix[rowIndex, colIndex];

                    if (cell.SuspendDraw)
                        return;

                    if (updateBeforePainting && dataSource != null)
                        UpdateCell(rowIndex, colIndex, true);

                    GridStyleInfo style = cell.Style;

                    if (!string.IsNullOrWhiteSpace(cell.TextString))
                    {
                        strText = cell.TextString;
                    }
                    else
                    {
                        strText = GetValueAsString(cell, cell.CellStructType, style);
                    }

                    Rect.X = tableInfo.VisibleColumns[colIndex].Left;// +2;
                    Rect.Width = tableInfo.VisibleColumns[colIndex].CellWidth;

                    if (IsMirrored)
                    {
                        if (!string.IsNullOrEmpty(tableInfo.FrozenColumn))
                        {
                            if (tableInfo.VisibleColumns.Contains(tableInfo.FrozenColumn) && Rect.X - Rect.Width > tableInfo.VisibleColumns[tableInfo.FrozenColumn].Left)
                            {
                                int frozeIndex = Table.GetVisibleColumnFromName(Table.FrozenColumn).CurrentPosition;

                                if (colIndex > frozeIndex)
                                    return;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(tableInfo.FrozenColumn))
                        {
                            if (Rect.X < 0)
                                return;

                            if (tableInfo.VisibleColumns.Contains(tableInfo.FrozenColumn) && Rect.X + Rect.Width < tableInfo.VisibleColumns[tableInfo.FrozenColumn].Left + tableInfo.VisibleColumns[tableInfo.FrozenColumn].CellWidth)
                            {
                                int frozeIndex = Table.GetVisibleColumnFromName(Table.FrozenColumn).CurrentPosition;

                                if (colIndex > frozeIndex)
                                    return;
                            }
                        }
                    }

                    if (gridType == GridGrouping.GridType.DataBound)
                        Rect.Y = Table.HeaderHeight + ((rowIndexToPaint - tableInfo.TopRow) * Table.RowHeight) + trasformValue;
                    else
                        Rect.Y = Table.HeaderHeight + ((rowIndex) * Table.RowHeight) + trasformValue;

                    if (IsGroupingEnabled())
                    {
                        int countPrev = 0;

                        for (int i = 0; i < GroupHeaderRowIds.Count; i++)
                        {
                            if (GroupHeaderRowIds[i].RowId < rowIndex && GroupHeaderRowIds[i].RowId >= tableInfo.TopRow && !GroupHeaderRowIds[i].Collapsed)
                                countPrev++;
                        }

                        Rect.Y += Table.RowHeight * countPrev;

                        if (tableInfo.CheckGroupHeaderAvailable(rowIndex))
                        {
                            int rowCount = 0;

                            for (int i = 0; i < GroupHeaderRowIds.Count; i++)
                            {
                                if (GroupHeaderRowIds[i].RowId <= rowIndex)
                                    rowCount++;
                            }

                            Rect.Height = Table.RowHeight * rowCount;
                        }
                        else
                        {
                            Rect.Height = Table.RowHeight;
                        }
                    }
                    else
                    {
                        Rect.Height = Table.RowHeight;
                    }


                    if (tableInfo.CellSelectionType == CellSelectionType.Row)
                    {
                        if (tableInfo.RowCelectionType == RowCelectionType.Single)
                        {
                            GridPainter.PaintCell(g, Rect, rowIndexToPaint, colIndex, cell, strText, style, rowIndexToPaint == tableInfo.CurrentRow);
                        }
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
                                    if (tableInfo.SelectedRecords != null && tableInfo.SelectedRecords.Count == 1)
                                        isSelected = rowIndexToPaint == tableInfo.CurrentRow;
                                    else
                                        isSelected = rowIndexToPaint == tableInfo.firstSelectedRow;

                                    if (!isSelected && tableInfo.selectedRecordIndexes.Contains(rowIndexToPaint))
                                        isSelected = true;
                                }
                                else
                                    isSelected = (rowIndexToPaint >= tableInfo.firstSelectedRow && rowIndexToPaint <= tableInfo.lastSelectedRow) || (rowIndexToPaint <= tableInfo.firstSelectedRow && rowIndexToPaint >= tableInfo.lastSelectedRow);
                            }
                            else if (tableInfo.RowCelectionType == RowCelectionType.None)
                            {
                                isSelected = false;
                            }

                            GridPainter.PaintCell(g, Rect, rowIndexToPaint, colIndex, cell, strText, style, isSelected);
                        }
                    }
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
                                if (tableInfo.SelectedRecords != null && tableInfo.SelectedRecords.Count == 1)
                                    isSelected = rowIndexToPaint == tableInfo.CurrentRow;
                                else
                                    isSelected = rowIndexToPaint == tableInfo.firstSelectedRow;

                                if (!isSelected && tableInfo.selectedRecordIndexes.Contains(rowIndexToPaint))
                                    isSelected = true;
                            }
                            else
                                isSelected = (rowIndexToPaint >= tableInfo.firstSelectedRow && rowIndexToPaint <= tableInfo.lastSelectedRow) || (rowIndexToPaint <= tableInfo.firstSelectedRow && rowIndexToPaint >= tableInfo.lastSelectedRow);
                        }

                        if (isSelected)
                            isSelected = colIndex == tableInfo.CurrentCol;

                        GridPainter.PaintCell(g, Rect, rowIndexToPaint, colIndex, cell, strText, style, isSelected);
                        //GridPainter.PaintCell(g, Rect, rowIndexToPaint, colIndex, cell, strText, style, (rowIndexToPaint == tableInfo.CurrentRow && colIndex == tableInfo.CurrentCol));
                    }

                    CleanCell(rowIndex, colIndex);
                }
            }
        }

        public void ApplyFilter()
        {
            ApplyFilter(true);
        }

        public void ApplyFilter(bool recreateMatrix)
        {
            try
            {
                if (dataSource == null)
                    return;

                OnBeforeApplyFilter();
                this.stopPaint = true;
                this.cellsToUpdate.Clear();
                this.cellsUnboundToUpdate.Clear();
                this.cellsNestedHeaders.Clear();
                this.cellsRowHeaders.Clear();
                this.cellsSummaryToUpdate.Clear();
                blinkRegistry.Clear();
                this.tableInfo.FilteredRecords.Clear();
                this.tableInfo.selectedRecordIndexes.Clear();

                tableInfo.IsFilterEnabled = true;

                Record tempRec = null;
                //
                //Filter Data
                //
                for (int i = 0; i <= dataSource.Count; i++)
                {
                    if (tableInfo.allRecords.Count > i)
                    {
                        tempRec = (tableInfo.allRecords[i] as Record);
                        tempRec.FilterReset = true;
                        tempRec.MeetsFilterCriteria(true);
                        tempRec.FilterReset = false;
                    }
                }

                if (recreateMatrix && tableInfo.FilterManager.IsFilterAvailable())
                {
                    if (this.GridType != GridGrouping.GridType.MultiColumn)
                        tableInfo.CreateMatrixFromFilteredRecords(tableInfo.VisibleColumnCount);
                    else
                        tableInfo.CreateMultiColumnMatrixFromFilteredRecords(tableInfo.VisibleColumnCount);
                }

                OnAfterApplyFilter();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                resetGrid = true;
                this.stopPaint = false;
                Table.ChangedSorting = true;

                try
                {
                    if (recreateMatrix)
                    {
                        if (InvokeRequired)
                        {
                            this.BeginInvoke(new System.Windows.Forms.MethodInvoker(delegate ()
                            {
                                Invalidate();
                            }));
                        }
                        else
                        {
                            Invalidate();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionsLogger.LogError(ex);
                }
            }
        }

        private void OnBeforeApplyFilter()
        {
            try
            {
                if (BeforeApplyFilter != null)
                    BeforeApplyFilter(this, null);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void OnAfterApplyFilter()
        {
            try
            {
                if (AfterApplyFilter != null && tableInfo?.FilteredRecords != null)
                {
                    List<object> filteredItems = new List<object>();

                    foreach (Record item in tableInfo.FilteredRecords)
                    {
                        if (item != null)
                            filteredItems.Add(item.ObjectBound);
                    }

                    AfterApplyFilter(this, filteredItems);
                }

            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        //
        //Previous Method
        //
        //protected virtual void PaintGrid(object sender, PaintEventArgs e)
        //{
        //    try
        //    {
        //        if (StopPaint)
        //            return;

        //        if (RecreateMatrixFlag)
        //            return;

        //        //
        //        //Use this if loading time should be improved
        //        //
        //        //if (isGridPaintedOnce)
        //        //    stopPaint = true;

        //        isMainPaint = true;

        //        SetColumnPositions();

        //        Rectangle rect = new Rectangle(e.ClipRectangle.X, e.ClipRectangle.Y, e.ClipRectangle.Width - VScroll.Width, e.ClipRectangle.Height);

        //        //
        //        //Draw Header
        //        //
        //        if (!tableInfo.HideHeader)
        //            GridPainter.PaintHeaders(e.Graphics, rect);

        //        this.TotalRecordCount = tableInfo.RowCount;
        //        this.VScroll.Maximum = tableInfo.RowCount - tableInfo.DisplayRows > 0 ? tableInfo.RowCount - tableInfo.DisplayRows : 0;

        //        if (isGridPaintedOnce || this.tableInfo.IsDirty)
        //        {
        //            if (this.tableInfo.IsDirty || isSortingRequired)
        //                tableInfo.SortSourceList();

        //            this.cellsToUpdate.Clear();
        //            this.cellsUnboundToUpdate.Clear();
        //            this.cellsSummaryToUpdate.Clear();
        //            this.cellsNestedHeaders.Clear();
        //            this.cellsRowHeaders.Clear();
        //            this.cellsToDraw.Clear();
        //            this.cellsUnboundToDraw.Clear();

        //            CheckCellsToUpdate(true);

        //            if (tableInfo.IsDirty)
        //                blinkRegistry.Clear();

        //            RaiseCommonQueryCellEvents();

        //            if (this.tableInfo.IsDirty || isSortingRequired)
        //                tableInfo.SortSourceList();
        //        }
        //        else
        //            tableInfo.SortSourceList();

        //        if (this.dataSource != null)
        //            PaintGridWithDataSource(e.Graphics, false);
        //        else
        //            PaintGridWithMatrix(e.Graphics, false);
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionsLogger.LogError(ex);
        //    }
        //    finally
        //    {
        //        isGridPaintedOnce = true;
        //        isMainPaint = false;
        //        resetGrid = false;
        //        this.tableInfo.IsDirty = false;
        //        isSortingRequired = false;
        //        stopPaint = false;
        //    }
        //}

        protected virtual void PaintGrid(object sender, Graphics g)
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


                Table.DisplayRows = (int)Math.Floor(((float)(this.Height - (Table.HeaderHeight * tableInfo.GetScale())) / (float)(Table.RowHeight * tableInfo.GetScale())));

                this.TotalRecordCount = tableInfo.RowCount;
                this.VScroll.Maximum = tableInfo.RowCount - tableInfo.DisplayRows;
                this.VScroll.Value = tableInfo.TopRow;

                int totalWidth = (int)(GetVisibleColumnWidth() * tableInfo.GetScale());

                if (TurnOnAutoResizeGridColumns)
                    this.HScroll.Maximum = (totalWidth > this.Width) ? ((this.IsMirrored) ? totalWidth - this.Width : totalWidth - this.Width) : 0;
                else
                {
                    if (horizontalOffset > 0)
                        this.HScroll.Maximum = Math.Abs(totalWidth - this.Width);
                    else
                        this.HScroll.Maximum = (totalWidth > this.Width) ? ((this.IsMirrored) ? totalWidth - this.Width : totalWidth - this.Width) : 0;
                }

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

                //
                //Draw Header
                //
                if (!isGridPaintedOnce || this.tableInfo.IsDirty
                    || this.tableInfo.AllowRowHeaders || this.tableInfo.AllowNestedHeaders
                    || (tableInfo.VisibleColumns.Count > 0 && startColumnLocation != tableInfo.VisibleColumns[0].Left) || trasformValue != 0)
                    PaintHeaders(g, rect);
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

                if (needAlign == true)
                {
                    needAlign = false;

                    if (!editingMode)
                        RTLAwarePosition(0);
                }


            }
        }

        internal virtual void SetColumnPositions()
        {
            SetColumnPositions(false);
        }

        internal virtual void SetColumnPositions(bool resetHorizontalOffset)
        {
            if (tableInfo.isHeaderMouseDown)
                return;

            if (tableInfo.VisibleColumns.Count <= 0)
                return;

            bool isMirrored = IsMirrored;
            int intLeftPos = 0;
            int initialLeft = 0;

            int frozenColumnIndex = -1;

            if (!GridGroupingControl.TurnOnAutoResizeGridColumns)
            {
                if (resetHorizontalOffset)
                {
                    horizontalOffset = 0;
                    tableInfo.XOffset = 0;
                }
                else
                {
                    if (tableInfo.IsDirty && tableInfo.VisibleColumns[tableInfo.VisibleColumns.Count - 1].Left + tableInfo.VisibleColumns[tableInfo.VisibleColumns.Count - 1].CellWidth < horizontalOffset)
                    {
                        //horizontalOffset = 0;
                        //tableInfo.XOffset = 0;
                    }
                }
            }

            if (!string.IsNullOrEmpty(Table.FrozenColumn))
            {
                frozenColumnIndex = Table.GetVisibleColumnFromName(Table.FrozenColumn).CurrentPosition;
            }

            if (!isMirrored)
            {
                if (frozenColumnIndex < 0)
                {
                    tableInfo.VisibleColumns[0].Left = 0 + tableInfo.RowHeaderWidth - (int)(horizontalOffset / (tableInfo.GetScale() * tableInfo.GetScale()));
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
                        initialLeft = tableInfo.VisibleColumns[0].CellWidth - (int)(horizontalOffset / (tableInfo.GetScale() * tableInfo.GetScale()));
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
                    if (frozenColumnIndex >= i)
                    {
                        tableInfo.VisibleColumns[i].Left = tableInfo.VisibleColumns[i - 1].Left + tableInfo.VisibleColumns[i - 1].CellWidth;
                        intLeftPos = tableInfo.VisibleColumns[i].Left;
                    }
                    else
                    {
                        if (frozenColumnIndex > 0 && frozenColumnIndex == i - 1)
                        {
                            intLeftPos -= (int)(horizontalOffset / (tableInfo.GetScale() * tableInfo.GetScale()));
                            intLeftPos += tableInfo.VisibleColumns[i - 1].CellWidth;
                        }

                        tableInfo.VisibleColumns[i].Left = intLeftPos;
                        intLeftPos = intLeftPos + tableInfo.VisibleColumns[i].CellWidth;
                    }
                }
            }
            else
            {
                if (frozenColumnIndex < 0)
                {
                    tableInfo.VisibleColumns[0].Left = (int)(this.tableInfo.TotalVisibleColumnWidth / tableInfo.GetScale()) + 5 - tableInfo.VisibleColumns[0].CellWidth;

                    double ration = 1 / tableInfo.GetScale();

                    if (/*frozenColumnIndex >= 0 && */ tableInfo.TotalVisibleColumnWidth * tableInfo.GetScale() < Width)
                        tableInfo.VisibleColumns[0].Left = (int)(Width / tableInfo.GetScale()) - (int)(tableInfo.VisibleColumns[0].CellWidth);

                    if (this.tableInfo.TotalVisibleColumnWidth * tableInfo.GetScale() > this.Width)
                        tableInfo.VisibleColumns[0].Left += (int)((Width - tableInfo.TotalVisibleColumnWidth) / tableInfo.GetScale()) + (int)((NewHScroll.Value * ration /** tableInfo.GetScale()*/));

                    // tableInfo.VisibleColumns[0].Left = (int)((this.Width / tableInfo.GetScale()) - tableInfo.VisibleColumns[0].CellWidth + ((NewHScroll.Maximum + NewHScroll.Value) / tableInfo.GetScale()));
                }
                else
                {
                    tableInfo.VisibleColumns[0].Left = (int)(this.tableInfo.TotalVisibleColumnWidth / tableInfo.GetScale()) + 5 - tableInfo.VisibleColumns[0].CellWidth;

                    if (/*frozenColumnIndex >= 0 && */ tableInfo.TotalVisibleColumnWidth > Width)
                        tableInfo.VisibleColumns[0].Left += (int)((Width - tableInfo.TotalVisibleColumnWidth) / tableInfo.GetScale());

                    if (/*frozenColumnIndex >= 0 && */ tableInfo.TotalVisibleColumnWidth < Width)
                        tableInfo.VisibleColumns[0].Left = (int)(Width / tableInfo.GetScale()) - (int)(tableInfo.VisibleColumns[0].CellWidth);
                }


                if (tableInfo.VisibleColumns.Count > 0)
                {
                    if (frozenColumnIndex < 0)
                    {
                        initialLeft = tableInfo.VisibleColumns[0].Left;
                    }
                    else
                    {
                        int internalOffset = HScroll.Maximum > 0 ? (int)(NewHScroll.Value / tableInfo.GetScale()) : 0;
                        initialLeft = (int)(tableInfo.VisibleColumns[0].Left + (internalOffset));
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
                    if (frozenColumnIndex >= i)
                    {
                        tableInfo.VisibleColumns[i].Left = tableInfo.VisibleColumns[i - 1].Left - tableInfo.VisibleColumns[i].CellWidth;
                        intLeftPos = tableInfo.VisibleColumns[i].Left;
                    }
                    else
                    {
                        if (frozenColumnIndex > 0 && frozenColumnIndex == i - 1)
                        {
                            int internalOffset = NewHScroll.Maximum > 0 ? NewHScroll.Value : 0;
                            intLeftPos += (internalOffset);
                        }

                        tableInfo.VisibleColumns[i].Left = intLeftPos - tableInfo.VisibleColumns[i].CellWidth;
                        intLeftPos = tableInfo.VisibleColumns[i].Left;
                    }
                }
            }
        }

        protected virtual void PaintGridWithMatrixOnUpdate(Graphics g, bool paintNew)
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

        protected virtual void PaintGridWithMatrix(Graphics g, bool paintNew)
        {
            try
            {
                if (tableInfo.CellMatrix == null || (gridType == GridGrouping.GridType.Virtual && tableInfo.sourceDataRowCount == -1))
                    return;

                TotalRecordCount = this.Table.RowCount;
                bool isDirty = false;

                bool isLastRowOmit = false;

                if (tableInfo.BottomRow == tableInfo.sourceDataRowCount && tableInfo.DisplayRows < tableInfo.BottomRow)
                {
                    //trasformValue = (this.Height - tableInfo.HeaderHeight) - (tableInfo.DisplayRows * tableInfo.RowHeight);
                    isLastRowOmit = true;
                }
                else
                {
                    isLastRowOmit = false;
                    //trasformValue = 0;
                }
                //
                //Draw Data
                //
                for (int i = 0; i < tableInfo.RowCount; i++)
                {
                    if (i == tableInfo.RowCount - 1)
                    {
                        if (isLastRowOmit)
                            continue;
                    }

                    if (i >= tableInfo.sourceDataRowCount)
                        continue;

                    if (tableInfo.CellMatrix.GetLength(0) > i)
                    {
                        for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
                        {
                            if (tableInfo.VisibleColumns[j].Left > this.Width)
                                continue;

                            if (j < tableInfo.CellMatrix.GetLength(1) && i < tableInfo.CellMatrix.GetLength(0))
                                isDirty = tableInfo.CellMatrix[i, j].IsDirty;
                            else
                                continue;

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
        protected virtual void PaintGridWithDataSource(Graphics g, bool drawAll)
        {
            try
            {
                bool isDirty = false;
                int colID = -1; ;
                TotalRecordCount = dataSource.Count;

                int index = tableInfo.TopRow;
                int bottomRow = tableInfo.BottomRow;
                //
                //Draw Data
                //

                if (bottomRow == tableInfo.RowCount && tableInfo.DisplayRows < bottomRow)
                {
                    if (tableInfo.GetScale() > 1)
                    {
                        //  trasformValue = (int)((this.Height - (tableInfo.HeaderHeight * tableInfo.GetScale())) - ((tableInfo.DisplayRows + 1) * tableInfo.RowHeight * tableInfo.GetScale()));
                    }
                    else
                    {
                        //  trasformValue = (this.Height - tableInfo.HeaderHeight) - (tableInfo.DisplayRows * tableInfo.RowHeight);
                    }

                }
                else
                {
                    trasformValue = 0;
                }

                for (int i = tableInfo.TopRow; i <= bottomRow; i++)
                {
                    if (i < dataSource.Count && i < tableInfo.CellMatrix.GetLength(0))
                    {
                        for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
                        {
                            if (tableInfo.VisibleColumns[j].Left > this.Width)
                                continue;

                            if (tableInfo.CellMatrix.GetLength(1) <= j)
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

                if (!string.IsNullOrEmpty(tableInfo.FrozenColumn))
                {
                    int frozenColumnIndex = tableInfo.GetVisibleColumnFromName(tableInfo.FrozenColumn).CurrentPosition + 1;

                    for (int j = 0; j < frozenColumnIndex; j++)
                    {
                        for (int i = tableInfo.TopRow; i <= tableInfo.BottomRow; i++)
                        {
                            if (i < dataSource.Count)
                            {
                                PaintCell(i, j, g);
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

        protected virtual void PaintGridWithDataSourceOnUpdate(Graphics g, bool paintNew)
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

                    if (!tableInfo.VisibleColumns[structure.ColIndex].IsUnbound || structure.CellModelType == CellType.Summary)
                    {
                        if (structure.RowIndex < dataSource.Count || structure.CellModelType == CellType.Summary)
                        {
                            if (tableInfo.VisibleColumns[structure.ColIndex].Left > this.Width)
                                continue;

                            if (tableInfo.AllowSummaryRows && tableInfo.NumberOfSummaryRows > 0)
                            {
                                int buffer = tableInfo.NumberOfSummaryRows;

                                if (DataSource.Count - 1 < tableInfo.BottomRow)
                                {
                                    if (structure.RowIndex < tableInfo.TopRow - 1 + buffer)
                                        continue;

                                    //continue;
                                }
                            }

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
                        if (structure.RowIndex < dataSource.Count && structure.RowIndex >= tableInfo.TopRow)
                        {
                            if (tableInfo.VisibleColumns[structure.ColIndex].Left > this.Width)
                                continue;

                            PaintCell(structure.RowIndex, structure.ColIndex, g);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(tableInfo.FrozenColumn))
                {
                    int frozenColumnIndex = tableInfo.GetVisibleColumnFromName(tableInfo.FrozenColumn).CurrentPosition;

                    for (int j = frozenColumnIndex; j <= frozenColumnIndex; j++)
                    {
                        for (int i = tableInfo.TopRow; i <= tableInfo.BottomRow; i++)
                        {
                            if (j >= 0 && i < dataSource.Count)
                            {
                                PaintCell(i, j, g);
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
                        //tableInfo.CellMatrix[rowIndex, colIndex].DrawText = true;

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
                //tableInfo.CellMatrix[rowIndex, colIndex].DrawText = true;

                //if (!this.tableInfo.VisibleColumns[colIndex].QueryStyle)
                //{
                //    tableInfo.CellMatrix[rowIndex, colIndex].Style = tableInfo.columnDefaultStyles[tableInfo.CellMatrix[rowIndex, colIndex].Column.Name];//tableInfo.CellMatrix[rowIndex, colIndex].OriginalStyle;
                //}
                //else
                //{
                //    tableInfo.CellMatrix[rowIndex, colIndex].Style = tableInfo.CellMatrix[rowIndex, colIndex].OriginalStyle;
                //}
            }

            if ((!isMainPaint || this.gridType == GridGrouping.GridType.Virtual) && !tableInfo.VisibleColumns[colIndex].IsCustomColumn)
                tableInfo.CellMatrix[rowIndex, colIndex].LastUpdatedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;

            tableInfo.CellMatrix[rowIndex, colIndex].UpdateCustomFormula = true;
        }

        protected virtual bool UpdateCell(int rowNo, int colNo, bool mainRefresh, bool onFullPainting = false)
        {
            bool isDirty = false;
            int colID = -1;

            if (rowNo < dataSource.Count)
            {
                TableInfo.TableColumn column = tableInfo.VisibleColumns[colNo];

                if (column.Left > this.Width && !onFullPainting)
                    return isDirty;

                isDirty = tableInfo.CellMatrix[rowNo, colNo].IsDirty;

                if (!column.IsUnbound)
                {
                    colID = column.Id;

                    if (colID >= 0)
                    {
                        if (!isDirty || resetGrid || mainRefresh)
                        {
                            SetValue(ref tableInfo.CellMatrix[rowNo, colNo], tableInfo.CellMatrix[rowNo, colNo].CellStructType, colID);

                            isDirty = tableInfo.CellMatrix[rowNo, colNo].IsDirty || mainRefresh;

                            if (isDirty && tableInfo.IsSortingEnabled)
                            {
                                //
                                //Need to test this
                                //
                                if (tableInfo.SortedColumnDescriptors[column.MappingName] != null || this.tableInfo.IsDirty) // && !mainRefresh
                                    isSortingRequired = true;
                            }
                        }
                        else
                        {
                            if (tableInfo.AllowBlink && tableInfo.CellMatrix[rowNo, colNo].Column.AllowBlink && tableInfo.CellMatrix[rowNo, colNo].CellModelType != CellType.Summary)
                            {
                                SetValue(ref tableInfo.CellMatrix[rowNo, colNo], tableInfo.CellMatrix[rowNo, colNo].CellStructType, colID);
                            }
                        }
                    }
                }
            }

            return isDirty;
        }

        protected virtual void SetValue(ref TableInfo.CellStruct cell, TableInfo.CellStructType type, int colID)
        {
            SetValue(ref cell, type, colID, null);
        }

        protected virtual void SetValue(ref TableInfo.CellStruct cell, TableInfo.CellStructType type, int colID, object value)
        {
            if (type == TableInfo.CellStructType.None)
                return;

            if (value == null)
            {
                switch (type)
                {
                    case TableInfo.CellStructType.String:
                        cell.TextString = GetStringValueUsingProperty(colID, cell.RowIndex, false);
                        break;
                    case TableInfo.CellStructType.Decimal:

                        //if (cell.Column.IsCustomFormulaColumn)
                        //{
                        //    //value = Convert.ToDecimal(cell.Column.Table.ExpressionFields[cell.Column.Name].GetValue(tableInfo.CellMatrix[cell.RowIndex, cell.ColIndex].Record));
                        //}
                        //else
                        value = GetDecimalValueUsingProperty(colID, cell.RowIndex, false);

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
                        else
                            cell.DrawText = true;

                        break;
                    case TableInfo.CellStructType.Double:
                        //cell.TextDouble = GetDoubleValueUsingProperty(colID, cell.RowNo, false);

                        //if (cell.Column.IsCustomFormulaColumn)
                        //{
                        //    //value = Convert.ToDouble(cell.Column.Table.ExpressionFields[cell.Column.Name].GetValue(tableInfo.CellMatrix[cell.RowIndex, cell.ColIndex].Record));
                        //}
                        //else
                        value = GetDoubleValueUsingProperty(colID, cell.RowIndex, false);

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
                        else
                            cell.DrawText = true;

                        break;
                    case TableInfo.CellStructType.Integer:
                        //if (cell.Column.IsCustomFormulaColumn)
                        //{
                        //    //value = Convert.ToDouble(cell.Column.Table.ExpressionFields[cell.Column.Name].GetValue(tableInfo.CellMatrix[cell.RowIndex, cell.ColIndex].Record));
                        //}
                        //else
                        value = GetIntValueUsingProperty(colID, cell.RowIndex, false);

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
                        else
                            cell.DrawText = true;

                        break;
                    case TableInfo.CellStructType.DateTime:
                        cell.TextDateTime = GetDateTimeValueUsingProperty(colID, cell.RowIndex, false);
                        break;
                    case TableInfo.CellStructType.Long:
                        //if (cell.Column.IsCustomFormulaColumn)
                        //{
                        //    //value = Convert.ToDouble(cell.Column.Table.ExpressionFields[cell.Column.Name].GetValue(tableInfo.CellMatrix[cell.RowIndex, cell.ColIndex].Record));
                        //}
                        //else
                        value = GetLongValueUsingProperty(colID, cell.RowIndex, false);

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
                        else
                            cell.DrawText = true;

                        break;
                    case TableInfo.CellStructType.Style:
                        cell.SetStyleCellValue(GetStyleUsingProperty(colID, cell.RowIndex));
                        if (cell.IsDirty)
                            tableInfo.CellMatrix[cell.RowIndex, cell.ColIndex].LastUpdatedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case TableInfo.CellStructType.String:
                        //cell.TextString = value.ToString();
                        // New logic to avoid removing leading zeros. Ex 001111 to 1111
                        if (double.TryParse(value.ToString(), out returnDouble) && value.ToString().Length== returnDouble.ToString().Length)
                        {
                            cell.IsNumeric = true;
                            cell.TextDouble = returnDouble;
                            cell.TextString = string.Empty;
                            cell.CellStructType |= TableInfo.CellStructType.Double;
                        }
                        else
                        {
                            // New logic to avoid removing leading zeros. Ex 001111 to 1111
                            if (long.TryParse(value.ToString(), out returnLong) && value.ToString().Length == returnLong.ToString().Length)
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
                    case TableInfo.CellStructType.Decimal:
                        if (decimal.TryParse(value.ToString(), out returnDecimal))
                        {
                            cell.IsNumeric = true;
                            cell.TextDecimal = returnDecimal;
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
                        if (cell.IsDirty)
                            tableInfo.CellMatrix[cell.RowIndex, cell.ColIndex].LastUpdatedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        break;
                }
            }
        }

        private long lastTick = 0;
        private Dictionary<string, object> summaryColumnData = new Dictionary<string, object>();

        public virtual object GetSumOfColumnValues(string columnName, bool getStringAsDouble = false)
        {
            if (Environment.TickCount - lastTick > 1000)
            {
                lastTick = Environment.TickCount;
                PopulateAllCellValues();
                summaryColumnData.Clear();

                for (int i = 0; i < Table.VisibleColumns.Count; i++)
                {
                    string colName = tableInfo.VisibleColumns[i].Name;
                    summaryColumnData.Add(colName, GetSumOfColumnValues(tableInfo.GetColumnFromName(colName), getStringAsDouble));
                }
            }

            if (summaryColumnData.ContainsKey(columnName))
                return summaryColumnData[columnName];
            else
                return GetSumOfColumnValues(tableInfo.GetColumnFromName(columnName), getStringAsDouble);
        }

        public virtual object GetSumOfColumnValues(TableInfo.TableColumn column, bool getStringAsDouble = false)
        {
            try
            {
                if (column == null || tableInfo.CellMatrix.GetLength(1) <= column.CurrentPosition)
                    return null;

                returnInt = 0;
                returnDouble = 0;
                returnLong = 0;

                for (int i = 0; i < tableInfo.CellMatrix.GetLength(0); i++)
                {
                    if (tableInfo.CellMatrix[i, column.CurrentPosition].CellModelType == CellType.Summary)
                        continue;

                    switch (column.CellType)
                    {
                        case TableInfo.CellStructType.Double:
                            returnDouble += tableInfo.CellMatrix[i, column.CurrentPosition].TextDouble;
                            break;
                        case TableInfo.CellStructType.Integer:
                            returnInt += tableInfo.CellMatrix[i, column.CurrentPosition].TextInt;
                            break;
                        case TableInfo.CellStructType.Long:
                            returnLong += tableInfo.CellMatrix[i, column.CurrentPosition].TextLong;
                            break;
                        case TableInfo.CellStructType.Decimal:
                            returnDecimal += tableInfo.CellMatrix[i, column.CurrentPosition].TextDecimal;
                            break;
                        case TableInfo.CellStructType.String:
                            if (getStringAsDouble)
                            {
                                double dblValue = tableInfo.CellMatrix[i, column.CurrentPosition].TextDouble;
                                returnDouble += dblValue;
                            }
                            break;
                    }
                }


                returnbject = null;

                switch (column.CellType)
                {
                    case TableInfo.CellStructType.Double:
                        returnbject = returnDouble;
                        break;
                    case TableInfo.CellStructType.Integer:
                        returnbject = returnInt;
                        break;
                    case TableInfo.CellStructType.Long:
                        returnbject = returnLong;
                        break;
                    case TableInfo.CellStructType.Decimal:
                        returnbject = returnDecimal;
                        break;
                    case TableInfo.CellStructType.String:
                        if (getStringAsDouble)
                        {
                            returnbject = returnDouble;
                        }
                        break;
                }

                return returnbject;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
                return null;
            }
        }

        internal virtual object GetRecordColumnValue(Record rec, TableInfo.TableColumn column)
        {
            if (column == null || rec == null || column.IsUnbound)
            {
                if (rec != null && column != null && column.IsUnbound)
                {
                    if (tableInfo.IsFilterEnabled)
                    {
                        int recIndex = this.Table.FilteredRecords.IndexOf(rec);

                        if (recIndex >= 0)
                            return GetCellMatrixValue(recIndex, column.CurrentPosition);
                    }
                    else
                    {
                        return GetCellMatrixValue(rec.CurrentIndex, column.CurrentPosition);
                    }
                }
                return null;
            }

            try
            {
                switch (column.CellType)
                {
                    case TableInfo.CellStructType.String:
                        return GetStringValueUsingProperty(column.Id, rec.CurrentIndex);
                    case TableInfo.CellStructType.Double:
                        return GetDoubleValueUsingProperty(column.Id, rec.CurrentIndex);
                    case TableInfo.CellStructType.Decimal:
                        return GetDecimalValueUsingProperty(column.Id, rec.CurrentIndex);
                    case TableInfo.CellStructType.Integer:
                        return GetIntValueUsingProperty(column.Id, rec.CurrentIndex);
                    case TableInfo.CellStructType.DateTime:
                        return GetDateTimeValueUsingProperty(column.Id, rec.CurrentIndex);
                    case TableInfo.CellStructType.Long:
                        return GetLongValueUsingProperty(column.Id, rec.CurrentIndex);
                    case TableInfo.CellStructType.Style:
                        return GetStyleUsingProperty(column.Id, rec.CurrentIndex);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }

            return null;
        }

        public string GetValueAsString(TableInfo.CellStruct cell, TableInfo.CellStructType type)
        {
            return GetValueAsString(cell, type, null);
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
                    {
                        //
                        //TODO:
                        //Need to add the decimal cellstructtype to grid
                        //This is a temp solution to avoid format issue in decimal type columns
                        //
                        if (cell.Column != null && cell.Column.Type != null && cell.Column.Type == typeof(decimal) && !string.IsNullOrEmpty(format))
                        {
                            decimal outValue = 0;

                            if (Decimal.TryParse(cell.Style.CellValue.ToString(), out outValue))
                                returnString = outValue.ToString(format);
                            else
                                returnString = cell.Style.CellValue.ToString();
                        }
                        else
                            returnString = cell.Style.CellValue.ToString();
                    }
                    break;
            }

            return returnString;
        }

        public virtual object GetValue(int rowIndex, int colIndex)
        {
            if (rowIndex >= this.tableInfo.CellMatrix.GetLength(0) || colIndex >= this.tableInfo.CellMatrix.GetLength(1))
                return null;

            TableInfo.CellStruct structure = this.tableInfo.CellMatrix[rowIndex, colIndex];

            return GetValue(structure, structure.CellStructType);
        }

        internal virtual object GetValue(TableInfo.CellStruct cell, TableInfo.CellStructType type)
        {
            returnbject = null;

            switch (type)
            {
                case TableInfo.CellStructType.String:
                    returnbject = cell.TextString;
                    break;
                case TableInfo.CellStructType.Double:
                    returnbject = cell.TextDouble;
                    break;
                case TableInfo.CellStructType.Decimal:
                    returnbject = cell.TextDecimal;
                    break;
                case TableInfo.CellStructType.Integer:
                    returnbject = cell.TextInt;
                    break;
                case TableInfo.CellStructType.DateTime:
                    returnbject = cell.TextDateTime;
                    break;
                case TableInfo.CellStructType.Long:
                    returnbject = cell.TextLong;
                    break;
                case TableInfo.CellStructType.Style:
                    returnbject = cell.Style.CellValue;
                    break;
            }

            return returnbject;
        }

        private string GetStringValueUsingProperty(int colID, int i)
        {
            bool lookInOriginalRecordList = true;

            if (this.GettingValuesForGroupingHeader)
                lookInOriginalRecordList = false;

            return GetStringValueUsingProperty(colID, i, lookInOriginalRecordList);
        }

        internal string GetStringValueUsingProperty(int colID, int i, bool lookInOriginalRecordList)
        {
            returnString = string.Empty;

            if (!PropertyList.ContainsKey(colID))
                return returnString;

            try
            {
                if (tableInfo.IsFilterEnabled && !lookInOriginalRecordList)
                {
                    if (tableInfo.filteredRecords != null && i < tableInfo.filteredRecords.Count)
                    {
                        returnbject = PropertyList[colID].Get((tableInfo.filteredRecords[i] as Record).ObjectBound);

                        if (returnbject != null)
                            returnString = returnbject.ToString();
                    }
                    else
                    {
                        if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        {
                            returnbject = PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound);

                            if (returnbject != null)
                                returnString = returnbject.ToString();
                        }
                    }
                }
                else
                {
                    if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                    {
                        returnbject = PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound);

                        if (returnbject != null)
                            returnString = returnbject.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (DateTime.Now.TimeOfDay.TotalMilliseconds > lastStringLoggedTime + 300 * 1000)
                    {
                        lastStringLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        stringConvertExceptionCount = 0;
                    }

                    stringConvertExceptionCount++;

                    if (stringConvertExceptionCount <= 20)
                    {
                        ExceptionsLogger.LogError(ex);
                    }
                    else if (stringConvertExceptionCount == 21)
                    {
                        lastStringLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        ExceptionsLogger.LogError("String Convert Exception Count Exceeded for Grid. Suspending logging.");
                    }
                }
                catch
                { }
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

            if (!PropertyList.ContainsKey(colID))
                return 0;
            try
            {
                if (tableInfo.IsFilterEnabled && !lookInOriginalRecordList)
                {
                    if (tableInfo.filteredRecords != null && i < tableInfo.filteredRecords.Count)
                        returnInt = Convert.ToInt32(PropertyList[colID].Get((tableInfo.filteredRecords[i] as Record).ObjectBound));

                    else
                    {
                        if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        {
                            returnInt = Convert.ToInt32(PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound));
                        }
                    }
                }
                else
                {
                    returnbject = PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound);

                    if (returnbject != null && tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        returnInt = Convert.ToInt32(returnbject);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (DateTime.Now.TimeOfDay.TotalMilliseconds > lastIntLoggedTime + 300 * 1000)
                    {
                        lastIntLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        intConvertExceptionCount = 0;
                    }

                    intConvertExceptionCount++;

                    if (intConvertExceptionCount <= 20)
                    {
                        ExceptionsLogger.LogError(ex);
                    }
                    else if (intConvertExceptionCount == 21)
                    {
                        lastIntLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        ExceptionsLogger.LogError("Int Convert Exception Count Exceeded for Grid. Suspending logging.");
                    }
                }
                catch
                { }
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

            if (!PropertyList.ContainsKey(colID))
                return 0;

            try
            {
                if (tableInfo.IsFilterEnabled && !lookInOriginalRecordList)
                {
                    if (tableInfo.filteredRecords != null && i < tableInfo.filteredRecords.Count)
                        returnLong = Convert.ToInt64(PropertyList[colID].Get((tableInfo.filteredRecords[i] as Record).ObjectBound));

                    else
                    {
                        if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        {
                            returnLong = Convert.ToInt64(PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound));
                        }
                    }
                }
                else
                {
                    returnbject = PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound);
                    if (returnbject != null && tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        returnLong = Convert.ToInt64(returnbject);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (DateTime.Now.TimeOfDay.TotalMilliseconds > lastLongLoggedTime + 300 * 1000)
                    {
                        lastLongLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        longConvertExceptionCount = 0;
                    }

                    longConvertExceptionCount++;

                    if (longConvertExceptionCount <= 20)
                    {
                        ExceptionsLogger.LogError(ex);
                    }
                    else if (longConvertExceptionCount == 21)
                    {
                        lastLongLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        ExceptionsLogger.LogError("Long Convert Exception Count Exceeded for Grid. Suspending logging.");
                    }
                }
                catch
                { }
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
                    if (tableInfo.filteredRecords != null && i < tableInfo.filteredRecords.Count)
                        returnbject = PropertyList[colID].Get((tableInfo.filteredRecords[i] as Record).ObjectBound);

                    else
                    {
                        if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        {
                            returnbject = PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound);
                        }
                    }
                }
                else
                {
                    if (tableInfo.IsFilterEnabled)
                    {
                        if (tableInfo.filteredRecords != null && i < tableInfo.filteredRecords.Count)
                            returnbject = PropertyList[colID].Get((tableInfo.filteredRecords[i] as Record).ObjectBound);
                    }
                    else if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        returnbject = PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (DateTime.Now.TimeOfDay.TotalMilliseconds > lastStyleLoggedTime + 300 * 1000)
                    {
                        lastStyleLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        styleConvertExceptionCount = 0;
                    }

                    styleConvertExceptionCount++;

                    if (styleConvertExceptionCount <= 20)
                    {
                        ExceptionsLogger.LogError(ex);
                    }
                    else if (styleConvertExceptionCount == 21)
                    {
                        lastStyleLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        ExceptionsLogger.LogError("Style Convert Exception Count Exceeded for Grid. Suspending logging.");
                    }
                }
                catch
                { }
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

            if (!PropertyList.ContainsKey(colID))
                return 0;

            try
            {
                if (colID == -2)
                    return 0;

                if (tableInfo.IsFilterEnabled && !lookInOriginalRecordList)
                {
                    if (tableInfo.filteredRecords != null && i < tableInfo.filteredRecords.Count)
                        returnDouble = Convert.ToDouble(PropertyList[colID].Get((tableInfo.filteredRecords[i] as Record).ObjectBound));

                    else
                    {
                        if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        {
                            returnDouble = Convert.ToDouble(PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound));
                        }
                    }
                }
                else
                {
                    if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        returnDouble = Convert.ToDouble(PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound));
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (DateTime.Now.TimeOfDay.TotalMilliseconds > lastDoubleLoggedTime + 300 * 1000)
                    {
                        lastDoubleLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        doubleConvertExceptionCount = 0;
                    }

                    doubleConvertExceptionCount++;

                    if (doubleConvertExceptionCount <= 20)
                    {
                        ExceptionsLogger.LogError(ex);
                    }
                    else if (doubleConvertExceptionCount == 21)
                    {
                        lastDoubleLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        ExceptionsLogger.LogError("Double Convert Exception Count Exceeded for Grid. Suspending logging.");
                    }
                }
                catch
                { }
            }

            return returnDouble;
        }

        protected decimal GetDecimalValueUsingProperty(int colID, int i)
        {
            return GetDecimalValueUsingProperty(colID, i, true);
        }

        protected decimal GetDecimalValueUsingProperty(int colID, int i, bool lookInOriginalRecordList)
        {
            returnDecimal = 0;

            if (!PropertyList.ContainsKey(colID))
                return 0;

            try
            {
                if (colID == -2)
                    return 0;

                if (tableInfo.IsFilterEnabled && !lookInOriginalRecordList)
                {
                    if (tableInfo.filteredRecords != null && i < tableInfo.filteredRecords.Count)
                        returnDecimal = Convert.ToDecimal(PropertyList[colID].Get((tableInfo.filteredRecords[i] as Record).ObjectBound));

                    else
                    {
                        if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        {
                            returnDecimal = Convert.ToDecimal(PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound));
                        }
                    }
                }
                else
                {
                    if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        returnDecimal = Convert.ToDecimal(PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound));
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (DateTime.Now.TimeOfDay.TotalMilliseconds > lastDecimalLoggedTime + 300 * 1000)
                    {
                        lastDecimalLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        decimalConvertExceptionCount = 0;
                    }

                    decimalConvertExceptionCount++;

                    if (decimalConvertExceptionCount <= 20)
                    {
                        ExceptionsLogger.LogError(ex);
                    }
                    else if (decimalConvertExceptionCount == 21)
                    {
                        lastDecimalLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        ExceptionsLogger.LogError("Decimal Convert Exception Count Exceeded for Grid. Suspending logging.");
                    }
                }
                catch
                { }
            }

            return returnDecimal;
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
                    if (tableInfo.filteredRecords != null && i < tableInfo.filteredRecords.Count)
                        returnDateTime = Convert.ToDateTime(PropertyList[colID].Get((tableInfo.filteredRecords[i] as Record).ObjectBound));

                    else
                    {
                        if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        {
                            returnDateTime = Convert.ToDateTime(PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound));
                        }
                    }
                }
                else
                {
                    if (tableInfo.allRecords != null && i < tableInfo.allRecords.Count)
                        returnDateTime = Convert.ToDateTime(PropertyList[colID].Get((tableInfo.allRecords[i] as Record).ObjectBound));
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (DateTime.Now.TimeOfDay.TotalMilliseconds > lastDateTimeLoggedTime + 300)
                    {
                        lastDateTimeLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        dateTimeConvertExceptionCount = 0;
                    }

                    dateTimeConvertExceptionCount++;

                    if (dateTimeConvertExceptionCount <= 20)
                    {
                        ExceptionsLogger.LogError(ex);
                    }
                    else if (dateTimeConvertExceptionCount == 21)
                    {
                        lastDateTimeLoggedTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                        ExceptionsLogger.LogError("DateTime Convert Exception Count Exceeded for Grid. Suspending logging.");
                    }
                }
                catch
                { }
            }

            return returnDateTime;
        }

        //
        //Previous Method
        //
        //protected virtual void PaintGridData(object sender, Graphics g)
        //{
        //    if (StopPaint)
        //        return;

        //    try
        //    {
        //        if (cellsToDraw.Count <= 0 && cellsUnboundToDraw.Count <= 0)
        //            return;

        //        g.CompositingQuality = CompositingQuality.HighSpeed;

        //        if (isGridPaintedOnce)
        //            stopPaint = true;

        //        g.SetClip(this.Bounds);

        //        bitmap = new Bitmap((int)g.ClipBounds.Width, (int)g.ClipBounds.Height);

        //        bitmapGraphic = Graphics.FromImage(bitmap);
        //        bitmapGraphic.CompositingQuality = CompositingQuality.HighSpeed;

        //        if (this.dataSource != null)
        //            PaintGridWithDataSourceOnUpdate(bitmapGraphic, true);
        //        else
        //            PaintGridWithMatrixOnUpdate(bitmapGraphic, true);

        //        g.DrawImageUnscaled(bitmap, new Point(0, 0));

        //        //g.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);

        //        bitmap.Dispose();
        //        bitmapGraphic.Dispose();

        //        //if (this.dataSource != null)
        //        //    PaintGridWithDataSourceOnUpdate(g, true);
        //        //else
        //        //    PaintGridWithMatrixOnUpdate(g, true);
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionsLogger.LogError(ex);
        //    }
        //    finally
        //    {
        //        isGridUpdatedOnce = true;
        //        isSortingRequired = false;
        //        stopPaint = false;
        //        resetGrid = false;
        //    }
        //}


        protected virtual void PaintGridData(object sender, Graphics g)
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

                if (this.Table.IsCustomHeader)
                {
                    if (PaintCustomHeaders != null)
                        PaintCustomHeaders(this, new PaintEventArgs(grafx.Graphics, rect));
                }

                if (this.dataSource != null)
                    PaintGridWithDataSourceOnUpdate(grafx.Graphics, true);
                else
                    PaintGridWithMatrixOnUpdate(grafx.Graphics, true);

                if (!isGridPaintedOnce || this.tableInfo.IsDirty || trasformValue != 0)
                    PaintHeaders(grafx.Graphics, rect);
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

        internal void PaintHeaders(Graphics g, Rectangle rect)
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

                if (this.gridType == GridGrouping.GridType.Virtual)
                {
                    for (int i = top; i < bottom; i++)
                    {
                        if (i <= tableInfo.sourceDataRowCount)
                        {
                            TableInfo.CellStruct structure = new TableInfo.CellStruct(i, -1, null, tableInfo.VisibleColumns[0], typeof(string));
                            structure.TextString = i.ToString();
                            this.cellsRowHeaders.Add(structure);
                        }
                    }
                }
                else
                {
                    for (int i = top; i < bottom; i++)
                    {
                        if (i <= tableInfo.RowCount)
                        {
                            TableInfo.CellStruct structure = new TableInfo.CellStruct(i, -1, null, tableInfo.VisibleColumns[0], typeof(string));
                            structure.TextString = i.ToString();
                            this.cellsRowHeaders.Add(structure);
                        }
                    }
                }

                OnQueryRowHeaderGridCells();
            }

            if (!tableInfo.HideHeader)
            {
                if (!this.Table.IsCustomHeader)
                    GridPainter.PaintHeaders(g, rect);
                else
                {
                    if (PaintCustomHeaders != null)
                        PaintCustomHeaders(this, new PaintEventArgs(grafx.Graphics, rect));
                }
            }
        }

        protected void OnQueryRowHeaderGridCells()
        {
            if (QueryRowHeaderGridCells != null)
            {
                GridQueryCellsEventArgs args = new GridQueryCellsEventArgs();
                args.CellsToUpdate = this.cellsRowHeaders;
                QueryRowHeaderGridCells(this, ref args);
            }
        }

        protected void OnQueryNestedHeaderGridCells()
        {
            if (QueryNestedHeaderGridCells != null)
            {
                GridQueryCellsEventArgs args = new GridQueryCellsEventArgs();
                args.CellsToUpdate = this.cellsNestedHeaders;
                QueryNestedHeaderGridCells(this, ref args);
            }
        }

        protected virtual void ProcessBlinkRegistry()
        {
            if (!isGridUpdatedOnce || resetGrid)
                blinkRegistry.Clear();

            List<string> itemsToRemove = null;

            if (tableInfo.AllowBlink)
            {
                foreach (BlinkInfo info in blinkRegistry.Values)
                {
                    if (info.Cell.BlinkType == BlinkType.None
                      || (DateTime.Now.TimeOfDay.TotalMilliseconds - info.Ticks) > this.blinkTime)
                    // && info.Ticks + this.blinkTime > DateTime.Now.TimeOfDay.TotalMilliseconds)
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

                        if (tableInfo.CellMatrix.GetLength(0) > info.Cell.RowIndex
                            && tableInfo.CellMatrix.GetLength(1) > info.Cell.ColIndex)
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

        protected virtual void RaiseCommonQueryCellEvents()
        {
            try
            {
                //Query Cells
                //
                OnQueryGridCells();

                OnQuerySummaryGridCells();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected void OnQuerySummaryGridCells()
        {
            if (tableInfo.AllowSummaryRows && QuerySummaryGridCells != null)
            {
                GridQueryCellsEventArgs args = new GridQueryCellsEventArgs();
                args.CellsToUpdate = this.cellsSummaryToUpdate;
                QuerySummaryGridCells(this, args);
            }
        }

        internal void OnQueryGridCells(List<TableInfo.CellStruct> cells, bool updateGrid)
        {
            if (this.QueryGridCells != null || this.QueryGridCell != null)
            {
                GridQueryCellsEventArgs args = new GridQueryCellsEventArgs();
                args.CellsToUpdate = cells;

                if (QueryGridCells != null)
                    QueryGridCells(this, args);
                else
                    QueryGridCellIndependantly(this, args);
            }

        }

        private void QueryGridCellIndependantly(object sender, GridQueryCellsEventArgs e)
        {
            if (e == null || e.CellsToUpdate == null || e.CellsToUpdate.Count == 0)
                return;

            if (this.Table == null)
                return;

            for (int i = 0; i < e.CellsToUpdate.Count; i++)
            {
                TableInfo.CellStruct structure = e.CellsToUpdate[i];

                if (GridType != GridType.Virtual)
                {
                    if (structure.Column == null || structure.Record == null)
                        continue;
                }

                try
                {
                    if (QueryGridCell != null)
                        QueryGridCell(this, structure);
                }
                catch (Exception ex)
                {
                    ExceptionsLogger.LogError(ex);
                }
            }

        }

        protected void OnQueryGridCells()
        {
            if (this.QueryGridCells != null || this.QueryGridCell != null)
            {
                GridQueryCellsEventArgs args = new GridQueryCellsEventArgs();
                args.CellsToUpdate = this.cellsToUpdate;

                if (dataSource == null && this.gridType == GridGrouping.GridType.Virtual)
                    args.CellsToUpdate = this.cellsUnboundToUpdate;

                if (this.QueryGridCells != null)
                    this.QueryGridCells(this, args);
                else
                    QueryGridCellIndependantly(this, args);

                if (dataSource == null)
                {
                    for (int i = 0; i < cellsUnboundToUpdate.Count; i++)
                    {
                        TableInfo.CellStruct structure = cellsUnboundToUpdate[i];
                        SetCellMatrixStyle(structure.RowIndex, structure.ColIndex, structure.Style);
                    }
                }
            }
        }

        protected void OnQueryGridRowCount()
        {
            try
            {
                if (QueryGridRowCount != null)
                    QueryGridRowCount(this, null);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        internal virtual void QueryUnboundData()
        {
            int bottomRow = this.tableInfo.BottomRow;

            for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
            {
                if (tableInfo.VisibleColumns[j].Left > this.Width)
                    continue;

                if (tableInfo.CellMatrix.GetLength(1) <= j)
                    continue;

                tableInfo.VisibleColumns[j].CurrentPosition = j;

                if (this.dataSource != null)
                {
                    if (tableInfo.VisibleColumns[j].IsUnbound)
                    {
                        for (int i = 0; i < dataSource.Count; i++)
                        {
                            if (tableInfo.allRecords.Count <= i)
                                continue;

                            if (tableInfo.allRecords[i] != null)
                                (tableInfo.allRecords[i] as Record).CurrentIndex = i;

                            if (UpdateRecordIndex(i, j) && !tableInfo.FilterManager.IsFilterAvailable(tableInfo.VisibleColumns[j].Name))
                            {
                                this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                            }
                        }
                    }
                }
            }

            OnQueryGridCells();
        }

        protected virtual void CheckCellsToUpdate(bool mainRefresh)
        {
            if (((tableInfo.IsFilterEnabled && tableInfo.filteredRecords.Count <= 0) && GridType != GridType.Virtual) || StopPaint || tableInfo.VisibleColumns.Count == 0)
                return;

            if (dataSource == null)
            {
                OnQueryGridRowCount();
            }

            int bottomRow = this.tableInfo.BottomRow;
            TableInfo.TableColumn currentColumn = null;

            for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
            {
                currentColumn = tableInfo.VisibleColumns[j];

                if (currentColumn.Left > this.Width)
                    continue;

                if (tableInfo.CellMatrix.GetLength(1) <= j)
                    continue;

                currentColumn.CurrentPosition = j;

                if (this.dataSource != null)
                {
                    for (int i = this.tableInfo.TopRow; i < bottomRow + 1; i++)
                    {
                        if (tableInfo.CellMatrix.GetLength(0) > i)
                            if (tableInfo.CellMatrix[i, j].CellModelType == CellType.Summary)
                                this.cellsSummaryToUpdate.Add(tableInfo.CellMatrix[i, j]);
                    }

                    if (!currentColumn.IsCustomColumn)
                    {
                        if (currentColumn.IsUnbound)
                        {
                            for (int i = 0; i < dataSource.Count; i++)
                            {
                                if (tableInfo.allRecords.Count <= i)
                                    continue;

                                if (tableInfo.allRecords[i] != null)
                                    (tableInfo.allRecords[i] as Record).CurrentIndex = i;

                                if (UpdateRecordIndex(i, j) && !tableInfo.FilterManager.IsFilterAvailable(tableInfo.VisibleColumns[j].Name))
                                {
                                    this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                                }
                            }
                        }
                        else if (currentColumn.QueryStyle /*&& !currentColumn.IsCustomFormulaColumn*/)
                        {
                            for (int i = 0; i < dataSource.Count; i++)
                            {
                                if (tableInfo.allRecords.Count <= i)
                                    continue;

                                (tableInfo.allRecords[i] as Record).CurrentIndex = i;

                                if (i >= tableInfo.TopRow && i < bottomRow + 1)
                                {
                                    if (UpdateRecordIndex(i, j))
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
                        else /*if (!currentColumn.IsCustomFormulaColumn)*/
                        {
                            for (int i = 0; i < dataSource.Count; i++)
                            {
                                if (tableInfo.allRecords.Count <= i)
                                    continue;
                                if (tableInfo.allRecords[i] != null)
                                    (tableInfo.allRecords[i] as Record).CurrentIndex = i;

                                if (i >= tableInfo.TopRow && i < bottomRow + 1)
                                {
                                    if (UpdateRecordIndex(i, j))
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
                        //
                        //Custom Columns
                        //
                        for (int i = 0; i < dataSource.Count; i++)
                        {
                            if (tableInfo.allRecords.Count <= i)
                                continue;

                            if (tableInfo.allRecords[i] != null)
                                (tableInfo.allRecords[i] as Record).CurrentIndex = i;

                            if (i >= tableInfo.TopRow && i < bottomRow + 1)
                            {
                                if (UpdateRecordIndex(i, j))
                                {
                                    //
                                    //TODO : Should check for Dirty ?
                                    //
                                    //if (tableInfo.CellMatrix[i, j].IsDirty || mainRefresh)
                                    //{

                                    if (!currentColumn.IsUnbound)
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
                                    //}

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
                    //    for (int i = tableInfo.TopRow; i < bottomRow + 1; i++)
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

                    //if (currentColumn.IsCustomFormulaColumn)
                    //{
                    //    for (int i = 0; i < dataSource.Count; i++)
                    //    {
                    //        if (tableInfo.allRecords.Count <= i)
                    //            continue;
                    //        if (tableInfo.allRecords[i] != null)
                    //            (tableInfo.allRecords[i] as Record).CurrentIndex = i;

                    //        if (i >= tableInfo.TopRow)
                    //        {
                    //            if (UpdateRecordIndex(i, j))
                    //            {
                    //                //if (currentColumn.Table.ExpressionFields[currentColumn.Name] != null && (mainRefresh || tableInfo.CellMatrix[i, j].UpdateCustomFormula || tableInfo.CellMatrix[i, j].IsEmpty))
                    //                //{
                    //                //    //
                    //                //    //TODO : Need to recheck this method and do optimizations
                    //                //    //
                    //                //    //if (resetGrid || mainRefresh || ShouldUpdateValue(i, j, currentColumn.Name))
                    //                //    {
                    //                //        SetValue(ref tableInfo.CellMatrix[i, j], tableInfo.CellMatrix[i, j].CellStructType, -2);

                    //                //        if (tableInfo.CellMatrix[i, j].IsDirty || resetGrid || mainRefresh && tableInfo.IsSortingEnabled)
                    //                //        {
                    //                //            if (tableInfo.SortedColumnDescriptors[currentColumn.MappingName] != null || this.tableInfo.IsDirty)
                    //                //                isSortingRequired = true;
                    //                //        }

                    //                //        this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                    //                //        tableInfo.CellMatrix[i, j].UpdateCustomFormula = false;

                    //                //        if (tableInfo.AllowBlink && isGridUpdatedOnce)
                    //                //            AddToBlinkRegistry(i, j);
                    //                //    }

                    //                //    if (!isGridUpdatedOnce || mainRefresh || this.tableInfo.IsDirty)
                    //                //        tableInfo.CellMatrix[i, j].IsBlinkCell = false;
                    //                //}
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
        }

        private bool ShouldUpdateValue(int i, int j, string name)
        {
            Record rec = tableInfo.CellMatrix[i, j].Record;
            object obj = null;

            foreach (string col in rec.CustomFormulaKeys.Keys)
            {
                if (col == name)
                {
                    foreach (TableInfo.TableColumn column in rec.CustomFormulaKeys[col].Keys)
                    {
                        obj = rec.GetValue(column.Name);

                        if (!obj.Equals(rec.CustomFormulaKeys[col][column]) || rec.CustomFormulaKeys[col][column] == null)
                        {
                            rec.CustomFormulaKeys[col][column] = obj;
                            return true;
                        }
                    }

                    break;
                }
            }

            return false;
        }

        public virtual void PopulateAllCellValues(bool isCustom = false)
        {
            if ((tableInfo.IsFilterEnabled && tableInfo.FilteredRecords.Count <= 0) || StopPaint || tableInfo.VisibleColumns.Count == 0)
                return;

            bool mainRefresh = true;

            if (dataSource == null)
            {
                OnQueryGridRowCount();
            }

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
                            for (int i = 0; i < dataSource.Count; i++)
                            {
                                if (tableInfo.AllRecords.Count <= i)
                                    continue;

                                if (tableInfo.AllRecords[i] != null)
                                    (tableInfo.AllRecords[i] as Record).CurrentIndex = i;

                                if (UpdateRecordIndex(i, j) && !tableInfo.FilterManager.IsFilterAvailable(tableInfo.VisibleColumns[j].Name))
                                {
                                    this.cellsToUpdate.Add(tableInfo.CellMatrix[i, j]);
                                }
                            }
                        }
                        else if (tableInfo.VisibleColumns[j].QueryStyle)
                        {
                            for (int i = 0; i <= dataSource.Count; i++)
                            {
                                if (tableInfo.AllRecords.Count <= i)
                                    continue;

                                (tableInfo.AllRecords[i] as Record).CurrentIndex = i;

                                if (UpdateRecordIndex(i, j))
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
                            for (int i = 0; i < dataSource.Count; i++)
                            {
                                if (tableInfo.AllRecords.Count <= i)
                                    continue;
                                if (tableInfo.AllRecords[i] != null)
                                    (tableInfo.AllRecords[i] as Record).CurrentIndex = i;

                                if (UpdateRecordIndex(i, j))
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
                        //
                        //Custom Columns
                        //
                        for (int i = 0; i < dataSource.Count; i++)
                        {
                            if (tableInfo.AllRecords.Count <= i)
                                continue;

                            if (tableInfo.AllRecords[i] != null)
                                (tableInfo.AllRecords[i] as Record).CurrentIndex = i;

                            if (UpdateRecordIndex(i, j))
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

                        for (int i = tableInfo.TopRow; i < this.tableInfo.BottomRow + 1; i++)
                        {
                            if (tableInfo.sourceDataRowCount == -1 || i < tableInfo.sourceDataRowCount)
                                this.cellsUnboundToUpdate.Add(new TableInfo.CellStruct(index, j, i, null, tableInfo.VisibleColumns[j], null));

                            index++;
                        }
                    }
                }
            }

            if (isCustom == false)
                RaiseCommonQueryCellEvents();
        }

        protected virtual bool UpdateRecordIndex(int row, int column)
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

                tableInfo.CellMatrix[row, column].Record = tableInfo.allRecords[row] as Record;
                if (tableInfo.allRecords[row] != null)
                    (tableInfo.allRecords[row] as Record).SourceIndex = row;
            }

            return true;
        }

        protected virtual void AddToBlinkRegistry(int rowIndex, int colIndex)
        {
            if ((this.tableInfo.IsDirty || isMainPaint) && this.GridType != GridGrouping.GridType.Virtual)
                return;

            if (tableInfo.CellMatrix[rowIndex, colIndex].IsBlinkCell && tableInfo.CellMatrix[rowIndex, colIndex].CellModelType != CellType.Summary)
            {
                if (!this.blinkRegistry.ContainsKey(tableInfo.CellMatrix[rowIndex, colIndex].Key))
                {
                    this.blinkRegistry.Add(tableInfo.CellMatrix[rowIndex, colIndex].Key, new BlinkInfo(tableInfo.CellMatrix[rowIndex, colIndex]));
                }
                else
                {
                    BlinkInfo info = blinkRegistry[tableInfo.CellMatrix[rowIndex, colIndex].Key];

                    if (info.Type != tableInfo.CellMatrix[rowIndex, colIndex].BlinkType)
                        info.Ticks = DateTime.Now.TimeOfDay.TotalMilliseconds;

                    info.Type = tableInfo.CellMatrix[rowIndex, colIndex].BlinkType;
                    info.Cell.BlinkType = tableInfo.CellMatrix[rowIndex, colIndex].BlinkType;
                }
            }
        }

        public virtual void SetCellDirty(int rowNumber, int colNumer)
        {
            try
            {
                if (this.tableInfo.CellMatrix.GetLength(0) > 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0))
                {
                    tableInfo.CellMatrix[rowNumber, colNumer].IsDirty = true;

                    if (tableInfo.CellMatrix[rowNumber, colNumer].Column.AllowEqualValueBlink)
                    {
                        tableInfo.CellMatrix[rowNumber, colNumer].IsBlinkCell = true;
                        tableInfo.CellMatrix[rowNumber, colNumer].BlinkType = BlinkType.Equal;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void SetRowDirty(int rowNumber)
        {
            try
            {
                if (this.tableInfo.CellMatrix.GetLength(0) > 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0))
                {
                    for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
                    {
                        tableInfo.CellMatrix[rowNumber, j].IsDirty = true;

                        if (tableInfo.CellMatrix[rowNumber, j].Column.AllowEqualValueBlink)
                        {
                            tableInfo.CellMatrix[rowNumber, j].IsBlinkCell = true;
                            tableInfo.CellMatrix[rowNumber, j].BlinkType = BlinkType.Equal;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void SetEntryDirty(object obj)
        {
            SetEntryDirty(obj, false);
        }

        public virtual void SetEntryDirty(object obj, bool updateValue)
        {
            try
            {
                int rowNumber = -1;

                Record rec = this.tableInfo.AllRecords.GetRecordFromObject(obj);

                if (rec == null)
                    return;

                //
                //GetRow from Sorted list
                //
                if (tableInfo.IsFilterEnabled)
                {
                    if (this.tableInfo.FilteredRecords.Contains(rec))
                        rowNumber = this.tableInfo.FilteredRecords.IndexOf(rec);
                }
                else
                    rowNumber = this.tableInfo.AllRecords.IndexOf(rec);

                if (rowNumber == -1 || rowNumber > tableInfo.BottomRow)
                    return;

                if (this.tableInfo.CellMatrix.GetLength(0) > 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0))
                {
                    for (int j = 0; j < tableInfo.VisibleColumns.Count; j++)
                    {
                        if (updateValue)
                        {
                            UpdateCell(rowNumber, j, false);
                        }
                        else
                            tableInfo.CellMatrix[rowNumber, j].IsDirty = true;

                        if (tableInfo.CellMatrix[rowNumber, j].Column.AllowEqualValueBlink)
                        {
                            tableInfo.CellMatrix[rowNumber, j].IsBlinkCell = true;
                            tableInfo.CellMatrix[rowNumber, j].BlinkType = BlinkType.Equal;
                            AddToBlinkRegistry(rowNumber, j);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void SetEntryDirty(object obj, string colName)
        {
            if (this.tableInfo.VisibleColumns.Count <= 0)
                return;

            TableInfo.TableColumn col = tableInfo.GetVisibleColumnFromName(colName);

            if (col == null)
                return;

            int colIndex = col.CurrentPosition;
            SetEntryDirty(obj, colIndex);
        }

        public virtual void SetEntryDirty(object obj, int colIndex)
        {
            try
            {
                int rowNumber = -1;

                Record rec = this.tableInfo.AllRecords.GetRecordFromObject(obj);

                if (rec == null)
                    return;

                //
                //GetRow from Sorted list
                //
                if (tableInfo.IsFilterEnabled)
                {
                    if (this.tableInfo.FilteredRecords.Contains(rec))
                        rowNumber = this.tableInfo.FilteredRecords.IndexOf(rec);
                }
                else
                    rowNumber = this.tableInfo.AllRecords.IndexOf(rec);

                if (rowNumber == -1 || rowNumber > tableInfo.BottomRow)
                    return;

                if (this.tableInfo.CellMatrix.GetLength(0) > 0 && rowNumber < this.tableInfo.CellMatrix.GetLength(0)
                    && colIndex < this.tableInfo.CellMatrix.GetLength(1))
                {
                    tableInfo.CellMatrix[rowNumber, colIndex].IsDirty = true;

                    if (tableInfo.CellMatrix[rowNumber, colIndex].Column != null && tableInfo.CellMatrix[rowNumber, colIndex].Column.AllowEqualValueBlink)
                    {
                        tableInfo.CellMatrix[rowNumber, colIndex].IsBlinkCell = true;
                        tableInfo.CellMatrix[rowNumber, colIndex].BlinkType = BlinkType.Equal;
                        AddToBlinkRegistry(rowNumber, colIndex);
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        #endregion

        #region UI Events

        protected virtual void resumeLayoutTimer_Tick_1(object sender, EventArgs e)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;
            StopPaint = false;
            resumeLayoutTimer.Stop();
            this.Invalidate();
        }

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
                {
                    if (!this.Table.IsCustomHeader)
                        GridPainter.PaintHeaders(grafx.Graphics, rect);
                    else
                    {
                        if (PaintCustomHeaders != null)
                            PaintCustomHeaders(this, new PaintEventArgs(grafx.Graphics, rect));
                    }
                }
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

        public int LastClickedColumn = -1;

        private void ProcessMouseDown(MouseEventArgs e)
        {
            LastClickedColumn = -1;
            var X = (e.X / tableInfo.GetScale()) + tableInfo.XOffset;

            for (int i = 0; i < tableInfo.VisibleColumnCount; i++)
            {
                if ((X >= tableInfo.VisibleColumns[i].Left + tableInfo.XOffset) & (X < (tableInfo.VisibleColumns[i].Left + tableInfo.Grid.horizontalOffset + tableInfo.VisibleColumns[i].CellWidth)))
                {
                    LastClickedColumn = i;
                }
            }

            return;

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

        protected virtual void GridGroupingControl_MouseDown(object sender, MouseEventArgs e)
        {
            //var arg = new MouseEventArgs(e.Button, e.Clicks, e.X * 2, e.Y * 2, e.Delta);
            ProcessMouseDown(e);

            try
            {
                if (this.gridType == GridGrouping.GridType.Virtual)
                    Table.ProcessMouseDown(tableInfo.SourceDataRowCount, e);
                else
                    Table.ProcessMouseDown(TotalRecordCount, e);

                HScroll.ShowScrollBar();

                VScroll.ShowScrollBar();

            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }


        private void GridGroupingControl_MouseLeave(object sender, EventArgs e)
        {
            if (Table.MouseRowNo == -2)//Resizing
                Table.ProcessMouseLeave();
        }

        protected virtual void GridGroupingControl_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //var arg = new MouseEventArgs(e.Button, e.Clicks, e.X * 2, e.Y * 2, e.Delta);

            try
            {
                if (!Table.IsDragging)
                {
                    if (this.gridType == GridGrouping.GridType.Virtual)
                        Table.ProcessMouseUp(tableInfo.SourceDataRowCount, e);
                    else
                        Table.ProcessMouseUp(TotalRecordCount, e);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected virtual void GridGroupingControl_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!Table.IsDragging)
                {
                    Table.ProcessMouseMove(e);
                }

            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void Table_OnDraggingChanged(object sender, EventArgs e)
        {
            try
            {
                if (Table.IsDragging == true)
                {
                    this.MouseUp -= new System.Windows.Forms.MouseEventHandler(GridGroupingControl_MouseUp);

                    MouseHandler.OnMouseUp -= new MouseEventHandler(MouseHandler_OnMouseUp);
                    MouseHandler.OnMouseUp += new MouseEventHandler(MouseHandler_OnMouseUp);
                }
                else
                {
                    MouseHandler.OnMouseUp -= new MouseEventHandler(MouseHandler_OnMouseUp);

                    System.Windows.Forms.Timer delayedTimer = new System.Windows.Forms.Timer();
                    delayedTimer.Interval = 100;
                    delayedTimer.Tick += (s1, e1) =>
                    {
                        delayedTimer.Stop();

                        this.MouseUp -= new System.Windows.Forms.MouseEventHandler(GridGroupingControl_MouseUp);
                        this.MouseUp += new System.Windows.Forms.MouseEventHandler(GridGroupingControl_MouseUp);
                    };
                    delayedTimer.Start();
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void MouseHandler_OnMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (Table.IsDragging)
                {
                    if (MouseButtons != System.Windows.Forms.MouseButtons.Left)
                        Table.ProcessMouseLeave();

                    Point location = this.PointToScreen(new Point(0, 0));
                    Point newLocation = new Point(e.Location.X - location.X, e.Location.Y - location.Y);

                    e = new MouseEventArgs(System.Windows.Forms.MouseButtons.Left, 0, newLocation.X, newLocation.Y, 0);

                    Table.ProcessMouseMove(e);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void MouseHandler_OnMouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                if (Table.IsDragging)
                {
                    Point location = this.PointToScreen(new Point(0, 0));
                    Point newLocation = new Point(e.Location.X - location.X, e.Location.Y - location.Y);

                    e = new MouseEventArgs(e.Button, e.Clicks, newLocation.X, newLocation.Y, e.Delta);

                    if (this.gridType == GridGrouping.GridType.Virtual)
                        Table.ProcessMouseUp(tableInfo.SourceDataRowCount, e);
                    else
                        Table.ProcessMouseUp(TotalRecordCount, e);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }


        protected virtual void GridGroupingControl_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //var arg = new MouseEventArgs(e.Button, e.Clicks, e.X * 2, e.Y * 2, e.Delta);

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

        protected virtual void GridGroupingControl_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
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

        protected virtual void GridGroupingControl_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Table.ProcessKeyUp(e);
        }

        protected virtual void GridGroupingControl_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            Table.ProcessKeyDown(e);
        }

        protected virtual void LoadSettings()
        {
            try
            {
                Table.RowHeight = 15;
                Table.HeaderHeight = Table.RowHeight + 5;

                this.isFormLoaded = true;
                this.SetBounds(0, 0, 0, 0);

                this.editTextBox = new EditTextBox();
                this.Controls.Add(editTextBox);
                this.editTextBox.Visible = false;
                //dataThread = new Thread(DoDataCalculationsNew);
                //dataThread.Priority = ThreadPriority.Lowest;
                //dataThread.IsBackground = true;
                //dataThread.Start();



                // Retrieves the BufferedGraphicsContext for the 
                // current application domain.
                context = BufferedGraphicsManager.Current;

                // Sets the maximum size for the primary graphics buffer
                // of the buffered graphics context for the application
                // domain.  Any allocation requests for a buffer larger 
                //than this will create a temporary buffered graphics 
                //context to host the graphics buffer.
                context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);

                // Allocates a graphics buffer the size of this form
                // using the pixel format of the Graphics created by 
                // the Form.CreateGraphics() method, which returns a 
                // Graphics object that matches the pixel format of the form.
                grafx = context.Allocate(this.CreateGraphics(),
                     new Rectangle(0, 0, this.Width, this.Height));

                //
                // Draw the first frame to the buffer.
                //
                DrawToBuffer(grafx.Graphics);

                this.Resize -= new EventHandler(OnGridResize);
                this.Resize += new EventHandler(OnGridResize);

                //
                //Starting auto refresh timer
                //
                if (this.refreshMode == GridRefreshMode.AutoRefresh)
                    tmrRefresh.Start();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                //ExceptionsLogger.LogError("Grid Settings Loaded Successfully.");
            }
        }

        public virtual void SetEditTextBox(TextBox textBox)
        {
            this.editTextBox = textBox;
            if (!this.Controls.Contains(editTextBox))
                this.Controls.Add(editTextBox);
            this.editTextBox.Visible = false;
        }

        public void DisplayEditTextAt(int rowIndex, int colIndex)
        {
            DisplayEditTextBox(rowIndex, colIndex, Point.Empty);
        }

        internal virtual void DisplayEditTextBox(int rowIndex, int colIndex, Point location)
        {
            if (!isEditable)
                return;

            try
            {
                if (colIndex < 0 || rowIndex < 0)
                    return;

                if (location != Point.Empty && !OnDisplayTextBox(rowIndex, colIndex, location))
                    return;

                if (!this.Table.VisibleColumns[colIndex].IsEditColumn)
                    return;

                //editTextBox.ShowDropDown = false;

                Rect.X = tableInfo.Columns[tableInfo.MouseColNo].Left + 2;
                Rect.Y = tableInfo.HeaderHeight + (tableInfo.MouseRowNo - tableInfo.TopRow) * tableInfo.RowHeight + 1 + trasformValue;
                Rect.Width = tableInfo.VisibleColumns[tableInfo.MouseColNo].CellWidth + 2;
                Rect.Height = tableInfo.RowHeight - 2;
                editTextBox.SetBounds(Rect.X, Rect.Y, Rect.Width - 4, Rect.Height);

                TableInfo.CellStruct cell = tableInfo.CellMatrix[tableInfo.MouseRowNo, tableInfo.MouseColNo];

                GridStyleInfo style = cell.Style;

                string strText = GetValueAsString(cell, cell.CellStructType, style);

                editTextBox.Text = strText;
                //editTextBox.Font = FontData;
                //editTextBox.ShowSearchImage();
                editTextBox.Visible = true;
                editTextBox.Select(0, editTextBox.Text.Length);
                editTextBox.Focus();
                editTextBox.BringToFront();

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

        protected bool OnDisplayTextBox(int rowIndex, int colIndex, Point location)
        {
            if (DisplayTextBox != null)
            {
                DisplayTextBoxEventArgs args = new DisplayTextBoxEventArgs(rowIndex, colIndex, location);
                DisplayTextBox(editTextBox, args);

                return !args.Cancel;
            }

            return true;
        }

        internal virtual void HideEditTextBox()
        {
            if (!isEditable)
                return;
            try
            {
                editTextBox.Visible = false;
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

        protected virtual void InitializeColumnResize(bool enable)
        {
            try
            {
                if (!TurnOnAutoResizeGridColumns)
                    return;

                if (tableInfo == null || this.gridType == GridGrouping.GridType.MultiColumn)
                    return;

                if (tableInfo.VisibleColumns.Count == 0)
                {
                    return;
                }

                designTimeWidths.Clear();
                this.Table.VisibleColumns.Changed -= new ListPropertyChangedEventHandler(OnVisibleColumnsChanged);
                this.Resize -= new EventHandler(GridGroupingControl_Resize);

                if (resizeTimer != null)
                {
                    resizeTimer.Tick -= new EventHandler(delegate (object timerObject, EventArgs ev)
                    {
                        resizeTimer.Stop();
                        GridColumnResize(this, false);
                    });

                    resizeTimer.Dispose();
                }

                if (enable)
                {
                    this.Table.VisibleColumns.Changed += new ListPropertyChangedEventHandler(OnVisibleColumnsChanged);
                    this.Resize += new EventHandler(GridGroupingControl_Resize);

                    resizeTimer = new System.Windows.Forms.Timer();
                    resizeTimer.Interval = 100;

                    resizeTimer.Tick += new EventHandler(delegate (object timerObject, EventArgs ev)
                    {
                        resizeTimer.Stop();
                        GridColumnResize(this, false);
                    });

                    if (designTimeWidths.Count > 0)
                        GridColumnResize(this, false);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public virtual void GridColumnResize(GridGroupingControl grid, bool isSetDesignWidth)
        {
            GridColumnResize(grid, isSetDesignWidth, false);
        }

        private int GetDPIAdjestedValue(double value)
        {
            return (int)(value * (grafx.Graphics.DpiX / 100));
        }

        public virtual void ResizeColumnsToFit()
        {
            try
            {
                PopulateAllCellValues();

                int totWidth = 0;

                for (int i = 0; i < this.Table.VisibleColumnCount; i++)
                {
                    int maxWidth = 0;
                    int countUpTo = 0;

                    if (Table.VisibleColumns[i].CellType == TableInfo.CellStructType.Style)
                    {
                        SizeF strSize = grafx.Graphics.MeasureString(Table.VisibleColumns[i].DisplayName, Table.TableStyle.GdipHeaderFont);

                        if (strSize.Width > 400) strSize.Width = 400;

                        if (maxWidth < strSize.Width)
                        {
                            maxWidth = (int)strSize.Width;
                        }

                        if (maxWidth <= 0)
                            maxWidth = Table.VisibleColumns[i].DesignWidth;
                        else
                            maxWidth += 20;

                        this.Table.VisibleColumns[i].CellWidth = maxWidth;

                        continue;
                    }

                    if (Table.RowCount > 60)
                    {
                        countUpTo = 60;
                    }
                    else
                    {
                        countUpTo = Table.RowCount;
                    }

                    for (int j = -1; j < countUpTo; j++)
                    {
                        string strObject = null;
                        Font strFont = null;
                        float extraWidth = 0;

                        if (j > -1 && j < Table.CellMatrix.GetLength(0))
                        {
                            strObject = GetCellMatrixObject(j, i).ToString();

                            if (Table.CellMatrix[j, i].CellStructType == TableInfo.CellStructType.Double
                                || Table.CellMatrix[j, i].CellStructType == TableInfo.CellStructType.Long
                                || Table.CellMatrix[j, i].CellStructType == TableInfo.CellStructType.Integer
                                || Table.CellMatrix[j, i].CellStructType == TableInfo.CellStructType.Decimal
                                )
                            {
                                try
                                {
                                    double dblVal = 0;

                                    if (double.TryParse(strObject.ToString(), out dblVal))
                                    {
                                        if (dblVal >= double.MaxValue || dblVal <= double.MinValue)
                                        {
                                            strObject = "";
                                        }
                                    }
                                    else
                                    {
                                        strObject = "";
                                    }
                                }
                                catch
                                {
                                    strObject = "";
                                }

                            }

                            strFont = Table.TableStyle.GdipFont;
                        }
                        else
                        {
                            strFont = Table.TableStyle.GdipHeaderFont;
                            strObject = Table.VisibleColumns[i].DisplayName;

                            foreach (var item in tableInfo.SortedColumnDescriptors)
                            {
                                if (item.Name == Table.VisibleColumns[i].MappingName)
                                {
                                    extraWidth = GetDPIAdjestedValue(20f);
                                }
                            }
                        }

                        SizeF strSize = new SizeF();

                        if (!string.IsNullOrEmpty(Table.VisibleColumns[i].CellModelType) && j >= 0)
                        {
                            if (QueryRenderCellWidth != null)
                            {
                                if (j < Table.CellMatrix.GetLength(0) && i < Table.CellMatrix.GetLength(1))
                                    strSize = QueryRenderCellWidth(Table.CellMatrix[j, i], grafx, strObject);
                            }
                        }

                        if (strSize.Width == 0)
                        {
                            strSize = grafx.Graphics.MeasureString(strObject, strFont);
                        }

                        strSize.Width += extraWidth;

                        if (strSize.Width > GetDPIAdjestedValue(400)) strSize.Width = GetDPIAdjestedValue(400);

                        if (maxWidth < strSize.Width)
                        {
                            maxWidth = (int)strSize.Width;
                        }
                    }

                    if (maxWidth <= 0)
                        maxWidth = Table.VisibleColumns[i].DesignWidth;
                    else
                        maxWidth += GetDPIAdjestedValue(20);



                    this.Table.VisibleColumns[i].CellWidth = (maxWidth);

                    totWidth += (maxWidth);
                }

                DoVeryFullRefresh = true;

                RTLAwarePosition(0);
                Table.ProcessMouseUp(0, new MouseEventArgs(System.Windows.Forms.MouseButtons.None, 0, 0, 0, 0));
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void RTLAwarePosition(int val)
        {
            if (Width < tableInfo.TotalVisibleColumnWidth && this.Table.Culture.TextInfo.IsRightToLeft)
            {
                HScroll.SetRTLAwarePosition(val);
                HorizontalScroll_ValueChanged(this, new EventArgs());
                //grafx.Graphics.ResetTransform();
                //horizontalOffset = tableInfo.TotalVisibleColumnWidth - this.Width;
            }
            else
            {
                HScroll.Value = 0;
            }

        }

        public virtual void GridColumnResize(GridGroupingControl grid, bool isSetDesignWidth, bool forceResize)
        {
            if ((!TurnOnAutoResizeGridColumns || suspendAutoResizeColumns || !EnableAutoResizeGridColumns) && !forceResize)
                return;

            if (grid == null)
            {
                return;
            }

            if (!forceResize && grid.GridType == GridGrouping.GridType.MultiColumn)
                return;

            if (grid.Table.VisibleColumns.Count == 0)
            {
                return;
            }

            designTimeTotalWidth = 0;

            foreach (KeyValuePair<string, int> pair in designTimeWidths)
            {
                designTimeTotalWidth += pair.Value;
            }

            if (!IsMirrored)
            {
                horizontalOffset = 0;
                tableInfo.XOffset = 0;
            }

            double minWidth = 0.0;

            if (autoResizeMinimumWidth <= 0)
                minWidth = (designTimeTotalWidth * 3) / 4;
            else
                minWidth = autoResizeMinimumWidth;

            if ((int)minWidth >= grid.Width && !forceResize)
            {
                return;
            }

            isGridColumnResizeInProgress = true;
            int currentTotalColumnWidth = 0;
            exactWidthOfAllColumns = 0.0;

            int gridWidth = grid.Width - grid.Table.RowHeaderWidth;
            //
            //TODO
            //
            int borderConst = 0;
            int fixedWidth = 0;

            //switch (grid.BorderStyle)
            //{
            //    case BorderStyle.FixedSingle:
            //        borderConst = 2;
            //        break;
            //    case BorderStyle.None:
            //        borderConst = 0;
            //        break;
            //    case BorderStyle.Fixed3D:
            //        borderConst = 4;
            //        break;
            //    default:
            //        borderConst = 0;
            //        break;
            //}

            TableColumnCollection visibleColumns = grid.Table.VisibleColumns;
            TableColumnCollection columns = grid.Table.Columns;

            Dictionary<string, double> tempCoumnWidths = new Dictionary<string, double>();

            try
            {
                for (int i = 0; i < visibleColumns.Count; i++)
                {
                    if (isSetDesignWidth)
                    {
                        columns[visibleColumns[i].Name].CellWidth = visibleColumns[visibleColumns[i].Name].DesignWidth;
                    }

                    currentTotalColumnWidth += visibleColumns[visibleColumns[i].Name].CellWidth;

                    if (!exactColumnWidths.ContainsKey(visibleColumns[i].Name))
                    {
                        if (!tempCoumnWidths.ContainsKey(visibleColumns[i].Name))
                        {
                            tempCoumnWidths.Add(visibleColumns[i].Name, (double)visibleColumns[visibleColumns[i].Name].CellWidth);
                        }
                    }
                    else
                        tempCoumnWidths[visibleColumns[i].Name] = exactColumnWidths[visibleColumns[i].Name];
                }

                exactColumnWidths = tempCoumnWidths;

                if (currentTotalColumnWidth == 0)
                {
                    return;
                }

                //checking to see whether the vertical scroll bar is visible or not...
                //if (grid.Table.VScrollBar.InnerScrollBar != null)
                //{
                int verticalScrollWidth = 0;// SystemInformation.VerticalScrollBarWidth;

                //if (grid.TableControl.UseSmallScrollBars)
                //    verticalScrollWidth = 13;

                fixedWidth = verticalScrollWidth + borderConst;
                //}
                //else
                //{
                //    fixedWidth = borderConst;
                //}

                //if (grid.TableDescriptor.GroupedColumns.Count > 0)
                //{
                //    fixedWidth += grid.TableDescriptor.GroupedColumns.Count * grid.Table.DefaultIndentWidth;
                //}
                //if (grid.TableDescriptor.Relations.Count > 0)
                //{
                //    fixedWidth += grid.Table.DefaultIndentWidth;
                //}

                if (currentTotalColumnWidth > gridWidth)
                {
                    int availableWidth = (currentTotalColumnWidth + fixedWidth) - gridWidth;

                    //
                    //Return if available width is too much
                    //
                    if (availableWidth > gridWidth * 3)
                        return;

                    int width = 0;
                    for (int i = 0; i < visibleColumns.Count; i++)
                    {
                        if (!columns.Contains(visibleColumns[i].Name))
                            continue;

                        if (i != (visibleColumns.Count - 1))
                        {
                            double shrinkFactor = (columns[visibleColumns[i].Name].CellWidth * availableWidth) / currentTotalColumnWidth;
                            int newWidth = 0;

                            if (autoResizeMinimumWidth <= 0)
                                newWidth = (int)Math.Max((columns[visibleColumns[i].Name].CellWidth) - ((int)Math.Ceiling(shrinkFactor) + 1), ((columns[visibleColumns[i].Name].DesignWidth * 3) / 4));
                            else
                                newWidth = ((columns[visibleColumns[i].Name].CellWidth) - ((int)Math.Ceiling(shrinkFactor) + 1));

                            if (columns[visibleColumns[i].Name].IsAutoResized)
                                columns[visibleColumns[i].Name].CellWidth = newWidth;
                            if (visibleColumns[visibleColumns[i].Name].IsAutoResized)
                                visibleColumns[visibleColumns[i].Name].CellWidth = newWidth;

                            width += newWidth;
                        }
                        else if (i == (visibleColumns.Count - 1))
                        {
                            TableInfo.TableColumn col = columns[visibleColumns[i].Name];

                            if (col != null && col.IsAutoResized)
                                col.CellWidth = gridWidth - (width + fixedWidth + 1);

                            col = visibleColumns[visibleColumns[i].Name];

                            if (col.IsAutoResized)
                                col.CellWidth = gridWidth - (width + fixedWidth + 1);
                        }

                    }
                }
                else if (currentTotalColumnWidth < gridWidth)
                {
                    int width = 0;
                    double truncatedValues = 0.0;

                    for (int i = 0; i < visibleColumns.Count; i++)
                    {
                        if (i != (visibleColumns.Count - 1))
                        {
                            double maxFactor = 0.0;
                            if (autoResizeMinimumWidth > 0 && gridWidth > minWidth)
                                maxFactor = ((double)columns[visibleColumns[i].Name].CellWidth * (double)(gridWidth - fixedWidth) / (double)currentTotalColumnWidth);
                            else
                                maxFactor = Math.Max((double)((double)columns[visibleColumns[i].Name].CellWidth * (double)(gridWidth - fixedWidth) / (double)currentTotalColumnWidth), ((columns[visibleColumns[i].Name].DesignWidth * 3) / 4));

                            maxFactor = Math.Round(maxFactor, 1);

                            int valueToBeSet = (int)Math.Floor(maxFactor);
                            truncatedValues += Math.Abs((maxFactor - valueToBeSet));

                            if (maxFactor - (double)valueToBeSet < 0)
                                valueToBeSet--;

                            if (columns[visibleColumns[i].Name].IsAutoResized)
                                columns[visibleColumns[i].Name].CellWidth = valueToBeSet;
                            if (visibleColumns[visibleColumns[i].Name].IsAutoResized)
                                visibleColumns[visibleColumns[i].Name].CellWidth = valueToBeSet;

                            width += (int)(maxFactor);

                            exactWidthOfAllColumns += maxFactor;

                            if (exactColumnWidths.ContainsKey(visibleColumns[i].Name))
                            {
                                if (Math.Truncate(maxFactor) != Math.Truncate(exactColumnWidths[visibleColumns[i].Name]))
                                {
                                    exactColumnWidths[visibleColumns[i].Name] = maxFactor;

                                    valueToBeSet = (int)(Math.Round(exactColumnWidths[visibleColumns[i].Name], MidpointRounding.ToEven));

                                    if (exactColumnWidths[visibleColumns[i].Name] - (double)valueToBeSet < 0)
                                    {
                                        valueToBeSet--;
                                        width -= (int)maxFactor;
                                        width += valueToBeSet;
                                    }

                                    if (columns[visibleColumns[i].Name].IsAutoResized)
                                        columns[visibleColumns[i].Name].CellWidth = valueToBeSet;
                                    if (visibleColumns[visibleColumns[i].Name].IsAutoResized)
                                        visibleColumns[visibleColumns[i].Name].CellWidth = valueToBeSet;
                                }
                            }
                            else
                                exactColumnWidths.Add(visibleColumns[i].Name, (double)columns[visibleColumns[i].Name].CellWidth);
                        }
                        else if (i == (visibleColumns.Count - 1))
                        {
                            int adjustedWidth = (int)(width + fixedWidth);

                            if (columns[visibleColumns[i].Name].IsAutoResized)
                                columns[visibleColumns[i].Name].CellWidth = gridWidth - adjustedWidth - (int)(Math.Round(Math.Abs(truncatedValues), MidpointRounding.ToEven));
                            if (visibleColumns[visibleColumns[i].Name].IsAutoResized)
                                visibleColumns[visibleColumns[i].Name].CellWidth = gridWidth - adjustedWidth - (int)(Math.Round(Math.Abs(truncatedValues), MidpointRounding.ToEven));
                        }
                    }
                }
                totalColumnWidth = currentTotalColumnWidth + fixedWidth;
                isGridColumnResizeInProgress = false;
                isResizedOnce = true;

                this.Refresh();

                if (AfterAutoResizingColumns != null)
                    AfterAutoResizingColumns(this, null);
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected virtual void OnVisibleColumnsChanged(object sender, ListPropertyChangedEventArgs e)
        {
            TableColumnCollection visblecolumns = sender as TableColumnCollection;
            GridGroupingControl grid = visblecolumns.Table.Grid;

            if (grid == null)
                return;

            if (e == null || e.Item == null)
                return;

            try
            {
                TableInfo.TableColumn visibleColumn = e.Item as TableInfo.TableColumn;
                if (visibleColumn.Table == null)
                    return;
                TableColumnCollection columns = visibleColumn.Table.Columns;

                if (e.Action == ListPropertyChangedType.Add)
                {
                    if (!isVisibleColumnRemoved)
                    {
                        designTimeWidths.Clear();
                        isVisibleColumnRemoved = true;
                    }
                    if (!designTimeWidths.ContainsKey(visibleColumn.MappingName))
                    {
                        if (columns[visibleColumn.Name].DesignWidth == -2)
                        {
                            designTimeWidths.Add(visibleColumn.MappingName, columns[visibleColumn.Name].CellWidth);
                            columns[visibleColumn.Name].DesignWidth = columns[visibleColumn.Name].CellWidth;
                        }
                        else
                        {
                            if (!designTimeWidths.ContainsKey(visibleColumn.MappingName))
                                designTimeWidths.Add(visibleColumn.MappingName, columns[visibleColumn.Name].DesignWidth);
                            else
                                designTimeWidths[visibleColumn.MappingName] = columns[visibleColumn.Name].DesignWidth;

                        }
                        //GridColumnResize(grid, true);
                    }
                }
                else if (e.Action == ListPropertyChangedType.Remove)
                {
                    designTimeWidths.Clear();
                    isVisibleColumnRemoved = true;
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected virtual void OnGridResize(object sender, EventArgs e)
        {
            try
            {
                // Re-create the graphics buffer for a new window size.
                context.MaximumBuffer = new Size(this.Width + 1, this.Height + 1);

                if (grafx != null)
                {
                    try
                    {
                        grafx.Dispose();
                        grafx = null;

                        if (context != null)
                            context.Invalidate();
                    }
                    catch (Exception ex)
                    {
                        ExceptionsLogger.LogError(ex);
                    }
                }

                grafx = context.Allocate(this.CreateGraphics(),
                    new Rectangle(0, 0, this.Width, this.Height));

                DrawToBuffer(grafx.Graphics);
                this.Refresh();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected virtual void GridGroupingControl_Resize(object sender, EventArgs e)
        {
            GridGroupingControl grid = sender as GridGroupingControl;

            if (grid == null)
                return;

            if (lastGridResizedWidth == 0)
                lastGridResizedWidth = grid.Width;

            if (Math.Abs(grid.Width - lastGridResizedWidth) > resizeLimit || !isResizedOnce)
            {
                GridColumnResize(grid, false);
                lastGridResizedWidth = grid.Width;
            }
            else if (grid.Width != lastGridResizedWidth)
            {
                if (resizeTimer != null)
                    resizeTimer.Start();
            }

            //
            //Expand view if possible
            //
            if (GridType != GridGrouping.GridType.Virtual && tableInfo.TopRow + tableInfo.DisplayRows > tableInfo.BottomRow)
                tableInfo.TopRow = (tableInfo.BottomRow - tableInfo.DisplayRows >= 0) ? tableInfo.BottomRow - tableInfo.DisplayRows : tableInfo.TopRow;
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
                HScroll.AttachControl = this;
                VScroll.AttachControl = this;

                //if (f != null)
                //{
                //    f.Move -= new EventHandler(TopLevelControl_Move);
                //    f.Move += new EventHandler(TopLevelControl_Move);
                //    f.Resize -= new EventHandler(TopLevelControl_Resize);
                //    f.Resize += new EventHandler(TopLevelControl_Resize);
                //    f.LocationChanged -= new EventHandler(TopLevelControl_LocationChanged);
                //    f.LocationChanged += new EventHandler(TopLevelControl_LocationChanged);
                //    f.VisibleChanged -= new EventHandler(TopLevelControl_VisibleChanged);
                //    f.VisibleChanged += new EventHandler(TopLevelControl_VisibleChanged);
                //}
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


        private void VScroll_OnMaximumValueChanges(int obj)
        {
            if (IsNewScrollBar)
            {
                this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    this.NewVScroll.Maximum = VScroll.Maximum;
                    this.NewVScroll.ViewportSize = this.tableInfo.DisplayRows;
                    this.NewVScroll.Minimum = 0;
                });
            }
        }

        private void HScroll_OnMaximumValueChanges(int obj)
        {
            if (IsNewScrollBar)
            {
                this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    this.NewHScroll.Maximum = HScroll.Maximum;
                    this.NewHScroll.ViewportSize = this.Width;
                    this.NewHScroll.Minimum = 0;
                });
            }
        }


        private void NewHScroll_ValueChanged(object sender, System.EventArgs e)
        {
            if (!IsNewScrollBar)
                return;

            //OnBufferedDataInsert();
            //HideEditTextBox();

            try
            {
                //if (VScroll.Maximum == 0)
                {
                    stopPaint = true;
                    this.horizontalOffset = NewHScroll.Value;
                    this.Table.XOffset = NewHScroll.Value;
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

        private void NewVScroll_ValueChanged(object sender, System.EventArgs e)
        {
            if (!IsNewScrollBar)
                return;

            OnBufferedDataInsert();
            HideEditTextBox();

            try
            {
                if (TotalRecordCount > 0)
                {
                    int vScrollBarValue = NewVScroll.Value;

                    if (vScrollBarValue >= 0)
                        Table.TopRow = vScrollBarValue;// -((vScrollBarValue > Table.DisplayRows) ? Table.DisplayRows : 0);
                    else
                        Table.TopRow = 0;

                    queryVirtualData = true;
                    Invalidate();
                }
                else
                {
                    Table.TopRow = 0;
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }


        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            if (IsNewScrollBar)
            {
                if (NewVScroll.Maximum != VScroll.Maximum)
                    NewVScroll.Maximum = VScroll.Maximum;

                this.NewVScroll.ViewportSize = this.tableInfo.DisplayRows;
                NewVScroll.Value = VScroll.Value;
                return;
            }

            if (!VScroll.IsVisible)
                return;

            OnBufferedDataInsert();
            HideEditTextBox();

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
                else
                {
                    Table.TopRow = 0;
                    Invalidate();
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected virtual void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            OnMouseWheel(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            OnBufferedDataInsert();

            try
            {
                if (VScroll.IsVisible)
                {
                    if (!this.tableInfo.IsFilterEnabled)
                        Table.ProcessMouseWheel(TotalRecordCount, e);
                    else
                        Table.ProcessMouseWheel(tableInfo.FilteredRecords.Count, e);

                    VScroll.Value = Table.TopRow;
                    Invalidate();
                }
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
                    return base.ProcessCmdKey(ref msg, keyData);

                OnBufferedDataInsert();
                bool blnProcess = false;

                if (TextSearchKeyPressed != null)
                {
                    KeyPressEventArgs args = new KeyPressEventArgs((char)keyData);
                    TextSearchKeyPressed(this, args);

                    blnProcess = args.Handled;
                }

                if (blnProcess)
                    return true;

                //
                // key up
                //
                if (keyData == Keys.Up || keyData == (Keys.Up | Keys.Shift))
                {
                    if (TotalRecordCount > 0)
                    {
                        if (Table.CurrentRow > 0 || this.GridType == GridType.Virtual)
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

                                if (Table.firstSelectedRow > 0)
                                    Table.firstSelectedRow -= 1;

                                Table.CurrentRow = Table.firstSelectedRow;

                                Table.lastSelectedRow = Table.firstSelectedRow;
                            }

                            if (Table.CurrentRow < Table.TopRow)
                                Table.TopRow -= 1;

                            if (this.GridType == GridType.Virtual && Table.firstSelectedRow == Table.lastSelectedRow)
                            {
                                if (Table.firstSelectedRow < Table.TopRow || Table.firstSelectedRow > Table.BottomRow - 1)
                                {
                                    Table.TopRow = Table.firstSelectedRow;
                                }
                            }

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
                                Table.firstSelectedRow += 1;
                                Table.lastSelectedRow = Table.firstSelectedRow;
                                Table.CurrentRow = Table.firstSelectedRow;
                            }

                            if (Table.CurrentRow > Table.BottomRow - 1)
                            {
                                Table.TopRow += 1;
                            }

                            if (this.GridType == GridType.Virtual && Table.firstSelectedRow == Table.lastSelectedRow)
                            {
                                if (Table.firstSelectedRow > Table.BottomRow - 1)
                                {
                                    Table.TopRow = Table.firstSelectedRow - (Table.DisplayRows - 1);
                                }
                            }

                            VScroll.Value = tableInfo.TopRow;
                            this.Invalidate(this.Bounds);
                        }
                    }
                    blnProcess = true;
                }
                else if (keyData == (Keys.Control | Keys.A))
                {
                    if (gridType == GridGrouping.GridType.Virtual)
                        tableInfo.lastSelectedRow = tableInfo.SourceDataRowCount;
                    else
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
                    //HScroll.Value -= 20;
                    //blnProcess = true;
                }
                else if (keyData == Keys.Right)
                {
                    //HScroll.Value += 20;
                    //blnProcess = true;
                }
                else if (keyData == Keys.Delete)
                {
                    //blnProcess = true;
                }
                else if (keyData == Keys.Insert)
                {
                    blnProcess = true;
                }

                var currentForm = this.FindForm();

                if (currentForm != null)
                    Helpers.WindowsApiClass.SendMessage(this.FindForm().Handle, Helpers.WindowsApiClass.WM_KEYDOWN, (int)keyData, 0);

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

        #region Menu Items

        public void ProcessMenuColumnInsert()
        {
            try
            {
                Table.InsertColumn(Table.MouseColNo);
                InvalidateGrid();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ProcessMenuColumnDelete()
        {
            try
            {
                Table.DeleteColumn(Table.MouseColNo);
                InvalidateGrid();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ProcessMenuColumnDeleteAll()
        {
            try
            {
                Table.DeleteAllColumns();
                InvalidateGrid();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ProcessMenuColumnWidth()
        {
            try
            {
                int intWidth = Table.VisibleColumns[Table.MouseColNo].CellWidth;

                if (intWidth < 10)
                    intWidth = 10;
                if (intWidth > 200)
                    intWidth = 200;

                Table.VisibleColumns[Table.MouseColNo].CellWidth = intWidth;
                Table.CalculateColumnLeft();
                InvalidateGrid();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ProcessMenuColumnAlignLeft()
        {
            try
            {
                Table.VisibleColumns[Table.MouseColNo].ColumnStyle.HorizontalAlignment = GridHorizontalAlignment.Left;
                InvalidateGrid();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ProcessMenuColumnAlignRight()
        {
            try
            {
                Table.VisibleColumns[Table.MouseColNo].ColumnStyle.HorizontalAlignment = GridHorizontalAlignment.Right;
                InvalidateGrid();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        public void ProcessMenuColumnAlignCenter()
        {
            try
            {
                Table.VisibleColumns[Table.MouseColNo].ColumnStyle.HorizontalAlignment = GridHorizontalAlignment.Center;
                InvalidateGrid();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        #endregion

        #region Scroller Methods

        private void HorizontalScroll_ValueChanged(object sender, EventArgs e)
        {
            if (IsNewScrollBar)
            {
                if (HScroll.Maximum != NewHScroll.Maximum)
                    NewHScroll.Maximum = HScroll.Maximum;

                this.NewHScroll.ViewportSize = this.Width;
                NewHScroll.Value = HScroll.Value;
                return;
            }

            OnBufferedDataInsert();
            HideEditTextBox();

            try
            {
                //if (VScroll.Maximum == 0)
                {
                    stopPaint = true;
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

        public void SetHorizontalOffset(int value)
        {
            try
            {
                stopPaint = true;
                this.horizontalOffset = value;
                this.Table.XOffset = value;
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                stopPaint = false;
                this.Invalidate();
            }
        }

        public int GetVisibleColumnWidth()
        {
            int totalLength = 0;
            for (int i = 0; i < tableInfo.VisibleColumns.Count; i++)
            {
                TableInfo.TableColumn columninfo = tableInfo.VisibleColumns[i];
                totalLength += columninfo.CellWidth;
            }
            return totalLength;
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

    public enum GridType
    {
        DataBound,
        Virtual,
        MultiColumn
    }
}
