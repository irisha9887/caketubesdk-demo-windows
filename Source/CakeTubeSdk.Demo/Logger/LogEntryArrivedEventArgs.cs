namespace CakeTubeSdk.Demo.Logger
{
    using System;

    /// <summary>
    /// <see cref="EventLoggerListener.LogEntryArrived"/> event arguments.
    /// </summary>
    internal class LogEntryArrivedEventArgs : EventArgs
    {
        /// <summary>
        /// <see cref="LogEntryArrivedEventArgs"/> constructor.
        /// </summary>
        /// <param name="entry">Log entry message.</param>
        public LogEntryArrivedEventArgs(string entry)
        {
            this.Entry = entry;
        }

        /// <summary>
        /// Log entry message.
        /// </summary>
        public string Entry { get; }
    }
}