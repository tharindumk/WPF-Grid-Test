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
using System.Globalization;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.CellTypes
{
	/// <summary>
	/// This class holds layout information and bounds for a cell.
	/// </summary>
	public class GridCellLayout
	{
		/// <overload>
		/// Initializes a new CellLayout with the given parameters.
		/// </overload>
		/// <summary>
		/// Initializes a new CellLayout with the given parameters.
		/// </summary>
		/// <param name="cellRectangle">This is the full cell rectangle including borders.</param>
		/// <param name="innerRectangle"> This is the cell area without borders.</param>
		/// <param name="buttons">An array of <see cref="Rectangle"/> with boundaries for all cell buttons.</param>
		/// <param name="textRectangle">This is the text rectangle. It is the cell rectangle without buttons, borders, or text margins.</param>
		[Obsolete("Use default ctor instead.")]
		public GridCellLayout(Rectangle cellRectangle, Rectangle innerRectangle, Rectangle[] buttons, Rectangle textRectangle)
		{
			this.cellRectangle = cellRectangle;      // the whole cell including borders
			this.innerRectangle = innerRectangle;    // the whole cell arex excluding borders
			this.buttons = buttons;                  // buttons 
			//this.clientRectangle = clientRectangle;  // cell area excluding borders and buttons
			this.textRectangle = textRectangle;      // ClientRectangle excluding TextMargins
		}

		/// <summary>
		///  Initializes an empty CellLayout. 
		/// </summary>
		public GridCellLayout()
		{
		}

		Rectangle cellRectangle; 
		Rectangle innerRectangle;
		Rectangle[] buttons;  
		Rectangle clientRectangle;
		Rectangle textRectangle;

		/// <summary>
		/// This is the full cell rectangle including borders.
		/// </summary>
		public Rectangle CellRectangle
		{
			get
			{
				return cellRectangle;
			}
			set
			{
				cellRectangle = value;
			}
		} 
		/// <summary>
		/// This is the cell area without borders.
		/// </summary>
		public Rectangle InnerRectangle
		{
			get
			{
				return innerRectangle;
			}
			set
			{
				innerRectangle = value;
			}
		} 
		/// <summary>
		/// An array of <see cref="Rectangle"/> with boundaries for all cell buttons.
		/// </summary>
		public Rectangle[] Buttons
		{
			get
			{
				return buttons;
			}
			set
			{
				buttons = value;
			}
		} 
		/// <summary>
		/// This is the client rectangle. It is the cell rectangle without buttons and borders.
		/// </summary>
		public Rectangle ClientRectangle
		{
			get
			{
				return clientRectangle;
			}
			set
			{
				clientRectangle = value;
			}
		} 
		/// <summary>
		/// This is the text rectangle. It is the cell rectangle without buttons, borders, or text margins.
		/// </summary>
		public Rectangle TextRectangle
		{
			get
			{
				return textRectangle;
			}
			set
			{
				textRectangle = value;
			}
		} 
	}
}
