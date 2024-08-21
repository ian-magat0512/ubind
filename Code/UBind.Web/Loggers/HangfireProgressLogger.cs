// <copyright file="HangfireProgressLogger.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Loggers
{
    using System.Diagnostics.Contracts;
    using Hangfire.Console;
    using Hangfire.Console.Progress;
    using Hangfire.Server;
    using Microsoft.Extensions.Logging;
    using UBind.Domain.Loggers;

    /// <summary>
    /// Hangfire console container class.
    /// </summary>
    public class HangfireProgressLogger : IProgressLogger
    {
        private readonly PerformContext performContext;
        private IProgressBar progressBar;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireProgressLogger"/> class.
        /// </summary>
        public HangfireProgressLogger()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireProgressLogger"/> class.
        /// </summary>
        /// <param name="performContext">The perform context instance.</param>
        public HangfireProgressLogger(PerformContext performContext)
        {
            Contract.Assert(performContext != null);

            this.performContext = performContext;
        }

        /// <summary>
        /// Gets the progress bar instance.
        /// </summary>
        public IProgressBar ProgressBar
        {
            get
            {
                if (this.progressBar == null)
                {
                    this.progressBar = this.performContext.WriteProgressBar();
                }

                return this.progressBar;
            }
        }

        /// <inheritdoc/>
        public void Log(LogLevel logLevel, string message)
        {
            ConsoleTextColor color;
            switch (logLevel)
            {
                case LogLevel.Error:
                    color = ConsoleTextColor.Red;
                    break;
                case LogLevel.Information:
                    color = ConsoleTextColor.Green;
                    break;
                default:
                    color = ConsoleTextColor.White;
                    break;
            }

            this.performContext?.WriteLine(color, message);
        }

        /// <inheritdoc/>
        public void UpdateProgress(double value)
        {
            this.ProgressBar.SetValue(value);
        }
    }
}
