// <copyright file="IUBindDbContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Validation;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Entities;
    using UBind.Domain.Events;
    using UBind.Domain.Models;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Accounting;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Organisation;
    using UBind.Domain.ReadModel.Person.Fields;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReadWriteModel.Email;

    public interface IUBindDbContext : IDisposable, IObjectContextAdapter
    {
        event EventHandler SavingChanges;

        event EventHandler SavedChanges;

        bool LoggingEnabled { get; set; }

        Guid Id { get; }

        Database Database { get; }

        DbChangeTracker ChangeTracker { get; }

        DbContextConfiguration Configuration { get; }

        DbSet<AdditionalPropertyDefinitionReadModel> AdditionalPropertyDefinitions { get; set; }

        DbSet<StreetAddressReadModel> AddressReadModels { get; set; }

        DbSet<ClaimAttachmentReadModel> ClaimAttachment { get; set; }

        DbSet<ClaimFileAttachment> ClaimFileAttachments { get; set; }

        DbSet<ClaimNumber> ClaimNumbers { get; set; }

        DbSet<ClaimReadModel> ClaimReadModels { get; set; }

        DbSet<ClaimVersionReadModel> ClaimVersions { get; set; }

        DbSet<CreditNoteNumber> CreditNoteNumbers { get; set; }

        DbSet<CustomerReadModel> CustomerReadModels { get; set; }

        DbSet<Deployment> Deployments { get; set; }

        DbSet<DeploymentTargetDetails> DeploymentTargetDetails { get; set; }

        DbSet<DeploymentTarget> DeploymentTargets { get; set; }

        DbSet<DevRelease> DevReleases { get; set; }

        DbSet<DocumentFile> DocumentFile { get; set; }

        DbSet<EmailAddressBlockingEvent> EmailAddressBlockingEvents { get; set; }

        DbSet<EmailAddressReadModel> EmailAddressReadModels { get; set; }

        DbSet<Email> Emails { get; set; }

        DbSet<EmailAttachment> EmailAttachments { get; set; }

        DbSet<SystemEmailTemplate> EmailTemplateSettings { get; set; }

        DbSet<EventRecordWithGuidId> EventRecordsWithGuidIds { get; set; }

        DbSet<EventRecordWithStringId> EventRecordsWithStringIds { get; set; }

        DbSet<FileContent> FileContents { get; set; }

        DbSet<InvoiceNumber> InvoiceNumbers { get; set; }

        DbSet<LoginAttemptResult> LoginAttemptResults { get; set; }

        DbSet<MessengerIdReadModel> MessengerReadModels { get; set; }

        DbSet<OrganisationReadModel> OrganisationReadModel { get; set; }

        DbSet<PasswordResetRecord> PasswordResetRecords { get; set; }

        DbSet<PersonReadModel> PersonReadModels { get; set; }

        DbSet<PhoneNumberReadModel> PhoneNumberReadModels { get; set; }

        DbSet<PolicyReadModel> Policies { get; set; }

        DbSet<PolicyNumber> PolicyNumbers { get; set; }

        DbSet<PolicyTransaction> PolicyTransactions { get; set; }

        [Obsolete("Will be removed in ticket UB-9510")]
        DbSet<Portal> Portals { get; set; }

        DbSet<PortalReadModel> PortalReadModels { get; set; }

        DbSet<PortalSettings> PortalSettings { get; set; }

        DbSet<ProductFeatureSetting> ProductFeatureSetting { get; set; }

        DbSet<ProductOrganisationSetting> ProductOrganisationSettings { get; set; }

        DbSet<ProductPortalSetting> ProductPortalSettings { get; set; }

        DbSet<Product> Products { get; set; }

        DbSet<QuoteDocumentReadModel> QuoteDocuments { get; set; }

        DbSet<QuoteEmailReadModel> QuoteEmailReadModel { get; set; }

        DbSet<QuoteEmailSendingReadModel> QuoteEmailSendingReadModel { get; set; }

        DbSet<QuoteFileAttachmentReadModel> QuoteFileAttachmentReadModels { get; set; }

        DbSet<QuoteFileAttachment> QuoteFileAttachments { get; set; }

        DbSet<NewQuoteReadModel> QuoteReadModels { get; set; }

        DbSet<QuoteVersionReadModel> QuoteVersions { get; set; }

        DbSet<Relationship> Relationships { get; set; }

        DbSet<ReleaseDetails> ReleaseDetails { get; set; }

        DbSet<Release> Releases { get; set; }

        DbSet<ReportFile> ReportFiles { get; set; }

        DbSet<ReportReadModel> ReportReadModels { get; set; }

        DbSet<DkimSettings> DkimSettings { get; set; }

        DbSet<PaymentReadModel> PaymentReadModels { get; set; }

        DbSet<RefundReadModel> RefundReadModels { get; set; }

        DbSet<PaymentAllocationReadModel> PaymentAllocationReadModels { get; set; }

        DbSet<RefundAllocationReadModel> RefundAllocationReadModels { get; set; }

        DbSet<Role> Roles { get; set; }

        DbSet<SavedPaymentMethod> SavedPaymentMethods { get; set; }

        DbSet<SettingDetails> SettingDetails { get; set; }

        DbSet<Setting> Settings { get; set; }

        DbSet<SocialMediaIdReadModel> SocialReadModels { get; set; }

        DbSet<StartupJob> StartupJobs { get; set; }

        DbSet<SystemAlert> SystemAlerts { get; set; }

        DbSet<SystemEvent> SystemEvents { get; set; }

        DbSet<Tag> Tags { get; set; }

        DbSet<Tenant> Tenants { get; set; }

        DbSet<TextAdditionalPropertyValueReadModel> TextAdditionalPropertValues { get; set; }

        DbSet<Sms> Sms { get; set; }

        DbSet<TokenSession> TokenSessions { get; set; }

        DbSet<UniqueIdentifier> UniqueIdentifiers { get; set; }

        DbSet<UserEmailReadModel> UserEmailReadModel { get; set; }

        DbSet<UserEmailSentReadModel> UserEmailSentReadModel { get; set; }

        DbSet<UserLoginEmail> UserLoginEmails { get; set; }

        DbSet<UserProfilePicture> UserProfilePictures { get; set; }

        DbSet<UserReadModel> Users { get; set; }

        DbSet<WebsiteAddressReadModel> WebAddressReadModels { get; set; }

        Stack<TransactionScope> TransactionStack { get; set; }

        DbSet<EntityJsonSettings> EntitySettings { get; set; }

        DbSet<DataTableDefinition> DataTableDefinitions { get; set; }

        DbSet<AuthenticationMethodReadModelSummary> AuthenticationMethods { get; set; }

        DbSet<PortalSignInMethodReadModel> PortalSignInMethods { get; set; }

        DbSet<UserLinkedIdentityReadModel> UserLinkedIdentities { get; set; }

        DbSet<OrganisationLinkedIdentityReadModel> OrganisationLinkedIdentities { get; set; }

        DbSet<JwtKey> JwtKeys { get; set; }

        DbSet<StructuredDataAdditionalPropertyValueReadModel> StructuredDataAdditionalPropertyValues { get; set; }

        DbSet<TinyUrl> TinyUrls { get; set; }

        DbSet<AggregateSnapshot> AggregateSnapshots { get; set; }

        void SetTimeout(int? timeout);

        bool HasTransaction();

        DbEntityEntry Entry(object entity);

        DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity)
            where TEntity : class;

        IEnumerable<DbEntityValidationResult> GetValidationErrors();

        int SaveChanges();

        Task<int> SaveChangesAsync();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        DbSet Set(Type entityType);

        DbSet<TEntity> Set<TEntity>()
            where TEntity : class;

        IDbSet<TEntity> GetDbSet<TEntity>()
            where TEntity : class;

        /// <summary>
        /// Gets a list of aggregates that have interacted with the DB context,
        /// so that they can be re-used without needed to reload them from the database.
        /// </summary>
        /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
        /// <returns>A list of the aggregates, or null if none have been touched by the DbContext.</returns>
        HashSet<TAggregate> GetContextAggregates<TAggregate>();

        /// <summary>
        /// Run the sql script on the database.
        /// </summary>
        /// <param name="sqlScript">The script to run against the database.</param>
        /// <param name="timeout">The override command timeout in minutes, set to 0 to wait for the query indefinitely.</param>
        /// <returns>The number of rows affected.</returns>
        int ExecuteSqlScript(string sqlScript, int? timeout = null);

        /// <summary>
        /// Runs a sql query on the database.
        /// </summary>
        /// <param name="sqlScript">The sql query to be ran.</param>
        /// <param name="parameters">The parameters to be used in the query, if any.</param>
        /// <param name="timeout">The override command timeout in minutes, if any.</param>
        /// <returns>The results of the query.</returns>
        /// <remarks>
        /// WARNING: Only use when running a query against the database will result in better performance for the startup job.
        /// In any circumstance, the query should have been tested againast a production backup to confirm that running the query
        /// will not cause any system shutdown.
        /// </remarks>
        List<TElement> ExecuteSqlQuery<TElement>(string sqlScript, List<string> parameters = null, int? timeout = null);

        /// <summary>
        /// Detaches all entities currently being tracked by the Entity Framework context, setting their state to 'Detached'.
        /// </summary>
        void DetachTrackedEntities();

        /// <summary>
        /// Removes all aggregates of the specified type from the context,
        /// allowing for cleanup after the aggregates are no longer needed
        /// or for resetting the context state.
        /// When working with a large collection of aggregates, each aggregate remains in memory until the entire command
        /// is completed and the dbContext is disposed of. Therefore, to ensure the app server's memory limits are not
        /// exceeded, this method can be called after processing each aggregate or batch of aggregates to free up memory.
        /// </summary>
        /// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
        void RemoveContextAggregates<TAggregate>();
    }
}
