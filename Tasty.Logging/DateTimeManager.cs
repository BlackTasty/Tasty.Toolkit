using System;

namespace Tasty.Logging
{
    public static class DateTimeManager
    {
        /// <summary>
        /// Get the current date formatted with the format set in <see cref="LoggerSettings"/>.
        /// </summary>
        /// <returns>A string representation of the current date</returns>
        public static string GetDate()
        {
            return string.Format("{0:" + LoggerSettings.TIMESTAMP_FORMAT_DATE + "}", DateTime.Now);
        }

        /// <summary>
        /// Get the current time formatted with the format set in <see cref="LoggerSettings"/>.
        /// </summary>
        /// <returns>A string representation of the current time</returns>
        public static string GetTime()
        {
            return string.Format("{0:" + LoggerSettings.TIMESTAMP_FORMAT_TIME + "}", DateTime.Now);
        }

        /// <summary>
        /// Get the current date + time formatted with the format set in <see cref="LoggerSettings"/>.
        /// </summary>
        /// <returns>A string representation of the current date and time, separated</returns>
        public static string GetTimestamp()
        {
            return string.Format("{0:" + LoggerSettings.TIMESTAMP_FORMAT_DATE + "}{1}{0:" + LoggerSettings.TIMESTAMP_FORMAT_TIME + "}",
                DateTime.Now, LoggerSettings.TIMESTAMP_FORMAT_SEPARATOR);
        }
    }
}
