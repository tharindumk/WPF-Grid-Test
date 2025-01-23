using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;
using Mubasher.ClientTradingPlatform.Infrastructure.Themes;
using Mubasher.ClientTradingPlatform.Infrastructure.Resources;
using Mubasher.ClientTradingPlatform.Infrastructure.Converter;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.ToolTipAdv
{
    public class ToolTipAdv : ToolTip, IDisposable
    {
        private Pen custPen = new Pen(SystemColors.ActiveBorder, 2.0f);
        private SolidBrush textBrush = new SolidBrush(ColorReference.Instance.LegacyInfoPrimaryBrush);
        private string toolTipHeader = string.Empty;

        public string ToolTipHeader
        {
            get { return toolTipHeader; }
            set { toolTipHeader = value; }
        }

        public SolidBrush TextBrush
        {
            get { return textBrush; }
            set
            {
                if (textBrush != null)
                {
                    textBrush.Dispose();
                }

                textBrush = value;
            }
        }
        private SolidBrush gradientBrush = null;

        private Image headerImage;

        public Image HeaderImage
        {
            get { return headerImage; }
            set { headerImage = value; }
        }

        public SolidBrush GradientBrush
        {
            get { return gradientBrush; }
            set
            {
                if (gradientBrush != null)
                    gradientBrush.Dispose();

                gradientBrush = value;
            }
        }

        public ToolTipAdv()
        {
            this.OwnerDraw = true;
            this.Draw += new DrawToolTipEventHandler(ToolTipAdv_Draw);
            this.Popup += new PopupEventHandler(ToolTipAdv_Popup);

            if (gradientBrush == null)
            {
                gradientBrush = new SolidBrush(ColorReference.Instance.LegacyBodySecondaryBrush);
            }

            //gradientBrush.ResetTransform();
            //gradientBrush.RotateTransform(90);
        }

        protected override void Dispose(bool disposing)
        {
            if (this.gradientBrush != null)
                this.gradientBrush.Dispose();

            if (this.textBrush != null)
                this.textBrush.Dispose();

            if (this.custPen != null)
                this.custPen.Dispose();

            base.Dispose(disposing);
        }

        new public void SetToolTip(Control control, string caption)
        {
            this.SetToolTip(control, caption, string.Empty);
            //base.Show(caption, control);
        }

        public void SetToolTip(Control control, string caption, string header)
        {
            this.ToolTipHeader = header;
            base.SetToolTip(control, caption);

            //base.Show(caption, control);
        }

        void ToolTipAdv_Popup(object sender, PopupEventArgs e)
        {
            int wid = e.ToolTipSize.Width + 40;
            int hgt = e.ToolTipSize.Height + 5;

            if (hgt < 30)
                hgt = 32;

            e.ToolTipSize = new Size(wid, hgt);
        }

        void ToolTipAdv_Draw(object sender, DrawToolTipEventArgs e)
        {
            try
            {
                //gradientBrush.ResetTransform();
                //gradientBrush.RotateTransform(90);
                //gradientBrush.ScaleTransform(e.Bounds.Width, e.Bounds.Height);

                // Draw the background and border.
                // e.DrawBackground();
                textBrush = new SolidBrush(ColorReference.Instance.LegacyInfoPrimaryBrush);
                custPen.Color = SystemColors.ActiveBorder;
                e.Graphics.FillRectangle(gradientBrush, e.Bounds);
                e.Graphics.DrawRectangle(custPen, e.Bounds);


                // Draw the text.
                using (StringFormat sf = new StringFormat())
                {
                    sf.Alignment = StringAlignment.Near;
                    sf.LineAlignment = StringAlignment.Center;

                    if (!string.IsNullOrEmpty(toolTipHeader))
                    {
                        //
                        // Note : When adding headers use line breaks to adjust size of the tool tip from the client side
                        //
                        Rectangle rect = new Rectangle(25, 5, e.Bounds.Width, 20);
                        Rectangle rectText = new Rectangle(25, 2, e.Bounds.Width, e.Bounds.Height + 20);
                        Font boldFont = FontUtil.CreateFont(e.Font.FontFamily.Name, e.Font.Size, FontStyle.Bold);
                        rectText.Location = new Point(4, 20);
                        e.Graphics.DrawString(this.ToolTipHeader, boldFont, textBrush, rect, sf);
                        e.Graphics.DrawString(e.ToolTipText, e.Font, textBrush, rectText, sf);
                    }
                    else
                    {
                        Rectangle rect = new Rectangle(30, 0, e.Bounds.Width, e.Bounds.Height);
                        e.Graphics.DrawString(e.ToolTipText, e.Font, textBrush, rect, sf);
                    }
                }

                e.Graphics.DrawString(UnicodeToFontSymbolConverter.ConvertToChar(ImageFontResources.INFORMATION), FontUtil.GetOrCreateImageFont(), new SolidBrush(ColorReference.Instance.LegacyGridBlinkBlueColorBrush), new Point(5, 6));

                //if (headerImage != null)
                //{
                //    e.Graphics.DrawImage(headerImage, 5, 5, headerImage.Width, headerImage.Height);
                //}
            }
            catch (Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex);
            }
        }

    }
}
