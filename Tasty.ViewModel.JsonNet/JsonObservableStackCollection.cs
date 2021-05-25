using Newtonsoft.Json;
using System;
using Tasty.ViewModel.Observer;

namespace Tasty.ViewModel.JsonNet
{
    /// <summary>
    /// Overrides some properties to include the <see cref="JsonIgnoreAttribute"/>.
    /// <para></para>
    /// <inheritdoc/>
    /// </summary>
    /// <typeparam name="T"><inheritdoc/></typeparam>
    class JsonObservableStackCollection<T> : VeryObservableStackCollection<T>
    {
        /// <inheritdoc/>
        [JsonIgnore]
        public override string Checksum => base.Checksum;

        [JsonIgnore]
        /// <inheritdoc/>
        public override int Count => base.Count;

        [JsonIgnore]
        /// <inheritdoc/>
        public override ObserverManager ObserverManager => base.ObserverManager;

        [JsonIgnore]
        /// <inheritdoc/>
        public override bool UnsavedChanged => base.UnsavedChanged;

        /// <inheritdoc/>
        public JsonObservableStackCollection(string collectionName, int limit, bool generateObservers = false, Enum message = null) : base(collectionName, limit, generateObservers, message)
        {

        }
    }
}
