// <copyright file="ExporterDependencyProvider.cs" company="uBind">
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

    /// <inheritdoc/>
    public class ExporterDependencyProvider : IExporterDependencyProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExporterDependencyProvider"/> class.
        /// </summary>
        /// <param name="formDataPrettifier">The form data prettifier.</param>
        /// <param name="emailRepository">The email repository.</param>
        /// <param name="fileContentsLoader">A file contents loader.</param>
        /// <param name="razorEngineService">A razor engine service.</param>
        /// <param name="msWordEngineService">A msword engine service.</param>
        /// <param name="smtpClientConfiguration">Configuration settings for SMTP clients.</param>
        /// <param name="configurationService">A configuration service.</param>
        /// <param name="emailConfiguration">The email configuration.</param>
        /// <param name="documentService">A document service.</param>
        /// <param name="quoteService">The quote service.</param>
        /// <param name="userService">The user service.</param>
        /// <param name="emailService">The quote email service to persist emails to.</param>
        /// <param name="emailQueryService">The quote email service to retrieve email records.</param>
        /// <param name="tenantService">the tenant service.</param>
        /// <param name="productService">the product service.</param>
        /// <param name="systemEventService">The system event service.</param>
        /// <param name="organisationService">The organisation service.</param>
        /// <param name="fileAttachmentRepository">The repository for file attachments.</param>
        /// <param name="mediator">The mediator.</param>
        /// <param name="jobClient">A client for queuing and parameterizing background jobs.</param>
        /// <param name="clock">the clock instance to get current time.</param>
        /// <param name="logger">Logger.</param>
        public ExporterDependencyProvider(
            IFormDataPrettifier formDataPrettifier,
            IEmailRepository emailRepository,
            IFileContentsLoader fileContentsLoader,
            IRazorEngineService razorEngineService,
            IMsWordEngineService msWordEngineService,
            ISmtpClientConfiguration smtpClientConfiguration,
            IConfigurationService configurationService,
            IEmailInvitationConfiguration emailConfiguration,
            IApplicationDocumentService documentService,
            IApplicationQuoteService quoteService,
            User.IUserService userService,
            IEmailService emailService,
            ICustomerService customerService,
            IEmailQueryService emailQueryService,
            ITenantService tenantService,
            UBind.Application.IProductService productService,
            ISystemEventService systemEventService,
            IOrganisationService organisationService,
            IFileAttachmentRepository<QuoteFileAttachment> fileAttachmentRepository,
            IPersonService personService,
            ICqrsMediator mediator,
            IJobClient jobClient,
            ICachingResolver cachingResolver,
            IClock clock,
            ILogger<IExporterDependencyProvider> logger,
            IFileContentRepository fileContentRepository)
        {
            this.CachingResolver = cachingResolver;
            this.FormDataPrettifier = formDataPrettifier;
            this.SystemEventService = systemEventService;
            this.EmailService = emailService;
            this.CustomerService = customerService;
            this.EmailQueryService = emailQueryService;
            this.EmailRepository = emailRepository;
            this.ProductService = productService;
            this.TenantService = tenantService;
            this.FileContentsLoader = fileContentsLoader;
            this.RazorEngineService = razorEngineService;
            this.MsWordEngineService = msWordEngineService;
            this.SmtpClientFactory = () => smtpClientConfiguration.GetSmtpClient();
            this.ConfigurationService = configurationService;
            this.EmailConfiguration = emailConfiguration;
            this.DocumentService = documentService;
            this.QuoteService = quoteService;
            this.UserService = userService;
            this.JobClient = jobClient;
            this.Clock = clock;
            this.Logger = logger;
            this.OrganisationService = organisationService;
            this.PersonService = personService;
            this.Mediator = mediator;
            this.FileAttachmentRepository = fileAttachmentRepository;
            this.FileContentRepository = fileContentRepository;
        }

        /// <inheritdoc/>
        public ICachingResolver CachingResolver { get; }

        /// <inheritdoc/>
        public IFormDataPrettifier FormDataPrettifier { get; }

        /// <inheritdoc/>
        public IFileContentsLoader FileContentsLoader { get; }

        /// <inheritdoc/>
        public IRazorEngineService RazorEngineService { get; }

        /// <inheritdoc/>
        public IMsWordEngineService MsWordEngineService { get; }

        /// <inheritdoc/>
        public Func<SmtpClient> SmtpClientFactory { get; }

        /// <inheritdoc/>
        public IConfigurationService ConfigurationService { get; }

        /// <inheritdoc/>
        public ILogger Logger { get; }

        /// <inheritdoc/>
        public IApplicationDocumentService DocumentService { get; }

        /// <inheritdoc/>
        public IApplicationQuoteService QuoteService { get; }

        /// <inheritdoc/>
        public User.IUserService UserService { get; }

        /// <inheritdoc/>
        public IEmailInvitationConfiguration EmailConfiguration { get; }

        /// <inheritdoc/>
        public IEmailService EmailService { get; }

        /// <inheritdoc/>
        public ICustomerService CustomerService { get; }

        /// <inheritdoc/>
        public IEmailQueryService EmailQueryService { get; }

        /// <inheritdoc/>
        public IPersonService PersonService { get; }

        /// <inheritdoc/>
        public UBind.Application.IProductService ProductService { get; }

        /// <inheritdoc/>
        public ITenantService TenantService { get; }

        /// <inheritdoc/>
        public IOrganisationService OrganisationService { get; }

        /// <inheritdoc/>
        public IEmailRepository EmailRepository { get; }

        /// <inheritdoc/>
        public IFileAttachmentRepository<QuoteFileAttachment> FileAttachmentRepository { get; }

        /// <inheritdoc/>
        public IClock Clock { get; }

        /// <inheritdoc/>
        public ISystemEventService SystemEventService { get; }

        /// <inheritdoc/>
        public IJobClient JobClient { get; }

        public ICqrsMediator Mediator { get; }

        public IFileContentRepository FileContentRepository { get; }
    }
}
