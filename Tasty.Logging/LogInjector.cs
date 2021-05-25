namespace Tasty.Logging
{
    public class LogInjector
    {
        private string prefix;

        public LogInjector(string prefix)
        {
            this.prefix = prefix;
        }

        protected void WriteLog(string msg)
        {
            Logger.Default.WriteLog(prefix, msg);
        }

        protected void WriteLog(string msg, params object[] param)
        {
            Logger.Default.WriteLog(prefix, msg, param);
        }

        protected void WriteLog(string msg, LogType logType, params object[] param)
        {
            Logger.Default.WriteLog(prefix, msg, logType, param);
        }
    }
}
