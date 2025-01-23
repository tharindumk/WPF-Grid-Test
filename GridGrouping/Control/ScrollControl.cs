using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Threading;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Controls
{
    /// <summary>
    /// Abstract Class for All Scrollbar Controls
    /// </summary>
    public abstract class ScrollBarEx : Form
    {
        public delegate void ScrollEventHandler(object sender, ScrollEventArgs e);
        public event ScrollEventHandler ScrollChange;
        public event EventHandler ValueChanged;

        protected SolidBrush brush = new SolidBrush(Color.Gray);
        protected Pen linePen = new Pen(Color.White);

        private bool rightToLeftMode;
        public bool RightToLeftMode
        {
            get { return rightToLeftMode; }
            set { rightToLeftMode = value; }
        }

        private bool showScrollBarTopMost = false;

        public bool ShowScrollBarTopMost
        {
            get { return showScrollBarTopMost; }
            set { showScrollBarTopMost = value; }
        }

        public enum ThumbLocaion
        {
            ThumbTop,
            Center,
            ThumbBottom,
        }

        public enum StepType
        {
            smallIncrement,
            LargeIncrement
        }

        public enum AnimatingStep
        {
            Fast,
            Delay
        }

        public enum JumpType
        {
            Increment,
            Decrement
        }
        [DllImport("uxtheme", ExactSpelling = true)]
        public extern static Int32 DrawThemeParentBackground(IntPtr hWnd, IntPtr hdc, ref Rectangle pRect);

        #region Browsable Properties
        private int pageSizeIndex = 10; // page size by index(when page up pressed index to be moved).
        [Browsable(true)]
        public int PageSizeIndex
        {
            get { return pageSizeIndex; }
            set { pageSizeIndex = value; }
        }
        protected int scrollOpacity = 100;
        // [Browsable(true)]
        //public int ScrollOpacity
        //{
        //    get { return scrollOpacity; }
        //    set { scrollOpacity = value; }
        //}
        private int defaultOpacity = 100;
        [Browsable(true)]
        public int DefaultOpacity
        {
            get { return defaultOpacity; }
            set { defaultOpacity = value; }
        }
        private int thumbOpacity = 50;
        [Browsable(true)]
        public int ThumbOpacity
        {
            get { return thumbOpacity; }
            set { thumbOpacity = value; }
        }

        public bool IsScrollingEnabled { get; set; }

        private Control attachControl = null;

        protected int gridHeaderHeight = 0;

        [Browsable(true)]
        public Control AttachControl
        {
            get { return attachControl; }
            set
            {
                if (value != null)
                {
                    attachControl = value;
                    attachControl.MouseLeave -= new EventHandler(Parent_MouseLeave);
                    attachControl.MouseLeave += new EventHandler(Parent_MouseLeave);
                    attachControl.MouseEnter -= new EventHandler(Parent_MouseEnter);
                    attachControl.MouseEnter += new EventHandler(Parent_MouseEnter);
                    attachControl.MouseHover -= new EventHandler(attachControl_MouseHover);
                    attachControl.MouseHover += new EventHandler(attachControl_MouseHover);
                    attachControl.MouseMove -= new MouseEventHandler(attachControl_MouseMove);
                    attachControl.MouseMove += new MouseEventHandler(attachControl_MouseMove);

                    attachControl.Resize -= new EventHandler(attachControl_Resize);
                    attachControl.Resize += new EventHandler(attachControl_Resize);
                    attachControl.KeyPress -= new KeyPressEventHandler(attachControl_KeyPress);
                    attachControl.KeyPress += new KeyPressEventHandler(attachControl_KeyPress);
                    attachControl.MouseWheel -= new MouseEventHandler(attachControl_MouseWheel);
                    attachControl.MouseWheel += new MouseEventHandler(attachControl_MouseWheel);

                    if (attachControl.Parent != null)
                    {
                        attachControl.Parent.Move -= new EventHandler(TopLevelControl_Move);
                        attachControl.Parent.Move += new EventHandler(TopLevelControl_Move);
                        attachControl.Parent.Resize -= new EventHandler(TopLevelControl_Resize);
                        attachControl.Parent.Resize += new EventHandler(TopLevelControl_Resize);
                        attachControl.Parent.LocationChanged -= new EventHandler(TopLevelControl_LocationChanged);
                        attachControl.Parent.LocationChanged += new EventHandler(TopLevelControl_LocationChanged);
                        attachControl.Parent.VisibleChanged -= new EventHandler(TopLevelControl_VisibleChanged);
                        attachControl.Parent.VisibleChanged += new EventHandler(TopLevelControl_VisibleChanged);
                    }

                    GridGroupingControl grid = attachControl as GridGroupingControl;

                    if (grid != null && !grid.Table.HideHeader)
                        gridHeaderHeight = grid.Table.HeaderHeight;
                }
            }
        }

        protected Control GetParentForm(Control baseControl)
        {
            if (baseControl == null)
                return baseControl;

            if (baseControl is Form)
                return baseControl;
            else
            {
                return GetParentForm(baseControl.Parent);
            }
        }

        void attachControl_MouseMove(object sender, MouseEventArgs e)
        {
            IntPtr c = WindowsApiClass.GetFocus();

            Control focusedControl = Control.FromHandle(c);

            if (focusedControl != null && !(focusedControl is ScrollBarEx) && (focusedControl != this.attachControl && focusedControl != this.attachControl.Parent))
                return;

            if (fullyInvisible || Maximum == 0)
                ShowScrollBar();
        }

        void attachControl_MouseHover(object sender, EventArgs e)
        {
            //IntPtr c = WindowsApiClass.GetFocus();

            //Control focusedControl = Control.FromHandle(c);

            //if (focusedControl != null && !(focusedControl is ScrollBarEx) && (focusedControl != this.attachControl && focusedControl != this.attachControl.Parent))
            //    return;

            //if (fullyInvisible || Maximum == 0)
            //    ShowScrollBar();
        }

        void TopLevelControl_VisibleChanged(object sender, EventArgs e)
        {

        }

        void TopLevelControl_LocationChanged(object sender, EventArgs e)
        {

        }

        void TopLevelControl_Resize(object sender, EventArgs e)
        {
            //if ((sender as Form).TopLevelControl != null)
            //     this.Location = (sender as Form).TopLevelControl.Location;
            //this.Visible = true;

            if (!IsObsolute)
            {
                if (IsVisible)
                    this.BringToFront();
            }
            //this.Opacity = 1;
            //this.ShowInTaskbar = true;
        }

        void TopLevelControl_Move(object sender, EventArgs e)
        {

        }


        void attachControl_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                default:
                    break;
            }
        }

        public event Action<int> OnMaximumValueChanges;

        private int maximumValue = 100;
        [Browsable(true)]
        public int Maximum
        {
            get { return maximumValue; }
            set
            {
                int oldMaximum = maximumValue;

                if (value >= 0)
                {
                    maximumValue = value;
                }
                else
                    maximumValue = 0;

                if (rightToLeftMode && oldMaximum != maximumValue && oldMaximum > 0 && maximumValue > 0)
                {
                    int change = maximumValue - oldMaximum;
                    int newValue = this.Value + change;

                    if (newValue <= maximumValue && newValue >= 0)
                    {
                        this.Value = newValue;
                        isUpdateRequiered = true;
                    }
                }

                if (OnMaximumValueChanges != null)
                    OnMaximumValueChanges(maximumValue);
            }
        }

        private int value = 0;

        [Browsable(true)]
        public virtual int Value
        {
            get { return this.value; }
            set
            {
                oldValue = this.value;
                this.value = value;
            }
        }

        private int thumbHeight = 16;

        [Browsable(true)]
        public int ThumbHeight
        {
            get { return thumbHeight; }
            set { thumbHeight = value; }
        }


        protected int thumbWidth = 10;
        [Browsable(true)]
        public int ThumbWidth
        {
            get { return thumbWidth; }
            set { thumbWidth = value; }
        }

        private Color thumbBackColor = Color.Gray;
        [Browsable(true)]
        public Color ThumbBackColor
        {
            get { return thumbBackColor; }
            set { thumbBackColor = value; }
        }

        private Color scrollBorderColor = Color.WhiteSmoke;
        [Browsable(true)]
        public Color ScrollBorderColor
        {
            get { return scrollBorderColor; }
            set { scrollBorderColor = value; }
        }
        private Color scrollbarBackColor = Color.DarkGray;
        [Browsable(true)]
        public Color ScrollbarBackColor
        {
            get { return scrollbarBackColor; }
            set { scrollbarBackColor = value; }
        }

        private int largeChange = 10;

        public int LargeChange
        {
            get { return largeChange; }
            set { largeChange = value; }
        }

        private int smallChange = 1;

        public int SmallChange
        {
            get { return smallChange; }
            set { smallChange = value; }
        }

        #endregion

        #region Local Variables
        protected int MinThumbHeight = 20;
        protected System.Windows.Forms.Timer pageMoveTimer = null;
        protected int oldValue = 0;
        protected Rectangle thumbRect;
        bool animating = false;
        protected bool pageMoveEnable = false;
        public bool IsVisible = true;
        protected bool draggingProgress = false;
        protected int draggingStartLocation = 0;
        protected double PixelFactor = 0; //Pixel Per index
        protected AnimatingStep animationDelay = AnimatingStep.Fast;
        public int XOffset = 5; //TODO
        public int Yoffset = 0;
        protected bool isMouseDrag = true;
        protected JumpType CurrentJumpType = JumpType.Increment;

        protected MouseEventArgs pageMoveBeginArgs = null;
        protected bool isButtonPressed = false;
        protected int ScrollableLength = 0;
        protected bool fullyInvisible = false;
        protected System.Windows.Forms.Timer updateTimer = new System.Windows.Forms.Timer();
        protected bool isUpdateRequiered = false;
        protected int buttonHeight = 15;
        #endregion

        #region Constructor

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                //createParams.ExStyle |= 0x00000020;
                return createParams;
            }
        }


        public ScrollBarEx()
        {
            //this.SetStyle(ControlStyles.Selectable, false);

            this.IsScrollingEnabled = true;
            this.Resize += new EventHandler(CustomScorllBar_Resize);

            //SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //SetStyle(ControlStyles.DoubleBuffer, true);
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.ShowInTaskbar = false;
            this.Owner = GridGroupingControl.mainForm;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowIcon = false;
            //SetStyle(ControlStyles.ResizeRedraw, false); 

            this.DoubleBuffered = true;

            this.Opacity = 0.50;
            //this.BackColor = Color.FromArgb(1, 1, 1, 1); // Color.Transparent;
            pageMoveTimer = new System.Windows.Forms.Timer();
            pageMoveTimer.Interval = 50;
            pageMoveTimer.Tick += new EventHandler(pageMoveTimer_Tick);
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Tick += new EventHandler(updateTimer_Tick);
            updateTimer.Interval = 50;

            this.Location = new Point(20000, 20000);
            //this.TopLevel = true;
            this.ShowInTaskbar = false;
            BindMainFormEventHandlers();
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            if (isUpdateRequiered)
                this.Update();
        }

        #endregion

        #region Virtual Methods
        protected virtual void AnimateStep()
        {

            while (animating)
            {
                int opacity = scrollOpacity - 10;

                if (opacity <= 0)
                {
                    this.BackColor = Color.Transparent;
                    scrollOpacity = 0;
                    thumbOpacity = 0;
                    this.Invalidate();

                    break;
                }
                scrollOpacity = opacity;

                if (animationDelay == AnimatingStep.Fast)
                {
                    Thread.Sleep(50);
                }
                else
                {
                    Thread.Sleep(2500);
                    scrollOpacity = 0;
                    this.Invalidate();
                    this.animationDelay = AnimatingStep.Fast;
                    break;
                }
                this.Invalidate();
            }
        }
        protected virtual void SetNewLocation(int offsetIndex)
        {
            //if (value == 0 && offsetIndex < 0)
            //    return;
            //if (value == maximumValue && offsetIndex > 0)
            //    return;
            SetLocation(offsetIndex);

            if (ScrollChange != null)
                ScrollChange(this, new ScrollEventArgs(ScrollEventType.ThumbPosition, Value));

            if (ValueChanged != null && oldValue != Value)
            {

                ValueChanged(this, new EventArgs());
                if (oldValue <= Int32.MinValue)
                    return;
                int change = Math.Abs(oldValue - value);
                this.Invalidate();
                if (maximumValue > 0 && change * 100 / maximumValue > 1)
                    this.Update();
            }
        }
        protected virtual void SetLocation(int index)
        {

        }

        #region Events

        protected virtual void AdjustLocation()
        {

        }

        protected virtual void AdjustSize()
        {

        }

        protected virtual void Parent_MouseEnter(object sender, EventArgs e)
        {
            try
            {
                IntPtr focusedHandle = WindowsApiClass.GetFocus();

                Control focusedControl = Control.FromHandle(focusedHandle);

                //if (isMouseMessageSent && (focusedControl != null &&
                //    (focusedControl.Name != "ShellFormRibbonNew" && (!(focusedControl is ScrollBarEx) && (focusedControl != this.attachControl && focusedControl != this.attachControl.Parent && (this.attachControl.Parent.Parent != null && focusedControl != this.attachControl.Parent.Parent))))))
                //{
                //        return;
                //}

                if (isMouseMessageSent && (focusedControl != null &&
                    (focusedControl.Name != "ShellFormRibbonNew" && (!(focusedControl is ScrollBarEx) && !CheckContains(focusedControl, attachControl)))))
                {
                    return;
                }

                //this.Owner = GridGroupingControl.mainForm;

                ShowScrollBar();
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private bool CheckContains(Control ctrl, Control checkingControl)
        {
            if (ctrl == null)
                return false;
            else if (ctrl == checkingControl)
                return true;
            else
            {
                if (checkingControl.Parent == null)
                    return false;
                else
                    return CheckContains(ctrl, checkingControl.Parent);
            }
        }

        public bool IsObsolute { get; set; }

        internal void ShowScrollBar()
        {
            if (IsObsolute || !IsScrollingEnabled)
                return;

            try
            {
                if (Maximum == 0)
                {
                    this.Visible = false;
                    fullyInvisible = true;
                    return;
                }

                if (this is VScrollBarEx)
                { }
                else
                { }


                if (IsVisible)
                {
                    this.Visible = true;
                    this.BringToFront();

                    scrollOpacity = defaultOpacity;
                    thumbOpacity = defaultOpacity;
                    if (!draggingProgress)
                    {
                        this.Opacity = 0.5f;
                    }

                    fullyInvisible = false;
                    //AdjustLocation();
                    AdjustSize();
                    AdjustLocation(); // Fixed vertical scrollbar appearing in middle issue

                    if (attachControl != null && attachControl.TopLevelControl != null)
                    {
                        Form parentControl = (this.attachControl.TopLevelControl as Form);

                        if (ShowScrollBarTopMost || (parentControl != null && parentControl.TopMost))
                            this.TopMost = true;
                    }

                    this.Invalidate();
                    isMouseMessageSent = true;
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        bool isMouseMessageSent = false;

        protected virtual void Parent_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                Point p = this.attachControl.PointToClient(MousePosition);

                this.Owner = null;
                this.animating = true;
                fullyInvisible = true;

                // new MethodInvoker(AnimateStep).BeginInvoke(null, null);
                this.Invalidate();
                this.Update();
                p = MousePosition;

                if (!this.Bounds.Contains(p))
                {
                    this.Opacity = defaultOpacity;

                    if (this.TopMost)
                        this.TopMost = false;

                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        protected virtual void CustomScorllBar_Resize(object sender, EventArgs e)
        {
            ScrollBarResize();
        }

        #endregion

        protected void attachControl_Resize(object sender, EventArgs e)
        {
            ParentControlResize();

            //this.Location = new Point(200, 200);
            this.TopLevel = true;
            if (this.ShowInTaskbar)
                this.ShowInTaskbar = false;

            this.Visible = false;
        }

        void attachControl_MouseWheel(object sender, MouseEventArgs e)
        {
            //this.animating = false;
            //this.scrollOpacity = DefaultOpacity;

            bool doMouseWheelOperation = true;

            if (IsObsolute)
            {
                if (this is VScrollBarEx)
                {
                    doMouseWheelOperation = false;
                }
            }

            if (!doMouseWheelOperation)
            {
                int offSet = -e.Delta / 60;
                int roundOffset = GetNoOfIndexToJump(StepType.smallIncrement) * offSet;
                SetNewLocation(roundOffset);
                SyncThumb(this.value);
                this.Invalidate();
            }
        }

        #endregion

        #region Overrides

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (draggingProgress)
            {
                this.Hide();
            }
            if (!this.AttachControl.Bounds.Contains(MousePosition))
            {
                //  animating = true;
                // new MethodInvoker(AnimateStep).BeginInvoke(null, null);
                this.fullyInvisible = true;
                pageMoveEnable = false;
                base.OnMouseLeave(e);
                this.Invalidate();
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (GridGroupingControl.mainForm != null && !GridGroupingControl.mainForm.IsDisposed)
                {
                    GridGroupingControl.mainForm.Resize -= new EventHandler(mainForm_Resize);
                }
            }
            catch
            {

            }

            base.Dispose(disposing);
        }

        private void BindMainFormEventHandlers()
        {
            try
            {
                if (GridGroupingControl.mainForm != null && GridGroupingControl.mainForm.IsHandleCreated)
                {
                    GridGroupingControl.mainForm.Resize -= new EventHandler(mainForm_Resize);
                    GridGroupingControl.mainForm.Resize += new EventHandler(mainForm_Resize);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        private void mainForm_Resize(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.IsDisposed || (this.AttachControl != null && this.AttachControl.IsDisposed))
                return;

            if (Maximum == 0)
                return;

            SetThumbSize();

            Point p = this.AttachControl.PointToClient(MousePosition);
            //  p.Offset(-5, -5);
            if (this.AttachControl.Parent.Bounds.Contains(p))
            {
                animating = false;
            }
            else
            {
                //return;
            }

            if (!IsVisible)
            {
                base.OnPaint(e);
                return;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            DrawBackGround(g);
            DrawScrollBarLine(g);
            DrawThumb(g);
            DrawButton(g);
            base.OnPaint(e);
        }


        protected override void OnMouseClick(MouseEventArgs e)
        {
            JumpSCroll(e);
            base.OnClick(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.PageDown:
                    SetNewLocation(PageSizeIndex);
                    this.Invalidate();
                    return base.ProcessCmdKey(ref msg, keyData);
                case Keys.PageUp:
                    SetNewLocation(-PageSizeIndex);
                    this.Invalidate();
                    return base.ProcessCmdKey(ref msg, keyData);

                default:
                    break;
            }

            if (attachControl != null)
                attachControl.PreProcessMessage(ref msg);

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            if (IsVisible)
            {
                this.Visible = true;
                scrollOpacity = defaultOpacity;
                thumbOpacity = defaultOpacity;
                fullyInvisible = false;
                this.Invalidate();
                base.OnMouseEnter(e);
            }

        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            this.animating = false;
            this.scrollOpacity = DefaultOpacity;
            int offSet = -e.Delta / 120;
            int roundOffset = GetNoOfIndexToJump(StepType.smallIncrement) * offSet;
            SetNewLocation(roundOffset);
            this.Invalidate();
            base.OnMouseWheel(e);
            //this.animationDelay = AnimatingStep.Delay;
            //this.animating = true;
            //new MethodInvoker(AnimateStep).BeginInvoke(null, null);

        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            pageMoveTimer.Enabled = false;
            pageMoveEnable = false;
            draggingProgress = false;
            base.OnMouseUp(e);
        }

        #endregion

        #region Abstract Methods
        protected abstract void DrawBackGround(Graphics g);
        protected abstract void DrawScrollBarLine(Graphics g);
        protected abstract void DrawThumb(Graphics g);
        protected abstract void JumpSCroll(MouseEventArgs e);
        protected abstract int GetCurrentLocation(ThumbLocaion thumbPos);
        protected abstract void SyncThumb(int index);
        protected abstract void ParentControlResize();
        protected abstract void ScrollBarResize();
        protected abstract void DrawButton(Graphics g);
        protected abstract void RecalculateParams();
        protected abstract int GetAvailableFixedLength();
        #endregion

        #region Helper Methods
        protected void ChangeScrollPosition(int newValue, ThumbLocaion thumbPos)
        {
            SetNewThumbPosition(newValue, thumbPos);
        }

        protected void SetNewThumbPosition(int newValue, ThumbLocaion thumbPos)
        {
            if (thumbPos == ThumbLocaion.Center)
            {
                int offsetIndex = GetOffsetValue(newValue, thumbPos);
                SetNewLocation(offsetIndex);
            }
        }

        private int GetOffsetValue(int newValue, ThumbLocaion thumbPos)
        {
            int currentPosition = GetCurrentLocation(thumbPos);
            int deltaPixel = newValue - draggingStartLocation;
            //int relativeDelta = deltaPixel + currentPosition;
            double deltaIndex = deltaPixel / PixelFactor;
            int test = (int)Math.Round(deltaIndex);
            if (test != 0)
                draggingStartLocation = newValue;
            return test;
        }

        private int GetIndex(int ScrollableLength, int currentPos)
        {
            return Value;
        }

        protected Color GetColorWithOpaque(Color c)
        {
            if (scrollOpacity > 0 & scrollOpacity < 100)
                return Color.FromArgb(255 * scrollOpacity / 100, c);
            else if (scrollOpacity == 0)
                return Color.Transparent;
            else
                return c;
        }

        protected Color GetColorWithOpaque(Color c, int Opaque)
        {
            if (Opaque > 0 & Opaque < 100)
                return Color.FromArgb(255 * Opaque / 100, c);
            else
                return GetColorWithOpaque(c);

        }

        protected int GetNoOfIndexToJump(StepType step)
        {
            switch (step)
            {
                case StepType.smallIncrement:
                    return smallChange;

                case StepType.LargeIncrement:
                    double offset = Math.Max(largeChange, Maximum * .1);
                    return (int)Math.Round(offset);
                default:
                    return 10;
            }

        }

        void pageMoveTimer_Tick(object sender, EventArgs e)
        {
            if (pageMoveEnable || isButtonPressed)
            {
                if (isButtonPressed)
                {
                    (sender as System.Windows.Forms.Timer).Interval = 50;
                    JumpScrollOnButtonPressed();
                    // this.Invalidate();
                }
                else
                {
                    (sender as System.Windows.Forms.Timer).Interval = 100;
                    JumpSCroll(pageMoveBeginArgs);
                    // this.Invalidate();
                }

            }
        }

        void JumpScrollOnButtonPressed()
        {
            switch (CurrentJumpType)
            {
                case JumpType.Increment:

                    SetNewLocation(10 * GetNoOfIndexToJump(StepType.smallIncrement));
                    break;
                case JumpType.Decrement:
                    SetNewLocation(-10 * GetNoOfIndexToJump(StepType.smallIncrement));
                    break;
                default:
                    break;
            }
        }

        public virtual void SetThumbSize()
        {
            int availableLength = GetAvailableFixedLength(); // this Fixed to avoid recursive calculation of params.

            if (maximumValue == 0)
                thumbHeight = availableLength - 1;

            else if (maximumValue > 0)
            {
                double test = availableLength / (1 + Math.Log10(maximumValue));

                double actualHeitght = Math.Max(MinThumbHeight, test);
                thumbHeight = (int)actualHeitght;
            }

            //System.Diagnostics.Debug.WriteLine(Maximum);
            if (Maximum == 0)
            {
                thumbHeight = this.Height - (2 * buttonHeight);
            }
            RecalculateParams(); //TODO .. use base clase
        }
        #endregion

        public static GraphicsPath RoundRect(RectangleF r, float r1, float r2, float r3, float r4)
        {
            float x = r.X, y = r.Y, w = r.Width, h = r.Height;
            GraphicsPath rr = new GraphicsPath();
            rr.AddBezier(x, y + r1, x, y, x + r1, y, x + r1, y);
            rr.AddLine(x + r1, y, x + w - r2, y);
            rr.AddBezier(x + w - r2, y, x + w, y, x + w, y + r2, x + w, y + r2);
            rr.AddLine(x + w, y + r2, x + w, y + h - r3);
            rr.AddBezier(x + w, y + h - r3, x + w, y + h, x + w - r3, y + h, x + w - r3, y + h);
            rr.AddLine(x + w - r3, y + h, x + r4, y + h);
            rr.AddBezier(x + r4, y + h, x, y + h, x, y + h - r4, x, y + h - r4);
            rr.AddLine(x, y + h - r4, x, y + r1);
            return rr;
        }

        public static GraphicsPath GetTriangle(Point centerPoint, int height, float angle)
        {

            GraphicsPath path = new GraphicsPath();
            Point P1 = new Point(centerPoint.X, centerPoint.Y - height);
            Point P2 = new Point(centerPoint.X + height, centerPoint.Y + height);
            Point P3 = new Point(centerPoint.X - height, centerPoint.Y + height);

            path.AddPolygon(new Point[] { P1, P2, P3 });
            path.CloseFigure();
            Matrix m = new Matrix();
            m.RotateAt(angle, new PointF(centerPoint.X, centerPoint.Y));
            path.Transform(m);
            return path;
        }

    }

    public class VScrollBarEx : ScrollBarEx
    {
        #region Private Properties

        private int thumbTopMostLocation = 0;// location of the top of thumb can go 
        private int thumbBottomMostLoaction = 100; // location of the top of thumb can go 
        private Rectangle UPRect = new Rectangle();
        private Rectangle DownRect = new Rectangle();
        private int buttonGap = 1;

        int buttonWidth = 16;

        #endregion

        #region Constructors

        public VScrollBarEx()
        {
            this.TopLevel = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ControlBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowIcon = false;
            thumbRect = new Rectangle(XOffset, Yoffset, ThumbWidth, ThumbHeight);
        }

        #endregion

        #region Base Override

        public override int Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if (!IsVisible)
                    return;

                base.Value = value;
                this.Invalidate();
            }
        }

        protected override void AdjustLocation()
        {
            try
            {
                if (this.AttachControl != null && this.AttachControl.Parent != null)
                {
                    Control ctrl = GetParentForm(this.AttachControl);
                    Point p = this.AttachControl.Parent.PointToScreen(new Point(this.AttachControl.Right - 2, this.AttachControl.Top));

                    if (AttachControl.RightToLeft == System.Windows.Forms.RightToLeft.Yes)
                    {
                        this.Location = ctrl.PointToScreen(new Point(this.AttachControl.Right - 2, this.AttachControl.Top));
                        this.Location = new Point(this.Location.X - 2, this.Location.Y + (p.Y - this.Location.Y + 2 + gridHeaderHeight));
                    }
                    else
                    {
                        if (ctrl != null)
                        {
                            this.Location = ctrl.PointToScreen(new Point(this.AttachControl.Right - 10, this.AttachControl.Top));
                            this.Location = new Point(this.Location.X - 12 + (p.X - this.Location.X), this.Location.Y + (p.Y - this.Location.Y + 2 + gridHeaderHeight));
                        }
                    }
                }

                if (this.AttachControl.TopLevelControl != null)
                {
                    if (this.Location.X > this.AttachControl.TopLevelControl.Right - 20)
                        this.Location = new Point(this.AttachControl.TopLevelControl.Right - this.Width - 3, this.Location.Y);

                    if (this.Location.Y > this.AttachControl.TopLevelControl.Bottom - 20)
                        this.Location = new Point(this.Location.X, this.AttachControl.TopLevelControl.Bottom - this.Height - 2);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }



        protected override void AdjustSize()
        {
            if (this.Width > 12)
                this.Width = 12;

            this.Height = this.AttachControl.Height - gridHeaderHeight - 4;
        }

        protected override void ParentControlResize()
        {
            this.Visible = false;
        }

        protected override void ScrollBarResize()
        {
            RecalculateParams();
            //thumbRect.Height = ThumbHeight;
            //thumbTopMostLocation = Yoffset + this.Margin.Top;
            //thumbBottomMostLoaction = this.Height - ThumbHeight - this.Margin.Bottom - 2 * buttonHeight - buttonGap;
            //ScrollableLength = thumbBottomMostLoaction - thumbTopMostLocation;
            //PixelFactor = (double)ScrollableLength / Maximum;
            //thumbRect.X = (this.Width - thumbRect.Width) / 2 + 1;
            //UPRect = new Rectangle(thumbRect.X, this.ClientSize.Height - 2 * buttonHeight - buttonGap, buttonWidth, buttonHeight);
            //DownRect = new Rectangle(thumbRect.X, this.ClientSize.Height - buttonHeight, buttonWidth, buttonHeight);
            SetNewLocation(0);
        }

        protected override void RecalculateParams()
        {
            thumbRect.Height = ThumbHeight;
            thumbTopMostLocation = Yoffset + this.Margin.Top;
            thumbBottomMostLoaction = this.Height - ThumbHeight - this.Margin.Bottom - 2 * buttonHeight - buttonGap;
            ScrollableLength = thumbBottomMostLoaction - thumbTopMostLocation;
            PixelFactor = (double)ScrollableLength / Maximum;
            thumbRect.X = (this.Width - thumbRect.Width) / 2 + 1;
            UPRect = new Rectangle(thumbRect.X, this.ClientSize.Height - 2 * buttonHeight - buttonGap, buttonWidth, buttonHeight);
            DownRect = new Rectangle(thumbRect.X, this.ClientSize.Height - buttonHeight, buttonWidth, buttonHeight);
        }

        protected override int GetAvailableFixedLength()
        {
            return this.Height - this.Margin.Bottom - 2 * buttonHeight - buttonGap - 2;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            isButtonPressed = false;
            if (UPRect.Contains(e.Location))
            {
                isButtonPressed = true;
                CurrentJumpType = JumpType.Decrement;
                pageMoveTimer.Enabled = true;

            }
            else if (DownRect.Contains(e.Location))
            {
                CurrentJumpType = JumpType.Increment;
                isButtonPressed = true;
                pageMoveTimer.Enabled = true;


            }
            else if (thumbRect.Contains(e.Location))
            {

                draggingStartLocation = e.Y;
                draggingProgress = true;
            }
            else
            {
                isMouseDrag = true;
                pageMoveEnable = true;
                pageMoveTimer.Enabled = true;
                pageMoveBeginArgs = e;

            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            updateTimer.Enabled = false;

            if (e.Button == System.Windows.Forms.MouseButtons.Left && draggingProgress)
            {
                int newValue = e.Y;
                ChangeScrollPosition(newValue, ThumbLocaion.Center);

                isUpdateRequiered = true;
                updateTimer.Enabled = true;
                //this.Invalidate();
            }
            base.OnMouseMove(e);
        }

        #endregion

        #region Draw Methods

        protected override void DrawBackGround(Graphics g)
        {
            Rectangle outterRect = new Rectangle(thumbRect.X, this.thumbTopMostLocation, ThumbWidth, this.thumbBottomMostLoaction + ThumbHeight + 2 * buttonHeight + buttonGap);

            brush.Color = GetColorWithOpaque(ScrollbarBackColor, scrollOpacity);
            linePen.Color = GetColorWithOpaque(ScrollBorderColor, scrollOpacity);
            g.DrawRectangle(linePen, outterRect);
            g.FillRectangle(brush, outterRect);

        }

        protected override void DrawThumb(Graphics g)
        {
            float radius = 4f;

            Rectangle drawRect = new Rectangle(thumbRect.X - 1, thumbRect.Y, thumbRect.Width, thumbRect.Height);

            //  if (IsHighlight(DownRect) || IsHighlight(UPRect) || IsHighlight(thumbRect))
            if (IsHighlight(this.ClientRectangle))
                brush.Color = GetColorWithOpaque(ThumbBackColor, 100);
            else
                brush.Color = GetColorWithOpaque(ThumbBackColor);

            if (drawRect.X < 0 || drawRect.Y < 0 || drawRect.Width <= 0 || drawRect.Height <= 0)
                return;

            using (GraphicsPath path = RoundRect(drawRect, radius, radius, radius, radius))
            {
                g.FillPath(brush, path);
            }
        }

        protected override void DrawScrollBarLine(Graphics g)
        {
            Rectangle outterRect = new Rectangle(thumbRect.X - 1, this.thumbTopMostLocation, ThumbWidth, this.thumbBottomMostLoaction + ThumbHeight);

            linePen.Color = GetColorWithOpaque(Color.DarkGray);
            g.DrawRectangle(linePen, outterRect);
        }

        protected override void DrawButton(Graphics g)
        {
            float radius = 4;
            Rectangle rectup = new Rectangle(UPRect.X - 1, UPRect.Y, UPRect.Width, UPRect.Height);

            Rectangle rectDown = new Rectangle(DownRect.X - 1, DownRect.Y, DownRect.Width, DownRect.Height);

            using (GraphicsPath path = ScrollBarEx.RoundRect(rectup, radius, radius, 0, 0))
            {
                using (GraphicsPath pathdown = ScrollBarEx.RoundRect(rectDown, 0, 0, radius, radius))
                {
                    // if (IsHighlight(DownRect) || IsHighlight(UPRect) || IsHighlight(thumbRect))
                    if (IsHighlight(this.ClientRectangle))
                        brush.Color = GetColorWithOpaque(ThumbBackColor, 100);
                    else if (!fullyInvisible)
                        brush.Color = GetColorWithOpaque(ThumbBackColor, DefaultOpacity);
                    else
                        brush.Color = GetColorWithOpaque(ThumbBackColor);

                    g.FillPath(brush, path); g.FillPath(brush, pathdown);
                }
            }

            brush.Color = Color.Wheat;

            using (GraphicsPath tringlDown = GetTriangle(new Point(rectDown.X + rectDown.Width / 2, rectDown.Y + rectDown.Height / 2), 3, 180))
            {
                g.FillPath(brush, tringlDown);
            }

            using (GraphicsPath tringlUp = GetTriangle(new Point(rectup.X + rectup.Width / 2, rectup.Y + rectup.Height / 2), 3, 0))
            {
                g.FillPath(brush, tringlUp);
            }
        }

        private void DrawUpButtun(Graphics g)
        {

        }

        private bool IsHighlight(Rectangle rect)
        {
            Point location = this.PointToClient(MousePosition);

            if (rect.Contains(location))
                return true;
            else
                return false;
        }
        #endregion

        #region Helper Methods

        private bool iswithinBounds(int boundLength)
        {

            if (boundLength < this.Height - ThumbHeight && boundLength > 0)
                return true;
            else
                return false;

        }

        protected override void JumpSCroll(MouseEventArgs e)
        {
            if (!thumbRect.Contains(e.Location))
            {
                int delta = GetThumbLocation(ThumbLocaion.Center) - e.Y;
                int distanceFromBorder = Math.Abs(delta) - ThumbHeight / 2;
                if (distanceFromBorder > 0) // outside Thumb
                {

                    if (delta != 0) // only when outside
                    {
                        int sign = Math.Sign(delta) * -1;
                        SetNewLocation(sign * GetNoOfIndexToJump(StepType.LargeIncrement));
                        //ChangeScrollPosition(newValue, ThumbLocaion.Center);
                    }
                }
            }
        }

        private int GetThumbLocation(ThumbLocaion location)
        {
            switch (location)
            {
                case ThumbLocaion.ThumbTop:
                    return thumbRect.Top + ThumbHeight / 2;


                case ThumbLocaion.Center:
                    return thumbRect.Top + ThumbHeight / 2;

                case ThumbLocaion.ThumbBottom:
                    return thumbRect.Top + ThumbHeight / 2;


                default:
                    return thumbRect.Top + ThumbHeight / 2;

            }

        }

        protected override int GetCurrentLocation(ThumbLocaion thumbPos)
        {
            switch (thumbPos)
            {
                case ThumbLocaion.ThumbTop:
                    double pixelLocationTop = Value * PixelFactor;
                    return (int)Math.Round(pixelLocationTop);
                case ThumbLocaion.Center:
                    double pixelLocation = Value * PixelFactor;
                    pixelLocation += ThumbHeight / 2; // center location
                    return (int)Math.Round(pixelLocation);

                case ThumbLocaion.ThumbBottom:
                    double pixelLocationBottom = Value * PixelFactor;
                    pixelLocationBottom += ThumbHeight; // center location
                    return (int)Math.Round(pixelLocationBottom);

                default:
                    double pixelLocationD = Value * PixelFactor;
                    pixelLocationD += ThumbHeight / 2; // center location
                    return (int)Math.Round(pixelLocationD);

            }

        }

        //get offsetByindex
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (UPRect.Contains(e.Location))
            {
                SetNewLocation(-PageSizeIndex);
                this.Invalidate();
            }
            else if (DownRect.Contains(e.Location))
            {
                SetNewLocation(PageSizeIndex);
                this.Invalidate();
            }
            else
                base.OnMouseClick(e);
        }

        protected override void SetLocation(int deltaIndex)
        {
            this.Value += deltaIndex;

            if (Value >= Maximum) // min max adjustement
                Value = Maximum;
            else if (Value <= 0)
                Value = 0;


            //      Rectangle ScrollableRect = new Rectangle(new Point(0, 0), new Size(ThumbWidth, ScrollableLength));
            SyncThumb(Value);

        }

        protected override void SyncThumb(int index)
        {
            if (index > 0 && index < Maximum)
            {
                int topY = GetCurrentLocation(ThumbLocaion.ThumbTop);
                thumbRect.Y = topY;
            }
            else if (index == 0)
                thumbRect.Y = thumbTopMostLocation;
            else if (index == Maximum)
                thumbRect.Y = thumbBottomMostLoaction;

        }

        #endregion
    }

    public class HScrollBarEx : ScrollBarEx
    {
        #region Private Properties

        private int thumbLEftMostLocation = 0;// location of the Left of thumb can go 
        private int thumbRightMostLoaction = 100; // location of the Left of thumb can go 
        private Rectangle RightRect = new Rectangle();
        private Rectangle LeftRect = new Rectangle();
        private int buttonGap = 1;
        //   int buttonHeight = 15;
        int buttonWidth = 16;

        #endregion

        #region Constructors

        public HScrollBarEx()
        {
            this.TopLevel = false;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ControlBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowIcon = false;
            thumbRect = new Rectangle(2, 2, ThumbHeight, ThumbWidth);
            // this.ThumbHeight = 20;
        }

        #endregion

        #region Base Override

        protected override void AdjustLocation()
        {
            if (this.AttachControl == null)
                this.Location = new Point(20000, 20000);

            if (this.AttachControl != null && this.AttachControl.Parent != null)
                this.Location = this.AttachControl.Parent.PointToScreen(new Point(this.AttachControl.Left, this.AttachControl.Bottom - 12));

            Control ctrl = GetParentForm(this.AttachControl);

            Point pp = this.Location;
            Point p = this.AttachControl.Parent.PointToScreen(new System.Drawing.Point(this.AttachControl.Left, this.AttachControl.Bottom - 12));

            if (AttachControl.RightToLeft == System.Windows.Forms.RightToLeft.Yes)
            {
                this.Location = ctrl.PointToScreen(new Point(this.AttachControl.Right, this.AttachControl.Bottom - 18));
                this.Location = new Point(this.Location.X + 18, this.Location.Y + (p.Y - this.Location.Y));
            }
            else
            {
                this.Location = new Point(this.Location.X, this.Location.Y - (this.Location.Y - p.Y) - this.AttachControl.Top);

                if (this.Location.Y < pp.Y)
                {
                    if (this.AttachControl.Dock != DockStyle.None)
                        this.Location = pp;
                    else
                    {
                        //this.Location = new Point(pp.X, pp.Y - 14);
                        this.Location = new Point(pp.X, pp.Y); // Fix the horizontal scrolling issue 
                    }
                }
            }


            if (this.AttachControl.TopLevelControl != null)
            {
                if (this.Location.X > this.AttachControl.TopLevelControl.Right - 20)
                    this.Location = new Point(this.AttachControl.TopLevelControl.Right - this.Width - 2, this.Location.Y);

                if (this.Location.Y > this.AttachControl.TopLevelControl.Bottom - 20)
                    this.Location = new Point(this.Location.X, this.AttachControl.TopLevelControl.Bottom - this.Height - 2);
            }

            //if (this.AttachControl != null && this.AttachControl.TopLevelControl != null)
            //{
            //    if (this.AttachControl.Parent != null && this.AttachControl.Parent is Form)
            //        this.Location = this.AttachControl.Parent.PointToScreen(new Point(this.AttachControl.Parent.Left, this.AttachControl.Parent.Bottom - 10));
            //    else
            //    {
            //        this.Location = this.AttachControl.TopLevelControl.PointToScreen(new System.Drawing.Point(this.AttachControl.Parent.Left, this.AttachControl.Parent.Bottom - 10));
            //    }//this.Location = this.AttachControl.TopLevelControl.PointToScreen(new System.Drawing.Point(this.AttachControl.Parent.Left, this.AttachControl.Parent.Bottom - 10));
            //}
            //else if (this.AttachControl != null && this.AttachControl.Parent != null)
            //    this.Location = this.AttachControl.Parent.PointToScreen(new Point(this.AttachControl.Parent.Left, this.AttachControl.Parent.Bottom - 10));


        }

        protected override void AdjustSize()
        {
            if (this.Height > 12)
                this.Height = 12;

            if (this.AttachControl.Parent != null)
                this.Width = this.AttachControl.Parent.Width - 12;
            else
                this.Width = this.AttachControl.Width - 12;
        }

        protected override void ParentControlResize()
        {
            // this.Width = AttachControl.ClientSize.Width - this.Margin.Left - this.Margin.Right;
            this.Visible = false;
        }

        protected override void ScrollBarResize()
        {
            RecalculateParams();
            //thumbLEftMostLocation = Yoffset + this.Margin.Left;
            //thumbRightMostLoaction = this.Width - ThumbHeight - this.Margin.Right - 2 * buttonHeight - buttonGap;
            //ScrollableLength = thumbRightMostLoaction - thumbLEftMostLocation;
            //PixelFactor = (double)ScrollableLength / Maximum;
            //thumbRect.Y = 0;
            //RightRect = new Rectangle(thumbRightMostLoaction + thumbRect.Height + buttonGap + buttonHeight, thumbRect.Y, buttonHeight, buttonWidth);
            //LeftRect = new Rectangle(thumbRightMostLoaction + thumbRect.Height, thumbRect.Y, buttonHeight, buttonWidth);
            SetNewLocation(0);
        }

        protected override void RecalculateParams()
        {
            thumbRect.Height = ThumbWidth;
            thumbRect.Width = ThumbHeight;
            thumbLEftMostLocation = Yoffset + this.Margin.Left;
            thumbRightMostLoaction = this.Width - ThumbHeight - this.Margin.Right - 2 * buttonHeight - buttonGap;

            ScrollableLength = thumbRightMostLoaction - thumbLEftMostLocation;
            PixelFactor = (double)ScrollableLength / Maximum;
            thumbRect.Y = 0;
            RightRect = new Rectangle(thumbRightMostLoaction + ThumbHeight + buttonGap + buttonHeight, thumbRect.Y, buttonHeight, buttonWidth);
            LeftRect = new Rectangle(thumbRightMostLoaction + ThumbHeight, thumbRect.Y, buttonHeight, buttonWidth);

            SyncThumb(Value);
        }

        protected override int GetAvailableFixedLength()
        {
            return this.Width - this.Margin.Left - 2 * buttonHeight - buttonGap;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            isButtonPressed = false;
            if (LeftRect.Contains(e.Location))
            {
                isButtonPressed = true;
                CurrentJumpType = JumpType.Decrement;
                pageMoveTimer.Enabled = true;

            }
            else if (RightRect.Contains(e.Location))
            {
                CurrentJumpType = JumpType.Increment;
                isButtonPressed = true;
                pageMoveTimer.Enabled = true;


            }
            else if (thumbRect.Contains(e.Location))
            {

                draggingStartLocation = e.X;
                draggingProgress = true;
            }
            else
            {
                isMouseDrag = true;
                pageMoveEnable = true;
                pageMoveTimer.Enabled = true;
                pageMoveBeginArgs = e;

            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {

            if (e.Button == System.Windows.Forms.MouseButtons.Left && draggingProgress)
            {
                int newValue = e.X;
                ChangeScrollPosition(newValue, ThumbLocaion.Center);

            }

            this.Invalidate(false);
            base.OnMouseMove(e);
        }
        #endregion

        #region Draw Methods

        protected override void DrawBackGround(Graphics g)
        {
            Rectangle outterRect = new Rectangle(thumbLEftMostLocation, thumbRect.Y, thumbRightMostLoaction + ThumbHeight + buttonGap + 2 * buttonHeight, ThumbWidth);

            brush.Color = GetColorWithOpaque(ScrollbarBackColor, scrollOpacity);
            linePen.Color = GetColorWithOpaque(ScrollBorderColor, scrollOpacity);
            g.DrawRectangle(linePen, outterRect);
            g.FillRectangle(brush, outterRect);

        }

        protected override void DrawThumb(Graphics g)
        {
            float radius = 4f;

            Rectangle drawRect = new Rectangle(thumbRect.X, thumbRect.Y, ThumbHeight, ThumbWidth);

            // if (IsHighlight(LeftRect) || IsHighlight(RightRect) || IsHighlight(thumbRect))
            if (IsHighlight(this.ClientRectangle))
                brush.Color = GetColorWithOpaque(ThumbBackColor, 100);
            else
                brush.Color = GetColorWithOpaque(ThumbBackColor);

            using (GraphicsPath path = RoundRect(drawRect, radius, radius, radius, radius))
            {
                g.FillPath(brush, path);
            }
        }

        protected override void DrawScrollBarLine(Graphics g)
        {
            Rectangle outterRect = new Rectangle(thumbLEftMostLocation, thumbRect.Y, thumbRightMostLoaction + ThumbHeight - thumbLEftMostLocation, ThumbWidth);

            linePen.Color = GetColorWithOpaque(Color.DarkGray);
            g.DrawRectangle(linePen, outterRect);
        }

        protected override void DrawButton(Graphics g)
        {

            float radius = 4;
            Rectangle rectup = new Rectangle(LeftRect.X, LeftRect.Y, LeftRect.Width, LeftRect.Height);

            Rectangle rectDown = new Rectangle(RightRect.X, RightRect.Y, RightRect.Width, RightRect.Height);

            using (GraphicsPath path = ScrollBarEx.RoundRect(rectup, radius, 0, 0, radius))
            {
                using (GraphicsPath pathdown = ScrollBarEx.RoundRect(rectDown, 0, radius, radius, 0))
                {
                    // if (IsHighlight(RightRect) || IsHighlight(LeftRect) || IsHighlight(thumbRect))
                    if (IsHighlight(this.ClientRectangle))
                        brush.Color = GetColorWithOpaque(ThumbBackColor, 100);
                    else if (!fullyInvisible)
                        brush.Color = GetColorWithOpaque(ThumbBackColor, DefaultOpacity);
                    else
                        brush.Color = GetColorWithOpaque(ThumbBackColor);

                    g.FillPath(brush, path);
                    g.FillPath(brush, pathdown);
                }
            }

            brush.Color = Color.Wheat;

            using (GraphicsPath tringl = GetTriangle(new Point(rectDown.X + rectDown.Width / 2, rectDown.Y + rectDown.Height / 2), 3, 90))
            {
                g.FillPath(brush, tringl);
            }

            using (GraphicsPath tringlleft = GetTriangle(new Point(rectup.X + rectup.Width / 2, rectup.Y + rectup.Height / 2), 3, 270))
            {
                g.FillPath(brush, tringlleft);
            }
        }


        private bool IsHighlight(Rectangle rect)
        {
            Point location = this.PointToClient(MousePosition);

            if (rect.Contains(location))
                return true;
            else
                return false;
        }
        #endregion

        #region Helper Methods

        public void SetRTLAwarePosition(int pos)
        {
            if (pos == 0 && RightToLeftMode)
            {
                Value = Maximum;
            }
            else
            {
                Value = pos;
            }

            SyncThumb(Value);
        }

        private bool iswithinBounds(int boundLength)
        {

            if (boundLength < this.Height - ThumbHeight && boundLength > 0)
                return true;
            else
                return false;

        }

        protected override void JumpSCroll(MouseEventArgs e)
        {
            if (!thumbRect.Contains(e.Location))
            {
                int delta = GetThumbLocation(ThumbLocaion.Center) - e.X;
                int distanceFromBorder = Math.Abs(delta) - ThumbHeight / 2;
                if (distanceFromBorder > 0) // outside Thumb
                {

                    if (delta != 0) // only when outside
                    {
                        int sign = Math.Sign(delta) * -1;
                        SetNewLocation(sign * GetNoOfIndexToJump(StepType.LargeIncrement));
                        //ChangeScrollPosition(newValue, ThumbLocaion.Center);
                    }
                }
            }
        }

        private int GetThumbLocation(ThumbLocaion location)
        {
            switch (location)
            {
                case ThumbLocaion.ThumbTop:
                    return thumbRect.Top + ThumbHeight / 2;


                case ThumbLocaion.Center:
                    return thumbRect.X + ThumbHeight / 2;

                case ThumbLocaion.ThumbBottom:
                    return thumbRect.Top + ThumbHeight / 2;


                default:
                    return thumbRect.Top + ThumbHeight / 2;

            }

        }

        protected override int GetCurrentLocation(ThumbLocaion thumbPos)
        {
            switch (thumbPos)
            {
                case ThumbLocaion.ThumbTop:
                    double pixelLocationTop = Value * PixelFactor;
                    return (int)Math.Round(pixelLocationTop);
                case ThumbLocaion.Center:
                    double pixelLocation = Value * PixelFactor;
                    pixelLocation += ThumbHeight / 2; // center location
                    return (int)Math.Round(pixelLocation);

                case ThumbLocaion.ThumbBottom:
                    double pixelLocationBottom = Value * PixelFactor;
                    pixelLocationBottom += ThumbHeight; // center location
                    return (int)Math.Round(pixelLocationBottom);

                default:
                    double pixelLocationD = Value * PixelFactor;
                    pixelLocationD += ThumbHeight / 2; // center location
                    return (int)Math.Round(pixelLocationD);

            }

        }

        //get offsetByindex
        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (LeftRect.Contains(e.Location))
            {
                SetNewLocation(-GetNoOfIndexToJump(StepType.smallIncrement));
                this.Invalidate();
            }
            else if (RightRect.Contains(e.Location))
            {
                SetNewLocation(GetNoOfIndexToJump(StepType.smallIncrement));
                this.Invalidate();
            }
            else
                base.OnMouseClick(e);
        }

        protected override void SetLocation(int deltaIndex)
        {
            this.Value += deltaIndex;

            if (Value >= Maximum) // min max adjustement
                Value = Maximum;
            else if (Value <= 0)
                Value = 0;

            SyncThumb(Value);
        }

        protected override void SyncThumb(int index)
        {
            if (index > 0 && index < Maximum)
            {
                int topY = GetCurrentLocation(ThumbLocaion.ThumbTop);
                thumbRect.X = topY;
            }
            else if (index == 0)
                thumbRect.X = thumbLEftMostLocation;
            else if (index == Maximum)
                thumbRect.X = thumbRightMostLoaction;
        }

        #endregion

    }
}
