using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Filters
{
    public class RecordFilterDescriptorCollection : IDisposable, IList
    {
        private ArrayList inner = new ArrayList();
        private TableInfo tableInfo;
        internal List<string> FilterList = new List<string>();

        public event EventHandler RecordFiltersCleared;

        /// <summary>
		/// The Table that this collection belongs to.
		/// </summary>
		public TableInfo TableInfo
		{
			get
			{
				return this.tableInfo;
			}
			set
			{
				this.tableInfo = value;
				foreach (RecordFilterDescriptor cd in this.inner)
				{
                    cd.SetTableDescriptor(value);
                    cd.SetCollection(this);
				}
			}
		}

        RecordFilterDescriptorCollection SyncRoot
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Returns an enumerator for the entire collection.
		/// </summary>
		/// <returns>An Enumerator for the entire collection.</returns>
		/// <remarks>Enumerators only allow reading the data in the collection.
		/// Enumerators cannot be used to modify the underlying collection.</remarks>
		public RecordFilterDescriptorCollectionEnumerator GetEnumerator()
		{
			return new RecordFilterDescriptorCollectionEnumerator(this);
		}

		/// <summary>
		/// Inserts a descriptor element into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the element should be inserted.</param>
		/// <param name="value">The element to insert. The value must not be a NULL reference (Nothing in Visual Basic). </param>
		public void Insert(int index, RecordFilterDescriptor value)
		{
			inner.Insert(index, value);
			value.SetCollection(this);
			
        }

		/// <summary>
		/// Removes the specified descriptor element from the collection.
		/// </summary>
		/// <param name="value">The element to remove from the collection. If the value is NULL or the element is not contained
		/// in the collection, the method will do nothing.</param>
		public void Remove(RecordFilterDescriptor value)
		{
			int index = IndexOf(value);
			inner.Remove(value);
            for (int i = 0; i < value.MappingNames.Count; i++)
            {
                FilterList.Remove(value.MappingNames[i]);
            }
            
        }

		/// <summary>
		/// Adds a filter descriptor to the end of the collection.
		/// </summary>
		/// <param name="value">The element to be added to the end of the collection. The value must not be a NULL reference (Nothing in Visual Basic). </param>
		/// <returns>The zero-based collection index at which the value has been added.</returns>
		public int Add(RecordFilterDescriptor value)
		{
			int index = inner.Add(value);
			value.SetCollection(this);

            if (value.MappingNames.Count == value.MappingColumnPositions.Count)
            {
                for (int i = 0; i < value.MappingNames.Count; i++)
                {
                    FilterList.Add(value.MappingNames[i]);
                }
            }
            else
            {
                for (int i = 0; i < value.MappingNames.Count; i++)
                {
                    FilterList.Add(value.MappingNames[i]);
                }
            }

            char[] array = value.Expression.ToCharArray();
            char[] name = new char[100];
            bool isName = false;
            int nameIndex = 0;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == '[')
                {
                    isName = true;
                }
                else if (array[i] == ']')
                {
                    isName = false;
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
                        string mappingName = new string(name);
                        mappingName = mappingName.Trim('\0');

                        if(!string.IsNullOrEmpty(mappingName) && !this.FilterList.Contains(mappingName))
                            this.FilterList.Add(mappingName);

                        name = new char[100];
                    }
                }
            }

			return index;
		}

		/// <summary>
		/// Creates a RecordFilterDescriptor based on the specified expression and adds it to the end of the collection.
		/// </summary>
		/// <param name="expression">The filter expression. See the Grid User's Guide for valid expressions.</param>
		/// <returns>The zero-based collection index at which the value has been added.</returns>
		public int Add(string expression)
		{
			RecordFilterDescriptor rd  = new RecordFilterDescriptor(expression);
			return Add(rd);
		}

		/// <summary>
		/// Creates a RecordFilterDescriptor and adds it to the end of the collection.
		/// </summary>
		/// <param name="name">The name of the filter.</param>
		/// <param name="compareOperator">The comparison operator.</param>
		/// <param name="compareValue">The comparison value.</param>
		/// <returns>The zero-based collection index at which the value has been added.</returns>
		public int Add(string name, FilterCompareOperator compareOperator, object compareValue)
		{
			RecordFilterDescriptor rd  = new RecordFilterDescriptor(name, new FilterCondition(compareOperator, compareValue));
			return Add(rd);
		}

		/// <summary>
		/// Removes the specified descriptor element with the specified name from the collection.
		/// </summary>
		/// <param name="name">The name of the element to remove from the collection. If no element with that name is found
		/// in the collection, the method will do nothing.</param>
		public void Remove(string name)
		{
			Remove(this[name]);
		}

		/// <summary>
		/// Removes the element at the specified index of the collection.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove. </param>
        public void RemoveAt(int index)
        {
            object value = inner[index];
            inner.RemoveAt(index);
            RecordFilterDescriptor filt = value as RecordFilterDescriptor;
            if (filt != null)
            {
                for (int i = 0; i < filt.MappingNames.Count; i++)
                {
                    FilterList.Remove(filt.MappingNames[i]);
                }
            }
        }

		/// <summary>
		/// Disposes the object and collection items.
		/// </summary>
		public void Dispose()
		{
            //foreach (DescriptorBase db in inner)
            //    db.Dispose();
			inner.Clear();
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Removes all elements from the collection.
		/// </summary>
		public void Clear()
		{
			inner.Clear();
            FilterList.Clear();
            tableInfo.SetRecreateFlag(true);

            OnRecordFiltersCleared();
		}

        private void OnRecordFiltersCleared()
        {
            try
            {
                if (RecordFiltersCleared != null)
                    RecordFiltersCleared(this, null);
            }
            catch (Exception ex)
            {
                Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Helpers.ExceptionsLogger.LogError(ex);
            }
        }
        
		/// <summary>
		/// Returns False.
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Returns False since this collection has no fixed size.
		/// </summary>
		public bool IsFixedSize
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Returns False.
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Gets the number of elements contained in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return inner.Count;
			}
		}

        /// <summary>
		/// Gets / sets the element at the zero-based index.
		/// </summary>
		public RecordFilterDescriptor this[int index]
		{
			get
			{
				return (RecordFilterDescriptor) inner[index];
			}
			set
			{
				if (inner[index] != value)
				{
					inner[index] = value;
					value.SetCollection(this);
					
                }
			}
		}

        /// <summary>
        /// Gets / sets the element with the specified name.
        /// </summary>
        public RecordFilterDescriptor this[string name]
        {
            get
            {
                int index = Find(name);
                if (index == -1)
                    return null;
                return (RecordFilterDescriptor)inner[index];
            }
            set
            {
                int index = Find(name);
                if (index == -1)
                {
                    value.Name = name;
                    Add(value);
                }
                else
                {
                    inner[index] = value;
                    value.Name = name;
                    value.SetCollection(this);
                }
            }
        }

        internal int Find(string name)
		{
			for (int n = 0; n < Count; n++)
				if (this[n].Name == name)
					return n;
			return -1;
		}

		/// <summary>
		/// Determines if the element belongs to this collection.
		/// </summary>
		/// <param name="value">The element to locate in the collection. The value can be a NULL reference (Nothing in Visual Basic).</param>
		/// <returns>True if item is found in the collection; otherwise, False.</returns>
		public bool Contains(RecordFilterDescriptor value)
		{
			if (value == null)
				return false;

			return inner.Contains(value);
		}

		/// <summary>
		/// Determines if the element with the specified name belongs to this collection.
		/// </summary>
		/// <param name="name">The name of the element to locate in the collection.</param>
		/// <returns>True if item is found in the collection; otherwise, False.</returns>
		public bool Contains(string name)
		{
			return Find(name) != -1;
		}

		/// <summary>
		/// Returns the zero-based index of the occurrence of the element in the collection.
		/// </summary>
		/// <param name="value">The element to locate in the collection. The value can be a NULL reference (Nothing in Visual Basic). </param>
		/// <returns>The zero-based index of the occurrence of the element within the entire collection, if found; otherwise, -1.</returns>
		public int IndexOf(RecordFilterDescriptor value)
		{
			return inner.IndexOf(value);
		}

		/// <summary>
		/// Searches for the element with the specified name.
		/// </summary>
		/// <param name="name">The name of the element to locate in the collection. </param>
		/// <returns>The zero-based index of the occurrence of the element with matching name within the entire collection, if found; otherwise, -1.</returns>
		public int IndexOf(string name)
		{
			return Find(name);
		}

		/// <summary>
		/// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from ArrayList. The array must have zero-based indexing. </param>
		/// <param name="index">The zero-based index in array at which copying begins. </param>
		public void CopyTo(RecordFilterDescriptor[] array, int index)
		{
			int count = Count;
			for (int n = 0; n < count; n++)
				array[index+n] = this[n];
		}


        /// <summary>
		/// Initializes a new empty collection and attaches it to a <see cref="TableDescriptor"/>.
		/// </summary>
		internal RecordFilterDescriptorCollection(TableInfo tableInfo)
		{
			this.tableInfo = tableInfo;
		}

        internal RecordFilterDescriptorCollection()
        {
        }

        /// <summary>
        /// Evaluates all filters for the given record and returns True if the record
        /// meets the filter's criteria.
        /// </summary>
        /// <param name="record">The record to be evaluated.</param>
        /// <returns>True if the record
        /// meets the filter's criteria; False otherwise.</returns>
        public bool CompareRecord(Record record)
        {
            if (record == null)
                return false;

            if (this.Count == 0)
                return true;

            //if (this.LogicalOperator == FilterLogicalOperator.And)
            //{
            foreach (RecordFilterDescriptor recordFilter in this)
            {
                if (!recordFilter.CompareRecord(record))
                    return false;
            }
            return true;
            //}
            //else
            //{
            //    foreach (RecordFilterDescriptor recordFilter in this)
            //    {
            //        if (recordFilter.CompareRecord(record))
            //            return true;
            //    }
            //    return false;
            //}
        }

        /// <summary>
		/// Creates a copy of the collection and all its elements.
		/// </summary>
		/// <returns>A copy of the collection and all its elements.</returns>
		public RecordFilterDescriptorCollection Clone()
		{
			RecordFilterDescriptorCollection coll = new RecordFilterDescriptorCollection();
            //coll.inner = new ArrayList();
            //coll.logicalOperator = logicalOperator;
            //coll.version = version+1000;
            //coll.tableDescriptor = tableDescriptor;
            //coll.fieldsVersion = -1;
			int count = Count;
			RecordFilterDescriptor[] filterDescriptors = new RecordFilterDescriptor[count];
			for (int n = 0; n < count; n++)
			{
                //coll.inner.Add(this[n].Clone());
                //coll[n].SetCollection(coll);
			}
			return coll;
		}

        #region IList Private Members

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (RecordFilterDescriptor)value;
            }
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (RecordFilterDescriptor)value);
        }

        void IList.Remove(object value)
        {
            Remove((RecordFilterDescriptor)value);
        }

        bool IList.Contains(object value)
        {
            return Contains((RecordFilterDescriptor)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((RecordFilterDescriptor)value);
        }

        int IList.Add(object value)
        {
            return Add((RecordFilterDescriptor)value);
        }

        #endregion

        #region ICollection Private Members

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((RecordFilterDescriptor[])array, index);
        }

        object ICollection.SyncRoot
        {
            get
            {
                return null;
            }
        }

        #endregion

        #region IEnumerable Private Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        internal void EnsureFieldDescriptors()
        {
            if (inner.Count > 0 && tableInfo != null)
            {
                foreach (RecordFilterDescriptor cd in this.inner)
                {
                    cd.InitColumn(tableInfo);
                }
            }
        }
    }

    /// <summary>
    /// Enumerator class for <see cref="RecordFilterDescriptor"/> elements of a <see cref="RecordFilterDescriptorCollection"/>.
    /// </summary>
    public class RecordFilterDescriptorCollectionEnumerator : IEnumerator
    {
        int _cursor = -1, _next = -1;
        RecordFilterDescriptorCollection _coll;

        /// <summary>
        /// Initalizes the enumerator and attaches it to the collection.
        /// </summary>
        /// <param name="parentCollection">The parent collection to enumerate.</param>
        public RecordFilterDescriptorCollectionEnumerator(RecordFilterDescriptorCollection parentCollection)
        {
            _coll = parentCollection;
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
        public RecordFilterDescriptor Current
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
}
