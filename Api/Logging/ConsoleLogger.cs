using System;

namespace MandraSoft.PokemonGo.Api.Logging
{
    /// <summary>
    /// A logger which writes all logs to the Console Output.
    /// Usefull for ASP.NET applications where no console is available.
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private LogLevel minimumLogLevel;

        /// <summary>
        /// Must define a minimum log level, all levels below won't be logged
        /// </summary>
        /// <param name="minimumLogLevel">Minimum loglevel</param>
        public ConsoleLogger(LogLevel minimumLogLevel)
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

            Console.WriteLine($"[{ DateTime.Now.ToString("HH:mm:ss")}] {message}");
        }
    }
}