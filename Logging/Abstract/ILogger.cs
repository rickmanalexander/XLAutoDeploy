using System;

namespace XLAutoDeploy.Logging
{
    internal interface ILogger
    {
        bool IsTraceEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }
        bool IsFatalEnabled { get; }
        string Name { get; }

        void Trace(string message);
        void Trace(Exception exception, string message);
        void Trace(Exception exception, string message, params object[] args);

        void Debug(string message);
        void Debug(Exception exception, string message);
        void Debug(Exception exception, string message, params object[] args);

        void Info(string message);

        void Warn(string message);
        void Warn(Exception exception, string message);
        void Warn(Exception exception, string message, params object[] args);

        void Error(string message);
        void Error(Exception exception, string message);
        void Error(Exception exception, string message, params object[] args);

        void Fatal(string message);
        void Fatal(Exception exception, string message);
        void Fatal(Exception exception, string message, params object[] args);
    }
}
