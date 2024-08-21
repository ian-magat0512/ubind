// <copyright file="ErrorNotificationService.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Middleware
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using MimeKit;
    using Sentry;
    using UBind.Application.Export;
    using UBind.Application.Services.Email;
    using UBind.Application.Services.Imports;
    using UBind.Application.Services.MachineInformation;
    using UBind.Domain;
    using UBind.Domain.Exceptions;
    using UBind.Web.Configuration;
    using UBind.Web.ViewModels;

    /// <inheritdoc/>
    public class ErrorNotificationService : IErrorNotificationService
    {
        private readonly ErrorNotificationConfiguration configuration;
        private readonly ISmtpClientConfiguration smtpClientConfiguration;
        private readonly IInternalUrlConfiguration internalUrlConfig;
        private readonly IMachineInformationService machineInformation;
        private readonly IHub sentryHub;
        private readonly string fromAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorNotificationService"/> class.
        /// </summary>
        /// <param name="configuration">Configuration from app settings.</param>
        /// <param name="smtpClientConfiguration">Smtp client configuration to use.</param>
        /// <param name="internalUrlConfig">The internal URL configuration.</param>
        /// <param name="machineInformation">The machine information to use.</param>
        public ErrorNotificationService(
            ErrorNotificationConfiguration configuration,
            ISmtpClientConfiguration smtpClientConfiguration,
            IInternalUrlConfiguration internalUrlConfig,
            IMachineInformationService machineInformation,
            IHub sentryHub)
        {
            this.configuration = configuration;
            this.smtpClientConfiguration = smtpClientConfiguration;
            this.internalUrlConfig = internalUrlConfig;
            this.machineInformation = machineInformation;
            this.fromAddress = "no-reply@ubind.io";
            this.sentryHub = sentryHub;
        }

        /// <inheritdoc/>
        public string GetErrorDetails(string tenantAlias, string environment, Exception exception, string additionalContext)
        {
            var exceptionViewModel = new ExceptionViewModel(exception);
            var body = new StringBuilder();
            body.AppendLine($"Tenant:");
            body.AppendLine($"{tenantAlias}");
            body.AppendLine();
            body.AppendLine($"Environment:");
            body.AppendLine($"{environment}");
            body.AppendLine();
            body.Append(this.machineInformation.GetServerDetails());
            body.AppendLine();
            if (!string.IsNullOrEmpty(additionalContext))
            {
                body.AppendLine($"{additionalContext}");
                body.AppendLine();
            }

            body.AppendLine($"Exception:");
            body.AppendLine($"{exceptionViewModel.PrettyPrint()}");
            return body.ToString();
        }

        /// <inheritdoc/>
        [DisplayName("Error notification: Tenant: '{0}', Environment: {1}")]
        public void SendGeneralErrorEmail(string tenantAlias, string environment, Exception exception, string additionalContext = null)
        {
            var subject = "An exception occurred in the uBind application";
            var message = "Please lodge a DEFECT ticket for this so that a developer can investigate and stop the issue from recurring.";
            var body = new StringBuilder();
            body.AppendLine(message);
            body.AppendLine(this.GetErrorDetails(tenantAlias, environment, exception, additionalContext));
            this.SendEmail(subject, body.ToString());
            Enum.TryParse(environment, true, out DeploymentEnvironment env);
            this.CaptureSentryException(exception, env, additionalContext);
        }

        /// <summary>
        /// Send an email notifying of http request that resulted in an unhandled exception.
        /// </summary>
        /// <param name="url">The URL of the request.</param>
        /// <param name="exceptionDetails">Details of the exception.</param>
        [DisplayName("Error notification: URL: '{0}', Exception: {1}")]
        public void SendHttpRequestErrorEmail(string url, ExceptionViewModel exceptionDetails)
        {
            var subject = "An exception occurred in the uBind application";
            var message = "Please lodge a DEFECT ticket for this so that a developer can investigate and stop the issue from recurring.";
            var body = new StringBuilder();
            body.AppendLine(message);
            body.Append(this.machineInformation.GetServerDetails());
            body.AppendLine();
            body.AppendLine($"Request:");
            body.AppendLine($"{url}");
            body.AppendLine();
            body.AppendLine($"Exception:");
            body.AppendLine($"{exceptionDetails.PrettyPrint()}");
            this.SendEmail(subject, body.ToString());
        }

        public void SendSystemNotificationEmail(string subject, string message)
        {
            var body = new StringBuilder();
            body.AppendLine(message);
            body.AppendLine();
            body.Append(this.machineInformation.GetServerDetails());
            this.SendEmail(subject, body.ToString());
        }

        public void SendEmail(string subject, string body)
        {
            using (var message = new MimeMessage())
            {
                message.From.Add(InternetAddress.Parse(this.fromAddress));
                message.Subject = subject;
                this.configuration.EmailRecipients?.ToList().ForEach(email =>
                {
                    message.To.Add(MailboxAddress.Parse(email));
                });

                var builder = new BodyBuilder();
                builder.HtmlBody = body;
                message.Body = builder.ToMessageBody();

                using (var client = this.smtpClientConfiguration.GetSmtpClient())
                {
                    client.Send(message);
                }
            }
        }

        public void CaptureSentryException(Exception exception, DeploymentEnvironment? environment, object? additionalContext = null)
        {
            this.sentryHub.CaptureException(exception, scope =>
            {
                scope.SetTag("Base Url: ", this.internalUrlConfig.BaseApi);
                scope.SetTag("IP Address: ", this.machineInformation.GetIPAddress());
                scope.Environment = scope.Environment = environment?.ToString()?.ToLower();

                if (exception is ErrorException errorException)
                {
                    // Set the tag of the event.
                    scope.SetTag("Code: ", errorException.Error.Code);
                    scope.SetTag("Title: ", errorException.Error.Title);
                    scope.SetExtra("Additional Details", errorException.Error.AdditionalDetails);
                    scope.SetExtra("Additional Data: ", errorException.Error.Data);
                }
                else
                {
                    scope.SetExtra("Additional Data: ", exception.Data);
                }

                if (additionalContext != null)
                {
                    exception.Data.Add("Additional Context", additionalContext);
                    scope.SetExtra("Additional Context", additionalContext);
                }
            });
        }
    }
}
