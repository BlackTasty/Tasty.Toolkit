namespace Tasty.Logging
{
    public static class ExtensionMethods
    {
        public static string ToPrefix(this LogType logType)
        {
            switch (logType)
            {
                case LogType.INFO:
                    return LoggerSettings.IDENTIFIER_INFO;

                case LogType.WARNING:
                    return LoggerSettings.IDENTIFIER_WARN;

                case LogType.ERROR:
                    return LoggerSettings.IDENTIFIER_ERROR;

                case LogType.DEBUG:
                    return LoggerSettings.IDENTIFIER_DEBUG;

                case LogType.VERBOSE:
                    return LoggerSettings.IDENTIFIER_VERBOSE;

                case LogType.FATAL:
                    return LoggerSettings.IDENTIFIER_FATAL;

                case LogType.CONSOLE:
                    return LoggerSettings.IDENTIFIER_CONSOLE;

                default:
                    return "";
            }
        }
    }
}
