using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasty.SQLiteManager.Table;

namespace Tasty.SQLiteManager.Events
{
    public class DatabaseEntryLoadedEventArgs<T> : EventArgs
    {
        private T loadedEntry;
        private int progress;
        private int max;

        public T LoadedEntry => loadedEntry;

        public int Progress => progress;

        public int Max => max;

        internal DatabaseEntryLoadedEventArgs(T loadedEntry, int progress, int max)
        {
            this.loadedEntry = loadedEntry;
            this.progress = progress;
            this.max = max;
        }
    }
}
