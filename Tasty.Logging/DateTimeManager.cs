using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.Logging
{
    public static class DateTimeManager
    {
        public static string GetDate(char splitter = '.')
        {
            return string.Format("{0:" + LoggerSettings.TIMESTAMP_FORMAT_DATE + "}", DateTime.Now);
        }

        public static string GetTime(char splitter = ':', bool includeMilliseconds = false)
        {
            return string.Format("{0:" + LoggerSettings.TIMESTAMP_FORMAT_TIME + "}", DateTime.Now);
        }

        public static string GetTimestamp(string splitterOverride)
        {
            return string.Format("{0:" + LoggerSettings.TIMESTAMP_FORMAT_DATE + "}{1}{0:" + LoggerSettings.TIMESTAMP_FORMAT_TIME + "}",
                DateTime.Now, LoggerSettings.TIMESTAMP_FORMAT_SEPARATOR);
        }

        public static string GetTimestamp()
        {
            return string.Format("{0:" + LoggerSettings.TIMESTAMP_FORMAT_DATE + "}{1}{0:" + LoggerSettings.TIMESTAMP_FORMAT_TIME + "}",
                DateTime.Now, LoggerSettings.TIMESTAMP_FORMAT_SEPARATOR);
        }
    }
}
