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
using System.Text;
using System.Globalization;
using System.ComponentModel.Design.Serialization;
using System.Collections.Generic;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Filters
{
	#region ExpressionFieldDescriptorCollection
	/// <summary>
	/// A collection of <see cref="ExpressionFieldDescriptor"/> fields with run-time formula expressions. 
	/// An instance of this collection is returned by the <see cref="TableDescriptor.ExpressionFields"/> property
	/// of a <see cref="TableDescriptor"/>.
	/// </summary>
	[ListBindableAttribute(false)]
	public class ExpressionFieldDescriptorCollection : FieldDescriptorCollection
	{
		/// <summary>
		/// A Read-only and empty collection.
		/// </summary>
		public static new readonly ExpressionFieldDescriptorCollection Empty = new ExpressionFieldDescriptorCollection(null);

		/// <override/>
		protected override FieldDescriptor InternalCreateFieldDescriptor(string name)
		{
			return base.InternalCreateFieldDescriptor(name);
		}

		/// <override/>
		protected override void EnsureInitialized(bool populate)
		{
		}

		/// <override/>
        protected override FieldDescriptorCollection CreateCollection(TableInfo td, FieldDescriptor[] columnDescriptors)
		{
            return new ExpressionFieldDescriptorCollection(td, columnDescriptors);
		}

		/// <override/>
		protected override int InternalAdd(string name)
		{
			return Add(new ExpressionFieldDescriptor(name));
		}


		/// <override/>
		protected override void CheckType(object obj)
		{
			if (obj != null && !(obj is ExpressionFieldDescriptor))
				throw new ArgumentException("Wrong type");
		}

		/// <summary>
		/// Initializes a new empty collection.
		/// </summary>
		public ExpressionFieldDescriptorCollection()
		{
		}

        internal ExpressionFieldDescriptorCollection(TableInfo tableDescriptor)
			: base(tableDescriptor)
		{
		}

        internal ExpressionFieldDescriptorCollection(TableInfo tableDescriptor, FieldDescriptor[] columnDescriptors)
			: base(tableDescriptor, columnDescriptors)
		{
		}

		/// <summary>
		/// Adds multiple elements at the end of the collection.
		/// </summary>
		/// <param name="values">The array whose elements should be added to the end of the collection. 
		/// The array and its elements cannot be NULL references (Nothing in Visual Basic). 
		/// </param>
		public void AddRange(ExpressionFieldDescriptor[] values)
		{
			base.AddRange((FieldDescriptor[]) values);
		}

		/// <summary>
		/// Creates a copy of the collection and all its elements.
		/// </summary>
		/// <returns>A copy of the collection and all its elements.</returns>
		public new ExpressionFieldDescriptorCollection Clone()
		{
			return (ExpressionFieldDescriptorCollection) InternalClone();
		}

		/// <override/>
		public override bool Equals(object obj)
		{
			if (this == null && obj == null)
				return true;
			else if (this == null)
				return false;
			else if (!(obj is ExpressionFieldDescriptorCollection))
				return false;

			return InternalEquals((ExpressionFieldDescriptorCollection) obj);
		}

		/// <override/>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		/// <summary>
		/// Gets / sets the element at the zero-based index.
		/// </summary>
		public new ExpressionFieldDescriptor this[int index]
		{
			get
			{
				return (ExpressionFieldDescriptor) base[index];
			}
			set
			{
				base[index] = value;
			}
		}

		/// <summary>
		/// Gets / sets the element with the specified name.
		/// </summary>
		public new ExpressionFieldDescriptor this[string name]
		{
			get
			{
				return (ExpressionFieldDescriptor) base[name];
			}
			set
			{
				base[name] = value;
			}
		}

		/// <summary>
		/// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from ArrayList. The array must have zero-based indexing. </param>
		/// <param name="index">The zero-based index in array at which copying begins. </param>
		public void CopyTo(ExpressionFieldDescriptor[] array, int index)
		{
			int n = 0;
			foreach (ExpressionFieldDescriptor item in this)
			{
				array[index+n] = item;
				n++;
			}
		}

		/// <summary>
		/// Returns an enumerator for the entire collection.
		/// </summary>
		/// <returns>An Enumerator for the entire collection.</returns>
		/// <remarks>Enumerators only allow reading the data in the collection. 
		/// Enumerators cannot be used to modify the underlying collection.</remarks>
		public new ExpressionFieldDescriptorCollectionEnumerator GetEnumerator()
		{
			return new ExpressionFieldDescriptorCollectionEnumerator(this);
		}
	
	}

	/// <summary>
	/// Enumerator class for the <see cref="ExpressionFieldDescriptor"/> elements of an <see cref="ExpressionFieldDescriptorCollection"/>.
	/// </summary>
	public class ExpressionFieldDescriptorCollectionEnumerator : IEnumerator 
	{
		int _cursor = -1, _next = -1;
		ExpressionFieldDescriptorCollection _coll;

		/// <summary>
		/// Initalizes the enumerator and attaches it to the collection.
		/// </summary>
		/// <param name="collection">The parent collection to enumerate.</param>
		public ExpressionFieldDescriptorCollectionEnumerator(ExpressionFieldDescriptorCollection collection)
		{
			_coll = collection;
			_next = _coll.Count > 0 ? 0 : -1;
		}

		#region IEnumerator Members

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		public virtual void Reset()
		{
			_cursor = -1;
			_next = _coll.Count > 0 ? 0 : -1;
		}

		object IEnumerator.Current
		{
			get
			{
				return Current;
			}
		}

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public ExpressionFieldDescriptor Current
		{
			get
			{
				return _coll[_cursor];
			}
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// True if the enumerator was successfully advanced to the next element; False if the enumerator has passed the end of the collection.
		/// </returns>
		public bool MoveNext()
		{
			if (_next == -1)
				return false;

			_cursor = _next;

			_next++;
			if (_next >= _coll.Count)
				_next = -1;

			return _cursor != -1;
		}
		#endregion

	}


	#endregion

	#region TypeConverter

    /// <summary>
    /// The type converter for <see cref="DescriptorBase"/> objects. <see cref="DescriptorBaseConverter"/> 
    /// is a <see cref="ExpandableObjectConverter"/>. It overrides the default behavior of the 
    /// <see cref="ConvertTo"/> method and returns the <see cref="DescriptorBase.GetName"/> result
    /// of the <see cref="DescriptorBase"/> object. <see cref="ConvertTo"/> 
    /// is called from a property grid to determine the name of the object to be displayed.
    /// </summary>
    public class DescriptorBaseConverter : ExpandableObjectConverter
    {
        /// <override/>
        public override /*TypeConverter*/ bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            else
                return base.CanConvertTo(context, destinationType);
        }

        /// <override/>
        public override /*TypeConverter*/ object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                string name = ((DescriptorBase)value).GetName();
                return name != null ? name : "";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        } // end of method ConvertTo
    }

    /// <summary>
    /// Base class for schema definition objects of the grouping engine.
    /// </summary>
    [TypeConverter(typeof(DescriptorBaseConverter))]
    public abstract class DescriptorBase : IDisposable
    {
        bool inDispose;
        bool inDisposed;
        bool isDisposed;

        //		~DescriptorBase()
        //		{
        //			inDispose = true;
        //			Dispose(false);
        //			inDispose = false;
        //		}

        /// <summary>
        /// Returns True if object is executing the <see cref="Dispose"/> method call.
        /// </summary>
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool Disposing
        {
            get
            {
                return inDispose;
            }
        }

        /// <summary>
        /// Returns after object was disposed and object is executing the <see cref="Disposed"/> event.
        /// </summary>
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool InDisposed
        {
            get
            {
                return inDisposed;
            }
        }

        /// <summary>
        /// Gets if object has been disposed.
        /// </summary>
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed || inDispose)
                return;

            inDispose = true;
            Dispose(true);
            inDispose = false;
            isDisposed = true;
            inDisposed = true;
            OnDisposed(EventArgs.Empty);
            inDisposed = false;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Occurs after the object was disposed.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Raises the <see cref="Disposed"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
        protected virtual void OnDisposed(EventArgs e)
        {
            if (Disposed != null)
                Disposed(this, e);
        }

        /// <summary>
        /// Called to clean up state of this object when it is disposed.
        /// </summary>
        /// <param name="disposing">True if called from <see cref="Dispose"/>; False if called from Finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            //			if (disposing)
            //			{
            //			}
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The framework calls this method to determine the name of this object. 
        /// </summary>
        /// <returns></returns>
        public abstract string GetName();

        /// <summary>
        /// The framework calls this method to determine whether calling Reset will have any effect.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanResetValue()
        {
            return ShouldSerialize();
        }

        /// <summary>
        /// The framework calls this method to reset the object back to its default state.
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// The framework calls this method to determine whether properties or child objects of this object
        /// should be serialized. (Code serialization and / or XML Serialization).
        /// </summary>
        /// <returns></returns>
        public virtual bool ShouldSerialize()
        {
            return true;
        }

        /// <override/>
        public override string ToString()
        {
            string name = GetName();
            if (name == null)
                return GetType().Name;
            return GetType().Name + " { " + name + "}";
        }

    }

	/// <summary>
	/// The type converter for <see cref="ExpressionFieldDescriptor"/> objects. <see cref="ExpressionFieldDescriptorTypeConverter"/> 
	/// is an <see cref="DescriptorBaseConverter"/>. It overrides the default behavior of the 
	/// <see cref="ConvertTo"/> method and adds support for design-time code serialization.
	/// </summary>
	public class ExpressionFieldDescriptorTypeConverter : DescriptorBaseConverter
	{
		/// <override/>
		public override /*TypeConverter*/ bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof(System.ComponentModel.Design.Serialization.InstanceDescriptor))
				return true;
			else
				return base.CanConvertTo(context, destinationType);
		}

		/// <override/>
		public override /*TypeConverter*/ object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(System.ComponentModel.Design.Serialization.InstanceDescriptor)
				&& (value is ExpressionFieldDescriptor))
			{

				//		public ExpressionFieldDescriptor(string name, string expression)
								
				ExpressionFieldDescriptor expressionField = (ExpressionFieldDescriptor) value;
				Type type = typeof(ExpressionFieldDescriptor);

				if (!expressionField.ShouldSerializeResultType())
				{
					return new InstanceDescriptor(type.GetConstructor(
						new Type[] { typeof(string), typeof(string) } 
						), 
						new object[] { expressionField.Name, expressionField.Expression }
						, true
						);	
				}
				else
				{
					return new InstanceDescriptor(type.GetConstructor(
						new Type[] { typeof(string), typeof(string), typeof(string) } 
						), 
						new object[] { expressionField.Name, expressionField.Expression, expressionField.ResultType }
						, true
						);	
				}

			}
			return base.ConvertTo(context, culture, value, destinationType);
		} // end of method ConvertTo

		/// <override/>
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			System.ComponentModel.PropertyDescriptorCollection pds
				= TypeDescriptor.GetProperties(value.GetType(), attributes);

			string[] atts = new string[]
			{
				"Name",
				"ResultType",
				"Expression",
			};

			return pds.Sort(atts);
		}

	}
	#endregion

	#region ExpressionFieldDescriptor

	/// <summary>
	/// ExpressionFieldDescriptor is a <see cref="FieldDescriptor"/> with support for run-time formula expressions.
	/// <para/>
	/// Expression fields are managed by the <see cref="ExpressionFieldDescriptorCollection"/> that
	/// is returned by the <see cref="TableDescriptor.ExpressionFields"/> property
	/// of a <see cref="TableDescriptor"/>.
	/// </summary>
	[TypeConverter(typeof(ExpressionFieldDescriptorTypeConverter))]
	public class ExpressionFieldDescriptor : FieldDescriptor
	{
		string resultType = "System.String";
		string expression = "";
		bool expressionModified = false;
		bool resultTypeModified = false;
		string compiledExpression = null;
		int fieldsVersion = -1;
        List<string> relatedColumnList = new List<string>();

		/// <summary>
		/// Initializes a new empty expression field.
		/// </summary>
		public ExpressionFieldDescriptor()
			: base()
		{
		}

		/// <summary>
		/// Initializes a new expression field with a specified name.
		/// </summary>
		/// <param name="name">The name of the field.</param>
		public ExpressionFieldDescriptor(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Initializes a new expression field with a name and expression.
		/// </summary>
		/// <param name="name">The name of the field.</param>
		/// <param name="expression">The expression. See the Grid User's Guide for expression syntax and examples.</param>
		public ExpressionFieldDescriptor(string name, string expression)
			: base(name, "", true, "")
		{
			this.expression = expression;
			expressionModified = true;
            UpdateRelatedColumnList();
		}

		/// <summary>
		/// Initializes a new expression field with a name, expression, and result type.
		/// </summary>
		/// <param name="name">The name of the field.</param>
		/// <param name="expression">The expression. See the Grid User's Guide for expression syntax and examples.</param>
		/// <param name="resultType">The result type.</param>
		public ExpressionFieldDescriptor(string name, string expression, Type resultType)
			: base(name, "", true, "")
		{
			this.expression = expression;
			expressionModified = true;
			this.resultType = String.Concat(resultType.FullName, ",", resultType.AssemblyQualifiedName.Split(',')[1]);;
			this.resultTypeModified = true;
		}

		/// <summary>
		/// Initializes a new expression field with a name, expression, and result type.
		/// </summary>
		/// <param name="name">The name of the field.</param>
		/// <param name="expression">The expression. See the Grid User's Guide for expression syntax and examples.</param>
		/// <param name="resultType">The result type.</param>
		public ExpressionFieldDescriptor(string name, string expression, string resultType)
			: base(name, "", true, "")
		{
			this.expression = expression;
			expressionModified = true;
			this.resultType = resultType;
            this.resultTypeModified = true;
            UpdateRelatedColumnList();
		}

		/// <override/>
		public override void InitializeFrom(FieldDescriptor other)
		{
			ExpressionFieldDescriptor gcd = (ExpressionFieldDescriptor) other;
			Expression = gcd.Expression;
			ResultType = gcd.ResultType;
			base.InitializeFrom(other);
		}

		/// <override/>
		public override FieldDescriptor Clone()
		{
			ExpressionFieldDescriptor ed = new ExpressionFieldDescriptor();
			base.CopyAllMembersTo(ed);
			this.CopyExpressionFieldMembersTo(ed);
			return ed;
		}

		protected void CopyExpressionFieldMembersTo(ExpressionFieldDescriptor ed)
		{
			ed.compiledExpression = this.compiledExpression;
			ed.expression = this.expression;
			ed.expressionModified = this.expressionModified;
			ed.fieldsVersion = -1;//this.fieldsVersion;
			ed.index = this.index;
			ed.resultType = this.resultType;
			ed.resultTypeModified = this.resultTypeModified;
		}

		/// <override/>
		public override bool Equals(object obj)
		{
			if (!base.Equals(obj))
				return false;

			if (!(obj is ExpressionFieldDescriptor))
				return false;

			return InternalEquals((ExpressionFieldDescriptor) obj);
		}

		bool InternalEquals(ExpressionFieldDescriptor other)
		{
			return other.Expression == Expression
				&& other.ResultType == ResultType
				&& other.Name == Name
				;
		}

		/// <override/>
		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}


		/// <summary>
		/// The result type that the expression should be converted to (default is System.String).
		/// </summary>
		[
		Browsable(true),
		TypeConverter(typeof(ResultTypeConverter)),
		RefreshProperties(RefreshProperties.Repaint),
		]
		[Description("The result type that the expression should be converted to (default is System.String).")]
		public string ResultType
		{
			get
			{
				return resultType;
			}
			set
			{
				if (resultType != value)
				{
					resultType = value;
					resultTypeModified = true;
				}
			}
		}
		/// <summary>
		/// Determines if <see cref="ResultType"/> was modified.
		/// </summary>
		/// <returns></returns>
		public bool ShouldSerializeResultType()
		{
			return resultTypeModified;
		}

		/// <override/>
		public override Type GetPropertyType()
		{
			if (this.ResultType != null)
				return Type.GetType(this.ResultType);
			return typeof(string);
		}


		/// <summary>
		/// Resets the result type to System.String.
		/// </summary>
		public void ResetResultType()
		{
			resultType = "System.String";
			resultTypeModified = false;
		}

		
		/// <summary>
		/// The formula expression. See the Grid user's guid for syntax and examples.
		/// </summary>
		[Description("The formula expression.")]
		public string Expression
		{
			get
			{
				return expression;
			}
			set
			{
				if (expression != value)
				{					
					expression = value;
					expressionModified = true;
                    compiledExpression = null;
                    UpdateRelatedColumnList();
					string s = GetCompiledExpression(); // Force recalc
				}
			}
		}

        internal void UpdateRelatedColumnList()
        {
            try
            {
                if (string.IsNullOrEmpty(this.Name))
                    return;

                if (TableDescriptor != null)
                {
                    string tempExp = expression;

                    char[] array = tempExp.ToCharArray();
                    char[] name = new char[100];
                    bool isName = false;
                    int nameIndex = 0;
                    List<string> names = new List<string>();

                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] == '[')
                        {
                            isName = true;
                        }
                        else if (array[i] == ']')
                        {
                            isName = false;
                            string mappingName = new string(name);
                            mappingName = mappingName.Trim('\0');
                            names.Add(mappingName);
                        }
                        else
                        {
                            if (isName)
                            {
                                name[nameIndex] = array[i];
                                nameIndex++;
                            }
                            else
                            {
                                nameIndex = 0;
                                name = new char[100];
                            }
                        }
                    }

                    for (int j = 0; j < this.TableDescriptor.AllRecords.Count; j++)
                    {
                        Record rec = TableDescriptor.AllRecords[j] as Record;

                        if (rec == null)
                            continue;

                        if (rec.CustomFormulaKeys.ContainsKey(this.Name))
                            rec.CustomFormulaKeys[this.Name].Clear();

                        for (int i = 0; i < names.Count; i++)
                        {
                            TableInfo.TableColumn col = TableDescriptor.GetColumnFromName(names[i]);

                            if (col != null)
                            {
                                if (rec != null && !rec.CustomFormulaKeys.ContainsKey(this.Name))
                                    rec.CustomFormulaKeys.Add(this.Name, new Dictionary<TableInfo.TableColumn, object>());

                                if (rec != null && !rec.CustomFormulaKeys[this.Name].ContainsKey(col))
                                    rec.CustomFormulaKeys[this.Name].Add(col, null);
                            }
                        }
                    }

                    for (int j = 0; j < this.TableDescriptor.FilteredRecords.Count; j++)
                    {
                        Record rec = TableDescriptor.FilteredRecords[j] as Record;

                        if (rec == null)
                            continue;

                        rec.CustomFormulaKeys.Clear();

                        for (int i = 0; i < names.Count; i++)
                        {
                            TableInfo.TableColumn col = TableDescriptor.GetColumnFromName(names[i]);

                            if (col != null)
                            {
                                if (rec != null && !rec.CustomFormulaKeys.ContainsKey(this.Name))
                                    rec.CustomFormulaKeys.Add(this.Name, new Dictionary<TableInfo.TableColumn, object>());

                                if (rec != null && !rec.CustomFormulaKeys[this.Name].ContainsKey(col))
                                    rec.CustomFormulaKeys[this.Name].Add(col, null);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionsLogger.LogError(ex);
            }
        }

		private bool ShouldSerializeExpression()
		{
			return expressionModified;
		}

		/// <summary>
		/// Resets the formula expression to empty.
		/// </summary>
		public void ResetExpression()
		{
			expression = "";
			expressionModified = false;
			compiledExpression = null;
		}

		/// <summary>
		/// Gets a string that holds pre-compiled information about the expression.
		/// </summary>
		/// <returns>A string that holds pre-compiled information about the expression.</returns>
		public string GetCompiledExpression()
		{
			if (expression.Length > 0 && TableDescriptor != null)
			{
				int tableDescriptorfieldsVersion = this.TableDescriptor.Version;
                if (this.fieldsVersion != tableDescriptorfieldsVersion)
				{
					IExpressionFieldEvaluator eval = GetExpressionEvaluator();
					string s = eval.PutTokensInFormula(expression.ToLower());
					compiledExpression = eval.Parse(s);
					this.fieldsVersion = tableDescriptorfieldsVersion;
				}
			}
			return compiledExpression;
		}

        /// <summary>
        /// Returns an array of field descriptor names that the expression references
        /// </summary>
        /// <returns></returns>
        protected override string DetermineReferencedFields()
        {
            ArrayList al = new ArrayList();
            string expr = expression.ToLower().Replace(" ", "");

            foreach (TableInfo.TableColumn fd in TableDescriptor.Columns)
            {
                string search = "[" + fd.Name.ToLower() + "]";
                if (expr.IndexOf(search) != -1)
                    al.Add(fd.Name);
            }
            StringBuilder sb = new StringBuilder();
            foreach(string s in al)
            {
                if (sb.Length == 0)
                    sb.Append(s);
                else
                    sb.Append(";" + s);
            }
            return sb.ToString();
        }

  
		/// <summary>
		/// Resets the compiled expression. It will be recompiled later on demand.
		/// </summary>
		public void ResetCompiledExpression()
		{
			compiledExpression = null;
		}

		/// <summary>
		/// Not used for expression fields.
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new string MappingName
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		/// <summary>
		/// Not used for expression fields.
		/// </summary>
		[System.Xml.Serialization.XmlIgnore]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public new string DefaultValue
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		/// <summary>
		/// Not used for expression fields.
		/// </summary>
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public override bool ReadOnly
		{
			get
			{
				return true;
			}

			set
			{
			}
		}

		/// <summary>
		/// Calculates the expression result value for the specified record.
		/// </summary>
		public override object GetValue(Record record)
		{
			IExpressionFieldEvaluator eval = GetExpressionEvaluator();
			return eval.ComputeFormulaValueAt(GetCompiledExpression(), record);
		}


		IExpressionFieldEvaluator GetExpressionEvaluator()
		{
			return TableDescriptor.ExpressionFieldEvaluator;
		}


	}

	#endregion

	/// <summary>
	/// Implements a <see cref="TypeConverter"/> for the <see cref="ExpressionFieldDescriptor.ResultType"/> property in
	/// <see cref="ExpressionFieldDescriptor"/>.
	/// </summary>
	public class ResultTypeConverter: TypeConverter
	{
		/// <override/>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)  
		{
			if (sourceType == typeof(System.String)) 
				return true;
			return base.CanConvertFrom(context,sourceType);
		}

		/// <override/>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)  
		{
			if (value is string)
			{
				return value;
			}

			if (value is Type)
			{
				Type type = (Type) value;
				return String.Concat(type.FullName, ",", type.AssemblyQualifiedName.Split(',')[1]);
			}

			return base.ConvertFrom(context,culture,value);
		}

		// no string conversion

		/// <override/>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)  
		{
			if (destinationType == typeof(string))
			{
				return value as string;
			}
			
			if (destinationType == typeof(Type))
			{
				string s = value as string;
			
				if (s == null || s.Length == 0)
					return typeof(object);

				switch (s)
				{
					case "System.String": return typeof(string);
					case "System.Double": return typeof(System.Double);
					case "System.Int32": return typeof(System.Int32);
					case "System.Boolean": return typeof(System.Boolean);
					case "System.DateTime": return typeof(System.DateTime);
					case "System.Int16": return typeof(System.Int16);
					case "System.Int64": return typeof(System.Int64);
					case "System.Single": return typeof(System.Single);
					case "System.Byte": return typeof(System.Byte);
					case "System.Char": return typeof(System.Char);
					case "System.Decimal": return typeof(System.Decimal);
					case "System.UInt16": return typeof(System.UInt16);
					case "System.UInt32": return typeof(System.UInt32);
					case "System.UInt64": return typeof(System.UInt64);
				}

				return Type.GetType((string) value);
			}

			return base.ConvertFrom(context,culture,value);

		}

		/// <override/>
		public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)  
		{
			return svc;
		}

		/// <override/>
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)  
		{
			return false;
		}

		/// <override/>
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)  
		{
			return true;
		}

		// Fields
		static ResultTypeConverter()
		{
			values = new string[] 
				{
					"System.String",
					"System.Double",
					"System.Int32",
					"System.Boolean",
					//"System.Drawing.Color, System.Drawing",
					"System.DateTime",
					"System.Int16",
					"System.Int64",
					"System.Single",
					"System.Byte",
					"System.Char",
					"System.Decimal",
					"System.UInt16",
					"System.UInt32",
					"System.UInt64",
				//"System.Windows.Forms.DockStyle, System.Windows.Forms",
				/*typeof(System.String),
					typeof(System.Double),
					typeof(System.Int32),
					typeof(System.Boolean),
					typeof(System.Drawing.Color),
					typeof(System.DateTime),
					typeof(System.Int16),
					typeof(System.Int64),
					typeof(System.Single),
					typeof(System.SByte),
					typeof(System.Byte),
					typeof(System.Char),
					typeof(System.Decimal),
					typeof(System.DBNull),
					typeof(System.UInt16),
					typeof(System.UInt32),
					typeof(System.UInt64),*/
			};
			Array.Sort(values);
//			Type[] types = new Type[values.Length];
//			for (int i = 0; i < values.Length; i++)
//				types[i] = Type.GetType(values[i]);
			svc = new TypeConverter.StandardValuesCollection(values);
		}

		private static string[] values;
		private static TypeConverter.StandardValuesCollection svc;
	}

}

