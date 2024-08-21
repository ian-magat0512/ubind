// <copyright file="EmailComposer.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.Messaging
{
    using System.IO;
    using System.Reflection;
    using MimeKit;
    using RazorEngine.Templating;

    /// <summary>
    /// Generic Email Composer.
    /// </summary>
    public class EmailComposer : IEmailComposer
    {
        private readonly IRazorEngineService razorEngineService;
        private string templatePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailComposer"/> class.
        /// </summary>
        /// <param name="razorEngineService">The Razor service.</param>
        public EmailComposer(IRazorEngineService razorEngineService)
        {
            this.razorEngineService = razorEngineService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailComposer"/> class.
        /// </summary>
        /// <param name="from">A string that contains the address of the sender of the message.</param>
        /// <param name="to">A string that contains the address of the recipient of the message.</param>
        /// <param name="cc">The carbon copy email address.</param>
        /// <param name="subject">The generic invitation view model.</param>
        /// <param name="templateName">The Name of the template.</param>
        /// <param name="emailParameter">The razor engine service.</param>
        /// <returns>MailMessage.</returns>
        public MimeMessage ComposeMailMessage(
            string from,
            string to,
            string cc,
            string subject,
            string templateName,
            object emailParameter)
        {
            this.templatePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Templates", templateName);
            if (!File.Exists(this.templatePath))
            {
                return null;
            }

            string template = File.ReadAllText(this.templatePath);
            string result = this.razorEngineService.RunCompile(template, templateName, null, emailParameter);

            var mailMessage = new MimeMessage();
            mailMessage.From.Add(MailboxAddress.Parse(from));
            mailMessage.To.Add(MailboxAddress.Parse(to));
            mailMessage.Cc.Add(MailboxAddress.Parse(cc));
            mailMessage.Subject = subject;
            var builder = new BodyBuilder();
            builder.HtmlBody = result;
            mailMessage.Body = builder.ToMessageBody();
            return mailMessage;
        }
    }
}
