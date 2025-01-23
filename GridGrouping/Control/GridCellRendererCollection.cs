using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Controls
{
    public class GridCellRendererCollection
    {
        CellRendererBase cachedRenderer = null;
        internal Hashtable content = new Hashtable();
        string cachedKey = CellType.Static;
        Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.GridGroupingControl grid;

        public GridCellRendererCollection(GridGroupingControl grid)
        {
            this.grid = grid;
        }

        public virtual CellRendererBase this[string key]
        {
            set
            {
                if (this.content.ContainsKey(key) && content[key] != value)
                    this.content.Remove(key);
                this.content.Add(key, value);
            }
            get
            {
                if (key == cachedKey && cachedRenderer != null)
                    return cachedRenderer;

                if (string.IsNullOrEmpty(key))
                    return cachedRenderer;

                cachedKey = key;
                if (!this.content.ContainsKey(key))
                {
                    cachedKey = key;

                    //
                    //First look in the grid dictionary for grid keys
                    //
                    if (grid.CellModels.ContainsKey(key))
                        cachedRenderer = grid.CellModels[key].GetCellRenderer(grid);
                    else
                        cachedRenderer = grid.CellModels["Static"].GetCellRenderer(grid);


                    if (key.Contains("Header") && grid.Table.IsCustomHeader && grid.CustomHeaderCellRenderer != null)
                        cachedRenderer = grid.CustomHeaderCellRenderer;

                    if (cachedRenderer != null)
                        content.Add(key, cachedRenderer);
                }
                else
                    cachedRenderer = (CellRendererBase)content[key];
                return cachedRenderer;
            }
        }
    }
}
