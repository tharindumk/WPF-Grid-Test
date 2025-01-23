﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes
{
    public class GridSummaryCellModel : GridCellModelBase
    {
        public override CellRendererBase GetCellRenderer(GridGroupingControl grid)
        {
            return new GridSummaryCellRenderer(grid);
        }
    }
}
