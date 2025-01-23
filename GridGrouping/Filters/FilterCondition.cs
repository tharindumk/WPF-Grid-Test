using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Comparer;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Filters
{
    ///
    /// Imported from Syncfusion Grid.Grouping
    /// 
    public class FilterCondition
    {
        #region Fields & Properties

        FilterConditionCollection parentCollection;
        object compareValue = "";
        object preparedCompareValue = dirty;
        FilterCompareOperator compareOperator = FilterCompareOperator.Equals;
        ICustomFilter customFilter;
        Regex regexValue = null;
        RecordFilterDescriptor _filterDescriptor;
        TableInfo.TableColumn _pd;
        static object dirty = new object();

        //WeakReference lastCompareRecord;
        //bool lastCompareRecordResult;
        //int lastCompareVersion = -1;

        public FilterConditionCollection Collection
        {
            get
            {
                return parentCollection;
            }
        }

        public RecordFilterDescriptor FilterDescriptor
        {
            get
            {
                return _filterDescriptor;
            }
        }

        /// <summary>
        /// The comparison operator.
        /// </summary>
        public FilterCompareOperator CompareOperator
        {
            get
            {
                return this.compareOperator;
            }
            set
            {
                if (this.compareOperator != value)
                {
                    this.compareOperator = value;
                    this.preparedCompareValue = dirty;
                    regexValue = null;
                }
            }
        }

        /// <summary>
        /// The comparison text.
        /// </summary>
        public string CompareText
        {
            get
            {
                if (CompareValue == null || CompareValue is DBNull)
                    return "(null)";
                return CompareValue.ToString();
            }
            set
            {
                if (value == "(null)")
                    CompareValue = null;
                else
                    CompareValue = value;
            }
        }


        public object CompareValue
        {
            get
            {
                return this.compareValue;
            }
            set
            {
                if (this.compareValue != value)
                {
                    this.compareValue = value;
                    this.preparedCompareValue = dirty;
                    regexValue = null;
                }
            }
        }


        public ICustomFilter CustomFilter
        {
            get
            {
                return customFilter;
            }
            set
            {
                if (this.customFilter != value)
                {
                    this.customFilter = value;
                }
            }
        }

        TableInfo.TableColumn Column
        {
            get
            {
                if (_filterDescriptor == null)
                    return null;
                if (this._pd != _filterDescriptor.Column)
                {
                    this._pd = _filterDescriptor.Column;
                    this.preparedCompareValue = dirty;
                    this.regexValue = null;
                }
                return _pd;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new empty condition.
        /// </summary>
        public FilterCondition()
        {
        }

        /// <summary>
        /// Initializes a new condition with comparison operator and comparison value.
        /// </summary>
        /// <param name="compareOperator">The comparison operator.</param>
        /// <param name="compareValue">The comparison value.</param>
        public FilterCondition(FilterCompareOperator compareOperator, object compareValue)
        {
            this.compareOperator = compareOperator;
            this.compareValue = compareValue;
        }

        #endregion

        #region Helper Methods

        public void InitializeFrom(FilterCondition other)
        {
            this.CompareOperator = other.CompareOperator;
            this.CompareValue = other.CompareValue;
            this.CustomFilter = other.CustomFilter;
        }

        internal void SetCollection(FilterConditionCollection parentCollection)
        {
            this.parentCollection = parentCollection;
        }      

        internal void SetFilterDescriptor(RecordFilterDescriptor filterDescriptor)
        {
            this._filterDescriptor = filterDescriptor;
        }
                
        /// <override/>
        public override bool Equals(object obj)
        {
            if (this == null && obj == null)
                return true;
            else if (this == null)
                return false;
            else if (!(obj is FilterCondition))
                return false;
            return Equals((FilterCondition)obj);
        }

        bool Equals(FilterCondition other)
        {
            return Object.ReferenceEquals(other._filterDescriptor, _filterDescriptor)
                && other.compareOperator == compareOperator
                && other.compareValue == compareValue
                && other.customFilter == customFilter;
        }

        /// <override/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        object GetValue(Record record)
        {
            if (record == null)
                return null;
            else if (Column == null)
                return record.GetData();
            else
            {
                if(record != null && record.tableInfo != null && record.tableInfo.Grid != null
                    && record.tableInfo.Grid.GridType == GridType.MultiColumn)
                {
                    TableInfo.TableColumn col = record.tableInfo.GetColumnFromNameForColumnSet(Column.Name, record.ColumnIndex);
                    
                    if(col != null)
                        return record.GetValue(Column.Name, col.CurrentPosition, false);
                    else
                        return record.GetValue(Column);
                }
                else
                    return record.GetValue(Column);
            }
        }

        bool IsNull(object x)
        {
            //
            //Modified by Mubasher New Grid
            //
            return x == null;// || x is DBNull;
        }

        static int Compare(object x, object y)
        {
            return SortColumnComparer._Compare(x, y);
        }

       
        /// <summary>
        /// Evaluates this condition for the given record and returns True if the record
        /// meets the condition.
        /// </summary>
        /// <param name="record">The record to be evaluated.</param>
        /// <returns>True if the record
        /// meets the condition; False otherwise.</returns>
        public bool CompareRecord(Record record)
        {
            if (this.CompareOperator == FilterCompareOperator.Custom)
            {
                if (this.customFilter != null)
                    return this.customFilter.CompareDescriptor(this, record);
            }

            if (Column == null)
                return true;

            object x = GetValue(record);

            if (object.ReferenceEquals(dirty, preparedCompareValue))
            {
                preparedCompareValue = compareValue;
                if (!IsNull(compareValue))
                {
                    if (this.compareOperator != FilterCompareOperator.Match
                        && this.compareOperator != FilterCompareOperator.Like
                        && this.compareOperator != FilterCompareOperator.Custom
                        )
                    {
                        Type t;
                        if (IsNull(x))
                            t = Column.Type;
                        else
                            t = x.GetType();

                        //
                        //Modified by Mubasher New Grid
                        //
                        //preparedCompareValue = Syncfusion.Styles.ValueConvert.ChangeType(compareValue, t, this.Column.TableDescriptor.Engine.Culture);
                    }
                    else
                        preparedCompareValue = compareValue;


                    if (preparedCompareValue == null)
                        preparedCompareValue = DBNull.Value;
                }
            }

            object y = preparedCompareValue;

            if (x == null)
                x = DBNull.Value;

            switch (CompareOperator)
            {
                case FilterCompareOperator.Equals:
                    return IsNull(x) && IsNull(y) || Compare(x, y) == 0;

                case FilterCompareOperator.GreaterThan:
                    return Compare(x, y) > 0;

                case FilterCompareOperator.GreaterThanOrEqualTo:
                    return Compare(x, y) >= 0;

                case FilterCompareOperator.LessThan:
                    return Compare(x, y) < 0;

                case FilterCompareOperator.LessThanOrEqualTo:
                    return Compare(x, y) <= 0;

                case FilterCompareOperator.Like:
                    if (IsNull(x) != IsNull(y))
                        return false;
                    else if (IsNull(x))
                        return true;
                    else
                        return Microsoft.VisualBasic.CompilerServices.StringType.StrLike(
                            x.ToString().ToLowerInvariant(),
                            y.ToString().ToLowerInvariant(),
                            Microsoft.VisualBasic.CompareMethod.Binary);
                // Modified by Mubasher to Compare filter text with pattern
                //earlier values like ` was returned false in the comparrison
                //eg: txt="KPROJ`O" and pattern *proj* will return false
                //so changed CompareMethod into Binary from string.

                case FilterCompareOperator.NotEquals:
                    return !(IsNull(x) && IsNull(y) || Compare(x, y) == 0);

                case FilterCompareOperator.Match:
                    if (IsNull(x) != IsNull(y))
                        return false;
                    else if (IsNull(y))
                        return true;
                    else
                    {
                        if (regexValue == null)
                            regexValue = new Regex(compareValue.ToString());

                        return regexValue.IsMatch(x.ToString());
                    }

                default:
                    //Debug.Assert(false, "Invalid enum value");
                    break;
            }

            return false;
        }

        #endregion
    }

    /// <summary>
    /// Provides a <see cref="CompareDescriptor"/> method when <see cref="FilterCompareOperator.Custom"/>
    /// is specified for a <see cref="FilterCondition"/>.
    /// </summary>
    public interface ICustomFilter
    {
        /// <summary>
        /// Called to determine if the record meets filter criteria of the specified condition.
        /// </summary>
        /// <param name="filterDescriptor">The condition.</param>
        /// <param name="record">The record to be tested.</param>
        /// <returns>True if record meets filter criteria; False otherwise.</returns>
        bool CompareDescriptor(FilterCondition filterDescriptor, Record record);
    }
}
