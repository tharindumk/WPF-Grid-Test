using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles
{
    public enum GridHorizontalAlignment
    {
        /// <summary>
        /// Specifies that the contents of a cell are aligned with the left.
        /// </summary>
        Left = 0,
        /// <summary>
        /// Specifies that the contents of a cell are aligned with the right.
        /// </summary>
        Right = 1,
        /// <summary>
        /// Specifies that the contents of a cell are aligned with the center.
        /// </summary>
        Center = 2
    }

    public enum GridTextAlign
    {
        /// <summary>
        /// Default. Use setting defined as default for the cell type.
        /// </summary>
        Default,

        /// <summary>
        /// Align text left of button elements. This is typical for combo boxes.
        /// </summary>
        Left,

        /// <summary>
        /// Align text right of button elements.
        /// </summary>
        Right
    }
}
