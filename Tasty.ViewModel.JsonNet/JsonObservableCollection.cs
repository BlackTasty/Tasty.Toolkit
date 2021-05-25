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
    public class JsonObservableCollection<T> : VeryObservableCollection<T>
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
        public JsonObservableCollection(string collectionName, bool generateObservers = false, Enum message = null) : base(collectionName, generateObservers, message)
        {

        }
    }
}
