// <copyright file="HangfireNotificationAttribute.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Filters
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using Hangfire;
    using Hangfire.Common;
    using Hangfire.States;
    using MimeKit;
    using UBind.Application.Export;
    using UBind.Application.Services.Imports;
    using UBind.Application.Services.Messaging;
    using UBind.Domain;
    using UBind.Domain.Extensions;

    /// <summary>
    /// This attribute is used to send email notification when a hangfire job fails.
    /// This will only send an email if the job is running in production environment.
    /// </summary>
    public class HangfireNotificationAttribute : JobFilterAttribute, IElectStateFilter
    {
        private readonly IESystemAlertConfiguration configuration;
        private readonly ISmtpClientConfiguration smtpClientConfiguration;
        private readonly IInternalUrlConfiguration urlConfiguration;
        private ElectStateContext context;
        private string currentDateTime;
        private string jobName;
        private string jobDisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="HangfireNotificationAttribute"/> class.
        /// </summary>
        public HangfireNotificationAttribute(
            IESystemAlertConfiguration configuration,
            ISmtpClientConfiguration smtpClientConfiguration,
            IInternalUrlConfiguration urlConfiguration)
        {
            this.configuration = configuration;
            this.smtpClientConfiguration = smtpClientConfiguration;
            this.urlConfiguration = urlConfiguration;
        }

        /// <summary>
        /// On state applied.
        /// </summary>
        /// <param name="context">The context.</param>
        public void OnStateElection(ElectStateContext context)
        {
            if (context == null ||
                context?.BackgroundJob == null ||
                context?.BackgroundJob?.Job == null ||
                !(context?.CandidateState is FailedState))
            {
                return;
            }

            string jobId = context.BackgroundJob.Id;
            string paramNotified = context.Connection.GetJobParameter(jobId, BackgroundJobParameter.Notified);
            bool.TryParse(paramNotified, out bool isNotified);
            if (isNotified)
            {
                return; // Email already sent on the first try.
            }

            string paramEnvironment = context.Connection.GetJobParameter(jobId, BackgroundJobParameter.Environment);
            DeploymentEnvironment? environment = string.IsNullOrEmpty(paramEnvironment)
                ? DeploymentEnvironment.None
                : paramEnvironment.ToEnumOrNull<DeploymentEnvironment>();
            if (environment == DeploymentEnvironment.Production || environment == DeploymentEnvironment.None)
            {
                this.context = context;
                this.currentDateTime = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                this.ProcessFailingJobNotification();
            }
        }

        private void ProcessFailingJobNotification()
        {
            this.IdentifyAndFormatJobName();
            string subject = $"Background Job Failing: {this.jobName}";
            string message = this.GenerateEmailBody();
            this.SendEmail(subject, message);
            string jobId = this.context.BackgroundJob.Id;
            this.context.Connection.SetJobParameter(jobId, BackgroundJobParameter.Notified, "true");
            this.context.Connection.SetJobParameter(jobId, BackgroundJobParameter.NotifiedTimestamp, this.currentDateTime);
        }

        private void IdentifyAndFormatJobName()
        {
            var jobDisplayNameAttribute = this.context.BackgroundJob.Job.Method.GetCustomAttribute<JobDisplayNameAttribute>()?.DisplayName;
            if (string.IsNullOrEmpty(jobDisplayNameAttribute))
            {
                this.jobName = this.context.BackgroundJob.Job.ToString();
                this.jobDisplayName = string.Empty;
            }
            else
            {
                this.jobName = jobDisplayNameAttribute.Split("|").First().Trim();
                if (jobDisplayNameAttribute.Contains('{'))
                {
                    var args = this.context.BackgroundJob.Job.Args.ToList();
                    this.jobDisplayName = string.Format(jobDisplayNameAttribute, args.ToArray());
                }
                else
                {
                    this.jobDisplayName = jobDisplayNameAttribute;
                }
            }
        }

        private string GenerateEmailBody()
        {
            string jobId = this.context.BackgroundJob.Id;
            string jobLink = $"{this.urlConfiguration.BaseApi}/hangfire/jobs/details/{jobId}";
            string parameters = this.GetJobParameters();

            StringBuilder message = new StringBuilder("<html><body>");
            message.AppendLine($"<html><body>");
            message.AppendLine($"<p>The following background job is failing: <b>{this.jobName}</b>. Please have someone investigate and fix this ASAP.</p>");
            message.AppendLine($"<p>{jobLink}<p>");
            message.AppendLine($"Job Id: {jobId}<br>");
            if (!string.IsNullOrEmpty(this.jobDisplayName))
            {
                message.AppendLine($"Job Display Name: {this.jobDisplayName}<br>");
            }
            message.AppendLine($"Job Type: {this.context.BackgroundJob.Job.Type.Name}<br>");
            message.AppendLine($"Job Method: {this.context.BackgroundJob.Job.Method.Name}<br>");
            message.AppendLine($"Job Created Date: {this.context.BackgroundJob.CreatedAt.ToLocalTime()}<br><br>");
            if (!string.IsNullOrEmpty(parameters))
            {
                message.AppendLine($"Job Parameters: <br> {parameters}");
            }

            message.AppendLine("<br>Other Details: <br>");
            message.AppendLine($"Server: {Environment.MachineName} <br>");
            message.AppendLine($"Date: {this.currentDateTime} <br><br>");
            message.AppendLine($"<b>Reason</b>: {this.context.CandidateState.Reason} <br><br>");
            if (this.context.CandidateState is FailedState failedState)
            {
                message.AppendLine($"<b>Exception Message</b>: {failedState.Exception.Message} <br><br>");
                message.AppendLine($"<b>Stack Trace</b>: {failedState.Exception.StackTrace} <br><br>");
            }
            message.AppendLine($"<b>Arguments</b>: {SerializationHelper.Serialize(this.context.BackgroundJob.Job.Args)}");
            message.AppendLine($"</body></html>");

            return message.ToString();
        }

        private string GetJobParameters()
        {
            string jobId = this.context.BackgroundJob.Id;
            var properties = JobStorage.Current.GetMonitoringApi().JobDetails(jobId).Properties;
            string parameters = string.Empty;
            string[] excludedKeys = { "Fetched", "CurrentCulture", "CurrentUICulture" };
            foreach (var item in properties)
            {
                if (excludedKeys.Contains(item.Key))
                {
                    continue;
                }
                parameters += $"<li>{item.Key}: {item.Value}</li>";
            }
            return string.IsNullOrEmpty(parameters) ? string.Empty : $"<ul>{parameters}</ul>";
        }

        private void SendEmail(string subject, string body)
        {
            string fromAddress = this.configuration.From;
            string toAddress = this.configuration.To;
            string ccAddress = this.configuration.CC;

            using (var message = new MimeMessage())
            {
                var builder = new BodyBuilder();
                builder.HtmlBody = body;
                message.Body = builder.ToMessageBody();
                message.From.Add(InternetAddress.Parse(fromAddress));
                message.To.Add(MailboxAddress.Parse(toAddress));
                message.Cc.Add(MailboxAddress.Parse(ccAddress));
                message.Subject = subject;

                using (var client = this.smtpClientConfiguration.GetSmtpClient())
                {
                    client.Send(message);
                }
            }
        }
    }
}
