using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;
using System.Drawing.Drawing2D;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers
{
    public class ProgressViewCellRenderer : GridStaticCellRenderer
    {
        private SolidBrush br = new SolidBrush(Color.White);

        #region Constructors

        public ProgressViewCellRenderer(Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.GridGroupingControl grid)
            : base(grid)
        {

        }

        #endregion

        #region Base Overrides

        protected override void OnDraw(Graphics g, Rectangle clientRectangle, ref TableInfo.CellStruct cell, string text, GridStyleInfo style, bool isSelected)
        {
            if (cell.TextDouble > 0)
            {
                Rectangle progressBar = new Rectangle(clientRectangle.X, clientRectangle.Y, (int)(clientRectangle.Width * cell.TextDouble / 100), clientRectangle.Height);
                progressBar.Inflate(-2, -3); // for borders.

                if (progressBar.Width == 0 || progressBar.Height == 0)
                    return;

                using (SolidBrush lgBrush = new SolidBrush(GridColorTable.Instance.ProgressLightColor))
                {
                    //lgBrush.SetSigmaBellShape(0.5f);
                    g.FillRectangle(lgBrush, progressBar);
                }

                br.Color = style.TextColor;
                string cellString = text + " % ";
                SizeF strSize = g.MeasureString(cellString, cell.Style.Font.GdipFont);
                int xStr = clientRectangle.X + (clientRectangle.Width / 2 - (int)strSize.Width / 2);
                int yStr = clientRectangle.Y + (clientRectangle.Height / 2 - (int)strSize.Height / 2);

                g.DrawString(cellString, cell.Style.Font.GdipFont, br, xStr, yStr);
            }
        }

        #endregion

    }
}
