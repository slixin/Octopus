using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace OctopusLib
{
    [Serializable]
    public class ObservableCollectionEx<T> : ObservableCollection<T> where T : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangedEventHandler ItemPropertyChanged;
        public event PropertyChangingEventHandler ItemPropertyChanging;

        /// <summary>
        /// Constructors
        /// </summary>
        public ObservableCollectionEx() : base() { CollectionChanged += new NotifyCollectionChangedEventHandler(ObservableCollectionEx_CollectionChanged); }
        public ObservableCollectionEx(List<T> l) : base(l) { CollectionChanged += new NotifyCollectionChangedEventHandler(ObservableCollectionEx_CollectionChanged); }
        public ObservableCollectionEx(IEnumerable<T> l) : base(l) { CollectionChanged += new NotifyCollectionChangedEventHandler(ObservableCollectionEx_CollectionChanged); }

        #region Sorting

        /// <summary>
        /// Sorts the items of the collection in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector">A function to extract a key from an item.</param>
        public void Sort<TKey>(Func<T, TKey> keySelector)
        {
            InternalSort(Items.OrderBy(keySelector));
        }

        /// <summary>
        /// Sorts the items of the collection in descending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector">A function to extract a key from an item.</param>
        public void SortDescending<TKey>(Func<T, TKey> keySelector)
        {
            InternalSort(Items.OrderByDescending(keySelector));
        }

        /// <summary>
        /// Sorts the items of the collection in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector">A function to extract a key from an item.</param>
        /// <param name="comparer">An <see cref="IComparer{T}"/> to compare keys.</param>
        public void Sort<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            InternalSort(Items.OrderBy(keySelector, comparer));
        }

        /// <summary>
        /// Moves the items of the collection so that their orders are the same as those of the items provided.
        /// </summary>
        /// <param name="sortedItems">An <see cref="IEnumerable{T}"/> to provide item orders.</param>
        private void InternalSort(IEnumerable<T> sortedItems)
        {
            var sortedItemsList = sortedItems.ToList();

            foreach (var item in sortedItemsList)
            {
                Move(IndexOf(item), sortedItemsList.IndexOf(item));
            }
        }

        #endregion // Sorting

        void ObservableCollectionEx_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {           
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (T item in e.OldItems)
                {
                    //Removed items
                    item.PropertyChanged -= new PropertyChangedEventHandler(item_PropertyChanged);
                    item.PropertyChanging -= new PropertyChangingEventHandler(item_PropertyChanging);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (T item in e.NewItems)
                {
                    //Added items
                    item.PropertyChanged += new PropertyChangedEventHandler(item_PropertyChanged);
                    item.PropertyChanging += new PropertyChangingEventHandler(item_PropertyChanging);
                }
            }
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs a = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(a);

            if (ItemPropertyChanged != null)
            {
                ItemPropertyChanged(sender, e);
            }
        }

        void item_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            NotifyCollectionChangedEventArgs a = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(a);

            if (ItemPropertyChanging != null)
            {
                ItemPropertyChanging(sender, e);
            }
        }
    }
}
