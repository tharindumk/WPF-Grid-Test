using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers
{
    public class WindowsApiClass
    {
        public static int WmNonClientLeftButtonDown = 0xA1;
        public static int HitCaption = 0x2;
        public static int HitTop = 12;
        public static int HitBottom = 15;
        public static int HitTopLeft = 13;
        public static int HitTopRight = 14;
        public static int HitBottomLeft = 16;
        public static int HitBottomRight = 17;
        public static int HitLeft = 10;
        public static int HitRight = 11;

        public static int WM_KEYDOWN = 0x100;
        public static int WM_KEYUP = 0x101;

        public static int SB_HORZ = 0;
        public static int SB_VERT = 1;
        public static int SB_CTL = 2;
        public static int SB_BOTH = 3;
        public const int WM_NCCALCSIZE = 0x83;
        public static int WM_SETREDRAW = 11;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern int ShowScrollBar(IntPtr hWnd, int wBar, int bShow);

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string xSection, string xKey, string xVal, string xFileName);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string xSection, string xKey, string xDef, StringBuilder xRetVal, int xSize, string xFileName);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr GetFocus();
    }

    public static class ExtensionMethods
    {
        public static void DoubleBufferButton(this Button xButton, bool setting)
        {
            Type type1 = xButton.GetType();
            PropertyInfo info1 = type1.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            info1.SetValue(xButton, setting, null);
        }
        public static void DoubleBufferPanel(this Panel xPanel, bool setting)
        {
            Type type1 = xPanel.GetType();
            PropertyInfo info1 = type1.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            info1.SetValue(xPanel, setting, null);
        }
        public static void DoubleBufferGrid(this DataGridView xGrid, bool setting)
        {
            Type type1 = xGrid.GetType();
            PropertyInfo info1 = type1.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            info1.SetValue(xGrid, setting, null);
        }
        public static void DoubleBufferLabel(this Label xLabel, bool setting)
        {
            Type type1 = xLabel.GetType();
            PropertyInfo info1 = type1.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            info1.SetValue(xLabel, setting, null);
        }
        public static void DoubleBufferCombo(this ComboBox xCombo, bool setting)
        {
            Type type1 = xCombo.GetType();
            PropertyInfo info1 = type1.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            info1.SetValue(xCombo, setting, null);
        }
        public static void DoubleBufferPicture(this PictureBox xPicture, bool setting)
        {
            Type type1 = xPicture.GetType();
            PropertyInfo info1 = type1.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            info1.SetValue(xPicture, setting, null);
        }
        public static void DoubleBufferRadio(this RadioButton xRadio, bool setting)
        {
            Type type1 = xRadio.GetType();
            PropertyInfo info1 = type1.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            info1.SetValue(xRadio, setting, null);
        }
        public static void DoubleBufferText(this TextBox xText, bool setting)
        {
            Type type1 = xText.GetType();
            PropertyInfo info1 = type1.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            info1.SetValue(xText, setting, null);
        }
        public static void DoubleBufferTab(this TabControl xTab, bool setting)
        {
            Type type1 = xTab.GetType();
            PropertyInfo info1 = type1.GetProperty("DoubleBuffered",
                BindingFlags.Instance | BindingFlags.NonPublic);
            info1.SetValue(xTab, setting, null);
        }
        public static void SuspendDrawing(this System.Windows.Forms.Control xControl)
        {
            WindowsApiClass.SendMessage(xControl.Handle, WindowsApiClass.WM_SETREDRAW, 0, 0);
        }
        public static void ResumeDrawing(this System.Windows.Forms.Control xControl)
        {
            WindowsApiClass.SendMessage(xControl.Handle, WindowsApiClass.WM_SETREDRAW, 1, 0);
            xControl.Refresh();
        }
        public static void SuspendGrid(this DataGridView xGrid)
        {
            WindowsApiClass.SendMessage(xGrid.Handle, WindowsApiClass.WM_SETREDRAW, 0, 0);
        }
        public static void ResumedGrid(this DataGridView xGrid)
        {
            WindowsApiClass.SendMessage(xGrid.Handle, WindowsApiClass.WM_SETREDRAW, 1, 0);
            xGrid.Refresh();
        }
        public static void SuspendForm(this Form xForm)
        {
            WindowsApiClass.SendMessage(xForm.Handle, WindowsApiClass.WM_SETREDRAW, 0, 0);
        }
    }
}
