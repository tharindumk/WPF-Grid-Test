using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Filters
{
    ///
    /// Imported from Syncfusion Grid.Grouping
    /// 

    public class FilterConditionCollection: IList
    {
        ArrayList inner = new ArrayList();
        
        internal bool insideCollectionEditor = false;

        /// <override/>
        public override string ToString()
        {
            return String.Format("FilterConditionCollection: Count {0}, InsideColl {1}",
                Count, insideCollectionEditor);
        }

        /// <summary>
        /// Gets / sets whether the collection is manipulated inside a collection editor.
        /// </summary>
        public bool InsideCollectionEditor
        {
            get
            {
                return insideCollectionEditor;
            }
            set
            {
                if (insideCollectionEditor != value)
                    insideCollectionEditor = value;
            }
        }

       
        /// <summary>
        /// Initializes a new empty collection.
        /// </summary>
        public FilterConditionCollection()
        {
        }

        protected internal FilterConditionCollection(FilterCondition[] filterDescriptors)
        {
            this.inner.AddRange(filterDescriptors);
            int count = Count;
            for (int n = 0; n < count; n++)
                this[n].SetCollection(this);
        }

        /// <override/>
        public override bool Equals(object obj)
        {
            if (this == null && obj == null)
                return true;
            else if (this == null)
                return false;
            else if (!(obj is FilterConditionCollection))
                return false;

            return Equals((FilterConditionCollection)obj);
        }

        /// <override/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        bool Equals(FilterConditionCollection other)
        {
            int count = Count;
            if (other.Count != count)
                return false;
            for (int n = 0; n < count; n++)
            {
                if (!this[n].Equals(other[n]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets / sets the element at the zero-based index.
        /// </summary>
        public FilterCondition this[int index]
        {
            get
            {
                return (FilterCondition)inner[index];
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
        /// Determines if the element belongs to this collection.
        /// </summary>
        /// <param name="value">The Object to locate in the collection. The value can be a NULL reference (Nothing in Visual Basic).</param>
        /// <returns>True if item is found in the collection; otherwise, False.</returns>
        public bool Contains(FilterCondition value)
        {
            if (value == null)
                return false;

            return inner.Contains(value);
        }

        /// <summary>
        /// Returns the zero-based index of the occurrence of the element in the collection.
        /// </summary>
        /// <param name="value">The element to locate in the collection. The value can be a NULL reference (Nothing in Visual Basic). </param>
        /// <returns>The zero-based index of the occurrence of the element within the entire collection, if found; otherwise, -1.</returns>
        public int IndexOf(FilterCondition value)
        {
            return inner.IndexOf(value);
        }

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from ArrayList. The array must have zero-based indexing. </param>
        /// <param name="index">The zero-based index in array at which copying begins. </param>
        public void CopyTo(FilterCondition[] array, int index)
        {
            int count = Count;
            for (int n = 0; n < count; n++)
                array[index + n] = this[n];
        }

        FilterConditionCollection SyncRoot
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
        /// <remarks>Enumerators only allow reading of the data in the collection.
        /// Enumerators cannot be used to modify the underlying collection.</remarks>
        public FilterConditionCollectionEnumerator GetEnumerator()
        {
            return new FilterConditionCollectionEnumerator(this);
        }

        /// <summary>
        /// Inserts a descriptor element into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the element should be inserted.</param>
        /// <param name="value">The element to insert. The value must not be a NULL reference (Nothing in Visual Basic). </param>
        public void Insert(int index, FilterCondition value)
        {
            inner.Insert(index, value);
            value.SetCollection(this);
        }

        /// <summary>
        /// Removes the specified descriptor element from the collection.
        /// </summary>
        /// <param name="value">The element to remove from the collection. If the value is NULL or the element is not contained
        /// in the collection, the method will do nothing.</param>
        public void Remove(FilterCondition value)
        {
            int index = IndexOf(value);
            inner.Remove(value);
        }

        /// <summary>
        /// Adds FilterCondition to the end of the collection.
        /// </summary>
        /// <param name="value">The element to be added to the end of the collection. The value must not be a NULL reference (Nothing in Visual Basic). </param>
        /// <returns>The zero-based collection index at which the value has been added.</returns>
        public int Add(FilterCondition value)
        {
            int index = inner.Add(value);
            value.SetCollection(this);
            return index;
        }

        /// <summary>
        /// Creates a FilterCondition and adds it to the end of the collection.
        /// </summary>
        /// <param name="compareOperator">The comparison operator.</param>
        /// <param name="compareValue">The comparison value.</param>
        /// <returns>The zero-based collection index at which the value has been added.</returns>
        public int Add(FilterCompareOperator compareOperator, object compareValue)
        {
            return Add(new FilterCondition(compareOperator, compareValue));
        }

        /// <summary>
        /// Removes the element at the specified index of the collection.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove. </param>
        public void RemoveAt(int index)
        {
            object value = inner[index];
            inner.RemoveAt(index);
        }

        /// <summary>
        /// Removes all elements from the collection.
        /// </summary>
        public void Clear()
        {
            inner.Clear();
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

        #region IList Private Members

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (FilterCondition)value;
            }
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (FilterCondition)value);
        }

        void IList.Remove(object value)
        {
            Remove((FilterCondition)value);
        }

        bool IList.Contains(object value)
        {
            return Contains((FilterCondition)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((FilterCondition)value);
        }

        int IList.Add(object value)
        {
            return Add((FilterCondition)value);
        }

        #endregion

        #region ICollection Private Members

        void ICollection.CopyTo(Array array, int index)
        {
            CopyTo((FilterCondition[])array, index);
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
    }

    public class FilterConditionCollectionEnumerator : IEnumerator
	{
		int _cursor = -1, _next = -1;
		FilterConditionCollection _coll;

		/// <summary>
		/// Initalizes the enumerator and attaches it to the collection.
		/// </summary>
		/// <param name="parentCollection">The parent collection to enumerate.</param>
		public FilterConditionCollectionEnumerator(FilterConditionCollection parentCollection)
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
		public FilterCondition Current
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
