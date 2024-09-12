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

        protected Database assignedDatabase;

        /// <summary>
        /// Name of this definition
        /// </summary>
        public string Name { get => name; }

        internal Database AssignedDatabase => assignedDatabase;

        /// <summary>
        /// </summary>
        protected void SaveState()
        {
            original = this;
        }
    }
}
