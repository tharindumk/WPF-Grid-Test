using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping
{
    public class GridPaintEventArgs : EventArgs
    {
        private Graphics gridGraphics;

        public Graphics GridGraphics
        {
            get { return gridGraphics; }
            set { gridGraphics = value; }
        }
    }

    public class GridCopyCellsEventArgs : EventArgs
    {

    }

    public class GridQueryCellsEventArgs : EventArgs
    {
        public List<TableInfo.CellStruct> CellsToUpdate;
    }

    public class GridCellMouseDownEventArgs : EventArgs
    {
        public Point MouseLocation = Point.Empty;
        public int RowIndex = -1;
        public int ColIndex = -1;
        public MouseButtons Button = MouseButtons.None;

        public GridCellMouseDownEventArgs(Point mouseLocation, int rowIndex, int colIndex, MouseButtons e = MouseButtons.None)
        {
            this.MouseLocation = mouseLocation;
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
            this.Button = e;
        }
    }

    public class GridCellMouseUpEventArgs : EventArgs
    {
        public Point MouseLocation = Point.Empty;
        public int RowIndex = -1;
        public int ColIndex = -1;
        public MouseButtons Button = MouseButtons.None;

        public GridCellMouseUpEventArgs(Point mouseLocation, int rowIndex, int colIndex, MouseButtons e = MouseButtons.None)
        {
            this.MouseLocation = mouseLocation;
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
            this.Button = e;
        }
    }

    public class GridCellMouseHoverEventArgs : EventArgs
    {
        public Point MouseLocation = Point.Empty;
        public int RowIndex = -1;
        public int ColIndex = -1;
        public MouseButtons Button = MouseButtons.None;
        public bool Cancel = false;
        public String ToolTipString = string.Empty;

        public GridCellMouseHoverEventArgs(Point mouseLocation, int rowIndex, int colIndex, MouseButtons e = MouseButtons.None)
        {
            this.MouseLocation = mouseLocation;
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
            this.Button = e;
            this.Cancel = false;
        }
    }

    public class GridCellMouseHoverLeaveEventArgs : EventArgs
    {
        public Point MouseLocation = Point.Empty;
        public int RowIndex = -1;
        public int ColIndex = -1;
        public MouseButtons Button = MouseButtons.None;

        public GridCellMouseHoverLeaveEventArgs(Point mouseLocation, int rowIndex, int colIndex, MouseButtons e = MouseButtons.None)
        {
            this.MouseLocation = mouseLocation;
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
            this.Button = e;
        }
    }

    public class GridCellMouseClickEventArgs : EventArgs
    {
        public Point MouseLocation = Point.Empty;
        public int RowIndex = -1;
        public int ColIndex = -1;
        public MouseButtons Button = MouseButtons.None;

        public GridCellMouseClickEventArgs(Point mouseLocation, int rowIndex, int colIndex, MouseButtons e = MouseButtons.None)
        {
            this.MouseLocation = mouseLocation;
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
            this.Button = e;
        }
    }

    public class GridCellMouseDoubleClickEventArgs : EventArgs
    {
        public Point MouseLocation = Point.Empty;
        public int RowIndex = -1;
        public int ColIndex = -1;
        public MouseButtons Button = MouseButtons.None;

        public GridCellMouseDoubleClickEventArgs(Point mouseLocation, int rowIndex, int colIndex, MouseButtons e = MouseButtons.None)
        {
            this.MouseLocation = mouseLocation;
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
            this.Button = e;
        }
    }

    public class GridCellButtonClickEventArgs : EventArgs
    {
        public Point MouseLocation = Point.Empty;
        public int RowIndex = -1;
        public int ColIndex = -1;
        public GridCellButton Button = null;
        public MouseButtons MouseButton = MouseButtons.None;

        public GridCellButtonClickEventArgs(Point mouseLocation, int rowIndex, int colIndex, GridCellButton button, MouseButtons e = MouseButtons.None)
        {
            this.MouseLocation = mouseLocation;
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
            this.Button = button;
            MouseButton = e;
        }
    }

    public class GridHeaderRightClickEventArgs : EventArgs
    {
        public Point MouseLocation = Point.Empty;
        public int RowIndex = -1;
        public int ColIndex = -1;
        public bool Cancel = false;

        public GridHeaderRightClickEventArgs(Point mouseLocation, int rowIndex, int colIndex)
        {
            this.MouseLocation = mouseLocation;
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
        }
    }

    public class GridCurrentCellChangingEventArgs : EventArgs
    {
        public Point MouseLocation = Point.Empty;
        public int RowIndex = -1;
        public int ColIndex = -1;
        public TableInfo.CellStruct CurrentCell;

        public GridCurrentCellChangingEventArgs(int rowIndex, int colIndex, TableInfo.CellStruct cell)
        {
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
            this.CurrentCell = cell;
        }
    }

    public class GridCurrentCellChangedEventArgs : EventArgs
    {
        public Point MouseLocation = Point.Empty;
        public int RowIndex = -1;
        public int ColIndex = -1;
        public TableInfo.CellStruct CurrentCell;

        public GridCurrentCellChangedEventArgs(int rowIndex, int colIndex, TableInfo.CellStruct cell)
        {
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
            this.CurrentCell = cell;
        }
    }

    public class GridSelectedRecordChangingEventArgs : EventArgs
    {
        public Record SelectedRecord;
        public int RowIndex = -1;
        public bool Cancel = false;

        public GridSelectedRecordChangingEventArgs(int rowIndex, Record record)
        {
            RowIndex = rowIndex;
            this.SelectedRecord = record;
        }
    }

    public class GridSelectedRecordChangedEventArgs : EventArgs
    {
        public Record SelectedRecord;
        public int RowIndex = -1;

        public GridSelectedRecordChangedEventArgs(int rowIndex, Record record)
        {
            RowIndex = rowIndex;
            this.SelectedRecord = record;
        }
    }

    public class GridCellEventArgs
    {
        int rowIndex;
        int colIndex;

        /// <summary>
        /// Initializes a new <see cref="GridCellEventArgs"/> object.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        public GridCellEventArgs(int rowIndex, int colIndex)
        {
            this.rowIndex = rowIndex;
            this.colIndex = colIndex;
        }

        public int RowIndex
        {
            get
            {
                return rowIndex;
            }
        }

        public int ColIndex
        {
            get
            {
                return colIndex;
            }
        }
    }

    public sealed class GridDrawCellButtonBackgroundEventArgs
    {
        Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes.GridCellButton button;
        Graphics graphics;
        Rectangle bounds;
        ButtonState buttonState;
        Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles.GridStyleInfo style;
        public bool Cancel = false;

        /// <summary>
        /// Initializes a <see cref="GridDrawCellButtonBackgroundEventArgs"/> object.
        /// </summary>
        /// <param name="button">The <see cref="GridCellButton"/> to be drawn.</param>
        /// <param name="graphics">The <see cref="System.Drawing.Graphics"/> context of the canvas.</param>
        /// <param name="bounds">The <see cref="System.Drawing.Rectangle"/> with the bounds.</param>
        /// <param name="buttonState">A <see cref="ButtonState"/> that specifies the current state.</param>
        /// <param name="style">The <see cref="GridStyleInfo"/> object that holds cell information.</param>
        public GridDrawCellButtonBackgroundEventArgs(Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes.GridCellButton button, Graphics graphics, Rectangle bounds,
            ButtonState buttonState, Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles.GridStyleInfo style)
        {
            this.button = button;
            this.graphics = graphics;
            this.bounds = bounds;
            this.buttonState = buttonState;
            this.style = style;
        }

        /// <summary>The <see cref="GridCellButton"/> to be drawn.</summary>

        public Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes.GridCellButton Button
        {
            get
            {
                return button;
            }
        }

        /// <summary>The <see cref="System.Drawing.Graphics"/> context of the canvas.</summary>

        public Graphics Graphics
        {
            get
            {
                return graphics;
            }
        }

        /// <summary>The <see cref="System.Drawing.Rectangle"/> with the bounds.</summary>

        public Rectangle Bounds
        {
            get
            {
                return bounds;
            }
        }

        /// <summary>A <see cref="ButtonState"/> that specifies the current state.</summary>

        public ButtonState ButtonState
        {
            get
            {
                return buttonState;
            }
        }

        /// <summary>The <see cref="GridStyleInfo"/> object that holds cell information.</summary>
        public Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles.GridStyleInfo Style
        {
            get
            {
                return style;
            }
        }
    }

    public class GridCellCancelEventArgs
    {
        int rowIndex;
        int colIndex;
        public bool Cancel;

        /// <summary>
        /// Initializes a new <see cref="GridCellCancelEventArgs"/> object.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        public GridCellCancelEventArgs(int rowIndex, int colIndex)
        {
            this.rowIndex = rowIndex;
            this.colIndex = colIndex;
            this.Cancel = false;
        }

        /// <summary>
        /// The row index.
        /// </summary>
        public int RowIndex
        {
            get
            {
                return rowIndex;
            }
        }

        /// <summary>
        /// The column index.
        /// </summary>
        public int ColIndex
        {
            get
            {
                return colIndex;
            }
        }
    }

    public sealed class GridCellButtonClickedEventArgs : GridCellCancelEventArgs
    {
        int buttonIndex;
        GridCellButton button;

        /// <summary>
        /// Initializes a new object.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <param name="buttonIndex">The index of the clicked cell button element.</param>
        /// <param name="button">A reference to the <see cref="GridCellButton"/> for the clicked button.</param>
        public GridCellButtonClickedEventArgs(int rowIndex, int colIndex, int buttonIndex, GridCellButton button)
            : base(rowIndex, colIndex)
        {
            this.buttonIndex = buttonIndex;
            this.button = button;
        }

        /// <summary>
        /// The index of the clicked cell button element.
        /// </summary>
        public int ButtonIndex
        {
            get
            {
                return buttonIndex;
            }
        }
        /// <summary>
        /// A reference to the <see cref="GridCellButton"/> for the clicked button.
        /// </summary>
        public GridCellButton Button
        {
            get
            {
                return button;
            }
        }
    }
    public sealed class GridDrawCellButtonEventArgs
    {
        GridCellButton button;
        Graphics graphics;
        int rowIndex;
        int colIndex;
        bool isActive;
        Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles.GridStyleInfo style;
        public bool Cancel;

        /// <summary>
        /// Initializes a <see cref="GridDrawCellButtonEventArgs"/> object.
        /// </summary>
        /// <param name="button">The <see cref="GridCellButton"/> to be drawn.</param>
        /// <param name="graphics">The <see cref="System.Drawing.Graphics"/> context of the canvas.</param>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <param name="isActive">True if this is the active current cell; False otherwise.</param>
        /// <param name="style">The <see cref="GridStyleInfo"/> object that holds cell information.</param>
        public GridDrawCellButtonEventArgs(GridCellButton button, Graphics graphics, int rowIndex, int colIndex, bool isActive, Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles.GridStyleInfo style)
        {
            this.button = button;
            this.graphics = graphics;
            this.rowIndex = rowIndex;
            this.colIndex = colIndex;
            this.isActive = isActive;
            this.style = style;
            Cancel = false;
        }

        /// <summary>The <see cref="GridCellButton"/> to be drawn.</summary>
        public GridCellButton Button
        {
            get
            {
                return button;
            }
        }

        /// <summary>The <see cref="System.Drawing.Graphics"/> context of the canvas.</summary>
        public Graphics Graphics
        {
            get
            {
                return graphics;
            }
        }

        /// <summary>The row index.</summary>
        public int RowIndex
        {
            get
            {
                return rowIndex;
            }
        }

        /// <summary>The column index.</summary>
        public int ColIndex
        {
            get
            {
                return colIndex;
            }
        }

        /// <summary>True if this is the active current cell; False otherwise.</summary>
        public bool IsActive
        {
            get
            {
                return isActive;
            }
        }

        /// <summary>The <see cref="GridStyleInfo"/> object that holds cell information.</summary>
        public Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles.GridStyleInfo Style
        {
            get
            {
                return style;
            }
        }
    }

    public class DisplayTextBoxEventArgs
    {
        public Point Location = Point.Empty;
        public int RowIndex = -1;
        public int ColIndex = -1;
        public bool Cancel = false;

        public DisplayTextBoxEventArgs(int rowIndex, int colIndex, Point location)
        {
            this.RowIndex = rowIndex;
            this.ColIndex = colIndex;
            this.Location = location;

            Cancel = false;
        }
    }

    public delegate void GridPaintCellsEventHandler(object sender, GridPaintEventArgs e);
    public delegate void GridCopyCellsEventHandler(object sender, GridCopyCellsEventArgs e);
    public delegate void GridQueryCellsEventHandler(object sender, GridQueryCellsEventArgs e);
    public delegate void GridQueryCellEventHandler(object sender, TableInfo.CellStruct e);
    public delegate void GridQueryCellsEventHandler2(object sender, ref GridQueryCellsEventArgs e);

    public delegate void GridCellMouseDownEventHandler(object sender, GridCellMouseDownEventArgs e);
    public delegate void GridCellMouseUpEventHandler(object sender, GridCellMouseUpEventArgs e);
    public delegate void GridCellMouseHoverEventHandler(object sender, GridCellMouseHoverEventArgs e);
    public delegate void GridCellMouseHoverLeaveEventHandler(object sender, GridCellMouseHoverLeaveEventArgs e);
    public delegate void GridCellMouseClickEventHandler(object sender, GridCellMouseClickEventArgs e);
    public delegate void GridCellMouseDoubleClickEventHandler(object sender, GridCellMouseDoubleClickEventArgs e);
    public delegate void GridCellButtonClickEventHandler(object sender, GridCellButtonClickEventArgs e);
    public delegate void GridHeaderRightClickEventHandler(object sender, GridHeaderRightClickEventArgs e);

    public delegate void GridCurrentCellChangingEventHandler(object sender, GridCurrentCellChangingEventArgs e);
    public delegate void GridCurrentCellChangedEventHandler(object sender, GridCurrentCellChangedEventArgs e);

    public delegate void GridSelectedRecordChangingEventHandler(object sender, GridSelectedRecordChangingEventArgs e);
    public delegate void GridSelectedRecordChangedEventHandler(object sender, GridSelectedRecordChangedEventArgs e);

    public delegate void GridCellEventHandler(object sender, GridCellEventArgs e);
    public delegate void GridDrawCellButtonBackgroundEventHandler(object sender, GridDrawCellButtonBackgroundEventArgs e);
    public delegate void GridCellButtonClickedEventHandler(object sender, GridCellButtonClickedEventArgs e);
    public delegate void GridDrawCellButtonEventHandler(object sender, GridDrawCellButtonEventArgs e);

    public delegate void DisplayTextBoxEventHandler(object sender, DisplayTextBoxEventArgs e);
    public delegate void GridFilteredRecordsHandler(object sender, List<object> filteredSourceItems);
}
