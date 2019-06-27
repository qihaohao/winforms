// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;

namespace System.Windows.Forms
{
    public partial class ListView
    {
        /// <summary>
        ///    <para>Represents the collection of items in a ListView or ListViewGroup</para>
        /// </summary>
        [ListBindable(false)]
        public partial class ListViewItemCollection : IList
        {

            /// A caching mechanism for key accessor
            /// We use an index here rather than control so that we don't have lifetime
            /// issues by holding on to extra references.
            private int lastAccessedIndex = -1;
           
            private readonly IInnerList innerList;

            public ListViewItemCollection(ListView owner)
            {
                // Kept for APPCOMPAT reasons.
                // In Whidbey this constructor is a no-op.

                // initialize the inner list w/ a dummy list.
                innerList = new ListViewNativeItemCollection(owner);
            }

            internal ListViewItemCollection(IInnerList innerList)
            {
                Debug.Assert(innerList != null, "Can't pass in null innerList");
                this.innerList = innerList;
            }

            private IInnerList InnerList
            {
                get
                {
                    return innerList;
                }
            }

            /// <summary>
            ///     Returns the total number of items within the list view.
            /// </summary>
            [Browsable(false)]
            public int Count
            {
                get
                {
                    return InnerList.Count;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return this;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return true;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            /// <summary>
            ///     Returns the ListViewItem at the given index.
            /// </summary>
            public virtual ListViewItem this[int index]
            {
                get
                {
                    if (index < 0 || index >= InnerList.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
                    }

                    return InnerList[index];
                }
                set
                {
                    if (index < 0 || index >= InnerList.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
                    }

                    InnerList[index] = value;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    if (value is ListViewItem)
                    {
                        this[index] = (ListViewItem)value;
                    }
                    else if (value != null)
                    {
                        this[index] = new ListViewItem(value.ToString(), -1);
                    }
                }
            }

            /// <summary>
            ///     <para>Retrieves the child control with the specified key.</para>
            /// </summary>
            public virtual ListViewItem this[string key]
            {
                get
                {
                    // We do not support null and empty string as valid keys.
                    if (string.IsNullOrEmpty(key))
                    {
                        return null;
                    }

                    // Search for the key in our collection
                    int index = IndexOfKey(key);
                    if (IsValidIndex(index))
                    {
                        return this[index];
                    }
                    else
                    {
                        return null;
                    }

                }
            }

            /// <summary>
            ///     Add an item to the ListView.  The item will be inserted either in
            ///     the correct sorted position, or, if no sorting is set, at the end
            ///     of the list.
            /// </summary>
            public virtual ListViewItem Add(string text)
            {
                return Add(text, -1);
            }

            int IList.Add(object item)
            {
                if (item is ListViewItem)
                {
                    return IndexOf(Add((ListViewItem)item));
                }
                else if (item != null)
                {
                    return IndexOf(Add(item.ToString()));
                }
                return -1;
            }

            /// <summary>
            ///     Add an item to the ListView.  The item will be inserted either in
            ///     the correct sorted position, or, if no sorting is set, at the end
            ///     of the list.
            /// </summary>
            public virtual ListViewItem Add(string text, int imageIndex)
            {
                ListViewItem li = new ListViewItem(text, imageIndex);
                Add(li);
                return li;
            }

            /// <summary>
            ///     Add an item to the ListView.  The item will be inserted either in
            ///     the correct sorted position, or, if no sorting is set, at the end
            ///     of the list.
            /// </summary>
            public virtual ListViewItem Add(ListViewItem value)
            {
                InnerList.Add(value);
                return value;
            }

            // <-- NEW ADD OVERLOADS IN WHIDBEY

            /// <summary>
            ///     Add an item to the ListView.  The item will be inserted either in
            ///     the correct sorted position, or, if no sorting is set, at the end
            ///     of the list.
            /// </summary>
            public virtual ListViewItem Add(string text, string imageKey)
            {
                ListViewItem li = new ListViewItem(text, imageKey);
                Add(li);
                return li;
            }

            /// <summary>
            ///     Add an item to the ListView.  The item will be inserted either in
            ///     the correct sorted position, or, if no sorting is set, at the end
            ///     of the list.
            /// </summary>
            public virtual ListViewItem Add(string key, string text, string imageKey)
            {
                ListViewItem li = new ListViewItem(text, imageKey)
                {
                    Name = key
                };
                Add(li);
                return li;
            }

            /// <summary>
            ///     Add an item to the ListView.  The item will be inserted either in
            ///     the correct sorted position, or, if no sorting is set, at the end
            ///     of the list.
            /// </summary>
            public virtual ListViewItem Add(string key, string text, int imageIndex)
            {
                ListViewItem li = new ListViewItem(text, imageIndex)
                {
                    Name = key
                };
                Add(li);
                return li;
            }


            // END - NEW ADD OVERLOADS IN WHIDBEY  -->

            public void AddRange(ListViewItem[] items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException(nameof(items));
                }

                InnerList.AddRange(items);
            }

            public void AddRange(ListViewItemCollection items)
            {
                if (items == null)
                {
                    throw new ArgumentNullException(nameof(items));
                }

                ListViewItem[] itemArray = new ListViewItem[items.Count];
                items.CopyTo(itemArray, 0);
                InnerList.AddRange(itemArray);
            }

            /// <summary>
            ///     Removes all items from the list view.
            /// </summary>
            public virtual void Clear()
            {
                InnerList.Clear();
            }

            public bool Contains(ListViewItem item)
            {
                return InnerList.Contains(item);
            }

            bool IList.Contains(object item)
            {
                if (item is ListViewItem)
                {
                    return Contains((ListViewItem)item);
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            ///     <para>Returns true if the collection contains an item with the specified key, false otherwise.</para>
            /// </summary>
            public virtual bool ContainsKey(string key)
            {
                return IsValidIndex(IndexOfKey(key));
            }

            public void CopyTo(Array dest, int index)
            {
                InnerList.CopyTo(dest, index);
            }

            /// <summary>
            ///     <para>Searches for Controls by their Name property, builds up an array
            ///           of all the controls that match.
            ///     </para>
            /// </summary>
            public ListViewItem[] Find(string key, bool searchAllSubItems)
            {
                ArrayList foundItems = FindInternal(key, searchAllSubItems, this, new ArrayList());

                ListViewItem[] stronglyTypedFoundItems = new ListViewItem[foundItems.Count];
                foundItems.CopyTo(stronglyTypedFoundItems, 0);

                return stronglyTypedFoundItems;
            }

            /// <summary>
            ///     <para>Searches for Controls by their Name property, builds up an arraylist
            ///           of all the controls that match.
            ///     </para>
            /// </summary>
            private ArrayList FindInternal(string key, bool searchAllSubItems, ListViewItemCollection listViewItems, ArrayList foundItems)
            {
                if ((listViewItems == null) || (foundItems == null))
                {
                    return null;  // 
                }

                for (int i = 0; i < listViewItems.Count; i++)
                {

                    if (WindowsFormsUtils.SafeCompareStrings(listViewItems[i].Name, key, /* ignoreCase = */ true))
                    {
                        foundItems.Add(listViewItems[i]);
                    }
                    else
                    {
                        if (searchAllSubItems)
                        {
                            // start from 1, as we've already compared subitems[0]
                            for (int j = 1; j < listViewItems[i].SubItems.Count; j++)
                            {
                                if (WindowsFormsUtils.SafeCompareStrings(listViewItems[i].SubItems[j].Name, key, /* ignoreCase = */ true))
                                {
                                    foundItems.Add(listViewItems[i]);
                                    break;
                                }
                            }
                        }
                    }
                }

                return foundItems;
            }

            public IEnumerator GetEnumerator()
            {
                if (InnerList.OwnerIsVirtualListView && !InnerList.OwnerIsDesignMode)
                {
                    // Throw the exception only at runtime.
                    throw new InvalidOperationException(SR.ListViewCantGetEnumeratorInVirtualMode);
                }
                return InnerList.GetEnumerator();
            }

            public int IndexOf(ListViewItem item)
            {
                for (int index = 0; index < Count; ++index)
                {
                    if (this[index] == item)
                    {
                        return index;
                    }
                }
                return -1;
            }

            int IList.IndexOf(object item)
            {
                if (item is ListViewItem)
                {
                    return IndexOf((ListViewItem)item);
                }
                else
                {
                    return -1;
                }
            }
            /// <summary>
            ///     <para>The zero-based index of the first occurrence of value within the entire CollectionBase, if found; otherwise, -1.</para>
            /// </summary>
            public virtual int IndexOfKey(string key)
            {
                // Step 0 - Arg validation
                if (string.IsNullOrEmpty(key))
                {
                    return -1; // we dont support empty or null keys.
                }

                // step 1 - check the last cached item
                if (IsValidIndex(lastAccessedIndex))
                {
                    if (WindowsFormsUtils.SafeCompareStrings(this[lastAccessedIndex].Name, key, /* ignoreCase = */ true))
                    {
                        return lastAccessedIndex;
                    }
                }

                // step 2 - search for the item
                for (int i = 0; i < Count; i++)
                {
                    if (WindowsFormsUtils.SafeCompareStrings(this[i].Name, key, /* ignoreCase = */ true))
                    {
                        lastAccessedIndex = i;
                        return i;
                    }
                }

                // step 3 - we didn't find it.  Invalidate the last accessed index and return -1.
                lastAccessedIndex = -1;
                return -1;
            }

            /// <summary>
            ///     <para>Determines if the index is valid for the collection.</para>
            /// </summary>
            private bool IsValidIndex(int index)
            {
                return ((index >= 0) && (index < Count));
            }

            public ListViewItem Insert(int index, ListViewItem item)
            {
                if (index < 0 || index > Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
                }
                InnerList.Insert(index, item);
                return item;
            }

            public ListViewItem Insert(int index, string text)
            {
                return Insert(index, new ListViewItem(text));
            }

            public ListViewItem Insert(int index, string text, int imageIndex)
            {
                return Insert(index, new ListViewItem(text, imageIndex));
            }

            void IList.Insert(int index, object item)
            {
                if (item is ListViewItem)
                {
                    Insert(index, (ListViewItem)item);
                }
                else if (item != null)
                {
                    Insert(index, item.ToString());
                }
            }

            // <-- NEW INSERT OVERLOADS IN WHIDBEY

            public ListViewItem Insert(int index, string text, string imageKey)
            {
                return Insert(index, new ListViewItem(text, imageKey));
            }

            public virtual ListViewItem Insert(int index, string key, string text, string imageKey)
            {
                ListViewItem li = new ListViewItem(text, imageKey)
                {
                    Name = key
                };
                return Insert(index, li);
            }

            public virtual ListViewItem Insert(int index, string key, string text, int imageIndex)
            {
                ListViewItem li = new ListViewItem(text, imageIndex)
                {
                    Name = key
                };
                return Insert(index, li);
            }

            // END - NEW INSERT OVERLOADS IN WHIDBEY -->

            /// <summary>
            ///     Removes an item from the ListView
            /// </summary>
            public virtual void Remove(ListViewItem item)
            {
                InnerList.Remove(item);
            }

            /// <summary>
            ///     Removes an item from the ListView
            /// </summary>
            public virtual void RemoveAt(int index)
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, string.Format(SR.InvalidArgument, nameof(index), index));
                }

                InnerList.RemoveAt(index);
            }

            /// <summary>
            ///     <para>Removes the child control with the specified key.</para>
            /// </summary>
            public virtual void RemoveByKey(string key)
            {
                int index = IndexOfKey(key);
                if (IsValidIndex(index))
                {
                    RemoveAt(index);
                }
            }

            void IList.Remove(object item)
            {
                if (item == null || !(item is ListViewItem))
                {
                    return;
                }

                Remove((ListViewItem)item);
            }
        }
    }
}