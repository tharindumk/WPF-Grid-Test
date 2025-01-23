using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Filters
{
    ///
    /// Imported from Syncfusion Grid.Grouping
    /// 
    public class RecordFilterDescriptor
    {
        string name = "";
        List<string> mappingNames = new List<string>();
        List<int> mappingColumnPositions = new List<int>();
        TableInfo.TableColumn column = null;
        RecordFilterDescriptorCollection parentCollection;
        FilterConditionCollection conditions;
        FilterLogicalOperator logicalOperator = FilterLogicalOperator.Or;
        TableInfo tableInfo;
        string expression = "";
        object[] uniqueGroupId = null;
        string compiledExpression = null;
        //WeakReference lastCompareRecord;
        bool lastCompareRecordResult;
        //int lastCompareVersion = -1;
        private bool isDirty = true;

        public bool IsDirty
        {
            get { return isDirty; }
            set { isDirty = value; }
        }

        /// <summary>
        /// Initializes a new empty filter.
        /// </summary>
        public RecordFilterDescriptor()
        {
            this.conditions = CreateFilterConditionCollection(null);
        }

        /// <summary>
        /// Initializes a new filter based on a formula expression similar to expressions used in <see cref="ExpressionFieldDescriptor"/>.
        /// </summary>
        /// <param name="expression">A formula expression similar to expressions used in <see cref="ExpressionFieldDescriptor"/>.</param>
        public RecordFilterDescriptor(string expression)
        {
            this.name = "";
            this.expression = expression;
            this.conditions = this.CreateFilterConditionCollection(null);
            

        }

        /// <summary>
        /// Initializes a new filter based on a formula expression similar to expressions used in <see cref="ExpressionFieldDescriptor"/>.
        /// </summary>
        /// <param name="name">The name of the field this filter is compared with. This name is used to look up fields in the Fields collection of the parent table descriptor.</param>
        /// <param name="expression">A formula expression similar to expressions used in <see cref="ExpressionFieldDescriptor"/>.</param>
        public RecordFilterDescriptor(string name, string expression)
        {
            this.name = name;
            this.expression = expression;
            this.conditions = CreateFilterConditionCollection(null);
        }

        /// <summary>
        /// Creates the <see cref="FilterConditionCollection"/> list.
        /// </summary>
        /// <returns>returns a new instance of FilterConditionCollection.</returns>
        protected virtual FilterConditionCollection CreateFilterConditionCollection(FilterCondition[] conditions)
        {
            if (conditions != null)
                return new FilterConditionCollection(conditions);
            else
                return new FilterConditionCollection();
        }
        /// <summary>
        /// Initializes a new filter based on a collection of <see cref="FilterCondition"/>.
        /// </summary>
        /// <param name="name">The name of the field descriptor.</param>
        /// <param name="mappingName">The name of the field this filter is compared with. This name is used to look up fields in the Fields collection of the parent table descriptor.</param>
        /// <param name="logicalOperator">The logical operator used if multiple conditions are given.</param>
        /// <param name="conditions">The collection of conditions.</param>
        public RecordFilterDescriptor(string name, List<string> mappingNames, FilterLogicalOperator logicalOperator, FilterCondition[] conditions)
        {
            this.mappingNames = mappingNames;
            this.name = name;
            this.logicalOperator = logicalOperator;
            this.conditions = CreateFilterConditionCollection(conditions);
        }

        /// <summary>
        /// Initializes a new filter based on a collection of <see cref="FilterCondition"/>.
        /// </summary>
        /// <param name="name">The name of the field this filter is compared with. This name is used to look up fields in the Fields collection of the parent table descriptor.</param>
        /// <param name="logicalOperator">The logical operator used if multiple conditions are given.</param>
        /// <param name="conditions">The collection of conditions.</param>
        public RecordFilterDescriptor(string name, FilterLogicalOperator logicalOperator, FilterCondition[] conditions)
        {
            this.name = name;
            this.logicalOperator = logicalOperator;
            this.conditions = CreateFilterConditionCollection(conditions);
        }

        /// <summary>
        /// Initializes a new filter based on a collection of <see cref="FilterCondition"/>.
        /// </summary>
        /// <param name="name">The name of the field this filter is compared with. This name is used to look up fields in the Fields collection of the parent table descriptor.</param>
        /// <param name="condition">The condition.</param>
        public RecordFilterDescriptor(string name, FilterCondition condition)
            : this(name, FilterLogicalOperator.Or, new FilterCondition[] { condition })
        {
        }

        /// <summary>
        /// Initializes a new filter based on a collection of <see cref="FilterCondition"/>.
        /// </summary>
        /// <param name="name">The name of the field descriptor.</param>
        /// <param name="mappingName">The name of the field this filter is compared with. This name is used to look up fields in the Fields collection of the parent table descriptor.</param>
        /// <param name="condition">The condition.</param>
        public RecordFilterDescriptor(string name, List<string> mappingNames, FilterCondition condition)
            : this(name, mappingNames, FilterLogicalOperator.Or, new FilterCondition[] { condition })
        {
        }

        public RecordFilterDescriptorCollection Collection
        {
            get
            {
                return parentCollection;
            }
        }
        
        internal void SetTableDescriptor(TableInfo tableDescriptor)
        {
            this.tableInfo = tableDescriptor;
        }

        public TableInfo TableInfo
        {
            get
            {
                return tableInfo;
            }
        }

        public FilterConditionCollection Conditions
        {
            get
            {
                return conditions;
            }
        }

        public FilterLogicalOperator LogicalOperator
        {
            get
            {
                return logicalOperator;
            }
            set
            {
                if (logicalOperator != value)
                { 
                    logicalOperator = value; 
                }
            }
        }

        public virtual string Name
        {
            get
            {
                if (name == "")
                {
                    if (expression != "")
                        return expression;
                    if (mappingNames != null && mappingNames.Count > 0)
                        return mappingNames[0];
                    if (column != null)
                        return column.Name;
                }
                return name;
            }
            set
            {
                if (name != value)
                { 
                    name = value; 
                }
            }
        }
        /// <summary>
        /// Determines if name is not empty.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeName()
        {
            return name != "";
        }
        /// <summary>
        /// Resets the name to be empty.
        /// </summary>
        public void ResetName()
        {
            name = "";
        }

        public virtual List<string> MappingNames
        {
            get
            {
                return mappingNames;
            }
            set
            {
                if (mappingNames != value)
                { 
                    mappingNames = value; 
                }
            }
        }

        public virtual List<int> MappingColumnPositions
        {
            get
            {
                return mappingColumnPositions;
            }
            set
            {
                if (mappingColumnPositions != value)
                {
                    mappingColumnPositions = value;
                }
            }
        }

        /// <summary>
        /// Determines if name is not empty.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeMappingName()
        {
            return mappingNames != null;
        }
        /// <summary>
        /// Resets the name to be empty.
        /// </summary>
        public void ResetMappingName()
        {
            mappingNames = null;
        }

        public object[] UniqueGroupId
        {
            get
            {
                return this.uniqueGroupId;
            }
            set
            {
                if (this.uniqueGroupId != value)
                { 
                    this.uniqueGroupId = value; 
                }
            }
        }

        /// <summary>
        /// Resets the <see cref="UniqueGroupId"/> to null.
        /// </summary>
        public void ResetUniqueGroupId()
        {
            this.UniqueGroupId = null;
        }

        /// <summary>
        /// Determines if <see cref="UniqueGroupId"/> should be serialized to code or xml.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerializeUniqueGroupId()
        {
            if (uniqueGroupId == null)
                return false;

            // No support for serializing conditions for UniformChildListRelations.
            // Avoid exception instead.
            for (int n = 0; n < uniqueGroupId.Length; n++)
                if (uniqueGroupId[n] is Record)
                    return false;

            return true;
        }

        public string Expression
        {
            get
            {
                return this.expression;
            }
            set
            {
                if (this.expression != value)
                { 
                    this.expression = value;
                    this.ResetCompiledExpression();
                    string s = GetCompiledExpression(); // Force recalc 
                }
            }
        }

        /// <summary>
        /// Gets a string that holds pre-compiled information about the expression.
        /// </summary>
        /// <returns>A string that holds pre-compiled information about the expression.</returns>
        public string GetCompiledExpression()
        {
            if (string.IsNullOrEmpty(compiledExpression) || (isDirty && expression.Length > 0 && TableInfo != null))
            {
                IExpressionFieldEvaluator eval = GetExpressionEvaluator();
                string s = eval.PutTokensInFormula(expression.ToLower());
                compiledExpression = eval.Parse(s);
                isDirty = false;
            }

            return compiledExpression;
        }

        /// <summary>
        /// Resets the compiled expression. It will be recompiled later on demand.
        /// </summary>
        public void ResetCompiledExpression()
        {
            compiledExpression = null;
            isDirty = true;
        }

        IExpressionFieldEvaluator GetExpressionEvaluator()
        {
            return TableInfo.ExpressionFieldEvaluator;
        }

        public TableInfo.TableColumn Column
        {
            get
            {
                if (parentCollection != null)
                    this.parentCollection.EnsureFieldDescriptors();
                return column;
            }
        }

        internal bool InitColumn(TableInfo tableDescriptor)
        {
            if (MappingColumnPositions.Count > 0)
            {
                column = tableDescriptor.VisibleColumns[MappingColumnPositions[0]];
            }
            else
            {
                if (this.mappingNames != null && this.mappingNames.Count > 0)
                    column = tableDescriptor.GetVisibleColumnFromName(this.mappingNames[0]);
                else
                {
                    if (this.mappingNames.Count > 0)
                    {
                        this.mappingNames[0] = this.name;
                    }

                    column = tableDescriptor.GetVisibleColumnFromName(this.Name);
                }

                if (column == null && this.mappingNames.Count > 0)
                    column = tableDescriptor.GetColumnFromName(this.mappingNames[0]);
            }

            foreach (FilterCondition condition in conditions)
            {
                condition.SetFilterDescriptor(this);
            }


            return column != null;
        }

        /// <summary>
        /// Evaluates this filter for the given record and returns True if the record
        /// meets the filters criteria.
        /// </summary>
        /// <param name="record">The record to be evaluated.</param>
        /// <returns>True if the record
        /// meets the filters criteria; False otherwise.</returns>
        public bool CompareRecord(Record record)
        {
            lastCompareRecordResult = _CompareRecord(record);

            return lastCompareRecordResult;
        }

        /// <exclude/>
        public void ResetCache()
        {
            //lastCompareRecord = null;
        }

        bool _CompareRecord(Record record)
        {
            bool result = false;

            if (Expression != "")
            {
                string expr = GetCompiledExpression();
                IExpressionFieldEvaluator eval = this.GetExpressionEvaluator();
                if (eval != null && expr != null)
                {
                    string s = eval.ComputeFormulaValueAt(expr, record);
                    if (s != "")
                        result = s == "1";
                }
                if (Conditions.Count == 0)
                    return result;
            }

            if (Conditions.Count == 0)
                return true;

            //
            //NOTE : May be needed for grouping
            //
            //Apply filter only to the specific group that it was selected for.
            //if (this.UniqueGroupId != null)
            //{
            //    bool groupMatch = false;
            //    Group g = record.ParentGroup;
            //    while (g != null && !g.IsTopLevelGroup)
            //    {
            //        if (EqualsCategory(this.UniqueGroupId, g.UniqueGroupId))
            //        {
            //            groupMatch = true;
            //            break;
            //        }
            //        g = g.ParentGroup;
            //    }

            //    if (!groupMatch)
            //        return true;
            //}

            if (this.LogicalOperator == FilterLogicalOperator.And)
            {
                foreach (FilterCondition condition in Conditions)
                {
                    condition.SetFilterDescriptor(this);
                    if (!condition.CompareRecord(record))
                        return false;
                }
                return true;
            }
            else // FilterLogicalOperator.Or
            {
                foreach (FilterCondition condition in Conditions)
                {
                    condition.SetFilterDescriptor(this);
                    if (condition.CompareRecord(record))
                        return true;
                }
                return false;
            }
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
            else if (Object.ReferenceEquals(x, y))
                cmp = 0;
            else if (x.GetType() != y.GetType())
                cmp = -1;
            else if (x is IComparable)
                cmp = ((IComparable)x).CompareTo(y);

            return cmp;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <override/>
        public override bool Equals(object obj)
        {
            if (this == null && obj == null)
                return true;
            else if (this == null)
                return false;
            else if (!(obj is RecordFilterDescriptor))
                return false;
            return Equals((RecordFilterDescriptor)obj);
        }

        bool Equals(RecordFilterDescriptor other)
        {
            return other.name == name
                //&& other.mappingNames == mappingNames
                && other.column == column
                && other.conditions == conditions
                && other.logicalOperator == logicalOperator
                && other.expression == expression
                && other.uniqueGroupId == uniqueGroupId;
        }

        internal void SetCollection(RecordFilterDescriptorCollection parentCollection)
        {
            this.parentCollection = parentCollection;
            if (parentCollection.TableInfo != null)
                this.tableInfo = parentCollection.TableInfo;
            foreach (FilterCondition condition in this.conditions)
            {
                condition.SetCollection(Conditions);
                condition.SetFilterDescriptor(this);
            }

            //
            //Modified by Mubasher New Grid
            //
            SetTableDescriptor(this.tableInfo);
        }
    }

    /// <summary>
    ///     Logical operator used by <see cref="RecordFilterDescriptor"/>.
    /// </summary>
    public enum FilterLogicalOperator
    {
        /// <summary>
        /// All conditions must be True.
        /// </summary>
        And,
        /// <summary>
        /// One of the conditions must be True.
        /// </summary>
        Or
    }

    /// <summary>
    /// Comparison operator used by <see cref="FilterCondition"/>.
    /// </summary>
    public enum FilterCompareOperator
    {
        /// <summary>
        /// The value is equal.
        /// </summary>
        Equals,
        /// <summary>
        /// The value is not equal.
        /// </summary>
        NotEquals,
        /// <summary>
        /// The left value is less than the right value.
        /// </summary>
        LessThan,
        /// <summary>
        /// The left value is less than or equal to the right value.
        /// </summary>
        LessThanOrEqualTo,
        /// <summary>
        /// The left value is greater than the right value.
        /// </summary>
        GreaterThan,
        /// <summary>
        /// The left value is greater than or equal to the right value.
        /// </summary>
        GreaterThanOrEqualTo,
        /// <summary>
        /// The left string matches the right pattern with wildcard characters, character lists, or character ranges.
        /// </summary>
        /// <remarks>
        /// The pattern-matching of the like operator allows you to match strings using wildcard characters,
        /// character lists, or character ranges in any combination. The following table shows the characters
        /// allowed in pattern and what they match:
        /// <list type="table">
        /// <listheader><term>Characters in pattern
        /// </term><description>Matches in string
        /// </description></listheader>
        /// <item><term>?</term><description>Any single character
        /// </description></item>
        /// <item><term>*
        /// </term><description>Zero or more characters
        /// </description></item>
        /// <item><term>#
        /// </term><description>Any single digit (0–9)
        /// </description></item>
        /// <item><term>[charlist]
        /// </term><description>Any single character in charlist
        /// </description></item>
        /// <item><term>[!charlist]
        /// </term><description>Any single character not in charlist
        /// </description></item>
        /// </list>
        /// For further information and examples, see the "Like operator" in the MSDN help (Visual Basic Language Reference).
        /// </remarks>
        Like,
        /// <summary>
        /// The left string matches the right regular expression pattern. See ".NET Framework Regular Expressions" in MSDN Help
        /// for discussion and examples for regular expressions.
        /// </summary>
        Match,
        /// <summary>
        /// A custom filter. A implementation object of <see cref="ICustomFilter"/> should be applied to the 
        /// <see cref="FilterCondition.CustomFilter"/> property of the <see cref="FilterCondition"/>  object.
        /// </summary>
        Custom
    }
}
