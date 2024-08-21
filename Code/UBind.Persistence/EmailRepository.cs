// <copyright file="EmailRepository.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;
    using LinqKit;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Email;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.ReadModel.User;
    using UBind.Domain.ReadWriteModel;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Domain.Repositories;
    using UBind.Persistence.ReadModels;
    using UBind.Persistence.ReadModels.Email;

    /// <inheritdoc/>
    public class EmailRepository : IEmailRepository
    {
        private readonly IUBindDbContext dbContext;
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="clock">The clock to get the current timestamp.</param>
        public EmailRepository(
            IUBindDbContext dbContext,
            IClock clock)
        {
            Contract.Assert(dbContext != null);
            Contract.Assert(clock != null);

            this.dbContext = dbContext;
            this.clock = clock;
        }

        /// <summary>
        /// Gets an expression for instantiating detail projection from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<EmailQuotePolicyUserCustomerReadModel, IEmailDetails>> DetailsSelector =>
            model => new EmailDetails
            {
                Recipient = model.Email.To,
                From = model.Email.From,
                CC = model.Email.CC,
                BCC = model.Email.BCC,
                Subject = model.Email.Subject,
                HtmlMessage = model.Email.HtmlBody,
                PlainMessage = model.Email.PlainTextBody,
                Tags = model.Tags,
                Relationships = model.Relationships,
                User = model.User != null
                    ? new UserData
                    {
                        Id = model.User.Id,
                        UserType = model.User.UserType,
                        FullName = model.User.FullName,
                        PreferredName = model.User.PreferredName,
                        Email = model.User.Email,
                        AlternativeEmail = model.User.AlternativeEmail,
                        MobilePhoneNumber = model.User.MobilePhoneNumber,
                        WorkPhoneNumber = model.User.WorkPhoneNumber,
                        HomePhoneNumber = model.User.HomePhoneNumber,
                        OrganisationId = model.User.OrganisationId,
                        CustomerId = model.User.CustomerId,
                    }
                    : null,
                Documents = model.DocumentFiles,
                FileContents = model.FileContents,
                EmailAttachments = model.EmailAttachments,
                CreatedTicksSinceEpoch = model.Email.CreatedTicksSinceEpoch,
                Id = model.Email.Id,
                Policy = model.Quote != null && model.Quote.PolicyIssued && model.Policy != null
                    ? new PolicyData
                    {
                        Id = model.Policy.Id,
                        PolicyNumber = model.Policy.PolicyNumber,
                        OwnerUserId = model.Policy.OwnerUserId,
                        OrganisationId = model.Policy.OrganisationId,
                        CustomerId = model.Policy.CustomerId,
                    }
                    : null,
                Quote = model.Quote != null
                    ? new QuoteData
                    {
                        Id = model.Quote.Id,
                        QuoteNumber = model.Quote.QuoteNumber,
                        Type = model.Quote.Type,
                        OwnerUserId = model.Quote.OwnerUserId,
                        OrganisationId = model.Quote.OrganisationId,
                        CustomerId = model.Quote.CustomerId,
                    }
                    : null,
                Claim = model.Claim != null
                    ? new ClaimData
                    {
                        Id = model.Claim.Id.ToString(),
                        ClaimNumber = model.Claim.ClaimNumber ?? model.Claim.ClaimReference,
                        OwnerUserId = model.Claim.OwnerUserId,
                        OrganisationId = model.Claim.OrganisationId,
                        CustomerId = model.Claim.CustomerId,
                    }
                    : null,
                PolicyTransaction = model.PolicyTransaction != null
                    ? new Domain.ReadModel.Email.PolicyTransactionData { TransactionData = model.PolicyTransaction } : null,
                Customer = model.Customer != null
                    ? new CustomerData()
                    {
                        Id = model.Customer.Id,
                        FullName = model.CustomerPrimaryPerson.FullName,
                        PreferredName = model.CustomerPrimaryPerson.PreferredName,
                        Email = model.CustomerPrimaryPerson.Email,
                        AlternativeEmail = model.CustomerPrimaryPerson.AlternativeEmail,
                        MobilePhoneNumber = model.CustomerPrimaryPerson.MobilePhoneNumber,
                        WorkPhoneNumber = model.CustomerPrimaryPerson.WorkPhoneNumber,
                        HomePhoneNumber = model.CustomerPrimaryPerson.HomePhoneNumber,
                        OwnerUserId = model.Customer.OwnerUserId,
                        OrganisationId = model.Customer.OrganisationId,
                    }
                    : model.Policy != null && model.Policy.CustomerId.HasValue
                    ? new CustomerData()
                    {
                        Id = model.Policy.CustomerId.Value,
                        FullName = model.Policy.CustomerFullName,
                        PreferredName = model.Policy.CustomerPreferredName,
                        Email = model.Policy.CustomerEmail,
                        AlternativeEmail = model.Policy.CustomerAlternativeEmail,
                        MobilePhoneNumber = model.Policy.CustomerMobilePhone,
                        WorkPhoneNumber = model.Policy.CustomerWorkPhone,
                        HomePhoneNumber = model.Policy.CustomerHomePhone,
                        OwnerUserId = model.Policy.OwnerUserId,
                        OrganisationId = model.Policy.OrganisationId,
                    }
                    : null,
                Organisation = model.Organisation != null
                    ? new OrganisationData
                    {
                        Id = model.Organisation.Id,
                        Name = model.Organisation.Name,
                    }
                    : null,
                OrganisationId = model.Email.OrganisationId,
                TenantId = model.Email.TenantId,
                ProductId = model.Email.ProductId,
            };

        /// <summary>
        /// Gets an expression for instantiating detail projection from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<ProductEmailReportModel, IEmailDetails>> ProductEmailReportDetailsSelector =>
            model => new EmailDetails
            {
                Recipient = model.Email.To,
                From = model.Email.From,
                CC = model.Email.CC,
                BCC = model.Email.BCC,
                Subject = model.Email.Subject,
                HtmlMessage = model.Email.HtmlBody,
                PlainMessage = model.Email.PlainTextBody,
                ProductName = model.Product.DetailsCollection
                    .OrderByDescending(d => d.CreatedTicksSinceEpoch)
                    .FirstOrDefault().Name,
                Environment = model.Policy.Environment,
                Documents = model.Email.EmailAttachments.Select(x => x.DocumentFile),
                EmailAttachments = model.Email.EmailAttachments,
                CreatedTicksSinceEpoch = model.Email.CreatedTicksSinceEpoch,
                Id = model.Email.Id,
                Policy = model.Quote != null && model.Quote.PolicyIssued && model.Policy != null
                    ? new PolicyData { Id = model.Policy.Id, PolicyNumber = model.Policy.PolicyNumber } : null,
                Quote = model.Quote != null
                    ? new QuoteData { Id = model.Quote.Id, QuoteNumber = model.Quote.QuoteNumber, Type = model.Quote.Type }
                    : null,
                Claim = model.Claim != null ? new ClaimData { ClaimNumber = model.Claim.ClaimNumber } : null,
                Customer = model.Customer != null
                    ? new CustomerData()
                    {
                        Id = model.Customer.Id,
                        FullName = model.Person != null ? model.Person.FullName : string.Empty,
                        PreferredName = model.Person != null ? model.Person.PreferredName : string.Empty,
                        Email = model.Person != null ? model.Person.Email : string.Empty,
                        AlternativeEmail = model.Person != null ? model.Person.AlternativeEmail : string.Empty,
                        MobilePhoneNumber = model.Person != null ? model.Person.MobilePhoneNumber : string.Empty,
                        WorkPhoneNumber = model.Person != null ? model.Person.WorkPhoneNumber : string.Empty,
                        HomePhoneNumber = model.Person != null ? model.Person.HomePhoneNumber : string.Empty,
                    }
                    : null,
                TenantId = model.Email.TenantId,
                ProductId = model.Email.ProductId,
            };

        /// <summary>
        /// Gets an expression for instantiating detail projection from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<SystemEmailReportModel, IEmailDetails>> SystemEmailReportDetailsSelector =>
            model => new EmailDetails
            {
                Recipient = model.Email.To,
                From = model.Email.From,
                CC = model.Email.CC,
                BCC = model.Email.BCC,
                Subject = model.Email.Subject,
                HtmlMessage = model.Email.HtmlBody,
                PlainMessage = model.Email.PlainTextBody,
                User = model.User != null
                    ? new UserData
                    {
                        Id = model.User.Id,
                        UserType = model.User.UserType,
                        FullName = model.User.FullName,
                        PreferredName = model.User.PreferredName,
                        Email = model.User.Email,
                        AlternativeEmail = model.User.AlternativeEmail,
                        MobilePhoneNumber = model.User.MobilePhoneNumber,
                        WorkPhoneNumber = model.User.WorkPhoneNumber,
                        HomePhoneNumber = model.User.HomePhoneNumber,
                    }
                    : null,
                Documents = model.Email.EmailAttachments.Select(x => x.DocumentFile),
                EmailAttachments = model.Email.EmailAttachments,
                CreatedTicksSinceEpoch = model.Email.CreatedTicksSinceEpoch,
                Id = model.Email.Id,
                TenantId = model.Email.TenantId,
                ProductId = model.Email.ProductId,
            };

        /// <summary>
        /// Gets an expression for instantiating detail projection from QuoteReadModels for use in EF projections.
        /// </summary>
        private Expression<Func<Email, IEmailSummary>> EmailSummarySelector =>
            model => new EmailSummary
            {
                Id = model.Id,
                CreatedTicksSinceEpoch = model.CreatedTicksSinceEpoch,
                Subject = model.Subject,
                Recipient = model.To,
                HasAttachment = model.HasAttachments,
            };

        /// <inheritdoc/>
        public void InsertEmailAndMetadata(EmailAndMetadata metadata)
        {
            this.Insert(metadata.Email);

            foreach (var relationship in metadata.Relationships)
            {
                this.InsertEmailRelationship(relationship);
            }

            foreach (var tag in metadata.Tags)
            {
                this.InsertEmailTag(tag);
            }

            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void CreateRelationshipFromEntityToEmail(
            Guid tenantId, EntityType fromEntitytype, Guid fromEntityId, RelationshipType type, Guid emailId)
        {
            this.dbContext.Relationships.Add(
                new Relationship(tenantId, fromEntitytype, fromEntityId, type, EntityType.Message, emailId, this.clock.Now()));
        }

        /// <inheritdoc/>
        public void CreateRelationshipFromEmailToEntity(
            Guid tenantId, Guid emailId, RelationshipType type, EntityType toEntitytype, Guid toEntityId)
        {
            this.dbContext.Relationships.Add(
                new Relationship(tenantId, EntityType.Message, emailId, type, toEntitytype, toEntityId, this.clock.Now()));
        }

        /// <inheritdoc/>
        public IEnumerable<IEmailSummary> GetSummaries(
            Guid tenantId,
            EntityListFilters filters,
            bool? overrideIncludeNonEnvironmentSpecificData = null)
        {
            filters.IncludeNonEnvironmentSpecificData =
                overrideIncludeNonEnvironmentSpecificData.HasValue ?
                overrideIncludeNonEnvironmentSpecificData.Value :
                true;

            return this.FilterEmail(tenantId, filters).Select(this.EmailSummarySelector);
        }

        /// <inheritdoc/>
        public IEnumerable<Email> GetAll(
            Guid tenantId,
            EntityListFilters filters,
            bool? overrideIncludeNonEnvironmentSpecificData = null)
        {
            filters.IncludeNonEnvironmentSpecificData =
                overrideIncludeNonEnvironmentSpecificData.HasValue ?
                overrideIncludeNonEnvironmentSpecificData.Value :
                true;

            return this.FilterEmail(tenantId, filters);
        }

        /// <inheritdoc/>
        public bool DoesEmailExists(Guid tenantId, Guid emailId)
        {
            return this.dbContext.Emails.Where(e => e.TenantId == tenantId && e.Id == emailId).Any();
        }

        /// <inheritdoc/>
        public IEnumerable<Email> GetByPolicyId(
            Guid tenantId,
            Guid policyId,
            EntityListFilters filters)
        {
            filters.PolicyId = policyId;
            return this.FilterEmail(tenantId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Email> GetByQuoteId(
            Guid tenantId,
            Guid quoteId,
            EntityListFilters filters)
        {
            filters.QuoteId = quoteId;
            return this.FilterEmail(tenantId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Email> GetByClaimId(
            Guid tenantId,
            Guid claimId,
            EntityListFilters filters)
        {
            filters.ClaimId = claimId;
            return this.FilterEmail(tenantId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Email> GetByPolicyTransactionId(
            Guid tenantId,
            Guid policyTransactionId,
            EntityListFilters filters)
        {
            filters.PolicyTransactionId = policyTransactionId;
            return this.FilterEmail(tenantId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Email> GetByQuoteVersionId(
            Guid tenantId,
            Guid quoteVersionId,
            EntityListFilters filters)
        {
            filters.QuoteVersionId = quoteVersionId;
            return this.FilterEmail(tenantId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Email> GetEmailsForCustomer(
            Guid tenantId,
            Guid customerId,
            EntityListFilters filters)
        {
            filters.CustomerId = customerId;
            return this.FilterEmail(tenantId, filters);
        }

        /// <inheritdoc/>
        public IEnumerable<Email> GetByUserId(
            Guid tenantId,
            Guid userId,
            EntityListFilters filters)
        {
            filters.UserAccountId = userId;
            filters.IncludeNonEnvironmentSpecificData = true;
            return this.FilterEmail(tenantId, filters);
        }

        /// <inheritdoc/>
        public Email GetById(Guid tenantId, Guid id)
        {
            return this.dbContext.Emails.SingleOrDefault(email => email.TenantId == tenantId && email.Id == id);
        }

        /// <inheritdoc/>
        public Email GetWithFilesById(Guid tenantId, Guid id)
        {
            return this.dbContext.Emails.IncludeAllProperties().SingleOrDefault(email => email.TenantId == tenantId && email.Id == id);
        }

        /// <inheritdoc/>
        public IEnumerable<Relationship> GetRelationships(Guid tenantId, Guid emailId, EntityType? entityType = null)
        {
            var relationshipQuery = this.dbContext.Emails
                .GroupJoin(
                     this.dbContext.Relationships,
                     email => email.Id,
                     relationship => relationship.FromEntityId,
                     (email, relationships) => new { Email = email, Relationships = relationships })
                .Where(x => x.Email.TenantId == tenantId && x.Email.Id == emailId)
                .GroupJoin(
                     this.dbContext.Relationships,
                     model => model.Email.Id,
                     relationship => relationship.ToEntityId,
                     (model, relationships) => new EmailTagRelationshipModel
                     {
                         Email = model.Email,
                         Relationships = model.Relationships.Concat(relationships),
                     })
                .Where(x => x.Email.Id == emailId);

            if (entityType != null)
            {
                relationshipQuery = relationshipQuery
                    .Where(x => x.Relationships.Any(y => y.FromEntityType == entityType || y.ToEntityType == entityType));
            }

            return relationshipQuery.SelectMany(x => x.Relationships).ToList();
        }

        /// <inheritdoc/>
        public IEnumerable<Tag> GetTags(Guid tenantId, Guid id)
        {
            var tags = this.dbContext.Tags.Where(x => x.EntityId == id);
            return tags.ToList();
        }

        /// <inheritdoc/>
        public bool CheckIfHasAttachments(Guid tenantId, Guid id)
        {
            var email = this.dbContext.Emails
                .Where(x => x.TenantId == tenantId && x.Id == id)
                .Include(q => q.EmailAttachments)
                .ToList();

            return email.Any(x => x.EmailAttachments != null);
        }

        /// <inheritdoc/>
        public IEmailDetails GetEmailDetailsWithAttachments(Guid tenantId, Guid id)
        {
            var detailQueryable = this.GetDetailQuery(tenantId, id);
            var returnValue = detailQueryable.Select(this.DetailsSelector).SingleOrDefault();
            return returnValue;
        }

        /// <inheritdoc/>
        public IEnumerable<IEmailDetails> GetSystemEmailForReport(
            Guid tenantId, Guid organisationId, DeploymentEnvironment env, Instant? fromTimestamp, Instant? toTimestamp)
        {
            var userSystemEmailQuery = from email in this.dbContext.Emails
                                       join tag in this.dbContext.Tags on email.Id equals tag.EntityId
                                       join relationship in this.dbContext.Relationships on email.Id equals relationship.FromEntityId
                                       join user in this.dbContext.Users on relationship.ToEntityId equals user.Id
                                       where tag.TagType == TagType.EmailType
                                            && email.ProductId == null
                                            && relationship.Type == RelationshipType.MessageRecipient
                                       select new SystemEmailReportModel
                                       {
                                           Email = email,
                                           User = user,
                                       };

            var customerSystemEmailQuery = from email in this.dbContext.Emails
                                           join tag in this.dbContext.Tags on email.Id equals tag.EntityId
                                           join relationship in this.dbContext.Relationships on email.Id equals relationship.FromEntityId
                                           join customer in this.dbContext.CustomerReadModels on relationship.ToEntityId equals customer.Id
                                           join person in this.dbContext.PersonReadModels on customer.Id equals person.CustomerId
                                           join user in this.dbContext.Users on person.UserId equals user.Id
                                           where tag.TagType == TagType.EmailType
                                                && email.ProductId == null
                                                && relationship.Type == RelationshipType.MessageRecipient
                                           select new SystemEmailReportModel
                                           {
                                               Email = email,
                                               User = user,
                                           };

            var concatUserCustomer = userSystemEmailQuery.Concat(customerSystemEmailQuery);
            concatUserCustomer = concatUserCustomer.Where(j => j.Email.TenantId == tenantId);

            if (organisationId != default)
            {
                concatUserCustomer = concatUserCustomer.Where(c => c.Email.OrganisationId == organisationId);
            }

            if (fromTimestamp != default)
            {
                var fromTicks = fromTimestamp?.ToUnixTimeTicks();
                concatUserCustomer = concatUserCustomer.Where(j => j.Email.CreatedTicksSinceEpoch >= fromTicks);
            }

            if (toTimestamp != default)
            {
                var toTicks = toTimestamp?.ToUnixTimeTicks();
                concatUserCustomer = concatUserCustomer.Where(j => j.Email.CreatedTicksSinceEpoch <= toTicks);
            }

            return concatUserCustomer.Select(this.SystemEmailReportDetailsSelector);
        }

        /// <inheritdoc/>
        public IEnumerable<IEmailDetails> GetProductEmailForReport(
            Guid tenantId,
            Guid organisationId,
            IEnumerable<Guid> productIds,
            DeploymentEnvironment env,
            Instant fromTimestamp,
            Instant toTimestamp,
            bool includeTestData)
        {
            var quoteCustomerEmailQuery = from email in this.dbContext.Emails
                                          join tag in this.dbContext.Tags on email.Id equals tag.EntityId
                                          join relationship in this.dbContext.Relationships on email.Id equals relationship.ToEntityId
                                          join quote in this.dbContext.QuoteReadModels on relationship.FromEntityId equals quote.Id
                                          join policy in this.dbContext.Policies on quote.PolicyId equals policy.Id
                                          join product in this.dbContext.Products on policy.ProductId equals product.Id
                                          join customer in this.dbContext.CustomerReadModels on policy.CustomerId equals customer.Id
                                          join person in this.dbContext.PersonReadModels on customer.PrimaryPersonId equals person.Id
                                          where relationship.FromEntityType == EntityType.Quote && relationship.Type == RelationshipType.QuoteMessage
                                            && tag.TagType == TagType.EmailType && tag.Value == "Customer"
                                          select new ProductEmailReportModel
                                          {
                                              Email = email,
                                              Quote = quote,
                                              Claim = null,
                                              Policy = policy,
                                              Product = product,
                                              Customer = customer,
                                              Person = person,
                                          };

            var quoteClientEmailQuery = from email in this.dbContext.Emails
                                        join tag in this.dbContext.Tags on email.Id equals tag.EntityId
                                        join relationship in this.dbContext.Relationships on email.Id equals relationship.ToEntityId
                                        join quote in this.dbContext.QuoteReadModels on relationship.FromEntityId equals quote.Id
                                        join policy in this.dbContext.Policies on quote.PolicyId equals policy.Id
                                        join product in this.dbContext.Products on policy.ProductId equals product.Id
                                        join customer in this.dbContext.CustomerReadModels on policy.CustomerId equals customer.Id
                                        join person in this.dbContext.PersonReadModels on customer.PrimaryPersonId equals person.Id
                                        where relationship.FromEntityType == EntityType.Quote && relationship.Type == RelationshipType.QuoteMessage
                                            && tag.TagType == TagType.EmailType && tag.Value == "Admin"
                                        select new ProductEmailReportModel
                                        {
                                            Email = email,
                                            Quote = quote,
                                            Claim = null,
                                            Policy = policy,
                                            Product = product,
                                            Customer = customer,
                                            Person = person,
                                        };

            var claimEmailQuery = from email in this.dbContext.Emails
                                  join tag in this.dbContext.Tags on email.Id equals tag.EntityId
                                  join relationship in this.dbContext.Relationships on email.Id equals relationship.ToEntityId
                                  join claim in this.dbContext.ClaimReadModels on relationship.FromEntityId equals claim.Id
                                  join policy in this.dbContext.Policies on claim.PolicyId equals policy.Id
                                  join product in this.dbContext.Products on claim.ProductId equals product.Id
                                  join customer in this.dbContext.CustomerReadModels on policy.CustomerId equals customer.Id
                                  join person in this.dbContext.PersonReadModels on customer.PrimaryPersonId equals person.Id
                                  where relationship.FromEntityType == EntityType.Claim && relationship.Type == RelationshipType.ClaimMessage
                                    && tag.TagType == TagType.EmailType
                                  select new ProductEmailReportModel
                                  {
                                      Email = email,
                                      Quote = null,
                                      Claim = claim,
                                      Policy = policy,
                                      Product = product,
                                      Customer = customer,
                                      Person = person,
                                  };

            var concatQuoteClaim = quoteCustomerEmailQuery.Concat(quoteClientEmailQuery).Concat(claimEmailQuery);
            concatQuoteClaim = concatQuoteClaim.Where(j => j.Email.TenantId == tenantId && j.Policy.Environment == env);

            if (organisationId != default)
            {
                concatQuoteClaim = concatQuoteClaim.Where(c => c.Email.OrganisationId == organisationId);
            }

            if (productIds.Any())
            {
                concatQuoteClaim = concatQuoteClaim.Where(j => j.Email.ProductId.HasValue && productIds.Contains(j.Email.ProductId.Value));
            }

            if (fromTimestamp != default)
            {
                var fromTicks = fromTimestamp.ToUnixTimeTicks();
                concatQuoteClaim = concatQuoteClaim.Where(j => j.Email.CreatedTicksSinceEpoch >= fromTicks);
            }

            if (toTimestamp != default)
            {
                var toTicks = toTimestamp.ToUnixTimeTicks();
                concatQuoteClaim = concatQuoteClaim.Where(j => j.Email.CreatedTicksSinceEpoch <= toTicks);
            }

            if (!includeTestData)
            {
                concatQuoteClaim = concatQuoteClaim.Where(j => j.Policy.IsTestData == false);
            }

            return concatQuoteClaim.Select(this.ProductEmailReportDetailsSelector);
        }

        /// <inheritdoc/>
        public void SaveChanges()
        {
            this.dbContext.SaveChanges();
        }

        /// <summary>
        /// Insert a new quote email model in the repository.
        /// </summary>
        /// <param name="model">The quote email object.</param>
        public void Insert(Email model)
        {
            this.dbContext.Emails.Add(model);
        }

        /// <inheritdoc/>
        public IEmailReadModelWithRelatedEntities GetEmailWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, Guid id, IEnumerable<string> relatedEntities)
        {
            var query = this.CreateQueryForEmailDetailsWithRelatedEntities(tenantId, environment, relatedEntities);
            return query.FirstOrDefault(e => e.Email.Id == id);
        }

        public IEnumerable<EmailReadModelWithRelatedEntities> GetEmailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, EntityListFilters filters, IEnumerable<string> relatedEntities)
        {
            var emails = this.GetAll(tenantId, filters).AsQueryable();
            return this.CreateQueryForEmailDetailsWithRelatedEntities(
                tenantId, environment, emails, relatedEntities).ToList();
        }

        /// <inheritdoc/>
        public IQueryable<EmailReadModelWithRelatedEntities> CreateQueryForEmailDetailsWithRelatedEntities(
            Guid tenantId, DeploymentEnvironment environment, IEnumerable<string> relatedEntities)
        {
            return this.CreateQueryForEmailDetailsWithRelatedEntities(
                tenantId, environment, this.dbContext.Emails, relatedEntities);
        }

        /// <inheritdoc/>
        public void InsertEmailRelationship(Relationship relationship)
        {
            this.dbContext.Relationships.Add(relationship);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void RemoveEmailRelationship(Relationship relationship)
        {
            this.dbContext.Relationships.Remove(relationship);
            this.dbContext.SaveChanges();
        }

        /// <inheritdoc/>
        public void DeleteEmailsAndAttachmentsForEntity(
            Guid tenantId,
            DeploymentEnvironment environment,
            EntityType entityType,
            Guid entitiyId)
        {
            var filters = new EntityListFilters()
            {
                Environment = environment,
                EntityType = entityType,
                EntityId = entitiyId,
            };
            this.DeleteEmailsAndAttachments(tenantId, filters);
        }

        private void DeleteEmailsAndAttachments(Guid tenantId, EntityListFilters filters)
        {
            var emailSummaries = this.GetSummaries(tenantId, filters);
            var emails = emailSummaries.Select(sm => this.GetById(tenantId, sm.Id));
            foreach (var email in emails)
            {
                foreach (var attachment in email.EmailAttachments)
                {
                    // commented two lines.
                    // there must be no other remaining references for the FileContent prior to deletion.
                    // a counter of references will be implemented at UB-10238.
                    // var fileContentsToDelete = this.dbContext.FileContents.Where(f => f.Id == attachment.DocumentFile.FileContentId);
                    // this.dbContext.FileContents.RemoveRange(fileContentsToDelete);
                    var documentFilesToDelete = this.dbContext.DocumentFile.Where(d => d.Id == attachment.DocumentFile.Id);
                    this.dbContext.DocumentFile.RemoveRange(documentFilesToDelete);
                    var attachmentToDelete = this.dbContext.EmailAttachments.Where(a => a.Id == attachment.Id);
                    this.dbContext.EmailAttachments.RemoveRange(attachmentToDelete);
                }
                var emailToDelete = this.dbContext.Emails.Where(e => e.Id == email.Id);
                this.dbContext.Emails.RemoveRange(emailToDelete);
            }
        }

        private IQueryable<EmailReadModelWithRelatedEntities> CreateQueryForEmailDetailsWithRelatedEntities(
            Guid tenantId,
            DeploymentEnvironment environment,
            IQueryable<Email> dataSource,
            IEnumerable<string> relatedEntities)
        {
            var query = from email in dataSource
                        join tags in this.dbContext.Tags on email.Id equals tags.EntityId
                        where email.TenantId == tenantId &&
                            tags.TagType == TagType.Environment &&
                            tags.Value == environment.ToString()
                        select new EmailReadModelWithRelatedEntities()
                        {
                            Email = email,
                            Attachments = new EmailAttachment[] { },
                            Documents = new DocumentFile[] { },
                            Tenant = default,
                            TenantDetails = new TenantDetails[] { },
                            Organisation = default,
                            Tags = new Tag[] { },
                            FromRelationships = new Relationship[] { },
                            ToRelationships = new Relationship[] { },
                        };

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.EmailMessage.Attachments))))
            {
                query = from email in dataSource
                        select new EmailReadModelWithRelatedEntities()
                        {
                            Email = email,
                            Attachments = email.EmailAttachments,
                            Documents = email.EmailAttachments.Select(c => c.DocumentFile),
                            Tenant = default,
                            TenantDetails = new TenantDetails[] { },
                            Organisation = default,
                            Tags = new Tag[] { },
                            FromRelationships = new Relationship[] { },
                            ToRelationships = new Relationship[] { },
                        };
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.EmailMessage.Tenant))))
            {
                query = query.Join(
                    this.dbContext.Tenants.IncludeAllProperties(),
                    e => e.Email.TenantId,
                    t => t.Id,
                    (e, tenant) => new EmailReadModelWithRelatedEntities
                    {
                        Email = e.Email,
                        Attachments = e.Attachments,
                        Documents = e.Documents,
                        Tenant = tenant,
                        TenantDetails = tenant.DetailsCollection,
                        Organisation = e.Organisation,
                        Tags = e.Tags,
                        FromRelationships = e.FromRelationships,
                        ToRelationships = e.ToRelationships,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.EmailMessage.Organisation))))
            {
                query = query.Join(
                    this.dbContext.OrganisationReadModel,
                    e => e.Email.OrganisationId,
                    t => t.Id,
                    (e, organisation) => new EmailReadModelWithRelatedEntities
                    {
                        Email = e.Email,
                        Attachments = e.Attachments,
                        Documents = e.Documents,
                        Tenant = e.Tenant,
                        TenantDetails = e.TenantDetails,
                        Organisation = organisation,
                        Tags = e.Tags,
                        FromRelationships = e.FromRelationships,
                        ToRelationships = e.ToRelationships,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.EmailMessage.Tags))))
            {
                query = query.GroupJoin(
                    this.dbContext.Tags,
                    e => e.Email.Id,
                    t => t.EntityId,
                    (e, tags) => new EmailReadModelWithRelatedEntities
                    {
                        Email = e.Email,
                        Attachments = e.Attachments,
                        Documents = e.Documents,
                        Tenant = e.Tenant,
                        TenantDetails = e.TenantDetails,
                        Organisation = e.Organisation,
                        Tags = tags,
                        FromRelationships = e.FromRelationships,
                        ToRelationships = e.ToRelationships,
                    });
            }

            if (relatedEntities.Any(a => a.EqualsIgnoreCase(nameof(Domain.SerialisedEntitySchemaObject.EmailMessage.Relationships))))
            {
                query = query.GroupJoin(
                    this.dbContext.Relationships,
                    e => e.Email.Id,
                    r => r.FromEntityId,
                    (e, relationships) => new EmailReadModelWithRelatedEntities
                    {
                        Email = e.Email,
                        Attachments = e.Attachments,
                        Documents = e.Documents,
                        Tenant = e.Tenant,
                        TenantDetails = e.TenantDetails,
                        Organisation = e.Organisation,
                        Tags = e.Tags,
                        FromRelationships = relationships,
                        ToRelationships = e.ToRelationships,
                    });

                query = query.GroupJoin(
                    this.dbContext.Relationships,
                    e => e.Email.Id,
                    r => r.ToEntityId,
                    (e, relationships) => new EmailReadModelWithRelatedEntities
                    {
                        Email = e.Email,
                        Attachments = e.Attachments,
                        Documents = e.Documents,
                        Tenant = e.Tenant,
                        TenantDetails = e.TenantDetails,
                        Organisation = e.Organisation,
                        Tags = e.Tags,
                        FromRelationships = e.FromRelationships,
                        ToRelationships = relationships,
                    });
            }

            return query;
        }

        private IQueryable<Email> FilterEmail(
           Guid tenantId,
           EntityListFilters filters)
        {
            var joinQuery = this.Query(tenantId, filters);
            var filtered = this.Filter(joinQuery, filters);
            return filtered.Select(x => x.Email);
        }

        /// <summary>
        /// Insert a tag record to the database.
        /// </summary>
        /// <param name="tag">The tag.</param>
        private void InsertEmailTag(Tag tag)
        {
            this.dbContext.Tags.Add(tag);
        }

        private IQueryable<EmailQuotePolicyUserCustomerReadModel> GetDetailQuery(
            Guid tenantId, Guid id)
        {
            var query = this.dbContext.Emails
                .Where(x => x.Id == id && x.TenantId == tenantId)
                .IncludeAllProperties()
                .GroupJoin(
                    this.dbContext.Relationships,
                    email => email.Id,
                    relationship => relationship.ToEntityId,
                    (email, fromRelationships) => new
                    {
                        Email = email,
                        Attachments = email.EmailAttachments,
                        Documents = email.EmailAttachments.Select(c => c.DocumentFile),
                        FromRelationships = fromRelationships,
                        QuoteRelationship = fromRelationships.FirstOrDefault(x => x.FromEntityType == EntityType.Quote && x.Type == RelationshipType.QuoteMessage),
                        ClaimRelationship = fromRelationships.FirstOrDefault(x => x.FromEntityType == EntityType.Claim && x.Type == RelationshipType.ClaimMessage),
                        PolicyTransactionRelationship = fromRelationships.FirstOrDefault(x => x.FromEntityType == EntityType.PolicyTransaction && x.Type == RelationshipType.PolicyTransactionMessage),
                    });

            var join = query.GroupJoin(
                     this.dbContext.Relationships,
                     model => model.Email.Id,
                     relationship => relationship.FromEntityId,
                     (model, toRelationships) => new
                     {
                         Email = model.Email,
                         Attachments = model.Attachments,
                         Documents = model.Documents,
                         Relationship = model.FromRelationships.Concat(toRelationships),
                         Quote = this.dbContext.QuoteReadModels.FirstOrDefault(o => model.QuoteRelationship.FromEntityId == o.Id),
                         Claim = this.dbContext.ClaimReadModels.FirstOrDefault(o => model.ClaimRelationship.FromEntityId == o.Id),
                         PolicyTransaction = this.dbContext.PolicyTransactions.FirstOrDefault(o => model.PolicyTransactionRelationship.FromEntityId == o.Id),
                         CustomerRelationship = toRelationships.FirstOrDefault(x =>
                             ((x.ToEntityType == EntityType.Customer || x.ToEntityType == EntityType.Person) && x.Type == RelationshipType.MessageRecipient)),
                         UserRelationship = model.FromRelationships.FirstOrDefault(x =>
                            ((x.ToEntityType == EntityType.User || x.ToEntityType == EntityType.Person) && x.Type == RelationshipType.MessageRecipient)),
                         EmailOrganisation = this.dbContext.OrganisationReadModel.FirstOrDefault(x => x.Id == model.Email.OrganisationId),
                     });

            var joinWithUserPerson = join.Select(
                    (model) => new
                    {
                        UserPerson = (model.UserRelationship.Type == RelationshipType.MessageRecipient
                            && model.UserRelationship.ToEntityType == EntityType.Person)
                            ? this.dbContext.PersonReadModels.FirstOrDefault(p => p.Id == model.UserRelationship.ToEntityId)
                            : null,
                        Email = model.Email,
                        Attachments = model.Attachments,
                        Documents = model.Documents,
                        Relationship = model.Relationship,
                        Quote = model.Quote,
                        Claim = model.Claim,
                        PolicyTransaction = model.PolicyTransaction,
                        CustomerRelationship = model.CustomerRelationship,
                        UserRelationship = model.UserRelationship,
                        EmailOrganisation = model.EmailOrganisation,
                    });

            // get the related quote.
            var join3 = joinWithUserPerson.GroupJoin(
                     this.dbContext.Policies,
                     model => model.Quote.PolicyId,
                     policy => policy.Id,
                     (model, policy) => new
                     {
                         Email = model.Email,
                         Attachments = model.Attachments,
                         Documents = model.Documents,
                         Relationship = model.Relationship,
                         CustomerRelationship = model.CustomerRelationship,
                         Quote = model.Quote,
                         EmailOrganisation = model.EmailOrganisation,
                         Claim = model.Claim,
                         PolicyTransaction = model.PolicyTransaction,
                         UserCustomer = model.UserPerson != null && model.UserPerson.CustomerId.HasValue
                            ? this.dbContext.CustomerReadModels.FirstOrDefault(c => c.Id == model.UserPerson.CustomerId.Value)
                            : null,
                         UserRelationship = model.UserRelationship,
                         Policy = policy.FirstOrDefault(),
                     });

            // get the related customer.
            var join4 = join3.GroupJoin(
                     this.dbContext.CustomerReadModels.IncludeAllProperties(),
                     model => model.CustomerRelationship.Type == RelationshipType.MessageRecipient
                        ? model.CustomerRelationship.ToEntityId : model.CustomerRelationship.FromEntityId,
                     customer => customer.Id,
                     (model, customer) => new
                     {
                         Email = model.Email,
                         Attachments = model.Attachments,
                         Documents = model.Documents,
                         Quote = model.Quote,
                         Claim = model.Claim,
                         EmailOrganisation = model.EmailOrganisation,
                         PolicyTransaction = model.PolicyTransaction,
                         Policy = model.Policy,
                         Relationship = model.Relationship,
                         UserRelationship = model.UserRelationship,
                         Customer = customer.FirstOrDefault() ?? model.UserCustomer,
                         CustomerRelationship = model.CustomerRelationship,
                     });

            // If you dont have customer, retrieve it on person instead.
            var join5 = join4.GroupJoin(
                     this.dbContext.PersonReadModels,
                     model => model.CustomerRelationship.Type == RelationshipType.MessageRecipient
                        ? model.CustomerRelationship.ToEntityId : model.CustomerRelationship.FromEntityId,
                     person => person.Id,
                     (model, person) => new
                     {
                         Email = model.Email,
                         Attachments = model.Attachments,
                         Documents = model.Documents,
                         Quote = model.Quote,
                         Claim = model.Claim,
                         EmailOrganisation = model.EmailOrganisation,
                         PolicyTransaction = model.PolicyTransaction,
                         Policy = model.Policy,
                         Relationship = model.Relationship,
                         UserRelationship = model.UserRelationship,
                         Customer = model.Customer != null
                             ? model.Customer
                             : person.FirstOrDefault() != null && person.FirstOrDefault().CustomerId != null
                                 ? this.dbContext.CustomerReadModels.FirstOrDefault(x => x.Id == person.FirstOrDefault().CustomerId)
                                 : null,
                     });

            // If you dont have user, retrieve it on person instead.
            var join6 = join5.GroupJoin(
                     this.dbContext.PersonReadModels,
                     model => model.UserRelationship.Type == RelationshipType.MessageRecipient
                        ? model.UserRelationship.ToEntityId : model.UserRelationship.FromEntityId,
                     person => person.Id,
                     (model, person) => new
                     {
                         Email = model.Email,
                         Attachments = model.Attachments,
                         Documents = model.Documents,
                         Quote = model.Quote,
                         Claim = model.Claim,
                         EmailOrganisation = model.EmailOrganisation,
                         PolicyTransaction = model.PolicyTransaction,
                         Policy = model.Policy,
                         Relationship = model.Relationship,
                         UserRelationship = model.UserRelationship,
                         Customer = model.Customer,
                         User = person.FirstOrDefault() == null
                             ? null
                             : person.FirstOrDefault() != null && person.FirstOrDefault().UserId != null
                                 ? this.dbContext.Users.FirstOrDefault(x => x.Id == person.FirstOrDefault().UserId)
                                 : null,
                     });

            // get related tags.
            var join7 = join6.GroupJoin(
                   this.dbContext.Tags,
                   model => model.Email.Id,
                   tag => tag.EntityId,
                   (model, tags) => new
                   {
                       Relationship = model.Relationship,
                       UserRelationship = model.UserRelationship,
                       Email = model.Email,
                       Attachments = model.Attachments,
                       Documents = model.Documents,
                       Quote = model.Quote,
                       Claim = model.Claim,
                       EmailOrganisation = model.EmailOrganisation,
                       PolicyTransaction = model.PolicyTransaction,
                       Policy = model.Policy,
                       Customer = model.Customer,
                       Tags = tags,
                       User = model.User,
                   });

            // get related user.
            var join8 = join7.GroupJoin(
                    this.dbContext.Users,
                    model => model.UserRelationship.ToEntityId,
                    user => user.Id,
                    (model, user) => new
                    {
                        Relationship = model.Relationship,
                        Email = model.Email,
                        Attachments = model.Attachments,
                        Documents = model.Documents,
                        Quote = model.Quote,
                        Claim = model.Claim,
                        Policy = model.Policy,
                        EmailOrganisation = model.EmailOrganisation,
                        PolicyTransaction = model.PolicyTransaction,
                        Customer = model.Customer,
                        Tags = model.Tags,
                        User = model.User != null ? model.User : user.FirstOrDefault(),
                    });

            return join8
                .Select(
                    model => new EmailQuotePolicyUserCustomerReadModel
                    {
                        Relationships = model.Relationship,
                        Email = model.Email,
                        EmailAttachments = model.Attachments,
                        DocumentFiles = model.Documents,
                        FileContents = model.Documents.Select(d => d.FileContent),
                        Quote = model.Quote,
                        Claim = model.Claim,
                        Organisation = model.EmailOrganisation,
                        PolicyTransaction = model.PolicyTransaction,
                        Policy = model.Policy,
                        Customer = model.Customer,
                        CustomerPrimaryPerson
                            = model.Customer.People.FirstOrDefault(p => p.Id == model.Customer.PrimaryPersonId),
                        Tags = model.Tags,
                        User = model.User,
                    });
        }

        /// <summary>
        /// Query and filter by tenantId and deployment environment.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// If one of the values is null, emails not associated with an environment (e.g. User emails) will be included.
        /// <param name="filters">The filters.</param>
        /// <returns>The IQueriable.</returns>
        private IQueryable<EmailTagRelationshipModel> Query(
            Guid tenantId, EntityListFilters filters)
        {
            var previousTimeout = this.dbContext.Database.CommandTimeout;
            try
            {
                // For some reason, EntityListFilters.QuoteId doesn't have a value even if
                // this queries emails against a quote
                if (!filters.QuoteId.HasValue && filters.EntityType == EntityType.Quote)
                {
                    filters.QuoteId = filters.EntityId;
                }

                this.dbContext.SetTimeout(180);
                var joinQuery = this.dbContext.Emails
                    .Where(e => e.TenantId == tenantId)
                    .GroupJoin(
                        this.dbContext.Tags,
                        email => email.Id,
                        tag => tag.EntityId,
                        (email, tag) => new { Email = email, Tag = tag })
                    .GroupJoin(
                        this.dbContext.Relationships,
                        model => model.Email.Id,
                        relationship => relationship.FromEntityId,
                        (model, relationship) => new { Email = model.Email, Tag = model.Tag, Relationship = relationship })
                    .GroupJoin(
                        this.dbContext.Relationships,
                        model => model.Email.Id,
                        relationship => relationship.ToEntityId,
                        (model, relationship) => new EmailTagRelationshipModel
                        {
                            Email = model.Email,
                            Tags = model.Tag,
                            Relationships = model.Relationship.Concat(relationship),
                        });

                var organisationPredicate = PredicateBuilder.New<EmailTagRelationshipModel>(false);

                if (filters.CanViewEmailsFromOtherOrgThatHasCustomer)
                {
                    organisationPredicate = organisationPredicate.Or(c =>
                        c.Relationships.Any(r => r.Type == RelationshipType.CustomerMessage)
                        || c.Relationships.Any(r => r.Type == RelationshipType.MessageRecipient
                            && (r.ToEntityType == EntityType.Customer || r.ToEntityType == EntityType.Organisation))
                        || c.Relationships.Any(r => r.Type == RelationshipType.OrganisationMessage
                            && r.FromEntityType == EntityType.Organisation));
                }

                bool hasOrganisationFilters = filters.OrganisationIds != null && filters.OrganisationIds.Any();
                if (hasOrganisationFilters)
                {
                    organisationPredicate = organisationPredicate.Or(c => filters.OrganisationIds.Contains(c.Email.OrganisationId));
                }

                if (filters.CanViewEmailsFromOtherOrgThatHasCustomer || hasOrganisationFilters)
                {
                    joinQuery = joinQuery.Where(organisationPredicate);
                }

                var result = joinQuery;
                if (filters.Environment != null)
                {
                    var environmentPredicate = PredicateBuilder.New<EmailTagRelationshipModel>(false);
                    if (filters.IncludeNonEnvironmentSpecificData)
                    {
                        environmentPredicate = environmentPredicate.Or(y => y.Email.Environment == null);
                    }

                    var environmentStr = filters.Environment.ToString();
                    var deploymentEnvironment = environmentStr.ToEnumOrThrow<DeploymentEnvironment>();
                    environmentPredicate = environmentPredicate.Or(y => y.Email.Environment == deploymentEnvironment);
                    result = result.Where(environmentPredicate);
                }

                if (filters.OwnerUserId.HasValue)
                {
                    var ownedCustomerPersonIdList = this.dbContext.CustomerReadModels
                        .Where(x => x.OwnerUserId == filters.OwnerUserId).Select(x => x.PrimaryPersonId);

                    result = result.Where(
                        x => x.Relationships.Any(
                            y => (y.ToEntityType == EntityType.User && y.Type == RelationshipType.MessageSender && y.ToEntityId == filters.OwnerUserId)
                            || (y.ToEntityType == EntityType.User && y.Type == RelationshipType.MessageRecipient && y.ToEntityId == filters.OwnerUserId)
                            || (y.FromEntityType == EntityType.Message && ownedCustomerPersonIdList.Any(a => a == y.ToEntityId)))
                        || (filters.QuoteId.HasValue
                            && x.Relationships
                                .Where(a => a.FromEntityType == EntityType.Quote && a.Type == RelationshipType.QuoteMessage)
                                .Join(this.dbContext.QuoteReadModels, a => a.FromEntityId, b => b.Id, (a, b) => b)
                                .Any(c => c.OwnerUserId == filters.OwnerUserId)));
                }

                if (filters.CustomerId.HasValue && filters.CustomerId.GetValueOrDefault() != default)
                {
                    // the records customer.
                    result = result.Where(
                        y => y.Relationships.Any(x => x.FromEntityType == EntityType.Customer && x.FromEntityId == filters.CustomerId)
                        || y.Relationships.Any(x => x.ToEntityType == EntityType.Customer && x.ToEntityId == filters.CustomerId)
                        || (filters.QuoteId.HasValue
                            && y.Relationships
                                .Where(a => a.FromEntityType == EntityType.Quote && a.Type == RelationshipType.QuoteMessage)
                                .Join(this.dbContext.QuoteReadModels, a => a.FromEntityId, b => b.Id, (a, b) => b)
                                .Any(c => c.CustomerId == filters.CustomerId)));

                    // This is needed because if we don't filter based upon the EmailType tag we will get results when we shouldn't
                    // These are not user tags, they are managed by the system for additional filtering beyond what consumers may specify.
                    result = result
                        .Where(x => x.Tags.Any(o => o.Value == DefaultEmailTags.Customer && o.TagType == TagType.EmailType));
                }

                if (filters.PolicyId != null)
                {
                    result = result.Where(y => y.Relationships.Any(x => x.FromEntityType == EntityType.Policy
                        && x.FromEntityId == filters.PolicyId && x.Type == RelationshipType.PolicyMessage));
                }

                if (filters.QuoteId != null)
                {
                    result = result.Where(y => y.Relationships.Any(x => x.FromEntityType == EntityType.Quote
                        && x.FromEntityId == filters.QuoteId && x.Type == RelationshipType.QuoteMessage));
                }

                if (filters.ClaimId != null)
                {
                    result = result.Where(y => y.Relationships.Any(x => x.FromEntityType == EntityType.Claim
                        && x.FromEntityId == filters.ClaimId && x.Type == RelationshipType.ClaimMessage));
                }

                if (filters.PolicyTransactionId != null)
                {
                    result = result.Where(y => y.Relationships.Any(x => x.FromEntityType == EntityType.PolicyTransaction
                        && x.FromEntityId == filters.PolicyTransactionId && x.Type == RelationshipType.PolicyTransactionMessage));
                }

                if (filters.QuoteVersionId != null)
                {
                    result = result.Where(y => y.Relationships.Any(x => x.FromEntityType == EntityType.QuoteVersion
                        && x.FromEntityId == filters.QuoteVersionId && x.Type == RelationshipType.QuoteVersionMessage));
                }

                if (filters.ClaimVersionId != null)
                {
                    result = result.Where(y => y.Relationships.Any(x => x.FromEntityType == EntityType.ClaimVersion
                        && x.FromEntityId == filters.ClaimVersionId && x.Type == RelationshipType.ClaimVersionMessage));
                }

                if (filters.UserAccountId != null)
                {
                    result =
                        result.Where(y => y.Relationships.Any(x =>
                        x.ToEntityType == EntityType.User && x.ToEntityId == filters.UserAccountId) ||
                        y.Relationships.Any(x => x.FromEntityType == EntityType.User && x.FromEntityId == filters.UserAccountId));
                }

                if (filters.EntityType.HasValue && filters.EntityId.HasValue)
                {
                    result =
                        result.Where(y => y.Relationships.Any(x =>
                        (x.FromEntityType == filters.EntityType && x.FromEntityId == filters.EntityId) ||
                        (x.ToEntityType == filters.EntityType && x.ToEntityId == filters.EntityId)));
                }

                return result;
            }
            finally
            {
                this.dbContext.SetTimeout(previousTimeout);
            }
        }

        /// <summary>
        /// Filter by read model filter.
        /// </summary>
        /// <param name="joinQuery">The query to filter.</param>
        /// <param name="filters">The filter.</param>
        /// <returns>The IQueriable.</returns>
        private IQueryable<EmailTagRelationshipModel> Filter(
            IQueryable<EmailTagRelationshipModel> joinQuery, EntityListFilters filters)
        {
            if (filters.Statuses.Any())
            {
                var statusPredicate = PredicateBuilder.New<EmailTagRelationshipModel>(false);

                // Manage statuses
                foreach (var statusString in filters.Statuses)
                {
                    if (!string.IsNullOrEmpty(statusString))
                    {
                        if (string.Equals(statusString, "Customer", StringComparison.OrdinalIgnoreCase))
                        {
                            // filter by email recipient to customer and has environment.
                            if (joinQuery.Any(j => j.Relationships.Any()))
                            {
                                statusPredicate = statusPredicate
                                 .Or(e =>
                                    e.Relationships.Any(o =>
                                        (o.Type == RelationshipType.MessageRecipient || o.Type == RelationshipType.CustomerMessage)
                                        && (o.FromEntityType == EntityType.Customer || o.ToEntityType == EntityType.Customer)))
                                 .And(e => e.Tags.Any(o => o.TagType == TagType.Environment));
                            }
                        }
                        else if (string.Equals(statusString, "Admin", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(statusString, "Client", StringComparison.OrdinalIgnoreCase))
                        {
                            // has no email recipient and is admin tagged
                            statusPredicate = statusPredicate
                                 .Or(e =>
                                    e.Relationships.Any(o =>
                                        (o.Type == RelationshipType.MessageRecipient || o.Type == RelationshipType.OrganisationMessage)
                                        && (o.FromEntityType == EntityType.Organisation || o.ToEntityType == EntityType.Organisation)));
                        }
                        else if (string.Equals(statusString, "User", StringComparison.OrdinalIgnoreCase))
                        {
                            // if email recipient is user.
                            statusPredicate = statusPredicate
                                 .Or(e =>
                                    e.Relationships.Any(o =>
                                        (o.Type == RelationshipType.MessageRecipient || o.Type == RelationshipType.UserMessage)
                                        && (o.FromEntityType == EntityType.User || o.ToEntityType == EntityType.User)));
                        }
                        else
                        {
                            statusPredicate = statusPredicate
                                .Or(e => e.Tags.Any(o => o.Value == statusString && o.TagType == TagType.UserDefined));
                        }
                    }
                }

                joinQuery = joinQuery.Where(statusPredicate);
            }

            // filter by Sources as entities.
            var entityTypes = this.ConvertFilterSourcesToEntityTypes(filters);
            if (entityTypes.Any())
            {
                joinQuery = joinQuery.Where(e => e.Relationships.Any(
                    o => entityTypes.Contains(o.FromEntityType) || entityTypes.Contains(o.ToEntityType)));
            }

            // filter by tags.
            if (filters.Tags != null && filters.Tags.Any())
            {
                var statusPredicate = PredicateBuilder.New<EmailTagRelationshipModel>(false);
                foreach (var tag in filters.Tags)
                {
                    statusPredicate = statusPredicate.Or(e => e.Tags.Any(o => o.Value == tag));
                }

                joinQuery = joinQuery.Where(statusPredicate);
            }

            if (filters.DateIsAfterTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                joinQuery = joinQuery.Where(e => e.Email.CreatedTicksSinceEpoch > filters.DateIsAfterTicks);
            }

            if (filters.DateIsBeforeTicks.HasValue && filters.DateFilteringPropertyName != null)
            {
                joinQuery = joinQuery.Where(e => e.Email.CreatedTicksSinceEpoch < filters.DateIsBeforeTicks);
            }

            if (filters.SearchTerms.Any())
            {
                var searchExpression = PredicateBuilder.New<EmailTagRelationshipModel>(false);
                foreach (var searchTerm in filters.SearchTerms)
                {
                    var unescapedSearchTerm = Uri.UnescapeDataString(searchTerm);
                    searchExpression.Or(e => e.Email.To.IndexOf(unescapedSearchTerm) >= 0
                                          || e.Email.Subject.IndexOf(unescapedSearchTerm) >= 0);
                }

                joinQuery = joinQuery.Where(searchExpression);
            }

            if (!string.IsNullOrEmpty(filters.SortBy))
            {
                var sortByVal = filters.SortBy.Contains("Email.") ? filters.SortBy : "Email." + filters.SortBy;
                joinQuery = joinQuery.Order(sortByVal, filters.SortOrder).Paginate(filters);
            }

            return joinQuery;
        }

        private List<EntityType> ConvertFilterSourcesToEntityTypes(EntityListFilters filters)
        {
            List<EntityType> entityTypes = new List<EntityType>();
            if (filters.Sources != null)
            {
                var names = Enum.GetNames(typeof(EntityType));
                foreach (var emailSource in filters.Sources)
                {
                    var match = names.FirstOrDefault(name => string.Equals(emailSource.ToLower(), name.ToLower()));

                    if (match != null)
                    {
                        var result = Enum.TryParse(match, out EntityType val);
                        entityTypes.Add(val);
                    }
                }
            }

            return entityTypes;
        }

        private class EmailTagRelationshipModel
        {
            public Email Email { get; set; }

            public IEnumerable<Relationship> Relationships { get; set; }

            public IEnumerable<Tag> Tags { get; set; }
        }

        private class ProductEmailReportModel
        {
            public Email Email { get; set; }

            public CustomerReadModel Customer { get; set; }

            public NewQuoteReadModel Quote { get; set; }

            public PolicyReadModel Policy { get; set; }

            public ClaimReadModel Claim { get; set; }

            public Product Product { get; set; }

            public PersonReadModel Person { get; set; }
        }

        private class SystemEmailReportModel
        {
            public Email Email { get; set; }

            public UserReadModel User { get; set; }
        }
    }
}
