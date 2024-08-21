// <copyright file="ExceptionViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ViewModels
{
    using System;
    using System.Text;
    using Newtonsoft.Json;
    using UBind.Application.Exceptions;

    /// <summary>
    /// View model for delivering exception information.
    /// </summary>
    public class ExceptionViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionViewModel"/> class.
        /// </summary>
        /// <param name="ex">The exception that has been caught.</param>
        public ExceptionViewModel(Exception ex)
        {
            this.Type = ex.GetType().ToString();
            this.Message = ex.Message;
            this.StackTrace = ex.StackTrace;

            if (ex.Message.ToLower().Contains(ErrorMessage.MessageCode))
            {
                var errorMessage = JsonConvert.DeserializeObject<ErrorMessage.ErrorMessageCode>(ex.Message);
                this.Code = errorMessage.Code;
                this.Message = errorMessage.Message;
            }

            if (ex.InnerException != null)
            {
                this.InnerException = new ExceptionViewModel(ex.InnerException);
            }
        }

        [JsonConstructor]
        private ExceptionViewModel()
        {
        }

        /// <summary>
        /// Gets a string indicating the type of the exception.
        /// </summary>
        [JsonProperty]
        public string Type { get; private set; }

        /// <summary>
        /// Gets a string indicating the error code of the exception.
        /// </summary>
        [JsonProperty]
        public string Code { get; private set; }

        /// <summary>
        /// Gets the exception message.
        /// </summary>
        [JsonProperty]
        public string Message { get; private set; }

        /// <summary>
        /// Gets the stack trace from the exception.
        /// </summary>
        [JsonProperty]
        public string StackTrace { get; private set; }

        /// <summary>
        /// Gets any inner exception.
        /// </summary>
        [JsonProperty]
        public ExceptionViewModel InnerException { get; private set; }

        /// <summary>
        /// Pretty print the exception details.
        /// </summary>
        /// <returns>A string containing pretty-printed exception details.</returns>
        public string PrettyPrint()
        {
            var stringBuilder = new StringBuilder();
            this.PrettyPrint(this, stringBuilder, 0);
            return stringBuilder.ToString();
        }

        private void PrettyPrint(ExceptionViewModel viewModel, StringBuilder stringBuilder, int indent)
        {
            stringBuilder.Append(' ', indent);
            stringBuilder.AppendLine("Type:");
            stringBuilder.Append(' ', indent);
            stringBuilder.AppendLine(viewModel.Type);
            stringBuilder.AppendLine();
            stringBuilder.Append(' ', indent);
            stringBuilder.AppendLine("Message:");
            stringBuilder.Append(' ', indent);
            stringBuilder.AppendLine(viewModel.Message);
            stringBuilder.AppendLine();
            stringBuilder.Append(' ', indent);
            stringBuilder.AppendLine("Stack Trace:");
            var stackTraceLines = viewModel.StackTrace != null
                ? viewModel.StackTrace.Replace("\r\n", "\n").Split('\n')
                : Array.Empty<string>();
            foreach (var line in stackTraceLines)
            {
                stringBuilder.Append(' ', indent);
                stringBuilder.AppendLine(line);
            }

            stringBuilder.AppendLine();
            stringBuilder.AppendLine();
            if (viewModel.InnerException != null)
            {
                stringBuilder.Append(' ', indent);
                stringBuilder.AppendLine("Inner exception:");
                stringBuilder.AppendLine();
                this.PrettyPrint(viewModel.InnerException, stringBuilder, indent + 4);
            }
        }
    }
}
