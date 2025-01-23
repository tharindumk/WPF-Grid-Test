using System;
using System.Drawing;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes
{
	/// <summary>
	/// Saves Hit-Test information for cell renderers and cell button elements.
	/// </summary>
	public class GridCellHitTestInfo : ICloneable
	{
		private Point point = Point.Empty;
		private int rowIndex = 0;
		private int colIndex = 0;
		private CellRendererBase cellRenderer = null;
		private Rectangle cellBounds = Rectangle.Empty;
		private GridCellButton cellButtonElement = null;
		private Rectangle cellButtonBounds = Rectangle.Empty;
		private int cellButtonIndex = 0;

		/// <summary>
		/// Initializes an empty <see cref="GridCellHitTestInfo"/> object.
		/// </summary>
		public GridCellHitTestInfo()
		{
		}

		/// <summary>
		/// Performs a copy and returns the new object.
		/// </summary>
		/// <returns>A copy of the current object.</returns>
		public object Clone()
		{
			GridCellHitTestInfo cc = new GridCellHitTestInfo();
			cc.point = point;
			cc.rowIndex = rowIndex;
			cc.colIndex = colIndex;
			cc.cellRenderer = cellRenderer;
			cc.cellBounds = cellBounds;
			cc.cellButtonElement = cellButtonElement;
			cc.cellButtonBounds = cellButtonBounds;
			cc.cellButtonIndex = cellButtonIndex;

			return cc;
		}

		/// <override/>
		public override string ToString()
		{
			return String.Concat( 
				"row = ", rowIndex.ToString(), "col = ", colIndex.ToString(),
				"index = ", cellButtonIndex.ToString(), " cellButtonBounds = ", cellButtonBounds,
				"cellButton = ", cellButtonElement.ToString());
		}

		/// <summary>
		/// The mouse coordinates.
		/// </summary>
		public Point Point
		{
			get
			{
				return point;
			}
			set
			{
				point = value;
			}
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
			set
			{
				rowIndex = value;
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
			set
			{
				colIndex = value;
			}
		}

		/// <summary>
		/// The <see cref="GridCellRendererBase"/>.
		/// </summary>
		public CellRendererBase CellRenderer
		{
			get
			{
				return cellRenderer;
			}
			set
			{
				cellRenderer = value;
			}
		}

		/// <summary>
		/// The cell boundaries.
		/// </summary>
		public Rectangle CellBounds
		{
			get
			{
				return cellBounds;
			}
			set
			{
				cellBounds = value;
			}
		}

		/// <summary>
		/// The affected <see cref="GridCellButton"/>.
		/// </summary>
		public GridCellButton CellButtonElement
		{
			get
			{
				return cellButtonElement;
			}
			set
			{
				cellButtonElement = value;
			}
		}

		/// <summary>
		/// The boundaries of the <see cref="GridCellButton"/>.
		/// </summary>
		public Rectangle CellButtonBounds
		{
			get
			{
				return cellButtonBounds;
			}
			set
			{
				cellButtonBounds = value;
			}
		}

		/// <summary>
		/// The index of the <see cref="GridCellButton"/> in the <see cref="GridCellRendererBase"/>.
		/// </summary>
		public int CellButtonIndex
		{
			get
			{
				return cellButtonIndex;
			}
			set
			{
				cellButtonIndex = value;
			}
		}
	}
}
