namespace Tasty.Logging
{
    /// <summary>
    /// Interface for outputting logged lines
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// Should verbose and debug lines be outputted?
        /// </summary>
        /// <returns>True for verbose logging, false to ignore verbose and debug lines.</returns>
        bool VerboseLogging();

        /// <summary>
        /// Executed whenever a new line has been logged where this object has been attached.
        /// </summary>
        /// <param name="str">Logged text</param>
        /// <param name="type">The type of the logged text</param>
        void WriteString(string str, LogType type);
    }
}
