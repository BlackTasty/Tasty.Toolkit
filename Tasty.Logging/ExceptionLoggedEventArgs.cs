using System;

namespace Tasty.Logging
{
    public class ExceptionLoggedEventArgs
    {
        private string msg;
        private LogType type;
        private Exception ex;

        public string Message => msg;

        public LogType Type => type;

        public Exception Exception => ex;

        public ExceptionLoggedEventArgs(string msg, LogType type, Exception ex)
        {
            this.msg = msg;
            this.type = type;
            this.ex = ex;
        }
    }
}
