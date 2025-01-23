using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes;
using System.Collections;
using Mubasher.ClientTradingPlatform.Infrastructure.Module.Logger;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers
{
    [Serializable]
    public class CellRendererBase : IDisposable
    {
        #region Fields

        //protected Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.GridGroupingControl gridControl;
        private ArrayList buttons = null;
        private SolidBrush backBrush = new SolidBrush(GridColorTable.Instance.BackColor);

        #endregion

        #region Properties

        public GridGroupingControl GridControl { get; set; }

        public ArrayList Buttons
        {
            get
            {
                if (buttons == null)
                    buttons = new ArrayList();

                return buttons;
            }
        }

        #endregion

        #region Constructors

        public CellRendererBase(Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.GridGroupingControl grid)
        {
            this.GridControl = grid;
        }

        #endregion

        #region Helper Methods

        public virtual void Draw(Graphics g, Rectangle cellRectangle, int rowIndex, int colIndex, string text, GridStyleInfo style, bool isSelectedCell)
        {
            OnLayout(rowIndex, colIndex, style, cellRectangle);
            DrawBackground(g, cellRectangle, style, rowIndex, colIndex, true, isSelectedCell);
            DrawBorders(g, cellRectangle, style, rowIndex, colIndex, isSelectedCell);
            OnDraw(g, cellRectangle, rowIndex, colIndex, text, style, isSelectedCell);
        }

        public virtual void Draw(Graphics g, Rectangle cellRectangle, ref TableInfo.CellStruct cell, string text, GridStyleInfo style, bool isSelectedCell)
        {
            if (style == null)
                return;

            cellRectangle = OnLayout(cell.RowIndex, cell.ColIndex, style, cellRectangle);
            DrawBackground(g, cellRectangle, style, ref cell, true, isSelectedCell);
            DrawBorders(g, cellRectangle, style, cell.RowIndex, cell.ColIndex, isSelectedCell);

            if (cell.DrawText)
                OnDraw(g, cellRectangle, ref cell, text, style, isSelectedCell);

            DrawButtons(g, ref cellRectangle, ref cell, style);
        }

        private void DrawButtons(Graphics g, ref Rectangle cellRectangle, ref TableInfo.CellStruct cell, GridStyleInfo style)
        {
            GridCellLayout layout = PerformLayout(cell.RowIndex, cell.ColIndex, style, cellRectangle);

            bool showButton = OnQueryShowButtons(cell.RowIndex, cell.ColIndex, style);

            if (showButton && cell.DrawText)
            {
                for (int i = 0; i < Buttons.Count; i++)
                {
                    GridCellButton button;
                    if ((button = GetButton(i)) != null)
                    {
                        if (!layout.Buttons[i].IsEmpty)
                        {
                            button.Bounds = layout.Buttons[i];

                            if (cellRectangle.Contains(button.Bounds))
                            {
                                OnDrawCellButton(button, g, cell.RowIndex, cell.ColIndex, GridControl.Table.HasCurrentCellAt(cell.RowIndex, cell.ColIndex), style);
                            }
                        }
                    }
                }
            }
        }

        public GridCellLayout PerformLayout(int rowIndex, int colIndex, GridStyleInfo style, Rectangle cellRectangle)
        {
            GridCellLayout layout = new GridCellLayout();
            layout.CellRectangle = cellRectangle;

            //GridMargins margins = Grid.Model.StyleInfoBordersToMargins(style);
            //if (Grid.IsRightToLeft())
            //    margins = margins.SwapRightToLeft();
            //layout.InnerRectangle = GridMargins.RemoveMargins(cellRectangle, margins);

            layout.InnerRectangle = cellRectangle;

            int count = Buttons.Count;
            Rectangle[] buttonsBounds = new Rectangle[count];
            layout.ClientRectangle = OnLayout(rowIndex, colIndex, style, layout.InnerRectangle, buttonsBounds);
            for (int n = 0; n < count; n++)
                GetButton(n).Bounds = buttonsBounds[n];
            layout.Buttons = buttonsBounds;

            layout.TextRectangle = layout.ClientRectangle;// RemoveMargins(layout.ClientRectangle, style);
            return layout;
        }

        internal virtual Rectangle OnLayout(int rowIndex, int colIndex, GridStyleInfo style, Rectangle innerBounds, Rectangle[] buttonsBounds)
        {
            Rectangle clientRectangle = innerBounds;
            int count = Buttons.Count;
            if (count > 0 && this.OnQueryShowButtons(rowIndex, colIndex, style))
            {
                //edited by Mubasher - Viraj, to calculate only the visible button count
                //to remove unwanted spaces from the cells with no cell buttons
                int visibleCount = 0;
                innerBounds.Inflate(-1, -1);
                //int nButtonBarWidth = Model.ButtonBarSize.Width;
                int nButtonBarWidth = 0;
                for (int i = 0; i < Buttons.Count; i++)
                {
                    if (((GridCellButton)Buttons[i]).Visible)
                    {
                        visibleCount++;
                        nButtonBarWidth += 16;
                    }

                }
                int width = 0;// nButtonBarWidth / count;
                if (visibleCount > 0)
                {
                    width = nButtonBarWidth / visibleCount;
                }
                clientRectangle.Width -= nButtonBarWidth;

                int height = innerBounds.Height;
                if (height == 0)
                    height = innerBounds.Height;

                GridTextAlign textAlign = style.TextAlign;
                GridVerticalAlignment verticalAlign = style.VerticalAlignment;

                int yOffset = innerBounds.Top;
                if (verticalAlign == GridVerticalAlignment.Middle)
                    yOffset += (innerBounds.Height - height) / 2;
                else if (verticalAlign == GridVerticalAlignment.Bottom)
                    yOffset += (innerBounds.Height - height);

                int xOffset = innerBounds.Left;
                if ((style.TextAlign == GridTextAlign.Right) != GridControl.IsMirrored)
                    clientRectangle.Offset(nButtonBarWidth, 0);
                else
                    xOffset += innerBounds.Width - width * count; //edited by Mubasher - Viraj(only visible button length)

                for (int n = 0; n < count; n++)
                {
                    Rectangle bounds = new Rectangle(xOffset, yOffset, width, height);
                    buttonsBounds[n] = bounds;
                    xOffset += width;
                }
            }
            return clientRectangle;
        }

        protected virtual bool OnQueryShowButtons(int rowIndex, int colIndex, GridStyleInfo style)
        {
            return GridControl.Table.VisibleColumns[colIndex].ShowButtons;
        }

        internal Rectangle PerformCellLayout(int rowIndex, int colIndex, GridStyleInfo style, Rectangle innerBounds)
        {
            return OnLayout(rowIndex, colIndex, style, innerBounds);
        }

        protected virtual Rectangle OnLayout(int rowIndex, int colIndex, GridStyleInfo style, Rectangle innerBounds)
        {
            return innerBounds;
        }

        protected virtual void DrawBackground(Graphics g, Rectangle rect, GridStyleInfo style, ref TableInfo.CellStruct cell, bool fillBackground, bool isSelectedCell)
        {
            try
            {
                if (!isSelectedCell)
                {
                    if (cell.RowIndex % 2 == 0)
                    {
                        backBrush.Color = GridColorTable.Instance.BackColor;
                        g.FillRectangle(backBrush, rect);
                    }
                    else
                    {
                        backBrush.Color = GridColorTable.Instance.BackColorAlt;
                        g.FillRectangle(backBrush, rect);
                    }
                }
                else
                {
                    backBrush.Color = GridColorTable.Instance.BackColorSelectedUp;
                    g.FillRectangle(backBrush, rect);
                }
            }
            catch (Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
            }
        }

        protected virtual void DrawBackground(Graphics g, Rectangle rect, GridStyleInfo style, int rowIndex, int colIndex, bool fillBackground, bool isSelectedCell)
        {

        }

        protected virtual void DrawBorders(Graphics g, Rectangle rect, GridStyleInfo style, int rowIndex, int colIndex, bool isSelectedCell)
        {

        }

        /// <summary>
        /// This method will draw internal Cell Content
        /// </summary>
        /// <param name="g"></param>
        /// <param name="clientRectangle"></param>
        /// <param name="rowIndex"></param>
        /// <param name="colIndex"></param>
        /// <param name="style"></param>
        protected virtual void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, string text, GridStyleInfo style, bool isSelected)
        {

        }

        /// <summary>
        /// This method will draw internal Cell Content
        /// </summary>
        /// <param name="g"></param>
        /// <param name="clientRectangle"></param>
        /// <param name="cell"></param>
        /// <param name="style"></param>
        protected virtual void OnDraw(Graphics g, Rectangle clientRectangle, ref TableInfo.CellStruct cell, string text, GridStyleInfo style, bool isSelected)
        {

        }

        internal void RaiseDrawCellButtonBackground(GridCellButton button, Graphics g, Rectangle rect, ButtonState buttonState, GridStyleInfo style)
        {
            OnDrawCellButtonBackground(button, g, rect, buttonState, style);
        }

        protected virtual void OnDrawCellButtonBackground(GridCellButton button, Graphics g, Rectangle rect, ButtonState buttonState, GridStyleInfo style)
        {
            GridDrawCellButtonBackgroundEventArgs e = new GridDrawCellButtonBackgroundEventArgs(button, g, rect, buttonState, style);
            GridControl.RaiseDrawCellButtonBackground(e);

            if (!e.Cancel)
            {
                button.DrawButton(g, rect, buttonState, style);
            }
        }

        protected virtual void OnDrawCellButton(GridCellButton button, Graphics g, int rowIndex, int colIndex, bool bActive, Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles.GridStyleInfo style)
        {
            GridDrawCellButtonEventArgs e = new GridDrawCellButtonEventArgs(button, g, rowIndex, colIndex, bActive, style);

            GridControl.RaiseDrawCellButton(e);
            if (!e.Cancel)
            {
                button.Draw(g, rowIndex, colIndex, GridControl.Table.HasCurrentCellAt(rowIndex, colIndex)
                    , style);
            }
        }

        protected void AddButton(GridCellButton button)
        {
            if (buttons == null)
                buttons = new ArrayList();

            buttons.Add(button);
            button.Clicked += new GridCellEventHandler(ButtonClicked);
        }

        internal void IntAddButton(GridCellButton button)
        {
            AddButton(button);
        }

        void ButtonClicked(object sender, GridCellEventArgs e)
        {
        }

        protected void RemoveButton(GridCellButton button)
        {
            if (buttons == null)
                return;

            if (buttons.Contains(button))
                buttons.Remove(button);
            button.Clicked -= new GridCellEventHandler(ButtonClicked);
        }

        internal void IntRemoveButton(GridCellButton button)
        {
            RemoveButton(button);
        }

        public GridCellButton GetButton(int index)
        {
            if (buttons == null || index >= buttons.Count)
                return null;
            return (GridCellButton)buttons[index];
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (buttons != null)
            {
                foreach (GridCellButton button in buttons)
                {
                    button.Clicked -= new GridCellEventHandler(ButtonClicked);
                    button.Dispose();
                }
                buttons.Clear();
                buttons = null;
            }

            if (backBrush != null)
                backBrush.Dispose();
        }

        #endregion
    }
}
