using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers
{
    public class GridUtil
    {
        public static StringAlignment ConvertToStringAlignment(GridVerticalAlignment align)
        {
            switch (align)
            {
                case GridVerticalAlignment.Bottom:
                    return StringAlignment.Far;
                case GridVerticalAlignment.Middle:
                    return StringAlignment.Center;
                default:
                    return StringAlignment.Near;
            }
        }

        public static StringAlignment ConvertToStringAlignment(GridHorizontalAlignment align)
        {
            switch (align)
            {
                case GridHorizontalAlignment.Right:
                    return StringAlignment.Far;
                case GridHorizontalAlignment.Center:
                    return StringAlignment.Center;
                default:
                    return StringAlignment.Near;
            }
        }

        internal static Rectangle GetImageRectangle(Image image, Size clientSize, PictureBoxSizeMode sizeMode)
        {
            Rectangle imageRect = new Rectangle(0, 0, 0, 0);
            if (image != null)
            {
                switch (sizeMode)
                {
                    case PictureBoxSizeMode.Normal:
                    case PictureBoxSizeMode.AutoSize:
                        imageRect.Size = image.Size;
                        break;
                    case PictureBoxSizeMode.StretchImage:
                        imageRect.Size = clientSize;
                        break;

                    case PictureBoxSizeMode.CenterImage:
                        imageRect.Size = image.Size;
                        imageRect.X = ((clientSize.Width - imageRect.Width) / 2);
                        imageRect.Y = ((clientSize.Height - imageRect.Height) / 2);
                        break;
                }
            }
            return imageRect;
        }

        internal static PictureBoxSizeMode ConvertToPictureBoxSizeMode(GridBackgroundImageAlign mode)
        {
            switch (mode)
            {
                case GridBackgroundImageAlign.Normal:
                    return PictureBoxSizeMode.Normal;

                case GridBackgroundImageAlign.CenterImage:
                    return PictureBoxSizeMode.CenterImage;

                case GridBackgroundImageAlign.StretchImage:
                    return PictureBoxSizeMode.StretchImage;
            }
            return PictureBoxSizeMode.Normal;
        }

        public static string GetAlphaLabel(int nCol)
        {
            char[] cols = new char[10];
            int n = 0;
            while (nCol > 0 && n < 9)
            {
                nCol--;
                cols[n] = (char)(nCol % 26 + 'A');
                nCol = nCol / 26;
                n++;
            }

            char[] chs = new char[n];
            for (int i = 0; i < n; i++)
                chs[n - i - 1] = cols[i];

            return new String(chs);
        }

        public static string GetNumericLabel(int nRow)
        {
            return nRow.ToString();
        }

        /// <summary>
        /// Returns a centered rectangle of a specified size within a given rectangle.
        /// </summary>
        /// <param name="rect">The outer rectangle.</param>
        /// <param name="size">The size of the rectangle to be centered.</param>
        /// <returns>The centered rectangle.</returns>
        static public Rectangle CenterInRect(Rectangle rect, Size size)
        {
            int dx = 0;
            if (size.Width < rect.Width)
                dx = rect.Width - size.Width;

            int dy = 0;
            if (size.Height < rect.Height)
                dy = rect.Height - size.Height;

            return new Rectangle(rect.Left + dx / 2, rect.Top + dy / 2,
                Math.Min(size.Width, rect.Width), Math.Min(size.Height, rect.Height));
        }

        internal static string ContainsAny(string txt, List<string> keywords)
        {
            Dictionary<int, string> dic = new Dictionary<int, string>();

            foreach (string item in keywords)
            {
                if (txt.IndexOf(item, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                    dic.Add(txt.IndexOf(item, 0, StringComparison.CurrentCultureIgnoreCase), item);
            }

            if (dic.Count > 0)
            {
                int[] indexes = dic.Keys.ToArray<int>();
                int i = indexes.Min();
                string text;

                if (dic.TryGetValue(i, out text))
                    return text;
                else
                    return string.Empty;
            }
            else
                return string.Empty;
        }
    }
}
