using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.ViewModel.Observer;

namespace Tasty.ViewModel.JsonNet.Observer
{
    /// <summary>
    /// Overrides some properties to include the <see cref="JsonIgnoreAttribute"/>.
    /// <para></para>
    /// <inheritdoc/>
    /// </summary>
    class JsonObservableClass : ObservableClass
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [JsonIgnore]
        public new ObserverManager ObserverManager => base.ObserverManager;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [JsonIgnore]
        public new bool UnsavedChanges => base.UnsavedChanges;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public JsonObservableClass() : base()
        {

        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public JsonObservableClass(string json) : base(json)
        {

        }
    }
}
