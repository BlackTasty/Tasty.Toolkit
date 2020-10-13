namespace Tasty.Logging
{
    public enum LogType
    {
        /// <summary>
        /// Default type for logging. Useful for almost every log entry. (Prefix: [INF])
        /// </summary>
        INFO,
        /// <summary>
        /// Default type for error logging. Mostly used for errors which don't introduce instabilities inside the application. (Prefix: [WRN])
        /// </summary>
        WARNING,
        /// <summary>
        /// Useful for errors which hinder the execution of more important functions without introducing instabilities. (Prefix: [ERR])
        /// </summary>
        ERROR,
        /// <summary>
        /// For errors which introduce instabilities or errros from which the application cannot recover without a restart. (Prefix: [FAT])
        /// </summary>
        FATAL,
        /// <summary>
        /// Use this flag if you only want to log to an attached console. (Prefix: [CON])
        /// </summary>
        CONSOLE,
        /// <summary>
        /// This flag only works if the Build mode is set to DEBUG! (Prefix: [DBG])
        /// </summary>
        DEBUG,
        /// <summary>
        /// Similar to the <see cref="LogType.DEBUG"/> flag. Logging only happens if an <see cref="IConsole"/> is attached an "VerboseLogging()" returns true. (Prefix: [VRB])
        /// </summary>
        VERBOSE,
        /// <summary>
        /// This flag won't add a prefix to the beginning of the log line.
        /// </summary>
        NULL
    }
}
