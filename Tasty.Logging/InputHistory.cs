using System.Collections.Generic;

namespace Tasty.Logging
{
    /// <summary>
    /// Not implemented
    /// </summary>
    internal class InputHistory
    {
        private List<string> history;
        private int capacity;
        private int index = -1;

        /// <summary>
        /// Creates a new instance of <see cref="InputHistory"/>.
        /// </summary>
        /// <param name="capacity">The maximum amount of saved commands.</param>
        public InputHistory(int capacity)
        {
            this.capacity = capacity;
            history = new List<string>(capacity);
        }

        /// <summary>
        /// Add a new command to the history. If the amount of commands exceeds the set capacity, the oldest command will be removed.
        /// </summary>
        /// <param name="command">The command to store.</param>
        public void Push(string command)
        {
            history.Insert(0, command);
            index = -1;

            if (history.Count > capacity)
                history.RemoveAt(capacity - 1);
        }

        /// <summary>
        /// Returns a command at the current index. The position of the index raises or lowers depending if you "peek up or down".
        /// </summary>
        /// <param name="isUp">Passing true will return an older command, false returns a newer command (depending on the current index)</param>
        /// <returns>A stored command.</returns>
        public string Peek(bool isUp)
        {
            if (isUp)
            {
                if (index < history.Count - 1)
                    index++;


                if (index > -1)
                    return history[index];
                else
                    return "";
            }
            else
            {
                if (index > 0)
                {
                    index--;
                    return history[index];
                }
                else
                {
                    index = -1;
                    return "";
                }
            }
        }
    }
}
