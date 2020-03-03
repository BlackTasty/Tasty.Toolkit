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
            return DateTime.Now.ToString(string.Format("dd{0}MM{0}yyyy", splitter));
        }

        public static string GetTime(char splitter = ':', bool includeMilliseconds = false)
        {
            return DateTime.Now.ToString(string.Format(!includeMilliseconds ? "HH{0}mm{0}ss" : "HH{0}mm{0}ss.fff", splitter));
        }
        public static string GetDateAndTime()
        {
            return GetDate() + ", " + GetTime(':', true);
        }

        /*public static string GetDateAndTime(bool includeMilliseconds)
        {
            return GetDate() + ", " + GetTime(':', includeMilliseconds);
        }*/

        public static string GetDateAndTime(char mainSplitter, char splitter)
        {
            return GetDate(splitter) + mainSplitter + GetTime(splitter, true);
        }
    }
}
