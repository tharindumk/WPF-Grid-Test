using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles
{
    public enum GridVerticalAlignment
    {
        /// <summary>
        /// Specifies that the contents of a cell are aligned with the top.
        /// </summary>
        Top = 0,
        /// <summary>
        /// Specifies that the contents of a cell are aligned with the center.
        /// </summary>
        Middle = 1,
        /// <summary>
        /// Specifies that the contents of a control are aligned with the bottom.
        /// </summary>
        Bottom = 2
    }

    public enum GridBackgroundImageAlign
    {

        /// <summary>
        /// The image is placed in the upper-left corner of the cell. The image is clipped if it is larger than the cell it is contained in.
        /// </summary>
        Normal,

        /// <summary>
        /// The image is displayed in the center if the cell is larger than the image. If the image is larger than the cell, the picture is placed in the center of the cell and the outside edges are clipped.
        /// </summary>
        CenterImage,

        /// <summary>
        /// The image within the cell is stretched or shrunk to fit the size of the cell.
        /// </summary>
        StretchImage
        //TODO: TileImage
        //TODO: AlphaBlend Interior
    }
}
