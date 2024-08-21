// <copyright file="IErrorNotificationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Email
{
    using System;
    using UBind.Domain;

    /// <summary>
    /// Service for sending notification emails when errors occur.
    /// </summary>
    public interface IErrorNotificationService
    {
        /// <summary>
        /// Send a general email for errors that resulted in an unhandled exception.
        /// </summary>
        /// <param name="tenantAlias">The tenant Alias.</param>
        /// <param name="environment">The environment.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="additionalContext">The additional context information for sending a error email.</param>
        void SendGeneralErrorEmail(string tenantAlias, string environment, Exception exception, string additionalContext = null);

        /// <summary>
        /// Send a system notification email.
        /// </summary>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="message">The text message to appear in the email.</param>
        void SendSystemNotificationEmail(string subject, string message);

        string GetErrorDetails(string tenantAlias, string environment, Exception exception, string additionalContext);

        /// <summary>
        /// Sends out normal email.
        /// </summary>
        /// <param name="subject">Email subject.</param>
        /// <param name="body">Email content.</param>
        void SendEmail(string subject, string body);

        void CaptureSentryException(Exception exception, DeploymentEnvironment? environment, object? additionalContext = null);
    }
}
