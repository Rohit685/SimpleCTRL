namespace SimpleCTRL.Engine.InternalSystems
{
    internal enum LoggingLevel
    {
        DEBUG,
        INFO,
        WARNING,
        ERROR
    }

    /// <summary>
    /// Provides logging functionality for debugging, error handling, information, and warnings.
    /// </summary>
    internal static class Logging
    {
        /// <summary>
        /// Logs a debug message with optional caller information and exception.
        /// </summary>
        /// <param name="message">The debug message.</param>
        /// <param name="caller">The method or class name invoking the debug.</param>
        /// <param name="ex">Optional exception to log.</param>
        internal static void Debug(string message, string caller, Exception ex = null)
        {
            Log(LoggingLevel.DEBUG, message, caller, ex);
        }

        /// <summary>
        /// Logs an error message with optional caller information and exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="caller">The method or class name invoking the error.</param>
        /// <param name="ex">Optional exception to log.</param>
        internal static void Error(string message, string caller, Exception ex = null)
        {
            Log(LoggingLevel.ERROR, message, caller, ex);
        }

        /// <summary>
        /// Logs an informational message with optional caller information and exception.
        /// </summary>
        /// <param name="message">The informational message.</param>
        /// <param name="caller">The method or class name invoking the information.</param>
        /// <param name="ex">Optional exception to log.</param>
        internal static void Info(string message, string caller, Exception ex = null)
        {
            Log(LoggingLevel.INFO, message, caller, ex);
        }

        /// <summary>
        /// Logs a warning message with optional caller information and exception.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="caller">The method or class name invoking the warning.</param>
        /// <param name="ex">Optional exception to log.</param>
        internal static void Warning(string message, string caller, Exception ex = null)
        {
            Log(LoggingLevel.WARNING, message, caller, ex);
        }

        /// <summary>
        /// Logs a message with the specified logging level, caller information, and optional exception.
        /// </summary>
        /// <param name="level">The logging level.</param>
        /// <param name="message">The message to log.</param>
        /// <param name="caller">The method or class name invoking the logging.</param>
        /// <param name="ex">Optional exception to log.</param>
        private static void Log(LoggingLevel level, string message, string caller, Exception ex = null)
        {
            // Check if the specified logging level is greater than or equal to the configured logging level
            if ((int)level >= Settings.LogLevel)
            {
                // InternalLogger the message along with the caller information
                Game.LogTrivial("[" + caller + "] " + message);

                // If an exception is provided, log its message and stack trace as an error and debug information
                if (ex != null)
                {
                    Error(ex.Message, caller);
                    Debug(ex.StackTrace, caller);
                }
            }
        }
    }
}