using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasty.Logging
{
    public class Logger
    {
        protected static Logger instance;
        protected static readonly string logFolderPath = AppDomain.CurrentDomain.BaseDirectory + "Logs\\";
        protected string fileName = "log.txt";

        protected string filePath;
        protected int sessionId = -1;
        protected Random rnd = new Random();

        public static Logger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger();
                }

                return instance;
            }
        }

        public static IConsole AttachedConsole { get; set; }

        public string FilePath => filePath;

        public int SessionID => sessionId;

        public bool ShowSessionId => false;

        protected Logger()
        {
            sessionId = rnd.Next(10000000, 99999999);
            filePath = Path.Combine(logFolderPath, fileName);
            LogFileStart = "";
        }

        public static string LogFileStart { get; set; }

        /// <summary>
        /// Writes a formatted message into the log with <see cref="LogType.INFO"/> tag.
        /// </summary>
        /// <param name="msg">The message to print (with formatters).</param>
        /// <param name="param">Optional: All parameters for the formatted string.</param>
        public virtual void WriteLog(string msg, params object[] param)
        {
            WriteLog(msg, LogType.INFO, param);
        }

        /// <summary>
        /// Writes a formatted exception message into the log with <see cref="LogType.WARNING"/>.
        /// </summary>
        /// <param name="msg">The message to print (with formatters).</param>
        /// <param name="ex">The exception to write into the log.</param>
        /// <param name="param">Optional: All parameters for the formatted string.</param>
        public virtual void WriteLog(string msg, Exception ex, params object[] param)
        {
            WriteLog(msg, LogType.WARNING, ex, param);
        }

        /// <summary>
        /// Writes a message into the log with <see cref="LogType.INFO"/>
        /// </summary>
        /// <param name="msg">The message to print</param>
        public virtual void WriteLog(string msg)
        {
            WriteLog(msg, LogType.INFO);
        }

        /// <summary>
        /// Writes an exception message into the log with <see cref="LogType.WARNING"/>.
        /// </summary>
        /// <param name="msg">The message to print.</param>
        /// <param name="ex">The exception to write into the log.</param>
        public virtual void WriteLog(string msg, Exception ex)
        {
            WriteLog(msg, LogType.WARNING, ex);
        }

        /// <summary>
        /// Writes a formatted message into the log
        /// </summary>
        /// <param name="msg">The message to print (with formatters)</param>
        /// <param name="type">The <see cref="LogType"/> tag</param>
        /// <param name="param">Optional: All parameters for the formatted string</param>
        public virtual void WriteLog(string msg, LogType type, params object[] param)
        {
            WriteLog(string.Format(msg, param), type);
        }

        /// <summary>
        /// Writes a formatted exception message into the log.
        /// </summary>
        /// <param name="msg">The message to print (with formatters).</param>
        /// <param name="type">The <see cref="LogType"/> tag.</param>
        /// <param name="ex">The exception to write into the log.</param>
        /// <param name="param">All parameters for the formatted string.</param>
        public virtual void WriteLog(string msg, LogType type, Exception ex, params object[] param)
        {
            WriteLog(string.Format(msg, param), type, ex);
        }

        /// <summary>
        /// Writes an exception message into the log.
        /// </summary>
        /// <param name="msg">The message to print.</param>
        /// <param name="type">The <see cref="LogType"/> tag.</param>
        /// <param name="ex">The exception to write into the log.</param>
        public virtual void WriteLog(string msg, LogType type, Exception ex)
        {
            WriteInnerException(msg, type, ex, false);
            if (ex.InnerException != null)
                WriteInnerException(msg, type, ex.InnerException, true);
        }

        /// <summary>
        /// Writes a message into the log. This is the core method!
        /// </summary>
        /// <param name="msg">The message to print.</param>
        /// <param name="type">The <see cref="LogType"/> tag.</param>
        public virtual void WriteLog(string msg, LogType type)
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Directory.CreateDirectory(logFolderPath);

                    if (!File.Exists(filePath) && fileName != null)
                    {
                        string logStart = LogFileStart;
                        logStart = logStart.Replace("[DATE]", DateTimeManager.GetDate());
                        logStart = logStart.Replace("[TIME]", DateTimeManager.GetTime());
                        logStart = logStart.Replace("[NAME]", fileName);
                        File.WriteAllText(filePath, logStart);
                    }

                    StringBuilder logBuilder = new StringBuilder();
                    string identifier = "";

                    switch (type)
                    {
                        case LogType.INFO:
                            identifier = "[INF] ";
                            break;

                        case LogType.WARNING:
                            identifier = "[WRN] ";
                            break;

                        case LogType.ERROR:
                            identifier = "[ERR] ";
                            break;

                        case LogType.DEBUG:
                            identifier = "[DBG] ";
                            break;

                        case LogType.VERBOSE:
                            identifier = "[VER] ";
                            break;

                        case LogType.FATAL:
                            identifier = "[FAT] ";
                            break;

                        case LogType.CONSOLE:
                            identifier = "[CON] ";
                            break;
                    }

                    StringBuilder logFileBuilder = new StringBuilder(identifier);
                    Console.Write(identifier);

                    //if (!string.IsNullOrWhiteSpace(prefix))
                    //{
                    //    prefix = string.Format(" [{0}]", prefix);
                    //    logFileBuilder.Append(prefix);
                    //    Console.Write(foregroundColor);
                    //}

                    if (ShowSessionId)
                    {
                        msg = string.Format("({0}) {1}: {2}", sessionId, DateTimeManager.GetDateAndTime(), msg);
                    }
                    else
                    {
                        msg = string.Format("{0}: {1}", DateTimeManager.GetDateAndTime(), msg);
                    }

                    logFileBuilder.Append(msg);
                    logBuilder.Append(msg);

                    if (fileName != null)
                    {
                        if (type != LogType.VERBOSE)
                        {
                            File.AppendAllText(filePath, logFileBuilder.ToString() + "\r");
                        }
                        else
                        {
                            File.AppendAllText(filePath + ".verbose", logFileBuilder.ToString() + "\r");
                        }
                    }
                    #if DEBUG
                    Console.WriteLine(logBuilder.ToString());
                    #endif

                    if (AttachedConsole != null)
                    {
                        if (fileName != null)
                            AttachedConsole.WriteString(string.Format("({0}) {1}\n", fileName, logBuilder), type);
                        else
                            AttachedConsole.WriteString(string.Format("{0}\n", logBuilder), type);
                    }
                    break;
                }
                catch
                {

                }
            }
        }

        protected virtual void WriteInnerException(string msg, LogType type, Exception ex, bool isInner)
        {
            string stackTrace = GetExceptionLine(ex);

            if (msg == string.Empty)
            {
                if (stackTrace != "-1")
                    WriteLog(string.Format("[{0}] {1} \"{2}\": {3} (Class \"{4}\", Line {5})",
                        ex.GetType().Name, GetExceptionText(isInner), ex.TargetSite, ex.Message, ex.Source, stackTrace),
                        type);
                else
                    WriteLog(string.Format("[{0}] {1} \"{2}\": {3} (Class \"{4}\")",
                        ex.GetType().Name, GetExceptionText(isInner), ex.TargetSite, ex.Message, ex.Source),
                        type);
            }
            else
            {
                if (stackTrace != "-1")
                    WriteLog(string.Format("[{0}] {1} - {2} \"{3}\": {4} (Class \"{5}\", Line {6})",
                        ex.GetType().Name, msg, GetExceptionText(isInner), ex.TargetSite, ex.Message, ex.Source, stackTrace),
                        type);
                else
                    WriteLog(string.Format("[{0}] {1} - {2} \"{3}\": {4} (Class \"{5}\")",
                        ex.GetType().Name, msg, GetExceptionText(isInner), ex.TargetSite, ex.Message, ex.Source),
                        type);
            }
        }

        public static string GetExceptionLine(Exception ex)
        {
            if (ex.StackTrace != null)
            {
                string stackTrace = ex.StackTrace;
                stackTrace = ex.StackTrace.Remove(0, ex.StackTrace.LastIndexOf(' ') + 1);
                return stackTrace.Replace(".", "");
            }
            else
                return "-1";
        }

        private string GetExceptionText(bool isInner)
        {
            return !isInner ? "Exception thrown in method" : "Inner exception";
        }
    }
}