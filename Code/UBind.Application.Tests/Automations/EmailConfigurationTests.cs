// <copyright file="EmailConfigurationTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Automations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Newtonsoft.Json.Linq;
    using UBind.Application.Automation;
    using UBind.Application.Automation.Attachment;
    using UBind.Application.Automation.Data;
    using UBind.Application.Automation.Email;
    using UBind.Application.Automation.Providers;
    using UBind.Application.Automation.Providers.File;
    using UBind.Application.Helpers;
    using UBind.Application.Tests.Automations.Fakes;
    using UBind.Domain;
    using UBind.Domain.Product;
    using Xunit;

    public class EmailConfigurationTests
    {
        /// <summary>
        /// Unit test for generate email should include only the attachments with include condition is true.
        /// </summary>
        /// <returns>Returns.</returns>
        [Fact]
        public async Task GenerateMailMessage_Should_Only_Attach_Files_With_IncludeCondition_Is_True()
        {
            // Arrange
            var json = JObject.Parse(@"{""data"" : ""test""}");
            var mockTextProviderBuilder = new StaticBuilder<Data<string>>() { Value = json.SelectToken("data").ToString() };
            var mockTextProvider = mockTextProviderBuilder.Build(null);

            json = JObject.Parse(@"{""email"" : ""test@email.com""}");
            var mockEmailTextProviderBuilder = new StaticBuilder<Data<string>>() { Value = json.SelectToken("email").ToString() };
            var mockEmailTextProvider = mockEmailTextProviderBuilder.Build(null);

            var fileName1 = "testing.txt";
            var fileInfo = new FileInfo("test.txt", Encoding.UTF8.GetBytes("testing"));
            var mime = ContentTypeHelper.GetMimeTypeForFileExtension(fileName1);
            var fileAttachment1 = new FileAttachmentInfo(fileName1, fileInfo, mime, true);
            var mockFileAttachment1ProviderBuilder = new StaticBuilder<Data<FileAttachmentInfo>>() { Value = fileAttachment1 };
            var mockFileAttachment1Provider = mockFileAttachment1ProviderBuilder.Build(null);

            var fileName2 = "testing2.txt";
            var fileInfo2 = new FileInfo("test.txt", Encoding.UTF8.GetBytes("testing2"));
            var mime2 = ContentTypeHelper.GetMimeTypeForFileExtension(fileName1);
            var fileAttachment2 = new FileAttachmentInfo(fileName2, fileInfo2, mime2, false);
            var mockFileAttachment2ProviderBuilder = new StaticBuilder<Data<FileAttachmentInfo>>() { Value = fileAttachment2 };
            var mockFileAttachment2Provider = mockFileAttachment2ProviderBuilder.Build(null);

            var email = new EmailConfiguration(
                mockEmailTextProvider,
                new List<IProvider<Data<string>>>() { mockEmailTextProvider },
                new List<IProvider<Data<string>>>() { mockEmailTextProvider },
                new List<IProvider<Data<string>>>() { mockEmailTextProvider },
                new List<IProvider<Data<string>>>() { mockEmailTextProvider },
                mockTextProvider,
                mockTextProvider,
                mockTextProvider,
                new List<IProvider<Data<FileAttachmentInfo>>>() { mockFileAttachment1Provider, mockFileAttachment2Provider },
                mockTextProvider,
                new List<IProvider<Data<string>>>() { mockTextProvider },
                new List<IProvider<KeyValuePair<string, IEnumerable<string>>>>());
            var releaseContext = new ReleaseContext(
                Guid.Empty,
                Guid.Empty,
                DeploymentEnvironment.Staging,
                Guid.Empty);
            var automationData = AutomationData.CreateForContextEntities(
                releaseContext,
                Guid.Empty,
                MockAutomationData.GetDefaultServiceProvider());
            var context = new ProviderContext(automationData);
            var emailData = await email.ResolveEmailProperties(context);

            // Act
            var mailMessage = email.ConvertToMailMessage(emailData);

            // Assert
            mailMessage.Should().NotBeNull();
            mailMessage.To.ToString().Should().Contain("test@email.com");
            mailMessage.Bcc.ToString().Should().Contain("test@email.com");
            mailMessage.Cc.ToString().Should().Contain("test@email.com");
            mailMessage.From.ToString().Should().Be("test@email.com");
            mailMessage.TextBody.Should().Be("test");
            mailMessage.HtmlBody.Should().Be("test");

            // mail message should have 1 attachment only because the other attachment has include condition equal to false.
            mailMessage.Attachments.Should().HaveCount(1);
            mailMessage.Attachments.First().ContentDisposition.FileName.Should().Be("testing.txt");
        }
    }
}
