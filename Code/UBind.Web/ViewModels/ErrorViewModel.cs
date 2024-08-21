// <copyright file="ErrorViewModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ViewModels
{
    using System;
    using System.Security.Authentication;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using UBind.Web.Middleware;

    /// <summary>
    /// View model for delivering exception information.
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorViewModel"/> class with information about the underlying exception.
        /// </summary>
        /// <param name="error">The error type.</param>
        /// <param name="ex">The exception that has been caught.</param>
        /// <param name="isShowException">if show the exception.</param>
        public ErrorViewModel(ApiError error, Exception ex, bool isShowException = false)
        {
            this.Error = error;
            var exception = new ExceptionViewModel(ex);
            if (this.Error == ApiError.WorkbookError)
            {
                this.Code = exception.Code;
                this.Message = exception.Message;
                if (isShowException)
                {
                    this.Exception = exception.InnerException;
                }
            }
            else if (ex is AuthenticationException)
            {
                // custom message error code
                this.Code = "401";
                this.Message = exception.Message;
                this.Exception = exception.InnerException;
            }

            // this is the error that propagates to the user without vpn
            // gettype contains is needed to capture generic type as well.
            else if (ex.GetType().Name.Contains("CustomException"))
            {
                // custom message error code
                this.Code = "418";
                this.Message = exception.Message;
                this.Data = ex.Data;
            }
            else if (isShowException && this.Error != ApiError.WorkbookError)
            {
                this.Exception = exception;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorViewModel"/> class.
        /// </summary>
        /// <param name="error">The error type.</param>
        public ErrorViewModel(ApiError error)
        {
            this.Error = error;
        }

        /// <summary>
        /// Gets a string indicating the type of the exception.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public ApiError Error { get; }

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
        /// Gets the exception data.
        /// </summary>
        [JsonProperty]
        public object Data { get; private set; }

        /// <summary>
        /// Gets any inner exception.
        /// </summary>
        public ExceptionViewModel Exception { get; }
    }
}
