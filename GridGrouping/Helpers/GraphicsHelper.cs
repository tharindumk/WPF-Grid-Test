using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers
{
    /// <summary>
    /// Class by Mubasher to aid in Drawing 
    /// Background of controls
    /// </summary>
    public class GraphicsHelperGrid
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="g"> Graphis Object</param>
        /// <param name="imgLeft"> Left Part of the Image</param>
        /// <param name="imgFill"> Filling part of the Image</param>
        /// <param name="imgRight">Right part of the Image</param>
        /// <param name="width">Width of the control</param>
        /// <param name="height">Height of the control</param>
        public static void DrawBackgroundImage(Graphics g, Bitmap imgLeft, Bitmap imgFill, Bitmap imgRight,
             int x, int y, int width, int height)
        {
            g.DrawImage(imgLeft, x, y, imgLeft.Width, height);
            g.DrawImage(imgRight, (width - imgRight.Width), y, imgRight.Width, height);

            using (Brush fillBrush = new TextureBrush(imgFill))
            {
                g.FillRectangle(fillBrush, new Rectangle(imgLeft.Width, y, (width - (imgLeft.Width + imgRight.Width)), height));
            }
        }

        public static void DrawBackgroundImage(Graphics g, Bitmap imgLeft, Bitmap imgFill, Bitmap imgRight,
             Rectangle cliRect)
        {
            DrawBackgroundImage(g, imgLeft, imgFill, imgRight, cliRect.X, cliRect.Y, cliRect.Width, cliRect.Height);
        }

        public static Bitmap GetBackgroundImage(Bitmap imgLeft, Bitmap imgFill, Bitmap imgRight,
             int width, int height)
        {
            Bitmap fullImage = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(fullImage);

            g.DrawImage(imgLeft, 0, 0, imgLeft.Width, height);
            g.DrawImage(imgRight, (width - imgRight.Width), 0, imgRight.Width, height);
            using (Brush fillBrush = new TextureBrush(imgFill))
            {
                g.FillRectangle(fillBrush, new Rectangle(imgLeft.Width, 0, (width - (imgLeft.Width + imgRight.Width)), height));
            }

            g.Dispose();
            return fullImage;
        }

        public static void FillImageIntoRectangle(Graphics g, Image img, Rectangle rect)
        {
            using (Brush fillBrush = new TextureBrush(img))
            {
                g.FillRectangle(fillBrush, rect);
            }
        }

        public static Bitmap GetFillImg(Bitmap bgImageFill, RectangleF rectFill)
        {
            Bitmap imgFill = new Bitmap((int)rectFill.Width, (int)rectFill.Height);
            Graphics g = Graphics.FromImage(imgFill);
            TextureBrush brshFill = new TextureBrush(bgImageFill, WrapMode.Tile);
            g.FillRectangle(brshFill, new Rectangle(0, 0, imgFill.Width, imgFill.Height));
            brshFill.Dispose();
            g.Dispose();
            return imgFill;
        }

        /// <summary>
        /// By Mubasher.
        /// Calculate the graphics path that representing the figure in the bitmap 
        /// excluding the transparent color which is the top left pixel.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static GraphicsPath CalculateControlGraphicsPath(Bitmap bitmap)
        {
            // Create GraphicsPath for our bitmap calculation
            GraphicsPath graphicsPath = new GraphicsPath();

            // Use the top left pixel as our transparent color
            Color colorTransparent = bitmap.GetPixel(0, 0);

            // This is to store the column value where an opaque pixel is first found.
            // This value will determine where we start scanning for trailing 
            // opaque pixels.
            int colOpaquePixel = 0;

            // Go through all rows (Y axis)
            for (int row = 0; row < bitmap.Height; row++)
            {
                // Reset value
                colOpaquePixel = 0;

                // Go through all columns (X axis)
                for (int col = 0; col < bitmap.Width; col++)
                {
                    // If this is an opaque pixel, mark it and search 
                    // for anymore trailing behind
                    if (bitmap.GetPixel(col, row) != colorTransparent)
                    {
                        // Opaque pixel found, mark current position
                        colOpaquePixel = col;

                        // Create another variable to set the current pixel position
                        int colNext = col;

                        // Starting from current found opaque pixel, search for 
                        // anymore opaque pixels trailing behind, until a transparent
                        // pixel is found or minimum width is reached
                        for (colNext = colOpaquePixel; colNext < bitmap.Width; colNext++)
                            if (bitmap.GetPixel(colNext, row) == colorTransparent)
                                break;

                        // Form a rectangle for line of opaque pixels found and 
                        // add it to our graphics path
                        graphicsPath.AddRectangle(new Rectangle(colOpaquePixel,
                                                   row, colNext - colOpaquePixel, 1));

                        // No need to scan the line of opaque pixels just found
                        col = colNext;
                    }
                }
            }

            // Return calculated graphics path
            return graphicsPath;
        }


        public static Color AdjustColorRGB(Color color, int value)
        {
            int r = 0;
            int g = 0;
            int b = 0;

            if (value < 0)
            {
                value = -1 * value;
                r = color.R > value ? color.R - value : 0;
                g = color.G > value ? color.G - value : 0;
                b = color.B > value ? color.B - value : 0;
            }
            else
            {
                r = color.R < 255 - value ? color.R + value : 255;
                g = color.G < 255 - value ? color.G + value : 255;
                b = color.B < 255 - value ? color.B + value : 255;
            }

            return Color.FromArgb(color.A, r, g, b);
        }

        public static bool IsWindows8
        {
            get
            {
                if (Environment.OSVersion.Version.Major >= 6 && Environment.OSVersion.Version.Minor == 2)
                    return true;
                else
                    return false;

            }
        }

        public static Color AdjustColorLuminosity(Color color, int value)
        {
            HslColorGrid HSLcolor = new HslColorGrid(color);

            HSLcolor.Luminosity += value;

            Color c2 = (Color)HSLcolor;

            return c2;
        }

        private const double altColorVariation = 1;
        private const double borderColorVariation = 0.925;

        public static Color GetAltColor(Color color, double variation = altColorVariation)
        {
            return Color.FromArgb(GetAltChannel(color.R, variation), GetAltChannel(color.G, variation), GetAltChannel(color.B, variation));
        }

        public static Color GetBorderColor(Color color)
        {
            return Color.FromArgb(GetAltChannel(color.R, borderColorVariation), GetAltChannel(color.G, borderColorVariation), GetAltChannel(color.B, borderColorVariation));
        }

        private static byte GetAltChannel(byte value, double variation)
        {
            double newValue = (value * (variation));

            byte newByte;

            if (byte.TryParse(Math.Round(newValue).ToString(), out newByte))
            {
                return newByte;
            }

            return value;
        }
    }
}
