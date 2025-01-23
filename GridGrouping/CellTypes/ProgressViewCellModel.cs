using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes
{
    class ProgressViewCellModel : GridCellModelBase
    {
        public override CellRendererBase GetCellRenderer(GridGroupingControl grid)
        {
            return new ProgressViewCellRenderer(grid);
        }
    }
}
