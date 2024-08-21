// <copyright file="UBindDbContext.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using UBind.Domain;
    using UBind.Domain.Aggregates.User;
    using UBind.Domain.Entities;
    using UBind.Domain.Events;
    using UBind.Domain.Extensions;
    using UBind.Domain.Models;
    using UBind.Domain.NumberGenerators;
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
    using UBind.Domain.Repositories;

    /// <summary>
    /// EF database context for application.
    /// </summary>
    public class UBindDbContext : DbContext, IObjectNumberDbContext, IUBindDbContext
    {
        /// <summary>
        ///  Name used by the index to enforce index on tag entity id.
        /// </summary>
        internal static readonly string TagEntityIndex = "AK_TagEntityIndex";

        /// <summary>
        ///  Name used by the index to enforce index on tag entity type and its hierarchy.
        /// </summary>
        internal static readonly string TagEntityTypeIndex = "AK_TagEntityTypeIndex";

        /// <summary>
        ///  Name used by the index to enforce index on data table definition entity type and id.
        /// </summary>
        internal static readonly string DataTableDefinitionEntityTypeIndex = "AK_DataTableDefinitionEntityTypeIndex";

        /// <summary>
        ///  Name used by the index to enforce index on data table tenant id.
        /// </summary>
        internal static readonly string DataTableDefinitionTenantIndex = "AK_DataTableDefinitionTenantIndex";

        /// <summary>
        ///  Name used by the index to enforce index on From entity relationships.
        /// </summary>
        internal static readonly string RelationshipFromEntityIndex = "AK_RelationshipFromEntityIndex";

        /// <summary>
        /// Name used by the index to enforce index on To entity relationships.
        /// </summary>
        internal static readonly string RelationshipToEntityIndex = "AK_RelationshipToEntityIndex";

        /// <summary>
        /// Name used by the index to enforce index on From entity relationships and its type hierarchy.
        /// </summary>
        internal static readonly string RelationshipFromTypeIndex = "AK_RelationshipFromTypeIndex";

        /// <summary>
        /// Name used by the index to enforce index on To entity relationships and its type hierarchy.
        /// </summary>
        internal static readonly string RelationshipToTypeIndex = "AK_RelationshipToTypeIndex";

        /// <summary>
        /// Name used for the index enforcing unique account emails per tenant.
        /// </summary>
        internal static readonly string UserTenantOrganisationAndEmailIndex = "AK_UserTenantOrganisationAndEmailIndex";

        /// <summary>
        /// Name used for the index enforcing unique unique identifiers.
        /// </summary>
        internal static readonly string UniqueIdentifierTypeProductEnvironmentAndIdentifierIndex = "AK_UniqueIdentifierTypeTenantProductEnvironmentAndIdentifierIndex";

        /// <summary>
        /// Name used for the index enforcing unique release numbers.
        /// </summary>
        internal static readonly string ReleaseProductAndNumberIndex = "AK_ReleaseProductAndNumber";

        /// <summary>
        /// Name used for the index enforcing unique policy numbers per product and environment.
        /// </summary>
        internal static readonly string PolicyNumberProductEnvironmentAndNumberIndex = "AK_PolicyNumberTenantProductEnvironmentAndNumberIndex";

        /// <summary>
        /// Name used for the index enforcing unique invoice numbers per product and environment.
        /// </summary>
        internal static readonly string InvoiceNumberProductEnvironmentAndNumberIndex = "AK_InvoiceNumberTenantProductEnvironmentAndNumberIndex";

        /// <summary>
        /// Name used for the index enforcing unique payment reference numbers per product and environment.
        /// </summary>
        internal static readonly string PaymentReferenceNumberProductEnvironmentAndNumberIndex = "AK_PaymentReferenceNumberTenantProductEnvironmentAndNumberIndex";

        /// <summary>
        /// Name used for the index enforcing unique claim numbers per product and environment.
        /// </summary>
        internal static readonly string ClaimNumberForProductEnvironmentAndNumberIndex = "AK_ClaimNumberTenantProductEnvironmentAndNumberIndex";

        /// <summary>
        /// Name used for the index tenant, product and environment for policies.
        /// </summary>
        internal static readonly string IX_Policies_TenantId_ProductId_Environment = "IX_Policies_TenantId_ProductId_Environment";

        /// <summary>
        /// Name used for the index PolicyId for policy transactions.
        /// </summary>
        internal static readonly string IX_PolicyTransactions_PolicyId = "IX_PolicyTransactions_PolicyId";

        /// <summary>
        /// Name used for the index QuoteId for policies.
        /// </summary>
        internal static readonly string IX_Policies_QuoteId = "IX_Policies_QuoteId";

        /// <summary>
        /// Name used for the index Id,TenantId for Products.
        /// </summary>
        internal static readonly string IX_Products_TenantId = "IX_Products_TenantId";

        /// <summary>
        /// Name used for the index Organisation for Policies.
        /// </summary>
        internal static readonly string IX_Policies_OrganisationId = "IX_Policies_OrganisationId";

        /// <summary>
        /// Name used for the index CreatedTicksSinceEpoch for PolicyTransactions.
        /// </summary>
        internal static readonly string IX_PolicyTransactions_CreatedTicksSinceEpoch = "IX_PolicyTransactions_CreatedTicksSinceEpoch";

        /// <summary>
        /// A map of HashSets by type of aggregate. This is an in-memory store of aggregates which
        /// have modified the db context in it's current scope.
        /// </summary>
        private Dictionary<Type, System.Collections.IEnumerable> contextAggregatesMap
            = new Dictionary<Type, System.Collections.IEnumerable>();

        private bool loggingEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="UBindDbContext"/> class.
        /// Using this constructor without a connection string would cause the connection to
        /// default to the (localdb)\\mssqllocaldb SqlExpress instance.
        /// </summary>
        public UBindDbContext()
        {
            // Uncomment the following line to see all of the SQL generated in the output console during debug mode.
            // Don't forget to comment it out again!
            // this.LoggingEnabled = true;
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UBindDbContext"/> class with a particular connection string.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="readOnly">Whether this context will be used only for read queries.</param>
        public UBindDbContext(string connectionString, bool readOnly = false)
            : base(connectionString + (readOnly ? ";ApplicationIntent=ReadOnly" : string.Empty))
        {
            if (!connectionString.Contains('='))
            {
                throw new InvalidOperationException("An attempt was made to instantiate UBindDbContext using a "
                    + "connection string name. Please use the actual connection string instead.");
            }

            // Uncomment the following line to see all of the SQL generated in the output console during debug mode.
            // Don't forget to comment it out again!
            // this.LoggingEnabled = true;
            this.Id = Guid.NewGuid();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UBindDbContext"/> class with a particular connection string.
        /// </summary>
        /// <param name="nameOrConnectionString">The connection string or name of connection string in settings.</param>
        /// <param name="timeoutSeconds">The command timeout value (seconds).</param>
        /// <param name="readOnly">Whether this context will be used only for read queries.</param>
        public UBindDbContext(string nameOrConnectionString, int timeoutSeconds, bool readOnly = false)
            : this(nameOrConnectionString, readOnly)
        {
            this.Database.CommandTimeout = timeoutSeconds;
        }

        /// <summary>
        /// Raised when changes are about to saved.
        /// </summary>
        public event EventHandler SavingChanges;

        /// <summary>
        /// Raised when changes have been saved.
        /// </summary>
        public event EventHandler SavedChanges;

        public bool LoggingEnabled
        {
            get => this.loggingEnabled;
            set
            {
                this.loggingEnabled = value;
                if (value)
                {
                    this.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                }
                else
                {
                    this.Database.Log = null;
                }
            }
        }

        /// <summary>
        /// Gets the unique identifier for this instance of the DbContext.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Gets or sets the set of user login emails.
        /// </summary>
        /// <remarks>Used to enforce uniqueness of login emails.</remarks>
        public DbSet<UserLoginEmail> UserLoginEmails { get; set; }

        /// <summary>
        /// Gets or sets the set of login attempts.
        /// </summary>
        public DbSet<LoginAttemptResult> LoginAttemptResults { get; set; }

        /// <summary>
        /// Gets or sets the set of password reset attempts.
        /// </summary>
        public DbSet<PasswordResetRecord> PasswordResetRecords { get; set; }

        /// <summary>
        /// Gets or sets the set of email blocking events.
        /// </summary>
        public DbSet<EmailAddressBlockingEvent> EmailAddressBlockingEvents { get; set; }

        /// <summary>
        /// Gets or sets the Guid-based event record set.
        /// </summary>
        public DbSet<EventRecordWithGuidId> EventRecordsWithGuidIds { get; set; }

        /// <summary>
        /// Gets or sets the string-based event record set.
        /// </summary>
        public DbSet<EventRecordWithStringId> EventRecordsWithStringIds { get; set; }

        /// <summary>
        /// Gets or sets the set of user read models.
        /// </summary>
        public DbSet<UserReadModel> Users { get; set; }

        /// <summary>
        /// Gets or sets the set of user profile pictures.
        /// </summary>
        public DbSet<UserProfilePicture> UserProfilePictures { get; set; }

        /// <summary>
        /// Gets or sets products set.
        /// </summary>
        public virtual DbSet<Product> Products { get; set; }

        /// <summary>
        /// Gets or sets releases set.
        /// </summary>
        public virtual DbSet<Release> Releases { get; set; }

        /// <summary>
        /// Gets or sets releases set.
        /// </summary>
        public DbSet<ReleaseDetails> ReleaseDetails { get; set; }

        /// <summary>
        /// Gets or sets development releases set.
        /// </summary>
        public DbSet<DevRelease> DevReleases { get; set; }

        /// <summary>
        /// Gets or sets deployments set.
        /// </summary>
        public virtual DbSet<Deployment> Deployments { get; set; }

        /// <summary>
        /// Gets or sets the assets dataset.
        /// </summary>
        public DbSet<Asset> Assets { get; set; }

        /// <summary>
        /// Gets or sets the set of policy number records.
        /// </summary>
        public DbSet<PolicyNumber> PolicyNumbers { get; set; }

        /// <summary>
        /// Gets or sets the set of invoice number records.
        /// </summary>
        public DbSet<InvoiceNumber> InvoiceNumbers { get; set; }

        /// <summary>
        /// Gets or sets the set of credit note number records.
        /// </summary>
        public DbSet<CreditNoteNumber> CreditNoteNumbers { get; set; }

        /////// <summary>
        /////// Gets or sets the set of invoice records.
        /////// </summary>
        ////public DbSet<Invoice> Invoices { get; set; }

        /// <summary>
        /// Gets or sets the set of claim number records.
        /// </summary>
        public DbSet<ClaimNumber> ClaimNumbers { get; set; }

        /// <summary>
        /// Gets or sets the set of OLD quote read models.
        /// </summary>
        public virtual DbSet<NewQuoteReadModel> QuoteReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of quote read models.
        /// </summary>
        public virtual DbSet<PolicyReadModel> Policies { get; set; }

        /// <summary>
        /// Gets or sets the product feature.
        /// </summary>
        public virtual DbSet<ProductFeatureSetting> ProductFeatureSetting { get; set; }

        /// <summary>
        /// Gets or sets the set of policy transactions.
        /// </summary>
        public DbSet<PolicyTransaction> PolicyTransactions { get; set; }

        /// <summary>
        /// Gets or sets the set of quote version read models.
        /// </summary>
        public DbSet<QuoteVersionReadModel> QuoteVersions { get; set; }

        /// <summary>
        /// Gets or sets the set of quote document read models.
        /// </summary>
        public DbSet<QuoteDocumentReadModel> QuoteDocuments { get; set; }

        /// <summary>
        /// Gets or sets the set of quote document read models.
        /// </summary>
        public DbSet<ClaimAttachmentReadModel> ClaimAttachment { get; set; }

        /// <summary>
        /// Gets or sets the set of claim read models.
        /// </summary>
        public DbSet<ClaimReadModel> ClaimReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of claim version read models.
        /// </summary>
        public DbSet<ClaimVersionReadModel> ClaimVersions { get; set; }

        /// <summary>
        /// Gets or sets the set of customer records.
        /// </summary>
        public virtual DbSet<CustomerReadModel> CustomerReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of person records.
        /// </summary>
        public DbSet<PersonReadModel> PersonReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of seeds that have been used to generate quote numbers.
        /// </summary>
        public DbSet<ReferenceNumberSequence> ReferenceNumberSequences { get; set; }

        /// <summary>
        /// Gets or sets the set of unique identifier records.
        /// </summary>
        public DbSet<UniqueIdentifier> UniqueIdentifiers { get; set; }

        /// <summary>
        /// Gets or sets the set of tenant records.
        /// </summary>
        public DbSet<Tenant> Tenants { get; set; }

        /// <summary>
        /// Gets or sets the set of organisation records.
        /// </summary>
        public virtual DbSet<OrganisationReadModel> OrganisationReadModel { get; set; }

        /// <summary>
        /// Gets or sets the set of portal records.
        /// </summary>
        public DbSet<Portal> Portals { get; set; }

        public DbSet<PortalReadModel> PortalReadModels { get; set; }

        /// <summary>
        /// Gets or sets the DeploymentTargets for portal records.
        /// </summary>
        public DbSet<DeploymentTarget> DeploymentTargets { get; set; }

        /// <summary>
        /// Gets or Sets the details of a referrer url record.
        /// </summary>
        public DbSet<DeploymentTargetDetails> DeploymentTargetDetails { get; set; }

        /// <summary>
        /// Gets or sets the set of setting records.
        /// </summary>
        public DbSet<Setting> Settings { get; set; }

        /// <summary>
        /// Gets or sets the set of setting details records.
        /// </summary>
        public DbSet<SettingDetails> SettingDetails { get; set; }

        /// <summary>
        /// Gets or sets the set of portal setting records.
        /// </summary>
        public DbSet<PortalSettings> PortalSettings { get; set; }

        /// <summary>
        /// Gets or sets the product portal settings records.
        /// </summary>
        public DbSet<ProductPortalSetting> ProductPortalSettings { get; set; }

        /// <summary>
        /// Gets or sets the product organisation setting records.
        /// </summary>
        public DbSet<ProductOrganisationSetting> ProductOrganisationSettings { get; set; }

        /// <summary>
        /// Gets or sets the account creation from landing page settings.
        /// </summary>
        public DbSet<EntityJsonSettings> EntitySettings { get; set; }

        /// <summary>
        /// Gets or sets the set of setting records.
        /// </summary>
        public DbSet<SystemEmailTemplate> EmailTemplateSettings { get; set; }

        /// <summary>
        /// Gets or sets the set of system alert records.
        /// </summary>
        public DbSet<SystemAlert> SystemAlerts { get; set; }

        /// <summary>
        /// Gets or sets the set of Claim file attachment.
        /// </summary>
        public DbSet<ClaimFileAttachment> ClaimFileAttachments { get; set; }

        /// <summary>
        /// Gets or sets the set of Quote file attachment.
        /// </summary>
        public DbSet<QuoteFileAttachment> QuoteFileAttachments { get; set; }

        /// <summary>
        /// Gets or sets the set of Quote file attachment read models.
        /// </summary>
        public DbSet<QuoteFileAttachmentReadModel> QuoteFileAttachmentReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of file contents.
        /// </summary>
        public DbSet<FileContent> FileContents { get; set; }

        /// <summary>
        /// Gets or sets the document file.
        /// </summary>
        public DbSet<DocumentFile> DocumentFile { get; set; }

        /// <summary>
        /// Gets or sets the set of email models.
        /// </summary>
        public virtual DbSet<Email> Emails { get; set; }

        /// <summary>
        /// Gets or sets the set of email attachment models.
        /// </summary>
        public virtual DbSet<EmailAttachment> EmailAttachments { get; set; }

        /// <summary>
        /// Gets or sets the set of email associations to associate multiple entities.
        /// </summary>
        public virtual DbSet<Relationship> Relationships { get; set; }

        /// <summary>
        /// Gets or sets the set of tags to aassociate to any selected entity.
        /// </summary>
        public virtual DbSet<Tag> Tags { get; set; }

        /// <summary>
        /// Gets or sets the set of quote email read models.
        /// </summary>
        public DbSet<QuoteEmailReadModel> QuoteEmailReadModel { get; set; }

        /// <summary>
        /// Gets or sets the set of roles.
        /// </summary>
        public DbSet<Role> Roles { get; set; }

        /// <summary>
        /// Gets or sets the set of user email read models.
        /// </summary>
        public DbSet<UserEmailReadModel> UserEmailReadModel { get; set; }

        /// <summary>
        /// Gets or sets the set the user email sent model.
        /// </summary>
        public DbSet<QuoteEmailSendingReadModel> QuoteEmailSendingReadModel { get; set; }

        /// <summary>
        /// Gets or sets the set the user email sent model.
        /// </summary>
        public DbSet<UserEmailSentReadModel> UserEmailSentReadModel { get; set; }

        /// <summary>
        /// Gets or sets the set system events persistence.
        /// </summary>
        public DbSet<SystemEvent> SystemEvents { get; set; }

        /////// <summary>
        /////// Gets or sets the set of user roles.
        /////// </summary>
        ////public DbSet<UserRoles> UserRoles { get; set; }

        ///// <summary>
        ///// Gets or sets the set of role permissions.
        ///// </summary>
        // public DbSet<RolePermission> RolePermissions { get; set; }

        /// <summary>
        /// Gets or sets token sessions.
        /// </summary>
        public DbSet<TokenSession> TokenSessions { get; set; }

        /// <summary>
        /// Gets or sets the set of report records.
        /// </summary>
        public DbSet<ReportReadModel> ReportReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of report records.
        /// </summary>
        public DbSet<DkimSettings> DkimSettings { get; set; }

        /// <summary>
        /// Gets or sets the payment records.
        /// </summary>
        public DbSet<PaymentReadModel> PaymentReadModels { get; set; }

        /// <summary>
        /// Gets or sets the refund records.
        /// </summary>
        public DbSet<RefundReadModel> RefundReadModels { get; set; }

        /// <summary>
        /// Gets or sets the payment allocations.
        /// </summary>
        public DbSet<PaymentAllocationReadModel> PaymentAllocationReadModels { get; set; }

        /// <summary>
        /// Gets or sets the refund allocations.
        /// </summary>
        public DbSet<RefundAllocationReadModel> RefundAllocationReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of report records.
        /// </summary>
        public DbSet<ReportFile> ReportFiles { get; set; }

        /// <summary>
        /// Gets or sets the set of email address records.
        /// </summary>
        public DbSet<EmailAddressReadModel> EmailAddressReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of phone number records.
        /// </summary>
        public DbSet<PhoneNumberReadModel> PhoneNumberReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of address records.
        /// </summary>
        public DbSet<StreetAddressReadModel> AddressReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of web address records.
        /// </summary>
        public DbSet<WebsiteAddressReadModel> WebAddressReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of messenger records.
        /// </summary>
        public DbSet<MessengerIdReadModel> MessengerReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of social records.
        /// </summary>
        public DbSet<SocialMediaIdReadModel> SocialReadModels { get; set; }

        /// <summary>
        /// Gets or sets the set of startup jobs.
        /// </summary>
        /// <remarks>Used to enforce uniqueness of login emails.</remarks>
        public DbSet<StartupJob> StartupJobs { get; set; }

        /// <summary>
        /// Gets or sets the set of additional property definition read model.
        /// </summary>
        public DbSet<AdditionalPropertyDefinitionReadModel> AdditionalPropertyDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the text additional property values for entities.
        /// </summary>
        public DbSet<TextAdditionalPropertyValueReadModel> TextAdditionalPropertValues { get; set; }

        /// <summary>
        /// Gets or sets the data table definition entities.
        /// </summary>
        public DbSet<Domain.Entities.DataTableDefinition> DataTableDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the sms.
        /// </summary>
        public DbSet<Sms> Sms { get; set; }

        /// <summary>
        /// Gets or sets the saved payment methods for customers.
        /// </summary>
        public DbSet<SavedPaymentMethod> SavedPaymentMethods { get; set; }

        public DbSet<AuthenticationMethodReadModelSummary> AuthenticationMethods { get; set; }

        public DbSet<PortalSignInMethodReadModel> PortalSignInMethods { get; set; }

        public DbSet<UserLinkedIdentityReadModel> UserLinkedIdentities { get; set; }

        public DbSet<OrganisationLinkedIdentityReadModel> OrganisationLinkedIdentities { get; set; }

        public DbSet<JwtKey> JwtKeys { get; set; }

        /// <summary>
        /// Gets or sets the structured data additional property values for entities.
        /// </summary>
        public DbSet<StructuredDataAdditionalPropertyValueReadModel> StructuredDataAdditionalPropertyValues { get; set; }

        public DbSet<AggregateSnapshot>? AggregateSnapshots { get; set; }
        public DbSet<TinyUrl> TinyUrls { get; set; }

        public Stack<TransactionScope> TransactionStack { get; set; } = new Stack<TransactionScope>();

        public IDbSet<TEntity> GetDbSet<TEntity>()
            where TEntity : class
        {
            return this.Set<TEntity>();
        }

        public bool HasTransaction()
        {
            return this.TransactionStack.Any();
        }

        /// <inheritdoc/>
        public override int SaveChanges()
        {
            this.OnSavingChanges();
            var result = base.SaveChanges();
            this.OnSavedChanges();
            return result;
        }

        /// <inheritdoc/>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.OnSavingChanges();
            int result = await base.SaveChangesAsync(cancellationToken);
            this.OnSavedChanges();
            return result;
        }

        /// <inheritdoc/>
        public override async Task<int> SaveChangesAsync()
        {
            this.OnSavingChanges();
            int result = await base.SaveChangesAsync();
            this.OnSavedChanges();
            return result;
        }

        /// <inheritdoc/>
        public HashSet<TAggregate> GetContextAggregates<TAggregate>()
        {
            if (this.contextAggregatesMap.TryGetValue(typeof(TAggregate), out System.Collections.IEnumerable aggregates))
            {
                return (HashSet<TAggregate>)aggregates;
            }

            HashSet<TAggregate> newAggregates = new HashSet<TAggregate>();
            this.contextAggregatesMap.Add(typeof(TAggregate), newAggregates);
            return newAggregates;
        }

        // Method to clear the cache for a specific type
        public void RemoveContextAggregates<TAggregate>()
        {
            this.contextAggregatesMap.Remove(typeof(TAggregate));
        }

        public void SetTimeout(int? timeout)
        {
            this.Database.CommandTimeout = timeout;
        }

        public int ExecuteSqlScript(string sqlScript, int? timeout = null)
        {
            if (sqlScript.IsNullOrWhitespace())
            {
                return -1;
            }

            var previousTimeout = this.Database.CommandTimeout;

            try
            {
                if (timeout != null)
                {
                    this.Database.CommandTimeout = timeout;
                }

                return this.Database.ExecuteSqlCommand(sqlScript);
            }
            finally
            {
                this.Database.CommandTimeout = previousTimeout;
            }
        }

        public List<TElement> ExecuteSqlQuery<TElement>(string sqlScript, List<string> parameters = null, int? timeout = null)
        {
            if (sqlScript.IsNullOrWhitespace())
            {
                return null;
            }

            var previousTimeout = this.Database.CommandTimeout;
            try
            {
                if (timeout.HasValue)
                {
                    this.Database.CommandTimeout = timeout.Value;
                }

                if (parameters != null)
                {
                    return this.Database.SqlQuery<TElement>(sqlScript, parameters).ToList<TElement>();
                }
                else
                {
                    return this.Database.SqlQuery<TElement>(sqlScript).ToList<TElement>();
                }
            }
            finally
            {
                this.Database.CommandTimeout = previousTimeout;
            }
        }

        public void DetachTrackedEntities()
        {
            var trackedEntities = this.ChangeTracker.Entries().ToList();
            foreach (var entry in trackedEntities)
            {
                entry.State = EntityState.Detached;
            }
        }

        /// <summary>
        /// Raises the SavingChanges event.
        /// </summary>
        protected void OnSavingChanges()
        {
            this.SavingChanges?.Invoke(this, new EventArgs());
        }

        protected void OnSavedChanges()
        {
            this.SavedChanges?.Invoke(this, new EventArgs());
        }

        /// <inheritdoc/>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            this.ConfigureCustomTableNames(modelBuilder);

            this.ConfigureCustomColumnTypes(modelBuilder);

            this.ConfigureCustomPrimaryKeys(modelBuilder);

            this.ConfigureNavigationProperties(modelBuilder);

            this.PersistPrivateProperties(modelBuilder);

            this.ApplyUniqueIndexes(modelBuilder);

            this.ConfigureComplexTypes(modelBuilder);

            this.ConfigureClassHierarchies(modelBuilder);
        }

        private void ConfigureClassHierarchies(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthenticationMethodReadModelSummary>().ToTable("AuthenticationMethods");
            modelBuilder.Entity<SamlAuthenticationMethodReadModel>().ToTable("SamlAuthenticationMethods");
            modelBuilder.Entity<LocalAccountAuthenticationMethodReadModel>().ToTable("LocalAccountAuthenticationMethods");
        }

        private void ConfigureComplexTypes(DbModelBuilder modelBuilder)
        {
            modelBuilder.ComplexType<QuoteExpirySettings>();
        }

        private void ConfigureCustomTableNames(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NewQuoteReadModel>()
                .ToTable("Quotes");
            modelBuilder.Entity<AdditionalPropertyDefinitionReadModel>()
               .ToTable("AdditionalPropertyDefinitions");
        }

        private void ConfigureCustomColumnTypes(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QuoteEmailReadModel>()
                .Property(qd => qd.EmailId)
                .HasColumnName("QuoteEmailModelId"); // For Migration Purposes
        }

        private void ConfigureCustomPrimaryKeys(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasKey(p => new { p.TenantId, p.Id });

            modelBuilder.Entity<UserReadModel>()
                .HasKey(q => q.Id);

            modelBuilder.Entity<PortalSignInMethodReadModel>()
                .HasKey(p => new { p.PortalId, p.Id });

            modelBuilder.Entity<UserLinkedIdentityReadModel>()
                .HasKey(u => new { u.UserId, u.AuthenticationMethodId });

            modelBuilder.Entity<OrganisationLinkedIdentityReadModel>()
                .HasKey(u => new { u.OrganisationId, u.AuthenticationMethodId });
        }

        private void ConfigureNavigationProperties(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasMany(p => p.DetailsCollection)
                .WithRequired();

            modelBuilder.Entity<Deployment>()
                .HasOptional(d => d.Release);

            modelBuilder.Entity<UniqueIdentifier>()
                .HasOptional(ui => ui.Consumption)
                .WithRequired(c => c.UniqueIdenitfier);

            modelBuilder.Entity<Tenant>()
                .HasMany(p => p.DetailsCollection)
                .WithRequired();

            modelBuilder.Entity<Portal>()
                .HasMany(p => p.PortalDetailsCollection)
                .WithRequired();

            modelBuilder.Entity<Setting>()
                .HasMany(s => s.DetailsCollection)
                .WithRequired();

            modelBuilder.Entity<PortalSettings>()
                .HasMany(s => s.DetailCollection)
                .WithRequired();

            modelBuilder.Entity<SystemEmailTemplate>();

            modelBuilder.Entity<ReportReadModel>()
                .HasMany(r => r.Products)
                .WithMany(p => p.Reports)
                .Map(m =>
                {
                    m.ToTable("ReportProducts");
                    m.MapLeftKey("ReportID");
                    m.MapRightKey("ProductID", "TenantId");
                });

            modelBuilder.Entity<PersonReadModel>()
                .HasMany(s => s.EmailAddresses);

            modelBuilder.Entity<CustomerReadModel>()
                .HasMany(c => c.People);

            modelBuilder.Entity<UserReadModel>()
                .HasMany(u => u.LinkedIdentities)
                .WithRequired()
                .HasForeignKey(uli => uli.UserId);
        }

        private void PersistPrivateProperties(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EventRecordWithStringId>()
                .Property(EventRecordWithStringId.EventJsonExpression);

            modelBuilder.Entity<EventRecordWithStringId>()
                .Property(EventRecordWithStringId.TicksSinceEpochExpression);

            modelBuilder.Entity<EventRecordWithGuidId>()
                .Property(EventRecordWithGuidId.EventJsonExpression);

            modelBuilder.Entity<EventRecordWithGuidId>()
                .Property(EventRecordWithGuidId.TicksSinceEpochExpression);

            modelBuilder.Entity<Role>()
                .Property(Role.PropertyListExpression);

            modelBuilder.Entity<EntityJsonSettings>()
                .Property(EntityJsonSettings.SettingsExpression);
        }

        private void ApplyUniqueIndexes(DbModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Release>()
                .Property(r => r.TenantId)
                .HasColumnType("UNIQUEIDENTIFIER")
                .HasUniqueIndexAnnotation(ReleaseProductAndNumberIndex, 0);
            modelBuilder
                .Entity<Release>()
                .Property(r => r.ProductId)
                .HasColumnType("UNIQUEIDENTIFIER")
                .HasUniqueIndexAnnotation(ReleaseProductAndNumberIndex, 1);
            modelBuilder
                .Entity<Release>()
                .Property(r => r.Number)
                .HasUniqueIndexAnnotation(ReleaseProductAndNumberIndex, 2);
            modelBuilder
             .Entity<Release>()
             .Property(r => r.MinorNumber)
             .HasUniqueIndexAnnotation(ReleaseProductAndNumberIndex, 3);

            modelBuilder
                .Entity<UserLoginEmail>()
                .Property(e => e.TenantId)
                .HasColumnType("UNIQUEIDENTIFIER")
                .HasUniqueIndexAnnotation(UserTenantOrganisationAndEmailIndex, 0);
            modelBuilder
                .Entity<UserLoginEmail>()
                .Property(e => e.LoginEmail)
                .HasColumnType("VARCHAR")
                .HasMaxLength(255)
                .HasUniqueIndexAnnotation(UserTenantOrganisationAndEmailIndex, 1);
            modelBuilder
                .Entity<UserLoginEmail>()
                .Property(e => e.OrganisationId)
                .HasColumnType("UNIQUEIDENTIFIER")
                .HasUniqueIndexAnnotation(UserTenantOrganisationAndEmailIndex, 2);

            modelBuilder
                .Entity<PolicyNumber>()
                .Property(pn => pn.TenantId)
                .HasColumnType("UNIQUEIDENTIFIER")
                .HasUniqueIndexAnnotation(PolicyNumberProductEnvironmentAndNumberIndex, 0);
            modelBuilder
                .Entity<PolicyNumber>()
                .Property(pn => pn.ProductId)
                .HasColumnType("UNIQUEIDENTIFIER")
                .HasUniqueIndexAnnotation(PolicyNumberProductEnvironmentAndNumberIndex, 1);
            modelBuilder
                .Entity<PolicyNumber>()
                .Property(pn => pn.Environment)
                .HasUniqueIndexAnnotation(PolicyNumberProductEnvironmentAndNumberIndex, 2);
            modelBuilder
                .Entity<PolicyNumber>()
                .Property(pn => pn.Number)
                .HasColumnType("VARCHAR")
                .HasMaxLength(100)
                .HasUniqueIndexAnnotation(PolicyNumberProductEnvironmentAndNumberIndex, 3);

            modelBuilder
                .Entity<InvoiceNumber>()
                .Property(inv => inv.TenantId)
                .HasColumnType("UNIQUEIDENTIFIER")
                .HasUniqueIndexAnnotation(InvoiceNumberProductEnvironmentAndNumberIndex, 0);

            modelBuilder
                .Entity<InvoiceNumber>()
                .Property(inv => inv.ProductId)
                .HasColumnType("UNIQUEIDENTIFIER")
                .HasUniqueIndexAnnotation(InvoiceNumberProductEnvironmentAndNumberIndex, 1);
            modelBuilder
                .Entity<InvoiceNumber>()
                .Property(inv => inv.Environment)
                .HasUniqueIndexAnnotation(InvoiceNumberProductEnvironmentAndNumberIndex, 2);
            modelBuilder
                .Entity<InvoiceNumber>()
                .Property(inv => inv.Number)
                .HasColumnType("VARCHAR")
                .HasMaxLength(100)
                .HasUniqueIndexAnnotation(InvoiceNumberProductEnvironmentAndNumberIndex, 3);

            modelBuilder
                .Entity<ClaimNumber>()
                .Property(cl => cl.TenantId)
                .HasColumnType("UNIQUEIDENTIFIER")
                .HasUniqueIndexAnnotation(ClaimNumberForProductEnvironmentAndNumberIndex, 0);
            modelBuilder
                .Entity<ClaimNumber>()
                .Property(cl => cl.ProductId)
                .HasColumnType("UNIQUEIDENTIFIER")
                .HasUniqueIndexAnnotation(ClaimNumberForProductEnvironmentAndNumberIndex, 1);
            modelBuilder
                .Entity<ClaimNumber>()
                .Property(cl => cl.Environment)
                .HasUniqueIndexAnnotation(ClaimNumberForProductEnvironmentAndNumberIndex, 2);
            modelBuilder
                .Entity<ClaimNumber>()
                .Property(cl => cl.Number)
                .HasColumnType("VARCHAR")
                .HasMaxLength(255)
                .HasUniqueIndexAnnotation(ClaimNumberForProductEnvironmentAndNumberIndex, 3);
            modelBuilder
                .Entity<UniqueIdentifier>()
                .Property(ui => ui.Type)
                .HasUniqueIndexAnnotation(UniqueIdentifierTypeProductEnvironmentAndIdentifierIndex, 0);
            modelBuilder
               .Entity<UniqueIdentifier>()
               .Property(ui => ui.TenantId)
               .HasColumnType("UNIQUEIDENTIFIER")
               .HasUniqueIndexAnnotation(UniqueIdentifierTypeProductEnvironmentAndIdentifierIndex, 1);
            modelBuilder
                .Entity<UniqueIdentifier>()
                .Property(ui => ui.ProductId)
                .HasColumnType("UNIQUEIDENTIFIER")
                .HasUniqueIndexAnnotation(UniqueIdentifierTypeProductEnvironmentAndIdentifierIndex, 2);
            modelBuilder
                .Entity<UniqueIdentifier>()
                .Property(ui => ui.Environment)
                .HasUniqueIndexAnnotation(UniqueIdentifierTypeProductEnvironmentAndIdentifierIndex, 3);
            modelBuilder
                .Entity<UniqueIdentifier>()
                .Property(ui => ui.Identifier)
                .HasColumnType("VARCHAR")
                .HasMaxLength(255)
                .HasUniqueIndexAnnotation(UniqueIdentifierTypeProductEnvironmentAndIdentifierIndex, 4);

            modelBuilder
              .Entity<Relationship>()
              .HasIndex(x => new { x.FromEntityId })
              .HasName(RelationshipFromEntityIndex);

            modelBuilder
                .Entity<Relationship>()
                .HasIndex(x => new { x.ToEntityId })
                .HasName(RelationshipToEntityIndex);

            modelBuilder
                .Entity<Relationship>()
                .HasIndex(x => new { x.Type, x.FromEntityType, x.FromEntityId })
                .HasName(RelationshipFromTypeIndex);

            modelBuilder
                .Entity<Relationship>()
                .HasIndex(x => new { x.Type, x.ToEntityType, x.ToEntityId })
                .HasName(RelationshipToTypeIndex);

            modelBuilder
                .Entity<Tag>()
                .HasIndex(x => new { x.EntityId })
                .HasName(TagEntityIndex);

            modelBuilder
                .Entity<Tag>()
                .HasIndex(x => new { x.TagType, x.EntityType, x.EntityId })
                .HasName(TagEntityTypeIndex);

            modelBuilder.Entity<DataTableDefinition>()
                .HasIndex(x => new { x.EntityType, x.EntityId })
                .HasName(DataTableDefinitionEntityTypeIndex);

            modelBuilder.Entity<DataTableDefinition>()
                .HasIndex(x => new { x.TenantId })
                .HasName(DataTableDefinitionTenantIndex);

            modelBuilder.Entity<PolicyReadModel>()
               .HasIndex(x => new { x.TenantId, x.ProductId, x.Environment })
               .HasName(IX_Policies_TenantId_ProductId_Environment);

            modelBuilder.Entity<PolicyReadModel>()
               .HasIndex(x => new { x.QuoteId })
               .HasName(IX_Policies_QuoteId);

            modelBuilder.Entity<PolicyReadModel>()
               .HasIndex(x => new { x.OrganisationId })
               .HasName(IX_Policies_OrganisationId);

            modelBuilder.Entity<Product>()
               .HasIndex(x => new { x.TenantId })
               .HasName(IX_Products_TenantId);
        }
    }
}
