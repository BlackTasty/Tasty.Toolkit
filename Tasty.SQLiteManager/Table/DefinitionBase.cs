using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.SQLiteManager.Table
{
    /// <summary>
    /// Base class for all definitions
    /// </summary>
    public class DefinitionBase
    {
        /// <summary>
        /// </summary>
        protected DefinitionBase original;

        /// <summary>
        /// </summary>
        protected string name;

        /// <summary>
        /// Name of this definition
        /// </summary>
        public string Name { get => name; }

        /// <summary>
        /// 
        /// </summary>
        protected void SaveState()
        {
            original = this;
        }
    }
}
