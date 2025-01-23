using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Controls;
using System.Windows.Forms;
using System;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;
using System.Collections.Specialized;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping
{
    partial class GridGroupingControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && (components != null))
                {
                    components.Dispose();

                    if (tmrRefresh != null)
                    {
                        this.tmrRefresh.Tick -= tmrRefresh_Tick;
                        this.tmrRefresh.Stop();
                        this.tmrRefresh.Dispose();
                    }

                    if (MouseHandler != null)
                    {
                        MouseHandler.OnMouseUp -= new MouseEventHandler(MouseHandler_OnMouseUp);
                        MouseHandler.OnMouseMove -= new MouseEventHandler(MouseHandler_OnMouseMove);
                        //MouseHandler = null;
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

                    if (toolTip != null)
                        toolTip.Dispose();

                    try
                    {
                        if (grafx != null)
                        {
                            grafx.Dispose();
                            grafx = null;

                            if (context != null)
                            {
                                context.Invalidate();
                                context = null;
                            }
                        }

                        System.Collections.IEnumerator col = this.CellRenderers.content.Values.GetEnumerator();

                        while (col.MoveNext() == true)
                        {
                            Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers.CellRendererBase renderer = col.Current as Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers.CellRendererBase;

                            if (renderer != null)
                                renderer.Dispose();
                        }

                        if (gridPainter != null)
                            gridPainter.Dispose();
                    }
                    catch (System.Exception ex)
                    {
                        Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers.ExceptionsLogger.LogError(ex);
                    }
                }
                base.Dispose(disposing);
            }
            catch (System.Exception ex)
            {
                Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers.ExceptionsLogger.LogError(ex);
            }
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.VScroll = new Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Controls.VScrollBarEx();
            this.tmrUpdate = new System.Windows.Forms.Timer(this.components);
            this.resumeLayoutTimer = new System.Windows.Forms.Timer(this.components);
            this.HScroll = new Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Controls.HScrollBarEx();
            this.SuspendLayout();
            //
            //New Scroll Bar
            //

            //this.NewHScroll = new HScrollBar();
            //this.NewHScroll.Dock = DockStyle.Bottom;
            //this.NewHScroll.LargeChange = 1;
            //this.Controls.Add(this.NewHScroll);
            //this.NewHScroll.ValueChanged += new System.EventHandler(NewHScroll_ValueChanged);

            //this.NewVScroll = new VScrollBar();
            //this.NewVScroll.Dock = DockStyle.Right;
            //this.NewVScroll.LargeChange = 1;
            //this.Controls.Add(this.NewVScroll);
            //this.NewVScroll.ValueChanged += new System.EventHandler(NewVScroll_ValueChanged);

            // 
            // vScrollBar1
            // 
            this.VScroll.AttachControl = this;
            this.VScroll.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.VScroll.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.VScroll.DefaultOpacity = 100;
            this.VScroll.Dock = System.Windows.Forms.DockStyle.Right;
            //this.VScrollBar1.Location = new System.Drawing.Point(357, 0);
            this.VScroll.Maximum = 100;
            this.VScroll.Name = "vScrollBar1";
            this.VScroll.PageSizeIndex = 1;
            this.VScroll.ScrollbarBackColor = System.Drawing.Color.DarkGray;
            this.VScroll.ScrollBorderColor = System.Drawing.Color.WhiteSmoke;
            this.VScroll.Size = new System.Drawing.Size(16, 166);
            this.VScroll.TabIndex = 105;
            this.VScroll.ThumbBackColor = System.Drawing.Color.Gray;
            this.VScroll.ThumbOpacity = 50;
            this.VScroll.ThumbWidth = 16;
            this.VScroll.Value = 0;
            this.VScroll.Maximum = 0;
            this.VScroll.ValueChanged += new System.EventHandler(this.vScrollBar1_ValueChanged);
            // 
            // resumeLayoutTimer
            // 
            this.resumeLayoutTimer.Tick += new System.EventHandler(this.resumeLayoutTimer_Tick_1);
            // 
            // HScrollbar
            // 
            this.HScroll.AttachControl = this;
            this.HScroll.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.HScroll.DefaultOpacity = 100;
            this.HScroll.Dock = System.Windows.Forms.DockStyle.Bottom;
            //this.HScrollbar.Location = new System.Drawing.Point(0, 154);
            this.HScroll.Maximum = 100;
            this.HScroll.Name = "HScrollbar";
            this.HScroll.PageSizeIndex = 1;
            this.HScroll.ScrollbarBackColor = System.Drawing.Color.DarkGray;
            this.HScroll.ScrollBorderColor = System.Drawing.Color.WhiteSmoke;
            this.HScroll.Size = new System.Drawing.Size(357, 12);
            this.HScroll.TabIndex = 107;
            this.HScroll.ThumbBackColor = System.Drawing.Color.Gray;
            this.HScroll.ThumbHeight = 106;
            this.HScroll.ThumbOpacity = 50;
            this.HScroll.ThumbWidth = 16;
            this.HScroll.Value = 0;
            this.HScroll.ValueChanged += new System.EventHandler(this.HorizontalScroll_ValueChanged);
            // 
            // OptimizedGrid
            // 
            //this.Controls.Add(this.HScrollbar);
            //this.Controls.Add(this.VScrollBar1);
            this.Name = "OptimizedGrid";
            this.Size = new System.Drawing.Size(373, 166);
            this.SizeChanged += new System.EventHandler(this.OptimizedGrid_SizeChanged);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.GridGroupingControl_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(GridGroupingControl_MouseUp);
            this.MouseLeave += new EventHandler(GridGroupingControl_MouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.GridGroupingControl_MouseMove);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(GridGroupingControl_MouseClick);
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(GridGroupingControl_MouseDoubleClick);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(GridGroupingControl_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(GridGroupingControl_KeyUp);
            this.Table.OnDraggingChanged += Table_OnDraggingChanged;
            MouseHandler.OnMouseMove += new MouseEventHandler(MouseHandler_OnMouseMove);
            //MouseHandler.OnMouseUp += new MouseEventHandler(MouseHandler_OnMouseUp);
            this.ResumeLayout(false);
        }

        #endregion

        public VScrollBarEx VScroll;
        public HScrollBarEx HScroll;

        public ScrollDataBounds NewVScroll;
        public ScrollDataBounds NewHScroll;

        private System.Windows.Forms.Timer tmrUpdate;
        private System.Windows.Forms.Timer resumeLayoutTimer;
    }
}
