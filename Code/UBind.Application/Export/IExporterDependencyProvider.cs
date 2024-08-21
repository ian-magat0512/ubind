// <copyright file="IExporterDependencyProvider.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Export
{
    using System;
    using MailKit.Net.Smtp;
    using Microsoft.Extensions.Logging;
    using NodaTime;
    using RazorEngine.Templating;
    using UBind.Application.FileHandling;
    using UBind.Application.Person;
    using UBind.Application.Services;
    using UBind.Application.SystemEvents;
    using UBind.Domain;
    using UBind.Domain.Configuration;
    using UBind.Domain.Entities;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.Processing;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services;

    /// <summary>
    /// Container for dependencies required for build exporters.
    /// </summary>
    public interface IExporterDependencyProvider
    {
        /// <summary>
        /// Gets a service for resolving aliases or ids on their respective records, cached.
        /// </summary>
        ICachingResolver CachingResolver { get; }

        /// <summary>
        /// Gets a service for prettifying form data according to a scheme.
        /// </summary>
        IFormDataPrettifier FormDataPrettifier { get; }

        /// <summary>
        /// Gets a file contents loader for loading file contents from OneDrive or the database.
        /// </summary>
        IFileContentsLoader FileContentsLoader { get; }

        /// <summary>
        /// Gets a factory method for creating SMTP clients to use for exports.
        /// </summary>
        Func<SmtpClient> SmtpClientFactory { get; }

        /// <summary>
        /// Gets the Razor engine service to use for exports.
        /// </summary>
        IRazorEngineService RazorEngineService { get; }

        /// <summary>
        /// Gets the Ms Word engine service to use for exports.
        /// </summary>
        IMsWordEngineService MsWordEngineService { get; }

        /// <summary>
        /// Gets the configurations ervice to use.
        /// </summary>
        IConfigurationService ConfigurationService { get; }

        /// <summary>
        /// Gets the quote email service to persist email records to.
        /// </summary>
        IEmailService EmailService { get; }

        /// <summary>
        /// Gets the customer service to persist email records to.
        /// </summary>
        ICustomerService CustomerService { get; }

        /// <summary>
        /// Gets the quote email service to query email records from.
        /// </summary>
        IEmailQueryService EmailQueryService { get; }

        /// <summary>
        /// Gets the document service.
        /// </summary>
        IApplicationDocumentService DocumentService { get; }

        /// <summary>
        /// Gets the quote service.
        /// </summary>
        IApplicationQuoteService QuoteService { get; }

        /// <summary>
        /// Gets the user service.
        /// </summary>
        User.IUserService UserService { get; }

        /// <summary>
        /// Gets the default email configurations for the system.
        /// </summary>
        IEmailInvitationConfiguration EmailConfiguration { get; }

        /// <summary>
        /// Gets the tenant service.
        /// </summary>
        ITenantService TenantService { get; }

        /// <summary>
        /// Gets the organisation service.
        /// </summary>
        IOrganisationService OrganisationService { get; }

        /// <summary>
        /// Gets the product service.
        /// </summary>
        UBind.Application.IProductService ProductService { get; }

        /// <summary>
        /// Gets the The email repository.
        /// </summary>
        IEmailRepository EmailRepository { get; }

        /// <summary>
        /// Gets the system event service.
        /// </summary>
        ISystemEventService SystemEventService { get; }

        /// <summary>
        /// Gets the client for queuing and parameterizing background jobs.
        /// </summary>
        IJobClient JobClient { get; }

        /// <summary>
        /// Gets the clock implementation to get current time.
        /// </summary>
        IClock Clock { get; }

        /// <summary>
        /// Gets the logger to use for logging exports.
        /// </summary>
        ILogger Logger { get; }

        /// <summary>
        /// Gets the quote file attachment repository.
        /// </summary>
        IFileAttachmentRepository<QuoteFileAttachment> FileAttachmentRepository { get; }

        /// <summary>
        /// Gets the person service.
        /// </summary>
        IPersonService PersonService { get; }

        /// <summary>
        /// Gets the mediator.
        /// </summary>
        ICqrsMediator Mediator { get; }

        IFileContentRepository FileContentRepository { get; }
    }
}
