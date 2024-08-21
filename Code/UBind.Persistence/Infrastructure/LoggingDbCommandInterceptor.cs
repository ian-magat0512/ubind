// <copyright file="LoggingDbCommandInterceptor.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Infrastructure
{
    using System;
    using System.Data.Common;
    using System.Data.Entity.Infrastructure.Interception;
    using System.Diagnostics;

    /// <summary>
    /// Interceptor for logging slow or failing SQL commands.
    /// </summary>
    public class LoggingDbCommandInterceptor : DbCommandInterceptor
    {
        private const string TimerKey = "timer";
        private static bool loggingEnabled;
        private readonly Action<string> logWarning;
        private readonly Action<string> logError;
        private readonly IDatabaseConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingDbCommandInterceptor"/> class.
        /// </summary>
        /// <param name="logWarning">Method for writing warnings to the log.</param>
        /// <param name="logError">Method for writing error to the log.</param>
        /// <param name="configuration">Database configuration specifying when to log.</param>
        public LoggingDbCommandInterceptor(Action<string> logWarning, Action<string> logError, IDatabaseConfiguration configuration)
        {
            this.logWarning = logWarning;
            this.logError = logError;
            this.configuration = configuration;
        }

        public static void EnableLogging(bool enable)
        {
            loggingEnabled = enable;
        }

        /// <inheritdoc/>
        public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            this.LogCommand(command);
            this.Executing(interceptionContext);
            base.ReaderExecuting(command, interceptionContext);
        }

        /// <inheritdoc/>
        public override void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            this.Executed(command, interceptionContext);
            base.ReaderExecuted(command, interceptionContext);
        }

        /// <inheritdoc/>
        public override void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            this.LogCommand(command);
            this.Executing(interceptionContext);
            base.NonQueryExecuting(command, interceptionContext);
        }

        /// <inheritdoc/>
        public override void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            this.Executed(command, interceptionContext);
            base.NonQueryExecuted(command, interceptionContext);
        }

        /// <inheritdoc/>
        public override void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            this.LogCommand(command);
            this.Executing(interceptionContext);
            base.ScalarExecuting(command, interceptionContext);
        }

        /// <inheritdoc/>
        public override void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            this.Executed(command, interceptionContext);
            base.ScalarExecuted(command, interceptionContext);
        }

        private void Executing<T>(DbCommandInterceptionContext<T> interceptionContext)
        {
            var timer = new Stopwatch();
            interceptionContext.SetUserState(TimerKey, timer);
            timer.Start();
        }

        private void Executed<T>(DbCommand command, DbCommandInterceptionContext<T> interceptionContext)
        {
            var timer = (Stopwatch)interceptionContext.FindUserState(TimerKey);
            timer.Stop();

            if (interceptionContext.Exception != null)
            {
                var logMessage = $"SQL COMMAND FAILED - {interceptionContext.Exception.Message}: "
                    + command.CommandText + " \n"
                    + interceptionContext.Exception.StackTrace;
                var innerException = interceptionContext.Exception.InnerException;
                while (innerException != null)
                {
                    logMessage += $"Inner exception - {innerException.Message}: \n"
                        + innerException.StackTrace;
                    innerException = innerException.InnerException;
                }

                this.logError(logMessage);
            }
            else if (timer.ElapsedMilliseconds >= this.configuration.SlowCommandThresholdMs)
            {
                this.logWarning($"SQL COMMAND SLOW ({timer.ElapsedMilliseconds}ms): "
                    + command.CommandText);
            }
        }

        private void LogCommand(DbCommand command)
        {
            if (!loggingEnabled)
            {
                return;
            }

            var query = command.CommandText;
            foreach (DbParameter param in command.Parameters)
            {
                string parameterValue;

                if (param.Value == DBNull.Value)
                {
                    parameterValue = "NULL";
                }
                else if (param.Value is string)
                {
                    parameterValue = "'" + param.Value.ToString().Replace("'", "''") + "'";
                }
                else if (param.Value is DateTime)
                {
                    parameterValue = "'" + ((DateTime)param.Value).ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
                }
                else if (param.Value is bool)
                {
                    parameterValue = (bool)param.Value ? "1" : "0";  // or 'TRUE'/'FALSE' depending on your SQL server
                }
                else if (param.Value is Guid)
                {
                    parameterValue = "'" + param.Value.ToString() + "'";
                }
                else if (param.Value is int || param.Value is long || param.Value is decimal || param.Value is double || param.Value is float)
                {
                    parameterValue = param.Value.ToString();
                }

                // Add checks for other types as necessary
                else
                {
                    // Default fallback, use ToString and hope for the best
                    parameterValue = param.Value.ToString();
                }

                // Replace the parameter placeholder with the value
                query = query.Replace("@" + param.ParameterName, parameterValue);
            }
            Debug.WriteLine("===================== SQL COMMAND =====================");
            Debug.WriteLine(query);
            Debug.WriteLine("=======================================================");
        }
    }
}
