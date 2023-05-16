using NLog;

using System;

namespace XLAutoDeploy.Logging
{
    internal sealed class NLoggerProxy<T> : XLAutoDeploy.Logging.ILogger
    {
        private static readonly NLog.ILogger _logger =
                     LogManager.GetLogger(typeof(T).FullName);

        public bool IsTraceEnabled => _logger.IsTraceEnabled;
        public bool IsDebugEnabled => _logger.IsDebugEnabled;
        public bool IsInfoEnabled => _logger.IsInfoEnabled;
        public bool IsWarnEnabled => _logger.IsWarnEnabled;
        public bool IsErrorEnabled => _logger.IsErrorEnabled;
        public bool IsFatalEnabled => _logger.IsFatalEnabled;
        public string Name => _logger.Name;


        public void Trace(string message)
        {
            _logger.Trace(message);
        }

        public void Trace(Exception exception, string message)
        {
            _logger.Trace(exception, message);
        }

        public void Trace(Exception exception, string message, params object[] args)
        {
            _logger.Trace(exception, message, args);
        }


        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Debug(Exception exception, string message)
        {
            _logger.Debug(exception, message);
        }

        public void Debug(Exception exception, string message, params object[] args)
        {
            _logger.Debug(exception, message, args);
        }

        
        public void Info(string message)
        {
            _logger.Info(message);
        }


        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public void Warn(Exception exception, string message)
        {
            _logger.Warn(exception, message);
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            _logger.Warn(exception, message, args);
        }


        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(Exception exception, string message)
        {
            _logger.Error(exception, message);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            _logger.Error(exception, message, args);
        }


        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        public void Fatal(Exception exception, string message)
        {
            _logger.Fatal(exception, message);
        }

        public void Fatal(Exception exception, string message, params object[] args)
        {
            _logger.Fatal(exception, message, args);
        }
    }
}
