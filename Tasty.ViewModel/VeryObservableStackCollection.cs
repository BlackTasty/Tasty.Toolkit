using System;
using System.Collections.Generic;
using System.Linq;

namespace Tasty.ViewModel
{
    /// <summary>
    /// Initialize collections with a limit.
    /// <inheritdoc/>
    /// </summary>
    /// <typeparam name="T"><inheritdoc/></typeparam>
    public class VeryObservableStackCollection<T> : VeryObservableCollection<T>
    {
        private int limit = 1;

        /// <summary>
        /// The maximum amount of items in this collection.
        /// </summary>
        public int Limit => limit;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="collectionName"><inheritdoc/></param>
        /// <param name="limit">The maximum allowed amount of items in this list. If amount is exceeded the last item is removed.</param>
        public VeryObservableStackCollection(string collectionName, int limit, bool generateObservers = false, Enum message = null) : base(collectionName, generateObservers, message)
        {
            this.limit = limit;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="items"><inheritdoc/></param>
        public override void AddRange(IEnumerable<T> items)
        {
            base.AddRange(items.Take(limit));
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="items"><inheritdoc/></param>
        /// <param name="sort"><inheritdoc/></param>
        public override void AddRange(List<T> items)
        {
            base.AddRange(items.Take(limit).ToList());
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="item"><inheritdoc/></param>
        public override void Add(T item)
        {
            if (Items.Count >= limit)
            {
                Items.RemoveAt(Items.Count - 1);
                List<T> itemsCopy = this.Reverse().ToList();
                itemsCopy.Add(item);
                Clear();
                AddRange(itemsCopy);
            }
            else
            {
                base.Add(item);
            }
        }
    }
}