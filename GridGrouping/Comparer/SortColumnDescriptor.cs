using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Accessors;

namespace Mubasher.ClientTradingPlatform.Infrastructure.UI.Controls.GridGrouping.Comparer
{
    public class SortColumnDescriptor : ICloneable
    {
        private string name = "";

        private IComparer comparer = null;
        private ListSortDirection sortDirection = ListSortDirection.Ascending;
        public TableInfo TableInfo = null;
        public SortComparisonMethod ComparisonMethod = SortComparisonMethod.Common;
        public PropertyAccessor Accessor = null;
        private string id = string.Empty;

        /// <summary>
        /// Initializes a new empty object.
        /// </summary>
        public SortColumnDescriptor()
        {

        }

        /// <summary>
        /// Initializes a new sort descriptor for the field with the given name.
        /// </summary>
        /// <param name="name">The field name.</param>
        public SortColumnDescriptor(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Initializes a new sort descriptor for the field with the given name.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <param name="sortDirection">The sort direction.</param>
        public SortColumnDescriptor(string name, ListSortDirection sortDirection, TableInfo info)
        {
            this.sortDirection = sortDirection;
            this.name = name;
            this.TableInfo = info;
        }

        /// <override/>
        public override string ToString()
        {
            return GetType().Name + " { " + name + " }";
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <summary>
        /// Called from <see cref="Clone"/> to create a new object of the correct type and copies all its members to this
        /// new object with <see cref="CopyAllMembersTo"/>.
        /// </summary>
        /// <returns>The new object.</returns>
        protected virtual object InternalClone()
        {
            SortColumnDescriptor sd = new SortColumnDescriptor();
            CopyAllMembersTo(sd);
            return sd;
        }

        /// <summary>
        /// Copies all members to another object without raising change events.
        /// </summary>
        /// <param name="sd">The target object.</param>
        protected void CopyAllMembersTo(SortColumnDescriptor sd)
        {
            sd.comparer = this.comparer;
            sd.isSorting = this.isSorting;
            sd.name = this.name;
            sd.sortDirection = this.sortDirection;
        }

        /// <summary>
        /// Creates a copy of this descriptor.
        /// </summary>
        /// <returns>A copy of this descriptor.</returns>
        public SortColumnDescriptor Clone()
        {
            return (SortColumnDescriptor)InternalClone();
        }

        /// <override/>
        public override bool Equals(object obj)
        {
            if (this == null && obj == null)
                return true;
            else if (this == null)
                return false;
            else if (!(obj is SortColumnDescriptor))
                return false;
            return Equals((SortColumnDescriptor)obj);
        }

        bool Equals(SortColumnDescriptor other)
        {
            return other.name == name
                && other.comparer == comparer
                && other.sortDirection == sortDirection
                && other.Id == Id;
        }

        /// <summary>
        /// Initializes this object and copies properties from another object. <see cref="PropertyChanging"/>
        /// and <see cref="PropertyChanged"/> events are raised for every property that is modified. If both
        /// objects are equal, no events are raised.
        /// </summary>
        /// <param name="other">The source object.</param>
        public virtual void InitializeFrom(SortColumnDescriptor other)
        {
            this.Name = other.Name;
            this.Comparer = other.Comparer;
            this.SortDirection = other.SortDirection;
        }

        /// <override/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        private bool isUnbound = false;

        public bool IsUnbound
        {
            get { return isUnbound; }
            set
            {
                isUnbound = value;
            }
        }

        public virtual string Name
        {
            get
            {
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

        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        bool isSorting = false;

        public bool IsSorting
        {
            get
            {
                return isSorting;
            }
            set
            {
                isSorting = value;
            }

        }


        /// <summary>
        /// Gets / sets a custom comparer for sorting records in the table.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IComparer Comparer
        {
            get
            {
                return comparer;
            }
            set
            {
                if (comparer != value)
                {

                    comparer = value;
                }
            }
        }

        /// <summary>
        /// The sort direction.
        /// </summary>
        [Description("The sort direction.")]
        public ListSortDirection SortDirection
        {
            get
            {
                return sortDirection;
            }
            set
            {
                if (sortDirection != value)
                {
                    sortDirection = value;
                }
            }
        }
    }

    public enum SortComparisonMethod
    {
        Common,
        Natural
    }

    public class SortColumnDescriptorCollection : List<SortColumnDescriptor>
    {
        public TableInfo TableInfo = null;

        //    /// <summary>
        //    /// Gets / sets the element with the specified name.
        //    /// </summary>
        public SortColumnDescriptor this[string name]
        {
            get
            {
                int index = Find(name);
                if (index == -1)
                    return null;

                return this[index];
            }
            set
            {
                CheckType(value);

                int index = Find(name);
                if (index == -1)
                {
                    value.Name = name;
                    Add(value);
                }
                else
                {
                    this[index] = value;
                    value.Name = name;

                    //
                    //Modified by Mubasher New Grid
                    //
                    //value.SetCollection(this);
                }
            }
        }

        public bool Contains(string name)
        {
            return Find(name) != -1;
        }

        public SortColumnDescriptorCollection(TableInfo table)
        {
            this.TableInfo = table;
        }

        internal int Find(string name)
        {
            for (int n = 0; n < Count; n++)
                if (this[n].Name == name)
                    return n;
            return -1;
        }

        //    /// <summary>
        //    /// Ensure type correctness when a new element is added to the collection.
        //    /// </summary>
        //    /// <param name="obj">The newly added object.</param>
        protected virtual void CheckType(object obj)
        {
            if (obj != null && !(obj is SortColumnDescriptor))
                throw new ArgumentException("Wrong type");
        }

        public void AddRange(SortColumnDescriptorCollection columns)
        {
            foreach (SortColumnDescriptor cd in columns)
                this.InternalAdd(cd.Name, cd.SortDirection, -1);
        }


        public void Remove(string name)
        {
            this.InternalRemove(name);
        }

        //    /// <summary>
        //    /// Creates a SortColumnDescriptor with ListSortDirection.Ascending and adds it to the end of the collection.
        //    /// </summary>
        //    /// <param name="name">The field name.</param>
        //    /// <returns>The zero-based collection index at which the value has been added.</returns>
        public int Add(string name)
        {
            return InternalAdd(name, ListSortDirection.Ascending, -1);
        }

        //    /// <summary>
        //    /// Creates a SortColumnDescriptor and adds it to the end of the collection.
        //    /// </summary>
        //    /// <param name="name">The field name.</param>
        //    /// <param name="sortDirection">The sort direction.</param>
        //    /// <returns>The zero-based collection index at which the value has been added.</returns>
        public int Add(string name, ListSortDirection sortDirection)
        {
            return InternalAdd(name, sortDirection, -1);
        }

        //    /// <summary>
        //    /// Creates a SortColumnDescriptor and adds it to the end of the collection.
        //    /// </summary>
        //    /// <param name="name">The field name.</param>
        //    /// <param name="sortDirection">The sort direction.</param>
        //    /// <param name="sortPosition">The sort position - This is used for multi column watchlists.</param>
        //    /// <returns>The zero-based collection index at which the value has been added.</returns>
        public int Add(string name, ListSortDirection sortDirection, int sortPosition)
        {
            return InternalAdd(name, sortDirection, sortPosition);
        }

        //    /// <summary>
        //    /// Called to create a SortColumnDescriptor and add it to the end of the collection.
        //    /// </summary>
        //    /// <param name="name">The field name.</param>
        //    /// <param name="sortDirection">The sort direction.</param>
        //    /// <returns>The zero-based collection index at which the value has been added.</returns>
        protected virtual int InternalAdd(string name, ListSortDirection sortDirection, int sortPosition)
        {
            //this.Add(new SortColumnDescriptor(name, sortDirection, TableInfo));
            TableInfo.TableColumn column = TableInfo.GetVisibleColumnFromName(name);

            if (column == null)
            {
                column = TableInfo.GetColumnFromName(name);
            }

            if (column != null)
            {
                if (sortPosition < 0)
                    TableInfo.AddSortingDescriptor(name, sortDirection, column.IsUnbound, true);
                else
                    TableInfo.AddSortingDescriptor(name, sortPosition, sortDirection, column.IsUnbound, SortComparisonMethod.Common, true);
            }

            return 1;
        }

        protected virtual int InternalRemove(string name)
        {
            //this.Add(new SortColumnDescriptor(name, sortDirection, TableInfo));
            TableInfo.TableColumn column = TableInfo.GetVisibleColumnFromName(name);
            
            if (column != null)
            {
                this.Remove(name);
            }

            return 1;
        }
    }

    //
    //Commented 
    //NOTE : May need this later
    //
    //public class SortColumnDescriptorCollection : IDisposable, IList, ICloneable, ICustomTypeDescriptor
    //{
    //    ArrayList inner = new ArrayList();
    //    internal int version;
    //    internal bool insideCollectionEditor = false;
    //    IComparer groupedColumnsComparer;
    //    SortColumnDescriptorCollection copy;
    //    bool inInitializeFrom = false;
    //    bool raiseChangeEvents = true;
    //    bool modified = false;
    //    bool inReset = false;
    //    bool shouldPopulate = true;

    //    bool disableShouldPopulate = false;

    //    /// <summary>
    //    /// Gets or sets whether collection should check for changes
    //    /// in engine schema or underlying datasource schema when EnsureInitialized gets called.
    //    /// </summary>
    //    [System.Xml.Serialization.XmlIgnore]
    //    [Browsable(false)]
    //    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    //    public bool ShouldPopulate
    //    {
    //        get
    //        {
    //            return shouldPopulate;
    //        }
    //        set
    //        {
    //            shouldPopulate = value;
    //        }
    //    }

    //    /// <summary>
    //    /// When called the ShouldPopulate property will be set true temporarily until
    //    /// the next EnsureInitialized call and then be reset again to optimize subsequent lookups.
    //    /// The Engine calls this method when schema changes occured (PropertyChanged was raised).
    //    /// </summary>
    //    public void EnableOneTimePopulate()
    //    {
    //        this.disableShouldPopulate = true;
    //        this.shouldPopulate = true;
    //    }

    //    /// <summary>
    //    /// Resets the collection to its default state. If the collection is bound to a <see cref="TableDescriptor"/>,
    //    /// the collection will autopopulate itself the next time an item inside the collection is accessed.
    //    /// </summary>
    //    public void Reset()
    //    {
    //        if (this.modified || this.inner.Count > 0)
    //        {
    //            inReset = true;
    //            modified = false;
    //            version++;
    //            inner.Clear();
    //        }
    //        inReset = false;
    //        this.modified = false;
    //    }

    //    /// <summary>
    //    /// Resets the collection to its default state, autopopulates the collection, and marks the collection
    //    /// as modified. Call this method if you want to load the default items for the collection and then
    //    /// modify them (e.g. remove members from the auto-populated list).
    //    /// </summary>
    //    public void LoadDefault()
    //    {
    //        if (this.IsModified)
    //            Reset();
    //        this.modified = true;
    //    }

    //    /// <summary>
    //    /// Determines if the collection was modified from its default state.
    //    /// </summary>
    //    public bool IsModified
    //    {
    //        get
    //        {
    //            return modified;
    //        }
    //    }


    //    internal SortColumnDescriptorCollection GetShadowedCopy()
    //    {
    //        if (copy == null || copy.version != version)
    //        {
    //            copy = this.Clone();
    //            copy.version = version;
    //        }
    //        return copy;
    //    }

    //    /// <override/>
    //    public override string ToString()
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        sb.AppendFormat("{0}: Count {1}", GetType().Name, inner.Count);
    //        foreach (SortColumnDescriptor sd in inner)
    //            sb.AppendFormat(" {0} ", sd.Name);
    //        return sb.ToString();
    //    }

    //    /// <summary>
    //    /// Returns an array of SortColumnDescriptor objects where elements reference items in this collection. (Elements are not copied).
    //    /// </summary>
    //    /// <returns></returns>
    //    public SortColumnDescriptor[] ToArray()
    //    {
    //        SortColumnDescriptor[] columnDescriptors = new SortColumnDescriptor[Count];
    //        for (int n = 0; n < columnDescriptors.Length; n++)
    //            columnDescriptors[n] = (SortColumnDescriptor)inner[n];
    //        return columnDescriptors;
    //    }

    //    SortColumnDescriptor[] CloneToArray()
    //    {
    //        SortColumnDescriptor[] columnDescriptors = new SortColumnDescriptor[Count];
    //        for (int n = 0; n < columnDescriptors.Length; n++)
    //            columnDescriptors[n] = ((SortColumnDescriptor)inner[n]).Clone();
    //        return columnDescriptors;
    //    }

    //    static SortColumnDescriptor[] CloneArray(SortColumnDescriptor[] src)
    //    {
    //        SortColumnDescriptor[] columnDescriptors = new SortColumnDescriptor[src.Length];
    //        for (int n = 0; n < columnDescriptors.Length; n++)
    //            columnDescriptors[n] = src[n].Clone();
    //        return columnDescriptors;
    //    }


    //    /// <summary>
    //    /// Ensure type correctness when a new element is added to the collection.
    //    /// </summary>
    //    /// <param name="obj">The newly added object.</param>
    //    protected virtual void CheckType(object obj)
    //    {
    //        if (obj != null && !(obj is SortColumnDescriptor))
    //            throw new ArgumentException("Wrong type");
    //    }

    //    internal int fieldsVersion = -1;

    //    internal bool isPrimaryKeyColumns = false;
    //    internal bool isRelationChildColumns = false;
    //    int relationVersion = -1;

    //    bool inEnsureFieldDescriptors = false;

    //    /// <summary>
    //    /// Called internally to ensure all field descriptors are up to date after table descriptor is changed.
    //    /// </summary>
    //    /// <returns>true if field descriptors need to be reinitialized</returns>
    //    protected virtual bool CheckOutOfDate()
    //    {
    //        return false;
    //    }


    //    /// <overload>
    //    /// Copies settings from another collection and raises <see cref="SortColumnDescriptorCollection.Changing"/> and <see cref="SortColumnDescriptorCollection.Changed"/>
    //    /// events if differences to the other collections are detected.
    //    /// </overload>
    //    /// <summary>
    //    /// Copies settings from another collection and raises <see cref="Changing"/> and <see cref="Changed"/>
    //    /// events if differences to the other collection are detected.
    //    /// </summary>
    //    /// <param name="other">The source collection.</param>
    //    public void InitializeFrom(SortColumnDescriptorCollection other)
    //    {
    //        InitializeFrom(other, true);
    //    }

    //    /// <summary>
    //    /// Copies settings from another collection and raises <see cref="Changing"/> and <see cref="Changed"/>
    //    /// events if differences to the other collection are detected.
    //    /// </summary>
    //    /// <param name="other">The source collection.</param>
    //    /// <param name="raiseChangeEvents">Specifies if Changing and Changed events should be raised.</param>
    //    public bool InitializeFrom(SortColumnDescriptorCollection other, bool raiseChangeEvents)
    //    {
    //        bool savedshouldPopulateThis = shouldPopulate;
    //        bool savedshouldPopulateOther = other.shouldPopulate;
    //        this.shouldPopulate = false;
    //        other.shouldPopulate = false;

    //        try
    //        {
    //            int i;
    //            inInitializeFrom = true;
    //            this.raiseChangeEvents = raiseChangeEvents;
    //            int v = version;
    //            int count = Math.Min(Count, other.Count);
    //            for (i = 0; i < count; i++)
    //                this[i].InitializeFrom(other[i].Clone());

    //            for (; i < other.Count; i++)
    //            {
    //                Add(other[i].Clone());
    //            }

    //            while (Count > other.Count)
    //                RemoveAt(Count - 1);
    //            this.raiseChangeEvents = true;
    //            if (v != version)
    //            {
    //                this.fieldsVersion = -1;
    //                return true;
    //            }
    //            return false;
    //        }
    //        finally
    //        {
    //            inInitializeFrom = false;
    //            this.shouldPopulate = true;
    //            if (!savedshouldPopulateThis)
    //                this.disableShouldPopulate = true;
    //            other.shouldPopulate = savedshouldPopulateOther;
    //        }
    //    }

    //    private TableInfo tableInfo;

    //    /// <summary>
    //    /// A Read-only and empty collection.
    //    /// </summary>
    //    public static readonly SortColumnDescriptorCollection Empty = new SortColumnDescriptorCollection();

    //    /// <summary>
    //    /// Initializes a new empty collection and attaches it to a <see cref="TableDescriptor"/>
    //    /// </summary>
    //    public SortColumnDescriptorCollection()
    //    {
    //    }

    //    /// <summary>
    //    /// Initializes a new empty collection and attaches it to a <see cref="TableInfo"/>
    //    /// </summary>
    //    public SortColumnDescriptorCollection(TableInfo tableInfo)
    //    {
    //        this.tableInfo = tableInfo;
    //    }

    //    internal SortColumnDescriptorCollection(params SortColumnDescriptor[] columnDescriptors)
    //    {
    //        this.inner.AddRange(columnDescriptors);
    //        for (int n = 0; n < columnDescriptors.Length; n++)
    //        {
    //            CheckType(columnDescriptors[n]);
    //            columnDescriptors[n].SetCollection(this);
    //        }
    //    }

    //    /// <summary>
    //    /// Adds multiple elements at the end of the collection.
    //    /// </summary>
    //    /// <param name="columnDescriptors">The array whose elements should be added to the end of the collection.
    //    /// The array and its elements cannot be NULL references (Nothing in Visual Basic).
    //    /// </param>
    //    public void AddRange(SortColumnDescriptor[] columnDescriptors)
    //    {
    //        foreach (SortColumnDescriptor columnDescriptor in columnDescriptors)
    //            Add(columnDescriptor);
    //    }

    //    SortColumnDescriptorCollection(SortColumnDescriptor[] columnDescriptors, int version)
    //        : this(columnDescriptors)
    //    {
    //        this.version = version;
    //    }

    //    /// <summary>
    //    /// Creates a copy of the collection and all its elements.
    //    /// </summary>
    //    /// <returns>A copy of the collection and all its elements.</returns>
    //    public SortColumnDescriptorCollection Clone()
    //    {
    //        return InternalClone();
    //    }

    //    /// <summary>
    //    /// Copies all members to another collection.
    //    /// </summary>
    //    /// <param name="coll">The target collection.</param>
    //    protected void CopyAllMembersTo(SortColumnDescriptorCollection coll)
    //    {
    //        coll.fieldsVersion = -1;
    //        coll.relationVersion = -1;
    //        coll.inInitializeFrom = this.inInitializeFrom;
    //        coll.inner = this.inner;
    //        coll.insideCollectionEditor = this.insideCollectionEditor;
    //        coll.version = this.version + 1000;


    //        coll.inner = new ArrayList();
    //        int count = Count;
    //        SortColumnDescriptor[] columnDescriptors = new SortColumnDescriptor[count];
    //        for (int n = 0; n < count; n++)
    //        {
    //            coll.inner.Add(this[n].Clone());
    //            coll[n].SetCollection(coll);
    //        }
    //    }


    //    /// <summary>
    //    /// Creates a copy of this collection and all its inner elements. This method is called from Clone.
    //    /// </summary>
    //    /// <returns></returns>
    //    protected virtual SortColumnDescriptorCollection InternalClone()
    //    {
    //        //			TraceUtil.TraceCurrentMethodInfo(this);
    //        SortColumnDescriptorCollection coll = new SortColumnDescriptorCollection();
    //        CopyAllMembersTo(coll);
    //        return coll;
    //    }

    //    /// <override/>
    //    public override bool Equals(object obj)
    //    {
    //        //			TraceUtil.TraceCurrentMethodInfo(this);
    //        if (this == null && obj == null)
    //            return true;
    //        else if (this == null)
    //            return false;
    //        else if (obj is SortColumnDescriptor[])
    //            return Equals((SortColumnDescriptor[])obj);
    //        if (!(obj is SortColumnDescriptorCollection))
    //            return false;

    //        return Equals((SortColumnDescriptorCollection)obj);
    //    }

    //    /// <override/>
    //    public override int GetHashCode()
    //    {
    //        return base.GetHashCode();
    //    }

    //    /// <summary>
    //    /// Increases the version counter for this collection.
    //    /// </summary>
    //    public void BumpVersion()
    //    {
    //        version++;
    //    }

    //    /// <summary>
    //    /// The version number of this collection. The version is increased each time the
    //    /// collection or an element within the collection was modified.
    //    /// </summary>
    //    public int Version
    //    {
    //        get
    //        {
    //            return version;
    //        }
    //    }

    //    bool Equals(SortColumnDescriptor[] other)
    //    {
    //        int count = Count;
    //        if (other.Length != count)
    //            return false;
    //        for (int n = 0; n < count; n++)
    //        {
    //            if (!this[n].Equals(other[n]))
    //                return false;
    //        }
    //        return true;
    //    }

    //    internal static bool Equals(SortColumnDescriptor[] src, SortColumnDescriptor[] other)
    //    {
    //        if (other.Length != src.Length)
    //            return false;
    //        for (int n = 0; n < other.Length; n++)
    //        {
    //            if (!src[n].Equals(other[n]))
    //                return false;
    //        }
    //        return true;
    //    }

    //    bool Equals(SortColumnDescriptorCollection other)
    //    {
    //        int count = Count;
    //        if (other.Count != count)
    //            return false;
    //        for (int n = 0; n < count; n++)
    //        {
    //            if (!this[n].Equals(other[n]))
    //                return false;
    //        }
    //        return true;
    //    }

    //    /// <summary>
    //    /// Gets / sets the element at the zero-based index.
    //    /// </summary>
    //    public SortColumnDescriptor this[int index]
    //    {
    //        get
    //        {
    //            return (SortColumnDescriptor)inner[index];
    //        }
    //        set
    //        {
    //            CheckType(value);

    //            if (inner[index] != value)
    //            {
    //                inner[index] = value;
    //                value.SetCollection(this);
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Gets / sets the element with the specified name.
    //    /// </summary>
    //    public SortColumnDescriptor this[string name]
    //    {
    //        get
    //        {
    //            int index = Find(name);
    //            if (index == -1)
    //                return null;

    //            return (SortColumnDescriptor)inner[index];
    //        }
    //        set
    //        {
    //            CheckType(value);
    //            //				TraceUtil.TraceCurrentMethodInfo(this, name, value);
    //            int index = Find(name);
    //            if (index == -1)
    //            {
    //                value.Name = name;
    //                Add(value);
    //            }
    //            else
    //            {
    //                inner[index] = value;
    //                value.Name = name;
    //                value.SetCollection(this);
    //            }
    //        }
    //    }

    //    internal int Find(string name)
    //    {
    //        for (int n = 0; n < Count; n++)
    //            if (this[n].Name == name)
    //                return n;
    //        return -1;
    //    }

    //    internal void FixCollection()
    //    {
    //        for (int n = 0; n < Count; n++)
    //            this[n].SetCollection(this);
    //    }

    //    internal void CheckCollection()
    //    {
    //        for (int n = 0; n < Count; n++)
    //            if (this[n].Collection != this)
    //                throw new InvalidOperationException();
    //    }

    //    /// <summary>
    //    /// Determines if the element belongs to this collection.
    //    /// </summary>
    //    /// <param name="value">The element to locate in the collection. The value can be a NULL reference (Nothing in Visual Basic).</param>
    //    /// <returns>True if item is found in the collection; otherwise, False.</returns>
    //    public bool Contains(SortColumnDescriptor value)
    //    {
    //        if (value == null)
    //            return false;
    //        CheckType(value);

    //        return inner.Contains(value);
    //    }

    //    /// <summary>
    //    /// Determines if the element with the specified name belongs to this collection.
    //    /// </summary>
    //    /// <param name="name">The name of the element to locate in the collection.</param>
    //    /// <returns>True if item is found in the collection; otherwise, False.</returns>
    //    public bool Contains(string name)
    //    {
    //        return Find(name) != -1;
    //    }

    //    /// <summary>
    //    /// Returns the zero-based index of the occurrence of the element in the collection.
    //    /// </summary>
    //    /// <param name="value">The element to locate in the collection. The value can be a NULL reference (Nothing in Visual Basic). </param>
    //    /// <returns>The zero-based index of the occurrence of the element within the entire collection, if found; otherwise, -1.</returns>
    //    public int IndexOf(SortColumnDescriptor value)
    //    {
    //        CheckType(value);
    //        return inner.IndexOf(value);
    //    }

    //    /// <summary>
    //    /// Searches for the element with the specified name.
    //    /// </summary>
    //    /// <param name="name">The name of the element to locate in the collection. </param>
    //    /// <returns>The zero-based index of the occurrence of the element with matching name within the entire collection, if found; otherwise, -1.</returns>
    //    public int IndexOf(string name)
    //    {
    //        return Find(name);
    //    }

    //    /// <summary>
    //    /// Copies the entire collection to a compatible one-dimensional Array, starting at the specified index of the target array.
    //    /// </summary>
    //    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the ArrayList. The array must have zero-based indexing. </param>
    //    /// <param name="index">The zero-based index in an array at which copying begins. </param>
    //    public void CopyTo(SortColumnDescriptor[] array, int index)
    //    {
    //        //			TraceUtil.TraceCurrentMethodInfo(this);
    //        int count = Count;
    //        for (int n = 0; n < count; n++)
    //            array[index + n] = this[n];
    //    }

    //    SortColumnDescriptorCollection SyncRoot
    //    {
    //        get
    //        {
    //            throw new NotSupportedException();
    //        }
    //    }

    //    /// <summary>
    //    /// Inserts a descriptor element into the collection at the specified index.
    //    /// </summary>
    //    /// <param name="index">The zero-based index at which the element should be inserted.</param>
    //    /// <param name="value">The element to insert. The value must not be a NULL reference (Nothing in Visual Basic). </param>
    //    public void Insert(int index, SortColumnDescriptor value)
    //    {
    //        CheckType(value);
    //        inner.Insert(index, value);
    //        value.SetCollection(this);
    //    }

    //    /// <summary>
    //    /// Removes the specified descriptor element from the collection.
    //    /// </summary>
    //    /// <param name="value">The element to remove from the collection. If the value is NULL or the element is not contained
    //    /// in the collection, the method will do nothing.</param>
    //    public void Remove(SortColumnDescriptor value)
    //    {
    //        CheckType(value);
    //        //			TraceUtil.TraceCurrentMethodInfo(this, value.Name);
    //        int index = IndexOf(value);
    //        inner.Remove(value);
    //    }

    //    /// <summary>
    //    /// Adds a SortColumnDescriptor to the end of the collection.
    //    /// </summary>
    //    /// <param name="value">The element to be added to the end of the collection. The value must not be a NULL reference (Nothing in Visual Basic). </param>
    //    /// <returns>The zero-based collection index at which the value has been added.</returns>
    //    public int Add(SortColumnDescriptor value)
    //    {
    //        CheckType(value);

    //        if (!this.modified && !this.inEnsureFieldDescriptors)
    //            inner.Clear();
    //        if (value.Name != null && value.Name.Length > 0)
    //        {
    //            if (Contains(value.Name))
    //                throw new Exception(String.Format("Column '{0}': Duplicates are not allowed ", value.Name));
    //        }

    //        int index = inner.Add(value);
    //        value.SetCollection(this);

    //        return index;
    //    }

    //    /// <summary>
    //    /// Creates a SortColumnDescriptor with ListSortDirection.Ascending and adds it to the end of the collection.
    //    /// </summary>
    //    /// <param name="name">The field name.</param>
    //    /// <returns>The zero-based collection index at which the value has been added.</returns>
    //    public int Add(string name)
    //    {
    //        return InternalAdd(name, ListSortDirection.Ascending);
    //    }

    //    /// <summary>
    //    /// Creates a SortColumnDescriptor and adds it to the end of the collection.
    //    /// </summary>
    //    /// <param name="name">The field name.</param>
    //    /// <param name="sortDirection">The sort direction.</param>
    //    /// <returns>The zero-based collection index at which the value has been added.</returns>
    //    public int Add(string name, ListSortDirection sortDirection)
    //    {
    //        return InternalAdd(name, sortDirection);
    //    }

    //    /// <summary>
    //    /// Called to create a SortColumnDescriptor and add it to the end of the collection.
    //    /// </summary>
    //    /// <param name="name">The field name.</param>
    //    /// <param name="sortDirection">The sort direction.</param>
    //    /// <returns>The zero-based collection index at which the value has been added.</returns>
    //    protected virtual int InternalAdd(string name, ListSortDirection sortDirection)
    //    {
    //        return Add(new SortColumnDescriptor(name, sortDirection));
    //    }

    //    /// <summary>
    //    /// Removes the specified descriptor element with the specified name from the collection.
    //    /// </summary>
    //    /// <param name="name">The name of the element to remove from the collection. If no element with that name is found
    //    /// in the collection, the method will do nothing.</param>
    //    public void Remove(string name)
    //    {
    //        int index = Find(name);
    //        if (index != -1)
    //            RemoveAt(index);
    //    }

    //    /// <summary>
    //    /// Removes the element at the specified index of the collection.
    //    /// </summary>
    //    /// <param name="index">The zero-based index of the element to remove. </param>
    //    public void RemoveAt(int index)
    //    {
    //        //			TraceUtil.TraceCurrentMethodInfo(this, index);
    //        object value = inner[index];
    //        inner.RemoveAt(index);

    //    }

    //    /// <summary>
    //    /// Disposes the object and collection items.
    //    /// </summary>
    //    public void Dispose()
    //    {

    //        inner.Clear();
    //        GC.SuppressFinalize(this);
    //    }

    //    /// <summary>
    //    /// Removes all elements from the collection.
    //    /// </summary>
    //    public void Clear()
    //    {
    //        if (inner.Count > 0)
    //        {
    //            inner.Clear();
    //        }
    //        this.modified = true;
    //    }

    //    /// <summary>
    //    /// Returns False.
    //    /// </summary>
    //    public bool IsReadOnly
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    /// <summary>
    //    /// Returns False since this collection has no fixed size.
    //    /// </summary>
    //    public bool IsFixedSize
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    /// <summary>
    //    /// Returns False.
    //    /// </summary>
    //    public bool IsSynchronized
    //    {
    //        get
    //        {
    //            return false;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets the number of elements contained in the collection. The property also
    //    /// ensures that the collection is in sync with the underlying
    //    /// table if changes have been made to the table or the TableDescriptor.
    //    /// </summary>
    //    /// <remarks>
    //    /// If changes in the TableDescriptor are detected, the
    //    /// method will reinitialize the field descriptors before returning the count.
    //    /// </remarks>
    //    public int Count
    //    {
    //        get
    //        {
    //            return inner.Count;
    //        }
    //    }


    //    #region ICloneable Private Members
    //    object ICloneable.Clone()
    //    {
    //        return Clone();
    //    }
    //    #endregion

    //    #region IList Private Members

    //    object IList.this[int index]
    //    {
    //        get
    //        {
    //            return this[index];
    //        }
    //        set
    //        {
    //            this[index] = (SortColumnDescriptor)value;
    //        }
    //    }

    //    void IList.Insert(int index, object value)
    //    {
    //        Insert(index, (SortColumnDescriptor)value);
    //    }

    //    void IList.Remove(object value)
    //    {
    //        Remove((SortColumnDescriptor)value);
    //    }

    //    bool IList.Contains(object value)
    //    {
    //        return Contains((SortColumnDescriptor)value);
    //    }

    //    int IList.IndexOf(object value)
    //    {
    //        return IndexOf((SortColumnDescriptor)value);
    //    }

    //    int IList.Add(object value)
    //    {
    //        return Add((SortColumnDescriptor)value);
    //    }

    //    #endregion

    //    #region ICollection Private Members

    //    void ICollection.CopyTo(Array array, int index)
    //    {
    //        CopyTo((SortColumnDescriptor[])array, index);
    //    }

    //    object ICollection.SyncRoot
    //    {
    //        get
    //        {
    //            return null;
    //        }
    //    }

    //    #endregion

    //    #region IEnumerable Private Members

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return null;
    //    }

    //    #endregion

    //    #region ICustomTypeDescriptor
    //    AttributeCollection ICustomTypeDescriptor.GetAttributes()
    //    {
    //        return TypeDescriptor.GetAttributes(this, true);
    //    }

    //    string ICustomTypeDescriptor.GetClassName()
    //    {
    //        return TypeDescriptor.GetClassName(this, true);
    //    }

    //    string ICustomTypeDescriptor.GetComponentName()
    //    {
    //        return TypeDescriptor.GetComponentName(this, true);
    //    }

    //    TypeConverter ICustomTypeDescriptor.GetConverter()
    //    {
    //        return TypeDescriptor.GetConverter(this, true);
    //    }

    //    EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
    //    {
    //        return TypeDescriptor.GetDefaultEvent(this, true);
    //    }

    //    PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
    //    {
    //        return TypeDescriptor.GetDefaultProperty(this, true);
    //    }

    //    object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
    //    {
    //        return TypeDescriptor.GetEditor(this, editorBaseType, true);
    //    }

    //    EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
    //    {
    //        return TypeDescriptor.GetEvents(this, true);
    //    }

    //    EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
    //    {
    //        return TypeDescriptor.GetEvents(this, attributes, true);
    //    }

    //    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
    //    {
    //        return ((ICustomTypeDescriptor)this).GetProperties(null);
    //    }

    //    PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
    //    {
    //        ArrayList pds = new ArrayList();
    //        Attribute[] att = new Attribute[] {
    //                                              new BrowsableAttribute(true),
    //                                              new System.Xml.Serialization.XmlIgnoreAttribute(),
    //                                              new RefreshPropertiesAttribute(RefreshProperties.All),
    //                                              new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden),
    //                                              new CategoryAttribute("Items")
    //                                          };

    //        ArrayList names = new ArrayList();


    //        PropertyDescriptorCollection pdc = new PropertyDescriptorCollection((PropertyDescriptor[])pds.ToArray(typeof(PropertyDescriptor)));
    //        return pdc.Sort((string[])names.ToArray(typeof(string)));
    //    }

    //    object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
    //    {
    //        return this;
    //    }

    //    #endregion
    //}

}
