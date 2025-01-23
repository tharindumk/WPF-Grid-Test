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
using System.Text;
using System.Data;

using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Comparer;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Accessors;
using System.Windows.Forms;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Comparer
{
    internal class RecordDataComparer : IComparer
    {
        IComparer _inner;

        public RecordDataComparer(IComparer inner)
        {
            _inner = inner;
        }

        public int Compare(object x, object y)
        {
            Record ry = (Record)y;
            if (x is Record)
            {
                Record rx = (Record)x;

                int cmp = 0;

                cmp = _inner.Compare(rx, ry);                
                
                if (cmp != 0)
                    return cmp;
                
                return rx.SourceIndex - ry.SourceIndex;
            }

            return 0;
        }
    }

    internal sealed class SortColumnComparer : IComparer
    {
        private TableInfo table = null;

        public SortColumnComparer(TableInfo table)
        {
            this.table = table;
        }

        public int Compare(object x, object y)
        {
            SortColumnDescriptor[] arrayOfColumnDescriptors = null;
            bool isSorted = false;

            table.GetSortInfo(out isSorted, out arrayOfColumnDescriptors);
            return SortColumnComparer.Compare(isSorted, arrayOfColumnDescriptors, x, y);
        }

        public static int Compare(bool isSorted, SortColumnDescriptor[] arrayOfColumnDescriptors, object x, object y)
        {
            return CompareColumns.CompareSortKeys(isSorted, arrayOfColumnDescriptors, x, y);
        }

        internal static int _Compare(SortColumnDescriptor columnDescriptor, object x, object y)
        {
            int cmp = 0;

            try
            {
                if (columnDescriptor.Comparer != null)
                    cmp = columnDescriptor.Comparer.Compare(x, y);
                else
                    cmp = _Compare(x, y);

                if (columnDescriptor.SortDirection == ListSortDirection.Descending)
                    return -cmp;
            }
            catch (Exception ex)
            {
                Form form = null;

                if ( columnDescriptor != null && columnDescriptor.TableInfo != null && columnDescriptor.TableInfo.Grid != null )
                    form = columnDescriptor.TableInfo.Grid.FindForm();

                string formName = form == null ? "Not Found" : form.Text;
                string columnName = columnDescriptor == null ? "NULL" : columnDescriptor.Name;
                Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers.ExceptionsLogger.LogError("ERROR IN COMPARE METHOD : Form, " + formName + ", Column : " + columnName, Helpers.LogEntryType.Error, ex);
            }

            return cmp;
        }

        internal static int _Compare(object x, object y)
        {
            int cmp = 0;
            bool xIsNull = (x == null || x is DBNull);
            bool yIsNull = (y == null || y is DBNull);

            if (yIsNull && xIsNull)
                cmp = 0;
            else if (xIsNull)
                cmp = -1;
            else if (yIsNull)
                cmp = 1;
            else 
            {               
                //
                // Sometimes different data types gets passed specially for unbounded columns ( Printed VWAP ). This needs to be handled
                // in order to avoid exceptions
                //
                if (x.GetType() == y.GetType())
                {
                    if (x is IComparable)
                        cmp = ((IComparable)x).CompareTo(y);
                }
                else
                {
                    if (x is String)
                        cmp = -1;
                    else if (y is String)
                        cmp = 1;
                    else
                        cmp = 0;
                }
            }           

            return cmp;
        }

    }


}

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Comparer
{
    /// <summary>
    /// Provides utilities for comparing two records or columns.
    /// </summary>
    internal class CompareColumns
    {
        internal static NaturalSortComparer naturalComparer = new NaturalSortComparer();

        /// <summary>
        /// Compares the sort keys for the two records specified with x and y.
        /// </summary>
        /// <param name="isSorted"></param>
        /// <param name="arrayOfColumnDescriptors"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int CompareSortKeys(bool isSorted, SortColumnDescriptor[] arrayOfColumnDescriptors, object x, object y)
        {
            Record ry = (Record)y;
            Record rx = (Record)x;
            
            if (isSorted)
            {
                int count = arrayOfColumnDescriptors.Length;
                int boundSortKeyID = -1;
                int unboundSortKeyID = -1;

                for (int n = 0; n < count; n++)
                {
                    object cx = null;
                    object cy = null;

                    SortColumnDescriptor columnDescriptor = arrayOfColumnDescriptors[n];

                    if (rx.ColumnIndex >= 0 && !string.IsNullOrEmpty(columnDescriptor.Id) && !columnDescriptor.Id.Contains(rx.ColumnIndex.ToString()))
                    {
                        boundSortKeyID++;
                        unboundSortKeyID++;
                        continue;
                    }

                    bool isCustomFormulaColumn = false;

                    //if (rx.UnboundColumns != null)
                    //{
                    //    for (int i = 0; i < rx.UnboundColumns.Count; i++)
                    //    {
                    //        if (rx.UnboundColumns[i].IsCustomFormulaColumn && columnDescriptor.Name == rx.UnboundColumns[i].Name)
                    //        {
                    //            isCustomFormulaColumn = true;
                    //            break;
                    //        }
                    //    }
                    //}

                    if (!columnDescriptor.IsUnbound && !isCustomFormulaColumn)
                    {
                        boundSortKeyID++;

                        if (rx.SortKeys.Count > 0 && rx.SortKeys.Count > boundSortKeyID)
                            cx = rx.SortKeys[boundSortKeyID].Get(rx.ObjectBound);
                        if (ry.SortKeys.Count > 0 && ry.SortKeys.Count > boundSortKeyID)
                            cy = ry.SortKeys[boundSortKeyID].Get(ry.ObjectBound);
                    }
                    else
                    {
                        unboundSortKeyID++;
                        int col = 0;
                        int row = 0;

                        if (rx.UnboundColumns != null)
                        {
                            for (int i = 0; i < rx.UnboundColumns.Count; i++)
                            {
                                if (columnDescriptor.Name == rx.UnboundColumns[i].MappingName)
                                {
                                    col = columnDescriptor.TableInfo.GetVisibleColumnFromName(rx.UnboundColumns[i].Name).CurrentPosition;
                                    row = rx.SourceIndex;

                                    if (col >= 0 && columnDescriptor.TableInfo.CellMatrix.GetLength(0) > row)
                                    {
                                        if (rx.tableInfo != null && rx.tableInfo.Grid != null && rx.tableInfo.Grid.GridType == GridType.MultiColumn)
                                        {
                                            row = rx.CurrentIndex;

                                            if (columnDescriptor.TableInfo.IsFilterEnabled)
                                            {
                                                if (columnDescriptor.TableInfo.FilteredRecords.Count > row)
                                                    cx = UnboundDataAccessor.Get(columnDescriptor.TableInfo.CellMatrix[row, col]);
                                            }
                                            else
                                                cx = UnboundDataAccessor.Get(columnDescriptor.TableInfo.CellMatrix[row, col]);
                                        }
                                        else
                                        {
                                            if (columnDescriptor.TableInfo.IsFilterEnabled)
                                            {
                                                if (columnDescriptor.TableInfo.FilteredRecords.Count > row)
                                                    cx = UnboundDataAccessor.Get(columnDescriptor.TableInfo.CellMatrix[row, col]);
                                            }
                                            else
                                                cx = UnboundDataAccessor.Get(columnDescriptor.TableInfo.CellMatrix[row, col]);
                                        }
                                    }

                                    break;
                                }
                            }
                        }

                        if (ry.UnboundColumns != null)
                        {
                            for (int i = 0; i < rx.UnboundColumns.Count; i++)
                            {
                                if (columnDescriptor.Name == rx.UnboundColumns[i].MappingName)
                                {
                                    col = columnDescriptor.TableInfo.GetVisibleColumnFromName(ry.UnboundColumns[i].Name).CurrentPosition;
                                    row = ry.SourceIndex;

                                    if (col >= 0 && columnDescriptor.TableInfo.CellMatrix.GetLength(0) > row)
                                    {
                                        if (ry.tableInfo != null && ry.tableInfo.Grid != null && ry.tableInfo.Grid.GridType == GridType.MultiColumn)
                                        {
                                            row = ry.CurrentIndex;

                                            if (columnDescriptor.TableInfo.IsFilterEnabled)
                                            {
                                                if (columnDescriptor.TableInfo.FilteredRecords.Count > row)
                                                    cy = UnboundDataAccessor.Get(columnDescriptor.TableInfo.CellMatrix[row, col]);
                                            }
                                            else
                                                cy = UnboundDataAccessor.Get(columnDescriptor.TableInfo.CellMatrix[row, col]);
                                        }
                                        else
                                        {
                                            if (columnDescriptor.TableInfo.IsFilterEnabled)
                                            {
                                                if (columnDescriptor.TableInfo.FilteredRecords.Count > row)
                                                    cy = UnboundDataAccessor.Get(columnDescriptor.TableInfo.CellMatrix[row, col]);
                                            }
                                            else
                                                cy = UnboundDataAccessor.Get(columnDescriptor.TableInfo.CellMatrix[row, col]);
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    
                    int cmp = 0;

                    if (columnDescriptor.ComparisonMethod == SortComparisonMethod.Common)
                    {
                        cmp = SortColumnComparer._Compare(columnDescriptor, cx, cy);

                        if (cmp != 0)
                            return cmp;
                    }
                    else
                    {
                        cmp = naturalComparer.Compare(cx.ToString(), cy.ToString());

                        if (columnDescriptor.SortDirection == ListSortDirection.Descending)
                            return -cmp;

                        if (cmp != 0)
                            return cmp;                        
                    }
                }
            }

            return rx.SourceIndex - ry.SourceIndex;
        }

        public static int CompareNullableObjects(SortColumnDescriptor columnDescriptor, object x, object y)
        {
            return SortColumnComparer._Compare(columnDescriptor, x, y);
        }

        public static int CompareNullableObjects(object x, object y)
        {
            return SortColumnComparer._Compare(x, y);
        }

    }
}