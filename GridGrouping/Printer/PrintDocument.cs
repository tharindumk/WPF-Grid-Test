using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Printing;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Printer
{
    public class GridPrinter
    {
        private PrintDocument printDocument = new PrintDocument();
        private GridGroupingControl grid = null;
        private String titleText = String.Empty;
        private int pageIndex = 0;
        private int pageIndexHorizontal = 0;
        private bool isFirstPagePrint = true;
        private int noOfPages = 0;
        private int noOfHpages = 0;
        private Font font = new Font(FontFamily.GenericSansSerif, 16);
        private SolidBrush brush = new SolidBrush(Color.White);
        private int calcColmStart = 0;
        private SolidBrush blankBrush = new SolidBrush(Color.White);

        public delegate void PrintingStartEvent(string caption);
        public event PrintingStartEvent OnPrintingStarted;

        public delegate void PrintingEndedEvent();
        public event PrintingEndedEvent OnPrintingEnded;
        private bool printingFirstElement = true;

        private int lastGapValue = 0;

        public GridPrinter()
        {
            printDocument.BeginPrint += new PrintEventHandler(printDocument_BeginPrint);
            printDocument.EndPrint += new PrintEventHandler(printDocument_EndPrint);
            printDocument.PrintPage += new PrintPageEventHandler(printDocument_PrintPage);
            printDocument.QueryPageSettings += new QueryPageSettingsEventHandler(printDocument_QueryPageSettings);
        }

        public void PrintGrid(GridGroupingControl gridControl, String title)
        {
            this.grid = gridControl;
            isFirstPagePrint = true;
            titleText = title;

            PrintDialog printDialog = new PrintDialog();

            try
            {

                printDocument.PrintController = new StandardPrintController();
                printDialog.Document = printDocument;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    if (OnPrintingStarted != null)
                    {
                        OnPrintingStarted(title);
                    }

                    gridControl.isPrinting = true;
                    printingFirstElement = true;
                    printDocument.PrinterSettings = printDialog.PrinterSettings;
                    isFirstPagePrint = true;
                    pageIndexHorizontal = 0;

                    if (grid.IsMirrored)
                        calcColmStart = grid.Table.VisibleColumnCount - 1;

                    printDocument.Print();

                    //ThreadStart starter = delegate {  };
                    //Thread thread = new Thread(starter);
                    ////thread.Priority = ThreadPriority.BelowNormal;
                    //thread.Start();

                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
            finally
            {
                gridControl.isPrinting = false;
            }
        }

        void printDocument_QueryPageSettings(object sender, QueryPageSettingsEventArgs e)
        {
            //throw new System.NotImplementedException();
        }

        void printDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {

                if (grid != null)
                {
                    if (isFirstPagePrint)
                    {
                        isFirstPagePrint = false;
                        pageIndex = 0;
                    }

                    isFirstPagePrint = false;
                    int sourceRowCount = 0;

                    if (grid.GridType != GridType.Virtual)
                    {
                        sourceRowCount = grid.Table.RowCount;

                        if (printingFirstElement)
                        {
                            printingFirstElement = false;
                            grid.PopulateAllCellValues();
                        }
                    }
                    else
                    {
                        sourceRowCount = grid.Table.sourceDataRowCount;

                        if (printingFirstElement)
                        {
                            printingFirstElement = false;
                            grid.ExportVirtualToExcelHelper();
                        }
                    }

                    RectangleF rect = new RectangleF(e.MarginBounds.Left, e.MarginBounds.Top - 50, e.MarginBounds.Width, 50);

                    StringFormat drawFormat = new StringFormat();

                    drawFormat.Alignment = StringAlignment.Center;
                    e.Graphics.DrawString(titleText, font, brush, rect, drawFormat);

                    // e.Graphics.SetClip(e.MarginBounds);
                    e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

                    e.Graphics.ResetTransform();
                    grid.Table.SetScale(e.Graphics);

                    int topGap = 0;

                    bool isFirstTime = true;
                    bool isTopFirstTime = true;

                    e.Graphics.TranslateTransform(
                        ((e.MarginBounds.Width) * -1 * pageIndexHorizontal) + e.MarginBounds.Left
                        , ((e.MarginBounds.Height) * -1 * pageIndex) + e.MarginBounds.Top + topGap);

                    int leftVal = ((e.MarginBounds.Width) * pageIndexHorizontal);
                    int topVal = e.MarginBounds.Height * pageIndex;
                    int totalWidth = e.MarginBounds.Right - e.MarginBounds.Left;
                    int totlaHeight = e.MarginBounds.Height;

                    int bufferLeft = 0;
                    int nowVal = totalWidth;
                    bool fNotify = true;

                    int LeftHideVal = 0;
                    int topHideVal = 0;

                    for (int i = 0; i < grid.Table.VisibleColumnCount; i++)
                    {
                        if (grid.Table.VisibleColumns[i].Left + grid.Table.VisibleColumns[i].CellWidth < leftVal)
                            continue;

                        if (grid.Table.VisibleColumns[i].Left + grid.Table.VisibleColumns[i].CellWidth > leftVal + totalWidth)
                        {
                            // i = grid.Table.VisibleColumnCount;
                            if (fNotify)
                            {
                                fNotify = false;
                                nowVal = grid.Table.VisibleColumns[i].Left;
                            }
                            continue;
                        }

                        if (isFirstTime)
                        {
                            isFirstTime = false;
                            bufferLeft = leftVal - grid.Table.VisibleColumns[i].Left;
                            leftVal -= bufferLeft;

                            e.Graphics.TranslateTransform(bufferLeft, 0);
                            lastGapValue += bufferLeft;

                            LeftHideVal = grid.Table.VisibleColumns[i].Left;
                        }

                        int rowHeight = grid.Table.RowHeight;

                        for (int j = 0; j < sourceRowCount; j++)
                        {
                            int rowTop = rowHeight * j;

                            if (rowTop + rowHeight < topVal)
                                continue;

                            if (rowTop + rowHeight > topVal + totlaHeight)
                                continue;

                            if (isTopFirstTime)
                            {
                                isTopFirstTime = false;
                                topHideVal = rowTop;
                            }
                            grid.PaintCellForPrinter(j, i, e.Graphics, true);
                        }
                    }

                    e.Graphics.ResetTransform();
                    grid.Table.SetScale(e.Graphics);

                    e.Graphics.TranslateTransform(
                         ((e.MarginBounds.Width) * -1 * pageIndexHorizontal) + e.MarginBounds.Left + bufferLeft,
                        ((e.MarginBounds.Height) * -1 * pageIndex) + e.MarginBounds.Top);

                    grid.GridPainter.PaintHeaders(e.Graphics, new Rectangle(0, 0, e.MarginBounds.Right - e.MarginBounds.Left, grid.Table.HeaderHeight), true);
                    //e.Graphics.ResetClip();

                    e.Graphics.FillRectangle(blankBrush, nowVal, -100, e.MarginBounds.Width, e.MarginBounds.Height);
                    e.Graphics.FillRectangle(blankBrush, LeftHideVal - e.MarginBounds.Left, -100, e.MarginBounds.Left, e.MarginBounds.Height);
                    e.Graphics.ResetClip();
                    int divVal = (sourceRowCount * grid.Table.RowHeight) / e.MarginBounds.Height;
                    noOfPages = divVal;

                    int widthVal = 0;

                    widthVal = grid.Table.TotalVisibleColumnWidth;

                    int hVal = widthVal / e.MarginBounds.Width;
                    noOfHpages = hVal;

                    e.HasMorePages = (pageIndex < noOfPages || pageIndexHorizontal < noOfHpages);

                    if (pageIndex <= noOfPages)
                        pageIndex++;

                    if (pageIndex > noOfPages)
                    {
                        if (pageIndexHorizontal < noOfHpages)
                            pageIndexHorizontal++;

                        isFirstPagePrint = true;
                    }

                    //if (pageIndexHorizontal < noOfHpages && noOfPages >= pageIndex )
                    //    pageIndexHorizontal++;
                }

            }
            catch (System.Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        void printDocument_EndPrint(object sender, PrintEventArgs e)
        {
            try
            {
                if (OnPrintingEnded != null)
                {
                    OnPrintingEnded();
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

        void printDocument_BeginPrint(object sender, PrintEventArgs e)
        {
            try
            {
                if (OnPrintingStarted != null)
                {
                    OnPrintingStarted(titleText);
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }


    }
}
