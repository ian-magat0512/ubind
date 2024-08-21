// <copyright file="ErrorMessage.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Exceptions
{
    using Newtonsoft.Json;

    /// <summary>
    /// Exception messages for application.
    /// </summary>
    public static class ErrorMessage
    {
        /// <summary>
        /// The MessageCode constant for string comparison purposes.
        /// </summary>
        public const string MessageCode = "messagecode";

        /// <summary>
        /// Error messages related to Policy.
        /// </summary>
        public static class Policy
        {
            /// <summary>
            /// Policy startdate date mismatch between form data and calculation result.
            /// </summary>
            public const string StartDateMismatch = "Policy could not be issued because the policy start date supplied does not match the value from the associated calculation result";

            /// <summary>
            /// Policy end date mismatch between form data and calculation result.
            /// </summary>
            public const string EndDateMismatch = "Policy could not be issued because the policy end date supplied does not match the value from the associated calculation result";
        }

        /// <summary>
        /// Error messages related to FlexCel.
        /// </summary>
        public static class FlexCel
        {
            /// <summary>
            /// Calculation result problem
            ///  Note : ErrorCode is required for frontend utilisation.
            /// </summary>
            /// <param name="hasConfigError">If there is a config error.</param>
            /// <returns>Error Message in Json format.</returns>
            public static string GetCalculationResultErrorMessage(bool hasConfigError)
            {
                return hasConfigError
                    ? "There was a problem calculating the price or amount due to a misconfiguration of " +
                      "the calculation spreadsheet. Our team has been notified about this and will investigate " +
                      "and fix the problem. For the time being, we ask you to reload the page and try again. " +
                      "Thank you for your patience."
                    : "There has been an error calculating a price or amount. Our team has been notified about " +
                      "this and will investigate and fix the problem. For the time being, we ask you to reload " +
                      "the page and try again. Thank you for your patience.";
            }
        }

        /// <summary>
        /// Error messages related to premium funding provider.
        /// </summary>
        public static class ExternalService
        {
            /// <summary>
            /// External Service error
            ///  Note : ErrorCode is required for frontend utilisation.
            /// </summary>
            /// <returns>Error Message in Json format.</returns>
            public static string GetExternalServiceErrorMessage()
            {
                var errorMessage = "There was a problem calculating pricing from the premium funding provider. Our team has been notified about this and will investigate and fix the problem. For the time being, we ask you to reload the page and try again. Thank you for your patience";
                return JsonConvert.SerializeObject(new ErrorMessageCode("10043", errorMessage));
            }
        }

        /// <summary>
        /// Error message with code.
        /// </summary>
        public class ErrorMessageCode
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ErrorMessageCode"/> class.
            /// </summary>
            /// <param name="code">The error code.</param>
            /// <param name="message">The error message.</param>
            public ErrorMessageCode(string code, string message)
            {
                this.Code = code;
                this.Message = message;
            }

            /// <summary>
            /// Gets or sets the error code.
            /// </summary>
            public string Code { get; set; }

            /// <summary>
            /// Gets or sets the error Message.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets the error Message.
            /// </summary>
            public string MessageCode { get; set; }
        }
    }
}
