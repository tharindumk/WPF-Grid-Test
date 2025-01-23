using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers
{
    public class GlobalMouseHandler : IMessageFilter
    {
        #region Fields And Properties

        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONUP = 0x0202;
        
        public event MouseEventHandler OnMouseMove;
        public event MouseEventHandler OnMouseUp;

        #endregion

        #region IMessageFilter Members

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_MOUSEMOVE)
            {
                if (OnMouseMove != null)
                    OnMouseMove(null, new MouseEventArgs(MouseButtons.None, 0, Cursor.Position.X, Cursor.Position.Y, 0));
            }
            else if (m.Msg == WM_LBUTTONUP)
            {
                if (OnMouseUp != null)
                    OnMouseUp(null, new MouseEventArgs(MouseButtons.Left, 0, Cursor.Position.X, Cursor.Position.Y, 0));
            }
           
            return false;
        }

        #endregion
    }
}
