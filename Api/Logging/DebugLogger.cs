using System;
using System.Diagnostics;

namespace MandraSoft.PokemonGo.Api.Logging
{
    /// <summary>
    /// A logger which writes all logs to the Debug Output log.
    /// </summary>
    public class DebugLogger : ILogger
    {
        private LogLevel minimumLogLevel;

        /// <summary>
        /// Must define a minimum log level, all levels below won't be logged
        /// </summary>
        /// <param name="minimumLogLevel">Minimum loglevel</param>
        public DebugLogger(LogLevel minimumLogLevel)
        {
            this.minimumLogLevel = minimumLogLevel;
        }

        /// <summary>
        /// Log a specific message by LogLevel.
        /// </summary>
        /// <param name="message">The log message</param>
        /// <param name="level">Optional. Default <see cref="LogLevel.Info"/>.</param>
        public void Write(string message, LogLevel level = LogLevel.Info)
        {
            if (level < minimumLogLevel)
                return;

            Debug.WriteLine($"[{ DateTime.Now.ToString("HH:mm:ss")}] {message}");
        }
    }
}