using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel.Observer;

namespace Tasty.ViewModel.JsonNet
{
    /// <summary>
    /// Overrides some properties to include the [JSONIgnore] attribute.
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

        [JsonIgnore]
        /// <inheritdoc/>
        public override bool AnyUnsavedChanges => base.AnyUnsavedChanges;

        /// <inheritdoc/>
        public JsonObservableStackCollection(string collectionName, int limit, bool generateObservers = false, Enum message = null) : base(collectionName, limit, generateObservers, message)
        {

        }
    }
}
