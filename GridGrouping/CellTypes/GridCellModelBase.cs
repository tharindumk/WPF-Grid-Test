using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes
{
    /// <summary>
    /// once new cell model created , insure to add that model in cellTypes and GridGroupingControl Basic Cell models.
    /// </summary>
    public class GridCellModelBase
    {
        public virtual CellRendererBase GetCellRenderer(GridGroupingControl grid)
        {
            return new CellRendererBase(grid);
        }
    }
}
