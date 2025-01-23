#region Copyright Syncfusion Inc. 2001 - 2008
//
//  Copyright Syncfusion Inc. 2001 - 2008. All rights reserved.
//
//  Use of this code is subject to the terms of our license.
//  A copy of the current license can be obtained at any time by e-mailing
//  licensing@syncfusion.com. Re-distribution in any form is strictly
//  prohibited. Any infringement will be prosecuted under applicable laws. 
//
#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using System.Drawing.Drawing2D;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Renderers;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Styles;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;
using Mubasher.ClientTradingPlatform.Infrastructure.Module.Logger;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes
{

    /// <summary>
    /// Defines a cell button element to be used with a cell renderer. A cell renderer can have several cell button elements. 
    /// Examples are numeric up and down buttons, combo box buttons, etc..
    /// </summary>
    /// <remarks>
    /// The cell button is XP Themes enabled. It will be drawn themed if <see cref="GridControlBase.ThemesEnabled"/> is true.
    /// </remarks>
    public class GridCellButton : IDisposable
    {
        protected CellRendererBase owner;
        protected Rectangle bounds;
        protected string text = "";
        protected GridCellHitTestInfo mouseDownHitTestInfo = null;
        protected bool fireClickOnMouseUp = true;
        //private ThemedPushButtonDrawing themedDrawing = null;
        private int radius = 2;
        protected GridCellContextValue hovering = new GridCellContextValue(false);
        protected GridCellContextValue mouseDown = new GridCellContextValue(false);
        protected GridCellContextValue pushed = new GridCellContextValue(false);


        //Added by Mubasher - Viraj
        // to get the button visibility
        protected bool visible = true;

        // events

        /// <summary>
        /// User clicked on cell button element.
        /// </summary>
        public event GridCellEventHandler Clicked;

        /// <summary>
        /// User hovers mouse over cell button element or moved mouse away from cell button element.
        /// </summary>
        public event GridCellEventHandler HoveringChanged;

        /// <summary>
        /// User pressed or released the mouse button while the cursor was over the cell button element.
        /// </summary>
        public event GridCellEventHandler MouseDownChanged;

        /// <summary>
        /// User moved mouse away from cell button element or back into it while pressing the mouse.
        /// </summary>
        public event GridCellEventHandler PushedChanged;

        // construction, destrcution

        /// <summary>
        /// Initializes a new <see cref="GridCellButton"/> and associates it with a <see cref="GridCellRendererBase"/>.
        /// </summary>
        /// <param name="owner">The <see cref="GridCellRendererBase"/> that manages the <see cref="GridCellButton"/>.</param>
        public GridCellButton(CellRendererBase owner)
        {
            this.owner = owner;
        }

        // Member functions

        /// <summary>
        /// Specifies the coordinates of the cell button element in grid client area coordinates.
        /// </summary>
        public virtual Rectangle Bounds
        {
            get
            {
                return this.bounds;
            }
            set
            {
                this.bounds = value;
            }
        }

        /// <summary>
        /// Added by Mubasher - Viraj
        /// to get the button visibility
        /// </summary>
        public bool Visible
        {
            get { return this.visible; }
            set { this.visible = value; }
        }


        public string ToolTip = string.Empty;


        // Text

        /// <summary>
        /// Text to be displayed inside cell button element.
        /// </summary>
        public virtual string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }

        /// <summary>
        /// Saved HitTest information.
        /// </summary>
        protected GridCellHitTestInfo MouseDownInfo
        {
            get
            {
                return this.mouseDownHitTestInfo;
            }
        }

        //
        //Added by Mubasher
        //
        private bool doBlink = false;

        public bool DoBlink
        {
            get { return doBlink; }
            set
            {
                if (value == true)
                    isBlinkButton = true;
                if (value == false)
                    blinkState = ButtonBlinkState.Default;

                doBlink = value;
            }
        }

        private bool isBlinkButton = false;

        public bool IsBlinkButton
        {
            get { return isBlinkButton; }
            set { isBlinkButton = value; }
        }

        private bool hasValues = true;

        public bool HasValues
        {
            get { return hasValues; }
            set { hasValues = value; }
        }

        private bool isCustomDrawing = false;

        public bool IsCustomDrawing
        {
            get { return isCustomDrawing; }
            set { isCustomDrawing = value; }
        }

        private CustomCellButtonType buttonType = CustomCellButtonType.Square;

        public CustomCellButtonType ButtonType
        {
            get { return buttonType; }
            set { buttonType = value; }
        }

        private ButtonBlinkState blinkState = ButtonBlinkState.Default;

        public ButtonBlinkState BlinkState
        {
            get { return blinkState; }
            set { blinkState = value; }
        }

        /// <summary>
        /// Draws the cell button element at the specified row and column index. 
        /// </summary>
        /// <param name="g">The <see cref="System.Drawing.Graphics"/> context of the canvas.</param>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <param name="bActive">True if this is the active current cell; False otherwise.</param>
        /// <param name="style">The <see cref="GridStyleInfo"/> object that holds cell information.</param>
        public virtual void Draw(Graphics g, int rowIndex, int colIndex, bool bActive, GridStyleInfo style)
        {
            ButtonState buttonState = ButtonState.Normal;
            Point mouseClientPosition = Grid.GetWindow().PointToClient(Control.MousePosition);
            bool isHovering = IsHovering(rowIndex, colIndex);
            if (isHovering && !this.bounds.Contains(mouseClientPosition))
            {
                this.hovering.ResetValue();
                isHovering = false;
            }
            bool isMouseDown = IsMouseDown(rowIndex, colIndex);
            if (isMouseDown && !this.bounds.Contains(mouseClientPosition))
            {
                this.mouseDown.ResetValue();
                isMouseDown = false;
            }
            isMouseDown |= IsPushed(rowIndex, colIndex) && Grid.Table.HasCurrentCellAt(rowIndex, colIndex);

            Rectangle rect = Bounds;
            Rectangle faceRect = Rectangle.FromLTRB(rect.Left + 1, rect.Top + 1, rect.Right - 2, rect.Bottom - 2);

            bool disabled = !style.Enabled;
            if (disabled)
                buttonState |= ButtonState.Inactive | ButtonState.Flat;

            else if (!isHovering && !isMouseDown)
                buttonState |= ButtonState.Flat;

            //
            //Modified by Mubasher to allow blink push buttons
            //
            //if (doBlink)
            //    buttonState = ButtonState.Checked;
            if (isMouseDown)
            {
                buttonState |= ButtonState.Pushed;
                faceRect.Offset(1, 1);
            }

            owner.RaiseDrawCellButtonBackground(this, g, rect, buttonState, style);
            // DrawButton(g, rect, buttonState);

            doBlink = false;

            string text = Text;
            if (text != null && text.Length > 0)
            {
                bool focusRect = bActive;
                Font font = style.GdipFont;

                StringAlignment alignment = StringAlignment.Center;

                //if (style.TextAlign == GridTextAlign.Left)
                //    alignment = StringAlignment.Near;
                //else if (style.TextAlign == GridTextAlign.Right)
                //    alignment = StringAlignment.Far;

                StringFormat format = new StringFormat();

                format.Alignment = alignment; // StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;
                //TODO
                //format.HotkeyPrefix = style.HotkeyPrefix;
                format.Trimming = style.Trimming;
                if (!style.WrapText)
                    format.FormatFlags = StringFormatFlags.NoWrap;


                Color textColor = style.TextColor;

                if (disabled)
                    ControlPaint.DrawStringDisabled(g, Text, font, textColor, faceRect, format);
                else
                {
                    using (SolidBrush br = new SolidBrush(textColor))
                    {
                        g.DrawString(Text, font, br, faceRect, format);
                    }
                }

                if (focusRect)
                {
                    Size size = g.MeasureString(Text, font, faceRect.Width).ToSize();
                    Rectangle r = GridUtil.CenterInRect(faceRect, size);
                    ControlPaint.DrawFocusRectangle(g, r);
                }
                format.Dispose();
            }
        }

        /// <summary>
        /// Draws a button using <see cref="ControlPaint.DrawButton"/> or if XP Themes
        /// are enabled, button will be drawn themed.
        /// </summary>
        /// <param name="g">The <see cref="System.Drawing.Graphics"/> context of the canvas.</param>
        /// <param name="rect">The <see cref="System.Drawing.Rectangle"/> with the bounds.</param>
        /// <param name="buttonState">A <see cref="ButtonState"/> that specifies the current state.</param>
        /// <param name="style">The style information for the cell.</param>
        public virtual void DrawButton(Graphics g, Rectangle rect, ButtonState buttonState, GridStyleInfo style)
        {
            DrawThemedButton(g, rect, buttonState, style);
        }

        private void DrawThemedButton(Graphics g, Rectangle rect, ButtonState buttonState, GridStyleInfo style)
        {
            float x = rect.X, y = rect.Y, w = rect.Width, h = rect.Height;
            float r1 = radius, r2 = radius, r3 = radius, r4 = radius;

            //Commented. Dont use Smoothing mode in this as this result additional fading in lines.
            //g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            w--; h -= 1;
            using (GraphicsPath rr1 = new GraphicsPath())
            {
                if (buttonType == CustomCellButtonType.Rounded)
                {
                    rr1.AddBezier(x, y + r1, x, y, x + r1, y, x + r1, y);
                    rr1.AddLine(x + r1, y, x + w - r2, y);
                    rr1.AddBezier(x + w - r2, y, x + w, y, x + w, y + r2, x + w, y + r2);
                    rr1.AddLine(x + w, y + r2, x + w, y + h - r3);
                    rr1.AddBezier(x + w, y + h - r3, x + w, y + h, x + w - r3, y + h, x + w - r3, y + h);
                    rr1.AddLine(x + w - r3, y + h, x + r4, y + h);
                    rr1.AddBezier(x + r4, y + h, x, y + h, x, y + h - r4, x, y + h - r4);
                    rr1.AddLine(x, y + h - r4, x, y + r1);
                }
                else if (buttonType == CustomCellButtonType.Square)
                {
                    rr1.AddLine(x + 2, y, x + w - 2, y);
                    rr1.AddLine(x + w, y, x + w, y + h);
                    rr1.AddLine(x + w, y + h, x, y + h);
                    rr1.AddLine(x, y + h, x, y);
                }

                Color textColor = style.TextColor;
                Color internalBorderColor = GridColorTable.Instance.ButtonSelectedInternalBorderColor;
                Color borderColor = GridColorTable.Instance.ButtonDefaultInternalBorderColor;

                if (isBlinkButton && doBlink && style.BackgroundImage == null)
                {
                    if (blinkState != ButtonBlinkState.Default)
                    {
                        if (blinkState == ButtonBlinkState.Up)
                        {
                            using (Brush brsh = new SolidBrush(GridColorTable.Instance.ButtonUpBottomColor))
                            {
                                g.FillPath(brsh, rr1);

                                using (Pen pen = new Pen(GridColorTable.Instance.ButtonUpBorderColor, 0.8f))
                                {
                                    g.DrawPath(pen, rr1);
                                }
                            }

                            //internalBorderColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(Office2007Colors.Default.ButtonUpBorderColor, 30);
                            internalBorderColor = GraphicsHelperGrid.AdjustColorLuminosity(GridColorTable.Instance.ButtonUpBottomColor, 45);
                            borderColor = GridColorTable.Instance.ButtonUpBorderColor;
                            textColor = GridColorTable.Instance.TextColorBlinkUp;
                        }
                        else if (blinkState == ButtonBlinkState.Down)
                        {
                            using (Brush brsh = new SolidBrush(GridColorTable.Instance.ButtonDownBottomColor))
                            {
                                g.FillPath(brsh, rr1);

                                using (Pen pen = new Pen(GridColorTable.Instance.ButtonDownBorderColor, 0.8f))
                                {
                                    g.DrawPath(pen, rr1);
                                }
                            }

                            //internalBorderColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(Office2007Colors.Default.ButtonDownBorderColor, 30);
                            internalBorderColor = GraphicsHelperGrid.AdjustColorLuminosity(GridColorTable.Instance.ButtonDownBottomColor, 45);
                            borderColor = GridColorTable.Instance.ButtonDownBorderColor;
                            textColor = GridColorTable.Instance.TextColorBlinkDown;
                        }
                        else if (blinkState == ButtonBlinkState.Update)
                        {
                            using (Brush brsh = new SolidBrush(GridColorTable.Instance.ButtonValueUpdateBottomColor))
                            {
                                g.FillPath(brsh, rr1);

                                using (Pen pen = new Pen(GridColorTable.Instance.ButtonValueUpdateBorderColor, 0.8f))
                                {
                                    g.DrawPath(pen, rr1);
                                }
                            }

                            internalBorderColor = GraphicsHelperGrid.AdjustColorLuminosity(GridColorTable.Instance.ButtonValueUpdateBottomColor, 45);
                            borderColor = GridColorTable.Instance.ButtonValueUpdateBorderColor;
                            textColor = GridColorTable.Instance.TextColorBlinkUpdate;
                        }
                    }
                }
                else
                {
                    if (buttonState == ButtonState.Flat)
                    {
                        Color bottomColor = style.BackColor;
                        Color topColor = GraphicsHelperGrid.AdjustColorLuminosity(style.BackColor, 20);

                        //
                        //Border color should be draker than back color
                        //
                        borderColor = GraphicsHelperGrid.AdjustColorLuminosity(style.BackColor, -25);

                        using (Brush brsh = new SolidBrush(bottomColor))
                        {
                            g.FillPath(brsh, rr1);

                            using (Pen pen = new Pen(borderColor, 0.8f))
                            {
                                g.DrawPath(pen, rr1);
                            }

                            internalBorderColor = GraphicsHelperGrid.AdjustColorLuminosity(style.BackColor, 45);

                            HslColorGrid hslColor = new HslColorGrid(internalBorderColor);

                            //
                            //If the brightness is too much, revert to back color
                            //
                            if (hslColor.Luminosity > 240)
                                internalBorderColor = style.BackColor;

                            //borderColor = Office2007Colors.Default.ButtonDefaultInternalBorderColor;
                        }
                    }
                    else if (buttonState == ButtonState.Checked)
                    {
                        Color bottomColor = GridColorTable.Instance.ButtonSelectedInternalBorderColor;
                        Color topColor = GraphicsHelperGrid.AdjustColorLuminosity(bottomColor, 20);

                        //
                        //Border color should be draker than back color
                        //
                        borderColor = GraphicsHelperGrid.AdjustColorLuminosity(bottomColor, -25);

                        using (Brush brsh = new SolidBrush(bottomColor))
                        {
                            g.FillPath(brsh, rr1);

                            using (Pen pen = new Pen(GridColorTable.Instance.ButtonSelectedInternalBorderColor, 0.8f))
                            {
                                g.DrawPath(pen, rr1);
                            }

                            internalBorderColor = GraphicsHelperGrid.AdjustColorLuminosity(bottomColor, 45);

                            HslColorGrid hslColor = new HslColorGrid(internalBorderColor);

                            //
                            //If the brightness is too much, revert to back color
                            //
                            if (hslColor.Luminosity > 240)
                                internalBorderColor = bottomColor;

                            //internalBorderColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(Office2007Colors.Default.ButtonSelectedBorderColor, 45);
                            //borderColor = Office2007Colors.Default.ButtonSelectedBorderColor;
                        }
                    }
                    //else if (buttonState == ButtonState.Pushed)
                    //{
                    //    using (Brush brsh = new LinearGradientBrush(rect, Office2007Colors.Default.ButtonPressedTopColor, Office2007Colors.Default.ButtonPressedBottomColor, 90F))
                    //    {
                    //        g.FillPath(brsh, rr1);
                    //        g.DrawPath(new Pen(Office2007Colors.Default.ButtonPressedBorderColor, 0.8f), rr1);
                    //        internalBorderColor = Syncfusion.Windows.Forms.Tools.GraphicsHelperSyncfusion.AdjustColorLuminosity(Office2007Colors.Default.ButtonPressedBorderColor, 45);
                    //        borderColor = Office2007Colors.Default.ButtonPressedBorderColor;
                    //    }
                    //}
                    else
                    {
                        Color bottomColor = GridColorTable.Instance.ButtonSelectedInternalBorderColor;
                        Color topColor = GraphicsHelperGrid.AdjustColorLuminosity(bottomColor, 20);

                        //
                        //Border color should be draker than back color
                        //
                        borderColor = GraphicsHelperGrid.AdjustColorLuminosity(bottomColor, -25);

                        using (Brush brsh = new SolidBrush(bottomColor))
                        {
                            g.FillPath(brsh, rr1);

                            using (Pen pen = new Pen(borderColor, 0.8f))
                            {
                                g.DrawPath(pen, rr1);
                            }

                            internalBorderColor = GraphicsHelperGrid.AdjustColorLuminosity(bottomColor, 45);

                            HslColorGrid hslColor = new HslColorGrid(internalBorderColor);

                            //
                            //If the brightness is too much, revert to back color
                            //
                            if (hslColor.Luminosity > 240)
                                internalBorderColor = bottomColor;

                            //borderColor = Office2007Colors.Default.ButtonDefaultInternalBorderColor;
                        }
                    }
                }

                //
                //Draw Outer Borders
                //
                if (buttonType == CustomCellButtonType.Square)
                {
                    if (!Grid.IsMirrored)
                    {
                        using (Pen pen = new Pen(borderColor, 0.8f))
                        {
                            g.DrawLine(pen, x, y, x + w - 1, y);
                            pen.Color = internalBorderColor;
                            g.DrawLine(pen, x + 1, y + 1, x + w - 1, y + 1);
                            pen.Color = internalBorderColor;
                            g.DrawLine(pen, x + 1, y + h - 1, x + 1, y + 1);
                        }
                    }
                    else
                    {
                        using (Pen pen = new Pen(borderColor, 0.8f))
                        {
                            g.DrawLine(pen, x, y, x + w - 1, y);
                            pen.Color = internalBorderColor;
                            g.DrawLine(pen, x + 1, y + 1, x + w - 1, y + 1);
                            pen.Color = internalBorderColor;
                            g.DrawLine(pen, x + w - 1, y + h - 1, x + w - 1, y + 1);
                        }
                    }
                }

                if (!isCustomDrawing && style.BackgroundImage == null)
                {
                    //
                    //Draw Text on Button using Static Cell Renderer
                    //
                    bool disabled = !style.Enabled;

                    bool isTextRightToLeft = style.RightToLeft == RightToLeft.Inherit && Grid.IsMirrored || style.RightToLeft == RightToLeft.Yes;
                    Rectangle buttonBounds = Bounds;
                    buttonBounds.Width -= 1;
                    //TODO
                    //GridStaticCellRenderer.DrawText(g, style.CellValue.ToString(), style.GdipFont, buttonBounds, style, textColor, isTextRightToLeft);
                }

                if (style.BackgroundImage != null)
                {
                    Image img = style.BackgroundImage;

                    int xImage = rect.X + (rect.Width / 2) - (img.Width / 2);
                    int yImage = rect.Y;

                    Rectangle rectImage = new Rectangle(xImage, yImage, img.Width, img.Height);

                    g.DrawImage(img, rectImage);
                }
            }
        }

        // system settings - see Microsoft.Win32.SystemEvents

        /// <summary>
        /// A reference to the parent grid.
        /// </summary>
        protected GridGroupingControl Grid
        {
            get
            {
                return this.owner.GridControl;
            }
        }

        /// <summary>
        /// A reference to the <see cref="GridCellRendererBase"/>.
        /// </summary>
        public CellRendererBase Owner
        {
            get
            {
                return this.owner;
            }
        }

        /// <summary>
        /// Specifies if button should fire a <see cref="Clicked"/> event when user clicks on button.
        /// </summary>
        public bool FireClickOnMouseUp
        {
            get
            {
                return fireClickOnMouseUp;
            }
            set
            {
                fireClickOnMouseUp = value;
            }
        }

        /// <summary>
        /// Determines whether the mouse is currently hovering over the button at the specified row and column.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <returns>True if mouse is over the button; False otherwise.</returns>
        public bool IsHovering(int rowIndex, int colIndex)
        {
            return (bool)hovering.GetValue(rowIndex, colIndex);
        }

        /// <summary>
        /// Saves current hovering state.
        /// </summary>
        /// <param name="ht">The <see cref="GridCellHitTestInfo"/> with row and column index.</param>
        /// <param name="value">True to set hovering; False to reset hovering.</param>
        public void SetHovering(GridCellHitTestInfo ht, bool value)
        {
            if (hovering.SetValue(ht.RowIndex, ht.ColIndex, value))
            {
                Rectangle r = ht.CellButtonBounds;
                OnHoveringChanged(new GridCellEventArgs(ht.RowIndex, ht.ColIndex));
            }
        }


        /// <summary>
        /// Determines whether the mouse is currently pressed at the specified row and column.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <returns>True if mouse is pressed over the button; False otherwise.</returns>
        public bool IsMouseDown(int rowIndex, int colIndex)
        {
            return (bool)mouseDown.GetValue(rowIndex, colIndex);
        }

        /// <summary>
        /// Saves current MouseDown state.
        /// </summary>
        /// <param name="ht">The <see cref="GridCellHitTestInfo"/> with row and column index.</param>
        /// <param name="value">True to set mouse down; False to reset mouse down state.</param>
        public void SetMouseDown(GridCellHitTestInfo ht, bool value)
        {
            if (mouseDown.SetValue(ht.RowIndex, ht.ColIndex, value))
            {
                OnMouseDownChanged(new GridCellEventArgs(ht.RowIndex, ht.ColIndex));
            }
        }

        /// <summary>
        /// Determines whether the button is marked as pushed at the specified row and column.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <returns>True if the button is marked as pushed; False otherwise.</returns>
        public bool IsPushed(int rowIndex, int colIndex)
        {
            return (bool)pushed.GetValue(rowIndex, colIndex);
        }

        /// <summary>
        /// Saves current pushed state.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <param name="bounds">The <see cref="System.Drawing.Rectangle"/> with the bounds.</param>
        /// <param name="value">True to set the button as pushed; False to reset pushed state.</param>
        public void SetPushed(int rowIndex, int colIndex, Rectangle bounds, bool value)
        {
            if (pushed.SetValue(rowIndex, colIndex, value))
            {
                Rectangle r = bounds;

                OnPushedChanged(new GridCellEventArgs(rowIndex, colIndex));
            }
        }

        /// <summary>
        /// Tests if the mouse is over the button and if the button wants to handle any subsequent mouse event.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="colIndex">The column index.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> with data for the current mouse event.</param>
        /// <param name="controller">A <see cref="IMouseController"/> that has indicated to handle the mouse event.</param>
        /// <returns>A non-zero value if the button can and wants to handle the mouse event; 0 if the
        /// mouse event is unrelated for this button.</returns>
        public virtual int HitTest(int rowIndex, int colIndex, MouseEventArgs e)
        {

            Point pt = new Point(e.X, e.Y);
            if ((e.Button == MouseButtons.Left || e.Button == MouseButtons.None) && Bounds.Contains(pt))
                return GridHitTestContext.CellButtonElement;
            return GridHitTestContext.None;
        }

        /// <summary>
        /// Occurs when the mouse is hovering over the button (and HitTest indicated it wants to handle the mouse event).
        /// </summary>
        /// <param name="ht">The <see cref="GridCellHitTestInfo"/> with saved hit-test information about the mouse event.</param>
        /// <remarks>
        /// <see cref="MouseHoverEnter"/> is called once before a series of <see cref="MouseHover"/> calls. 
        /// <see cref="MouseHoverLeave"/> is called when the mouse leaves the button or if the mouse button
        /// is pressed.</remarks>
        public virtual void MouseHoverEnter(GridCellHitTestInfo ht)
        {

            SetHovering(ht, true);
        }

        /// <summary>
        /// Occurs when the mouse is hovering over the button (and HitTest indicated it wants to handle the mouse event).
        /// </summary>
        /// <param name="ht">The <see cref="GridCellHitTestInfo"/> with saved hit-test information about the mouse event.</param>
        /// <param name="e">A <see cref="MouseEventArgs"/> with data about the mouse event.</param>
        /// <remarks>
        /// <see cref="MouseHoverEnter"/> is called once before a series of <see cref="MouseHover"/> calls. 
        /// <see cref="MouseHoverLeave"/> is called when the mouse leaves the button or if the mouse button
        /// is pressed.</remarks>
        public virtual void MouseHover(MouseEventArgs e, GridCellHitTestInfo ht)
        {

        }

        /// <summary>
        /// Occurs when the mouse has left hovering over the button (and HitTest indicated it wants to handle the mouse event).
        /// </summary>
        /// <param name="ht">The <see cref="GridCellHitTestInfo"/> with saved hit-test information about the mouse event.</param>
        /// <param name="e">A <see cref="EventArgs"/> with data about the mouse event (can also be <see cref="MouseEventArgs"/>).</param>
        /// <remarks>
        /// <see cref="MouseHoverEnter"/> is called once before a series of <see cref="MouseHover"/> calls.
        /// <see cref="MouseHoverLeave"/> is called when the mouse leaves the button or if the mouse button
        /// is pressed.</remarks>
        public virtual void MouseHoverLeave(EventArgs e, GridCellHitTestInfo ht)
        {

            SetHovering(ht, false);
        }

        /// <summary>
        /// This is called from <see cref="GridCellRendererBase"/> when <see cref="GridCellButton.HitTest"/>
        /// has indicated it wants to receive mouse events and the user has pressed the mouse button.
        /// </summary>
        /// <param name="ht">The <see cref="GridCellHitTestInfo"/> with saved hit-test information about the mouse event.</param>
        /// <param name="e">A <see cref="MouseEventArgs"/> with data about the mouse event.</param>
        /// <remarks>Once MouseDown has been called you are guaranteed to receive a MouseUp
        /// or CancelModel call.</remarks>
        public virtual void MouseDown(MouseEventArgs e, GridCellHitTestInfo ht)
        {

            SetMouseDown(ht, true);
            this.mouseDownHitTestInfo = ht;
        }

        /// <summary>
        /// This is called from <see cref="GridCellRendererBase"/> when <see cref="GridCellButton.HitTest"/>
        /// has indicated it wants to receive mouse events and the user has pressed the mouse button and is moving the mouse.
        /// </summary>
        /// <param name="ht">The <see cref="GridCellHitTestInfo"/> with saved hit-test information about the mouse event.</param>
        /// <param name="e">A <see cref="MouseEventArgs"/> with data about the mouse event.</param>
        /// <remarks>Once MouseDown has been called, you are guaranteed to receive a MouseUp
        /// or CancelModel call.</remarks>
        public virtual void MouseMove(MouseEventArgs e, GridCellHitTestInfo ht)
        {

            SetMouseDown(ht, ht.CellButtonBounds.Contains(new Point(e.X, e.Y)));
        }

        /// <summary>
        /// This is called from <see cref="GridCellRendererBase"/> when <see cref="GridCellButton.HitTest"/>
        /// has indicated it wants to receive mouse events and the user has released the mouse button.
        /// </summary>
        /// <param name="ht">The <see cref="GridCellHitTestInfo"/> with saved hit-test information about the mouse event.</param>
        /// <param name="e">A <see cref="MouseEventArgs"/> with data about the mouse event.</param>
        /// <remarks>Once MouseDown has been called you are guaranteed to receive a MouseUp
        /// or CancelModel call.</remarks>
        public virtual void MouseUp(MouseEventArgs e, GridCellHitTestInfo ht)
        {

            if (this.Grid != null && !this.Grid.IsDisposed)
                SetMouseDown(ht, false);
            if (FireClickOnMouseUp && Bounds.Contains(new Point(e.X, e.Y)))
                OnClicked(new GridCellEventArgs(ht.RowIndex, ht.ColIndex));
            mouseDownHitTestInfo = null;
        }

        /// <summary>
        /// Return the cursor that you want to display.
        /// </summary>
        /// <param name="ht">The <see cref="GridCellHitTestInfo"/> with saved hit-test information about the mouse event.</param>
        /// <returns>The <see cref="Cursor"/> to be displayed.</returns>
        public virtual Cursor GetCursor(GridCellHitTestInfo ht)
        {
            return Cursors.Default;
        }

        /// <summary>
        /// Occurs when the current mouse operation is canceled.
        /// </summary>
        /// <param name="ht">The <see cref="GridCellHitTestInfo"/> with saved hit-test information about the mouse event.</param>
        public virtual void CancelMode(GridCellHitTestInfo ht)
        {

            SetMouseDown(ht, false);
            SetHovering(ht, false);
        }

        /// <summary>
        /// Raises the <see cref="Clicked"/> event.
        /// </summary>
        /// <param name="e">A <see cref="GridCellEventArgs"/> with event data.</param>
        protected virtual void OnClicked(GridCellEventArgs e)
        {

            try
            {
                if (Clicked != null)
                    Clicked(this, e);
            }
            catch (Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
            }
        }

        /// <summary>
        /// Raises the <see cref="HoveringChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="GridCellEventArgs"/> with event data.</param>
        protected virtual void OnHoveringChanged(GridCellEventArgs e)
        {
            try
            {

                if (HoveringChanged != null)
                    HoveringChanged(this, e);
            }
            catch (Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
            }
        }

        /// <summary>
        /// Raises the <see cref="MouseDownChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="GridCellEventArgs"/> with event data.</param>
        protected virtual void OnMouseDownChanged(GridCellEventArgs e)
        {
            try
            {

                if (MouseDownChanged != null)
                    MouseDownChanged(this, e);
            }
            catch (Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
            }
        }

        /// <summary>
        /// Raises the <see cref="PushedChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="GridCellEventArgs"/> with event data.</param>
        protected virtual void OnPushedChanged(GridCellEventArgs e)
        {
            try
            {

                if (PushedChanged != null)
                    PushedChanged(this, e);
            }
            catch (Exception ex)
            {
                RootServiceProvider.ExceptionHandler.HandleException(ex, LoggerFileType.Grid);
            }
        }

        #region IDisposable Members

        public void Dispose()
        {

        }

        #endregion
    }

    public class GridHitTestContext
    {
        GridHitTestContext()
        {
        }

        /// <summary>
        /// None. 
        /// </summary>
        public const int None = 0;
        /// <summary>
        /// Mouse is over a vertical grid line between headers.
        /// </summary>
        public const int VerticalLine = 1;
        /// <summary>
        /// Mouse is over a horizontal grid line between headers.
        /// </summary>
        public const int HorizontalLine = 2;
        /// <summary>
        /// Mouse is over a cell.
        /// </summary>
        public const int Cell = 3;
        /// <summary>
        /// Mouse is over a header cell.
        /// </summary>
        public const int Header = 4;
        /// <summary>
        /// Mouse is over a selected range.
        /// </summary>
        public const int SelectedRange = 5;
        /// <summary>
        /// Mouse is over the edge of a selected range.
        /// </summary>
        public const int SelectedRangeEdge = 6;
        /// <summary>
        /// Mouse is over a cell button element (see <see cref="GridCellButton"/>).
        /// </summary>
        public const int CellButtonElement = 7;
        /// <summary>
        /// Mouse is over the checker box in a check box cell (see <see cref="GridCheckBoxCellRenderer"/>).
        /// </summary>
        public const int CheckBoxChecker = 7;
    }

    public enum CustomCellButtonType
    {
        Rounded,
        Square
    }

    public enum ButtonBlinkState
    {
        Up,
        Down,
        Update,
        Default
    }
}
