using SharpRaven;
using SharpRaven.Data;
using System;

namespace Tasty.Logging.Sentry
{
    public class SentryLogger : Logger
    {
        private RavenClient ravenClient;

        public static new SentryLogger Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SentryLogger();
                }

                return instance as SentryLogger;
            }
        }

        /// <summary>
        /// To be able to send data to Sentry you have to initalize it first with a dsn. For more information look at the documentation
        /// from Sentry, they know better than me.
        /// </summary>
        /// <param name="dsn">Required by Sentry to work properly.</param>
        public void SetRavenDSN(string dsn)
        {
            ravenClient = new RavenClient(dsn);
        }

        /// <summary>
        /// Writes an exception message into the log with <see cref="LogType.WARNING"/>.
        /// <para>This also sends the exception + message to your attached Sentry client.</para>
        /// </summary>
        /// <param name="msg">The message to print.</param>
        /// <param name="ex">The exception to write into the log.</param>
        public override void WriteLog(string msg, Exception ex)
        {
            base.WriteLog(msg, ex);
            WriteSentry(msg, LogType.WARNING, ex);
        }

        /// <summary>
        /// Writes a formatted exception message into the log with <see cref="LogType.WARNING"/>.
        /// <para>This also sends the exception + message to your attached Sentry client.</para>
        /// </summary>
        /// <param name="msg">The message to print. (with formatters)</param>
        /// <param name="ex">The exception to write into the log.</param>
        /// <param name="param">Optional: All parameters for the formatted string.</param>
        public override void WriteLog(string msg, Exception ex, params object[] param)
        {
            msg = string.Format(msg, param);
            base.WriteLog(msg, ex);
            WriteSentry(msg, LogType.WARNING, ex);
        }

        /// <summary>
        /// Writes an exception message into the log.
        /// <para>This also sends the exception + message to your attached Sentry client.</para>
        /// </summary>
        /// <param name="msg">The message to print.</param>
        /// <param name="type">The <see cref="LogType"/> tag.</param>
        /// <param name="ex">The exception to write into the log.</param>
        public override void WriteLog(string msg, LogType type, Exception ex)
        {
            base.WriteLog(msg, type, ex);
            WriteSentry(msg, type, ex);
        }

        /// <summary>
        /// Writes a formatted exception message into the log.
        /// <para>This also sends the exception + message to your attached Sentry client.</para>
        /// </summary>
        /// <param name="msg">The message to print. (with formatters)</param>
        /// <param name="type">The <see cref="LogType"/> tag.</param>
        /// <param name="ex">The exception to write into the log.</param>
        /// <param name="param">All parameters for the formatted string.</param>
        public override void WriteLog(string msg, LogType type, Exception ex, params object[] param)
        {
            msg = string.Format(msg, param);
            base.WriteLog(msg, type, ex);
            WriteSentry(msg, type, ex);
        }

        /// <summary>
        /// Send a log message directly to you Sentry client!
        /// </summary>
        /// <param name="msg">A custom message for your log message (passing null or an empty string will only send the exception to Sentry)</param>
        /// <param name="mode">The <see cref="LogType"/> tag.</param>
        /// <param name="ex">The exception you want to send to Sentry.</param>
        public void WriteSentry(string msg, LogType mode, Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(msg))
            {
                ravenClient.Capture(new SentryEvent(ex)
                {
                    Message = msg,
                    Level = GetErrorLevel(mode)
                });
            }
            else
            {
                ravenClient.Capture(new SentryEvent(ex)
                {
                    Level = GetErrorLevel(mode)
                });
            }
        }

        /// <summary>
        /// Converts a <see cref="LogType"/> object into an <see cref="ErrorLevel"/> for SharpRaven.Sentry.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected ErrorLevel GetErrorLevel(LogType type)
        {
            switch (type)
            {
                case LogType.DEBUG:
                    return ErrorLevel.Debug;
                case LogType.ERROR:
                    return ErrorLevel.Error;
                case LogType.FATAL:
                    return ErrorLevel.Fatal;
                case LogType.WARNING:
                    return ErrorLevel.Warning;
                default:
                    return ErrorLevel.Info;
            }
        }
    }
}
