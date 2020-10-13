using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.Logging
{
    public class LoggerSettings
    {
        public static string IDENTIFIER_INFO = "[ INF ]";
        public static string IDENTIFIER_WARN = "[ WRN ]";
        public static string IDENTIFIER_ERROR = "[ ERR ]";
        public static string IDENTIFIER_FATAL = "[ FAT ]";
        public static string IDENTIFIER_DEBUG = "[ DBG ]";
        public static string IDENTIFIER_VERBOSE = "[ VRB ]";
        public static string IDENTIFIER_CONSOLE = "[ CON ]";

        public static string TIMESTAMP_FORMAT_DATE = "dd.MM.yyyy";
        public static string TIMESTAMP_FORMAT_SEPARATOR = "-";
        public static string TIMESTAMP_FORMAT_TIME = "hh:mm:ss.fff";

        public static bool SHOW_LOG_NAME = true;
    }
}
