using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Controls;
using System.Collections.Specialized;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping
{
    partial class GridGroupingControlMC
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
        }

        #endregion
    }
}
