namespace CakeTubeSdk.Demo.Logger
{
    using System;

    using CakeTubeSdk.Core.Infrastructure;

    /// <summary>
    /// Event-based logger listener.
    /// </summary>
    internal class EventLoggerListener : BaseLoggerListener
    {
        /// <summary>
        /// Fires when new log entry arrived.
        /// </summary>
        public event EventHandler<LogEntryArrivedEventArgs> LogEntryArrived;

        /// <summary>
        /// Traces log message.
        /// </summary>
        /// <param name="message">Log message to trace.</param>
        public override void Trace(string message)
        {
            var result = this.CreateDecoratedMessage("TRACE", message);
            this.OnLogEntryArrived(result);
        }

        /// <summary>
        /// Traces log message with format args.
        /// </summary>
        /// <param name="message">Log message to trace (can contain format specifiers, like <see cref="string.Format(string,object)"/>.</param>
        /// <param name="args">Format args.</param>
        public override void Trace(string message, params object[] args)
        {
            var result = this.CreateDecoratedMessage("TRACE", message, args);
            this.OnLogEntryArrived(result);
        }

        /// <summary>
        /// Logs error message with format args.
        /// </summary>
        /// <param name="message">Log message (can contain format specifiers, like <see cref="string.Format(string,object)"/>.</param>
        /// <param name="args">Format args.</param>
        public override void Error(string message, params object[] args)
        {
            var result = this.CreateDecoratedMessage("ERROR", message, args);
            this.OnLogEntryArrived(result);
        }

        /// <summary>
        /// Logs error message with exception information.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="exception">Exception.</param>
        public override void Error(string message, Exception exception)
        {
            var result = this.CreateDecoratedMessage("Error", $"{message} |{this.CreateDecoratedMessage(exception)}");
            this.OnLogEntryArrived(result);
        }

        /// <summary>
        /// <see cref="LogEntryArrived"/> invoker.
        /// </summary>
        /// <param name="logEntry">Log entry message.</param>
        protected virtual void OnLogEntryArrived(string logEntry)
        {
            this.LogEntryArrived?.Invoke(this, new LogEntryArrivedEventArgs(logEntry));
        }
    }
}