using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Tasty.Logging
{
    /// <summary>
    /// Class which enables logging to file and attached objects.
    /// </summary>
    public class Logger : LoggerSettings
    {
        private const int DEFAULT_LINE_LIMIT = 2000;

        protected static Logger instance;
        protected static readonly string logFolderPath = AppDomain.CurrentDomain.BaseDirectory + "Logs\\";
        protected string fileName;

        protected string filePath;
        protected int sessionId = -1;
        protected Random rnd = new Random();
        protected bool isDebug;
        protected int fileLineLimit = -1;
        protected int currentLineCount;

        /// <summary>
        /// Fires whenever an exception has been logged with this <see cref="Logger"/>
        /// </summary>
        public event EventHandler<ExceptionLoggedEventArgs> ExceptionLogged;

        /// <summary>
        /// Returns the default logger or generates a new one
        /// </summary>
        public static Logger Default
        {
            get
            {
                if (instance == null)
                {
                    instance = new Logger(false, DEFAULT_LINE_LIMIT);
                }

                return instance;
            }
        }

        /// <summary>
        /// Initialize a new default logger
        /// </summary>
        /// <param name="isDebug">Set to true if you wish to output <see cref="LogType.DEBUG"/> and <see cref="LogType.VERBOSE"/> messages.</param>
        /// <returns>The default logger</returns>
        public static Logger Initialize(int fileLineLimit = DEFAULT_LINE_LIMIT, bool isDebug = false)
        {
            instance = new Logger(isDebug, fileLineLimit);
            return instance;
        }

        /// <summary>
        /// Initialize a new default logger
        /// </summary>
        /// <param name="fileName">The desired name of the lines file. Setting this to null uses the name set in <see cref="LoggerSettings"/>.DEFAULT_LOG_NAME</param>
        /// <param name="isDebug">Set to true if you wish to output <see cref="LogType.DEBUG"/> and <see cref="LogType.VERBOSE"/> messages.</param>
        /// <param name="generateId">Set to true to generate a session id for this <see cref="Logger"/></param>
        /// <param name="regenerateFile">Set to true if you wish that the lines file with the given name should be cleared each time the <see cref="Logger"/> is initialized</param>
        /// <returns>The default logger</returns>
        public static Logger Initialize(string fileName = null, int fileLineLimit = DEFAULT_LINE_LIMIT, bool isDebug = false,
            bool generateId = false, bool regenerateFile = false)
        {
            instance = new Logger(fileName, generateId, regenerateFile, fileLineLimit)
            {
                isDebug = isDebug
            };
            return instance;
        }

        /// <summary>
        /// Sets or gets the attached console. Used for outputting logged lines to an additional object
        /// </summary>
        public IConsole AttachedConsole { get; set; }

        /// <summary>
        /// If set to true, logging will be disabled altogether
        /// </summary>
        public bool DisableLogging { get; set; }

        /// <summary>
        /// If set to false, lines lines won't be outputted to the "AttachedConsole" object
        /// </summary>
        public bool LogToAttachedConsole { get; set; } = true;

        /// <summary>
        /// Should prefixes like [INF], [DBG], etc be attached to console strings?
        /// </summary>
        public bool AddIdentifierToConsole { get; set; } = false;

        /// <summary>
        /// The full path to this lines file
        /// </summary>
        public string FilePath => filePath;

        /// <summary>
        /// The current session id for this <see cref="Logger"/>. -1 = Unset
        /// </summary>
        public int SessionID => sessionId;

        public bool IsDebug
        {
            get => isDebug;
            set => isDebug = value;
        }

        //public bool ShowSessionId => false;

        /// <summary>
        /// Initialize a new logger
        /// </summary>
        /// <param name="isDebug">Set to true if you wish to output <see cref="LogType.DEBUG"/> and <see cref="LogType.VERBOSE"/> messages.</param>
        public Logger(bool isDebug, int fileLineLimit) : this(null, false, false, fileLineLimit)
        {
            this.isDebug = isDebug;
        }

        /// <summary>
        /// Initialize a new logger
        /// </summary>
        /// <param name="fileName">The desired name of the lines file. Setting this to null uses the name set in <see cref="LoggerSettings"/>.DEFAULT_LOG_NAME</param>
        /// <param name="generateId">Set to true to generate a session id for this <see cref="Logger"/></param>
        /// <param name="regenerateFile">Set to true if you wish that the lines file with the given name should be cleared each time the <see cref="Logger"/> is initialized</param>
        public Logger(string fileName, bool generateId, bool regenerateFile, int fileLineLimit)
        {
            this.fileLineLimit = fileLineLimit;
            if (fileName == null)
            {
                fileName = DEFAULT_LOG_NAME;
            }

            this.fileName = fileName;
            filePath = Path.Combine(logFolderPath, fileName);
            if (generateId)
            {
                sessionId = rnd.Next(10000000, 99999999);
            }

            if (regenerateFile)
            {
                ClearLog();
            }
            LogFileStart = "";
            currentLineCount = File.Exists(filePath) ? File.ReadAllLines(filePath).Length : 0;
        }

        [Obsolete]
        public static string LogFileStart { get; set; }

        /// <summary>
        /// Clears the log file
        /// </summary>
        public void ClearLog()
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                currentLineCount = 0;
            }
        }

        /// <summary>
        /// Writes a message into the lines with <see cref="LogType.INFO"/>
        /// </summary>
        /// <param name="msg">The message to print</param>
        public virtual void WriteLog(string msg)
        {
            WriteLog(msg, LogType.INFO);
        }

        /// <summary>
        /// Writes a formatted message into the lines with <see cref="LogType.INFO"/> tag.
        /// </summary>
        /// <param name="msg">The message to print (with formatters).</param>
        /// <param name="param">Optional: All parameters for the formatted string.</param>
        public virtual void WriteLog(string msg, params object[] param)
        {
            WriteLog(msg, LogType.INFO, param);
        }

        /// <summary>
        /// Writes a formatted exception message into the lines with <see cref="LogType.WARNING"/>.
        /// </summary>
        /// <param name="msg">The message to print (with formatters).</param>
        /// <param name="ex">The exception to write into the lines.</param>
        /// <param name="param">Optional: All parameters for the formatted string.</param>
        public virtual void WriteLog(string msg, Exception ex, params object[] param)
        {
            WriteLog(msg, LogType.WARNING, ex, param);
        }

        /// <summary>
        /// Writes an exception message into the lines with <see cref="LogType.WARNING"/>.
        /// </summary>
        /// <param name="msg">The message to print.</param>
        /// <param name="ex">The exception to write into the lines.</param>
        public virtual void WriteLog(string msg, Exception ex)
        {
            WriteLog(msg, LogType.WARNING, ex);
        }

        /// <summary>
        /// Writes a formatted message into the lines
        /// </summary>
        /// <param name="msg">The message to print (with formatters)</param>
        /// <param name="type">The <see cref="LogType"/> tag</param>
        /// <param name="param">Optional: All parameters for the formatted string</param>
        public virtual void WriteLog(string msg, LogType type, params object[] param)
        {
            WriteLog(string.Format(msg, param), type);
        }

        /// <summary>
        /// Writes a formatted exception message into the lines.
        /// </summary>
        /// <param name="msg">The message to print (with formatters).</param>
        /// <param name="type">The <see cref="LogType"/> tag.</param>
        /// <param name="ex">The exception to write into the lines.</param>
        /// <param name="param">All parameters for the formatted string.</param>
        public virtual void WriteLog(string msg, LogType type, Exception ex, params object[] param)
        {
            WriteLog(string.Format(msg, param), type, ex);
        }

        /// <summary>
        /// Writes an exception message into the lines.
        /// </summary>
        /// <param name="msg">The message to print.</param>
        /// <param name="type">The <see cref="LogType"/> tag.</param>
        /// <param name="ex">The exception to write into the lines.</param>
        public virtual void WriteLog(string msg, LogType type, Exception ex)
        {
            WriteInnerException(msg, type, ex, false);
            if (ex.InnerException != null)
            {
                WriteInnerException(msg, type, ex.InnerException, true);
            }

            OnExceptionLogged(new ExceptionLoggedEventArgs(msg, type, ex));
        }

        /// <summary>
        /// Writes a message into the lines. This is the core method!
        /// </summary>
        /// <param name="msg">The message to print.</param>
        /// <param name="type">The <see cref="LogType"/> tag.</param>
        public virtual void WriteLog(string msg, LogType type)
        {
            if (DisableLogging || !isDebug && (type == LogType.DEBUG || type == LogType.VERBOSE))
            {
                return;
            }

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
                    string identifier = type.ToPrefix();

                    StringBuilder logFileBuilder = new StringBuilder(identifier + " ");
                    Console.Write(identifier);

                    //if (!string.IsNullOrWhiteSpace(prefix))
                    //{
                    //    prefix = string.Format(" [{0}]", prefix);
                    //    logFileBuilder.Append(prefix);
                    //    Console.Write(foregroundColor);
                    //}

                    if (sessionId > -1)
                    {
                        msg = string.Format("({0}) {1}: {2}", sessionId, DateTimeManager.GetTimestamp(), msg);
                    }
                    else
                    {
                        msg = string.Format("{0}: {1}", DateTimeManager.GetTimestamp(), msg);
                    }

                    logFileBuilder.Append(msg);
                    logBuilder.Append(msg);

                    if (fileName != null)
                    {
                        string targetPath;
                        if (type != LogType.VERBOSE)
                        {
                            targetPath = filePath;
                        }
                        else
                        {
                            targetPath = filePath + ".verbose";
                        }

                        File.AppendAllText(targetPath, logFileBuilder.ToString() + "\r");
                        currentLineCount++;
                    }

                    if (isDebug)
                    {
                        Console.WriteLine((AddIdentifierToConsole ? logFileBuilder : logBuilder).ToString());
                    }

                    if (AttachedConsole != null && LogToAttachedConsole)
                    {
                        if (fileName != null && SHOW_LOG_NAME)
                            AttachedConsole.WriteString(string.Format("({0}) {1}\n", fileName, AddIdentifierToConsole ? logFileBuilder : logBuilder), type);
                        else
                            AttachedConsole.WriteString(string.Format("{0}\n", AddIdentifierToConsole ? logFileBuilder : logBuilder), type);
                    }
                    break;
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Clears all but the recent X lines logged, where X is the line limit set when initializing
        /// </summary>
        public void ApplyLineLimit()
        {
            if (fileLineLimit > -1 && currentLineCount > fileLineLimit)
            {
                string[] lines = File.ReadAllLines(filePath);
                int lineCountDiff = currentLineCount - fileLineLimit;

                string log = string.Join("\r", lines.Skip(lineCountDiff));
                File.WriteAllText(filePath, log);
                currentLineCount = fileLineLimit;
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
            if (!string.IsNullOrWhiteSpace(ex.StackTrace))
            {
                string stackTrace = ex.StackTrace.Remove(0, ex.StackTrace.LastIndexOf(' ') + 1);
                return stackTrace.Replace(".", "");
            }
            else
                return "NO STACKTRACE";
        }

        private string GetExceptionText(bool isInner)
        {
            return !isInner ? "Exception thrown in method" : "Inner exception";
        }

        protected virtual void OnExceptionLogged(ExceptionLoggedEventArgs e)
        {
            ExceptionLogged?.Invoke(this, e);
        }
    }
}