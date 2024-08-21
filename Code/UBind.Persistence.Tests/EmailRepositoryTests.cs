// <copyright file="EmailRepositoryTests.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Humanizer;
    using Moq;
    using NodaTime;
    using UBind.Application.ExtensionMethods;
    using UBind.Application.Infrastructure;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Organisation;
    using UBind.Domain.Extensions;
    using UBind.Domain.Permissions;
    using UBind.Domain.Quote;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel.Email;
    using UBind.Domain.Redis;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class EmailRepositoryTests
    {
        [Fact]
        public void GetById_RecordHasNewIds_WhenCreatingTenantSpecificEmails()
        {
            // Arrange
            ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var emailAddress = "xxx+1@email.com";
            var tenantId = Guid.NewGuid();
            var sampleStringList = new List<string>() { emailAddress };
            var emailAttachments = new List<EmailAttachment>();
            var email = new Email(tenantId, Guid.NewGuid(), null, DeploymentEnvironment.Development, Guid.NewGuid(), sampleStringList, emailAddress,
                sampleStringList, sampleStringList, sampleStringList, "test", "test", "test", emailAttachments, new TestClock().Timestamp);
            stack.EmailRepository.Insert(email);
            stack.EmailRepository.SaveChanges();

            // Act
            var retrievedEmail = stack.EmailRepository.GetById(email.TenantId, email.Id);

            // Assert
            retrievedEmail.Should().NotBeNull();
            retrievedEmail.TenantId.Should().Be(tenantId);
            retrievedEmail.ProductId.Should().BeNull();
        }

        [Fact]
        public void GetById_RecordHasNewIds_WhenCreatingProductSpecificEmails()
        {
            // Arrange
            ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var emailAddress = "xxx+1@email.com";
            var tenantId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var sampleStringList = new List<string>() { emailAddress };
            var emailAttachments = new List<EmailAttachment>();
            var email = new Email(tenantId, Guid.NewGuid(), productId, DeploymentEnvironment.Development, Guid.NewGuid(), sampleStringList, emailAddress,
                sampleStringList, sampleStringList, sampleStringList, "test", "test", "test", emailAttachments, new TestClock().Timestamp);
            stack.EmailRepository.Insert(email);
            stack.EmailRepository.SaveChanges();

            // Act
            var retrievedEmail = stack.EmailRepository.GetById(email.TenantId, email.Id);

            // Assert
            retrievedEmail.Should().NotBeNull();
            retrievedEmail.TenantId.Should().Be(tenantId);
            retrievedEmail.ProductId.Should().Be(productId);
        }

        [Fact]
        public async Task GetEmailSummary_SuccessfulAccess_WhenAccessingEmailsFromAnotherOrganisation()
        {
            // Arrange
            ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var environment = DeploymentEnvironment.Production;
            var tenantId = Guid.NewGuid();
            Tenant tenant = TenantFactory.Create(tenantId);
            var defaultOrg = Organisation.CreateNewOrganisation(tenantId, "defaultorg", "defaultorg", null, null, stack.Clock.Now());
            tenant.SetDefaultOrganisation(defaultOrg.Id, stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            stack.CreateTenant(tenant);
            await stack.CreateOrganisation(defaultOrg);
            var productId = Guid.NewGuid();
            var product = ProductFactory.Create(productId, tenantId);
            stack.CreateProduct(product);

            var orgB = Organisation.CreateNewOrganisation(tenantId, "orgB", "orgB", null, null, stack.Clock.Now());
            var orgC = Organisation.CreateNewOrganisation(tenantId, "orgC", "orgC", null, null, stack.Clock.Now());
            await stack.CreateOrganisation(orgB);
            await stack.CreateOrganisation(orgC);

            // create quote
            var quote = QuoteFactory.CreateNewBusinessQuote(tenantId, productId, environment, organisationId: orgB.Id);
            await stack.QuoteAggregateRepository.Save(quote.Aggregate.WithCustomer());

            // create email 1
            var email = this.CreateEmail(tenantId, "1", orgB.Id, productId, environment, stack.Clock.Timestamp);
            var emailModel = new EmailModel(email, environment);
            EmailAndMetadata emailAndMetadata = new EmailAndMetadata(emailModel, stack.Clock.Now(), stack.FileContentRepository);
            emailAndMetadata.CreateRelationshipFromEntityToEmail(EntityType.Organisation, orgB.Id, RelationshipType.OrganisationMessage);
            emailAndMetadata.CreateRelationshipFromEmailToEntity(RelationshipType.MessageRecipient, EntityType.Organisation, orgC.Id);
            emailAndMetadata.CreateRelationshipFromEntityToEmail(EntityType.Quote, quote.Id, RelationshipType.QuoteMessage);
            stack.EmailRepository.InsertEmailAndMetadata(emailAndMetadata);

            // create email 2
            var email2 = this.CreateEmail(tenantId, "2", orgB.Id, productId, environment, stack.Clock.Timestamp);
            var emailModel2 = new EmailModel(email2, environment);
            EmailAndMetadata emailAndMetadata2 = new EmailAndMetadata(emailModel2, stack.Clock.Now(), stack.FileContentRepository);
            emailAndMetadata2.CreateRelationshipFromEntityToEmail(EntityType.Organisation, orgB.Id, RelationshipType.OrganisationMessage);
            emailAndMetadata2.CreateRelationshipFromEmailToEntity(RelationshipType.MessageRecipient, EntityType.Organisation, defaultOrg.Id);
            emailAndMetadata2.CreateRelationshipFromEntityToEmail(EntityType.Quote, quote.Id, RelationshipType.QuoteMessage);
            stack.EmailRepository.InsertEmailAndMetadata(emailAndMetadata2);

            // create email 3
            var email3 = this.CreateEmail(tenantId, "3", orgB.Id, productId, environment, stack.Clock.Timestamp);
            var emailModel3 = new EmailModel(email3, environment);
            EmailAndMetadata emailAndMetadata3 = new EmailAndMetadata(emailModel3, stack.Clock.Now(), stack.FileContentRepository);
            emailAndMetadata3.CreateRelationshipFromEntityToEmail(EntityType.Organisation, orgB.Id, RelationshipType.OrganisationMessage);
            emailAndMetadata3.CreateRelationshipFromEmailToEntity(RelationshipType.MessageRecipient, EntityType.Customer, quote.CustomerId.GetValueOrDefault());
            emailAndMetadata3.CreateRelationshipFromEntityToEmail(EntityType.Quote, quote.Id, RelationshipType.QuoteMessage);
            stack.EmailRepository.InsertEmailAndMetadata(emailAndMetadata3);

            // try to login as default org.
            var entityListFilter = new EntityListFilters
            {
                OrganisationIds = new List<Guid> { defaultOrg.Id },
                TenantId = tenantId,
                Environment = environment,
                EntityType = EntityType.Quote,
                EntityId = quote.Id,
            };

            var permissions = new List<Permission>()
            {
                Permission.ViewMessages,
                Permission.ViewAllMessages,
                Permission.AccessProductionData,
                Permission.ViewAllCustomersFromAllOrganisations,
                Permission.ManageAllCustomersForAllOrganisations,
            };

            var sessionId = Guid.NewGuid();
            var claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                        new Claim(ClaimNames.TenantId, tenant.Id.ToString()),
                        new Claim(ClaimNames.OrganisationId, defaultOrg.Id.ToString()),
                        new Claim(ClaimNames.CustomerId, quote.CustomerId.GetValueOrDefault().ToString()),
                        new Claim("SessionId", sessionId.ToString()),
                    }));
            var userSessionModel = new UserSessionModel(
                claimsPrincipal.GetTenantId(),
                sessionId,
                claimsPrincipal.GetOrganisationId(),
                claimsPrincipal.GetId().GetValueOrDefault(),
                claimsPrincipal.GetUserType().Humanize(),
                claimsPrincipal.GetCustomerId(),
                "abc@def.com",
                "abc",
                permissions,
                stack.Clock.Now(),
                stack.Clock.Now());
            stack.UserSessionRepositoryMock.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult<UserSessionModel?>(userSessionModel));
            await stack.AuthorisationService.ApplyViewMessageRestrictionsToFilters(claimsPrincipal, entityListFilter);

            // Act
            var emails = stack.EmailRepository.GetSummaries(tenantId, entityListFilter).ToList();

            // Assert
            Assert.True(emails.Exists(x => x.Subject == "test3"));
            Assert.True(emails.Exists(x => x.Subject == "test2"));
        }

        [Fact]
        public async Task GetEmailDetailsWithAttachments_SuccessfulAccess_WhenAccessingEmailsFromAnotherOrganisation()
        {
            // Arrange
            ApplicationStack stack = new ApplicationStack(DatabaseFixture.TestConnectionStringName);
            var environment = DeploymentEnvironment.Production;
            var tenantId = Guid.NewGuid();
            Tenant tenant = TenantFactory.Create(tenantId);
            var defaultOrg = Organisation.CreateNewOrganisation(tenantId, "defaultorg", "defaultorg", null, null, stack.Clock.Now());
            tenant.SetDefaultOrganisation(defaultOrg.Id, stack.Clock.GetCurrentInstant().Plus(Duration.FromMinutes(1)));
            stack.CreateTenant(tenant);
            await stack.CreateOrganisation(defaultOrg);
            var productId = Guid.NewGuid();
            var product = ProductFactory.Create(productId, tenantId);
            stack.CreateProduct(product);

            var orgB = Organisation.CreateNewOrganisation(tenantId, "orgB", "orgB", null, null, stack.Clock.Now());
            var orgC = Organisation.CreateNewOrganisation(tenantId, "orgC", "orgC", null, null, stack.Clock.Now());
            await stack.CreateOrganisation(orgB);
            await stack.CreateOrganisation(orgC);

            // create quote
            var quote = QuoteFactory.CreateNewBusinessQuote(tenantId, productId, environment, organisationId: orgB.Id);
            await stack.QuoteAggregateRepository.Save(quote.Aggregate.WithCustomer());

            // create email 1
            var email = this.CreateEmail(tenantId, "1", orgB.Id, productId, environment, stack.Clock.Timestamp);
            var emailModel = new EmailModel(email, environment);
            EmailAndMetadata emailAndMetadata = new EmailAndMetadata(emailModel, stack.Clock.Now(), stack.FileContentRepository);
            emailAndMetadata.CreateRelationshipFromEntityToEmail(EntityType.Organisation, orgB.Id, RelationshipType.OrganisationMessage);
            emailAndMetadata.CreateRelationshipFromEmailToEntity(RelationshipType.MessageRecipient, EntityType.Organisation, orgC.Id);
            emailAndMetadata.CreateRelationshipFromEntityToEmail(EntityType.Quote, quote.Id, RelationshipType.QuoteMessage);
            stack.EmailRepository.InsertEmailAndMetadata(emailAndMetadata);

            // create email 2
            var email2 = this.CreateEmail(tenantId, "2", orgB.Id, productId, environment, stack.Clock.Timestamp);
            var emailModel2 = new EmailModel(email2, environment);
            EmailAndMetadata emailAndMetadata2 = new EmailAndMetadata(emailModel2, stack.Clock.Now(), stack.FileContentRepository);
            emailAndMetadata2.CreateRelationshipFromEntityToEmail(EntityType.Organisation, orgB.Id, RelationshipType.OrganisationMessage);
            emailAndMetadata2.CreateRelationshipFromEmailToEntity(RelationshipType.MessageRecipient, EntityType.Organisation, defaultOrg.Id);
            emailAndMetadata2.CreateRelationshipFromEntityToEmail(EntityType.Quote, quote.Id, RelationshipType.QuoteMessage);
            stack.EmailRepository.InsertEmailAndMetadata(emailAndMetadata2);

            // create email 3
            var email3 = this.CreateEmail(tenantId, "3", orgB.Id, productId, environment, stack.Clock.Timestamp);
            var emailModel3 = new EmailModel(email3, environment);
            EmailAndMetadata emailAndMetadata3 = new EmailAndMetadata(emailModel3, stack.Clock.Now(), stack.FileContentRepository);
            emailAndMetadata3.CreateRelationshipFromEntityToEmail(EntityType.Organisation, orgB.Id, RelationshipType.OrganisationMessage);
            emailAndMetadata3.CreateRelationshipFromEmailToEntity(RelationshipType.MessageRecipient, EntityType.Customer, quote.CustomerId.GetValueOrDefault());
            emailAndMetadata3.CreateRelationshipFromEntityToEmail(EntityType.Quote, quote.Id, RelationshipType.QuoteMessage);
            stack.EmailRepository.InsertEmailAndMetadata(emailAndMetadata3);

            var permissions = new List<Permission>()
            {
                Permission.ViewMessages,
                Permission.ViewAllMessages,
                Permission.AccessProductionData,
            };

            var sessionId = Guid.NewGuid();
            var claimsPrincipal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                        new Claim(ClaimTypes.Role, UserType.Client.Humanize()),
                        new Claim(ClaimNames.TenantId, tenant.Id.ToString()),
                        new Claim(ClaimNames.OrganisationId, defaultOrg.Id.ToString()),
                        new Claim(ClaimNames.CustomerId, quote.CustomerId.GetValueOrDefault().ToString()),
                        new Claim("SessionId", sessionId.ToString()),
                    }));
            var userSessionModel = new UserSessionModel(
                claimsPrincipal.GetTenantId(),
                sessionId,
                claimsPrincipal.GetOrganisationId(),
                claimsPrincipal.GetId().GetValueOrDefault(),
                claimsPrincipal.GetUserType().Humanize(),
                claimsPrincipal.GetCustomerId(),
                "abc@def.com",
                "abc",
                permissions,
                stack.Clock.Now(),
                stack.Clock.Now());
            stack.UserSessionRepositoryMock.Setup(s => s.Get(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Returns(Task.FromResult<UserSessionModel?>(userSessionModel));

            // Act
            var emailRecord = stack.EmailRepository.GetEmailDetailsWithAttachments(tenantId, email.Id);
            var email2Record = stack.EmailRepository.GetEmailDetailsWithAttachments(tenantId, email2.Id);
            var email3Record = stack.EmailRepository.GetEmailDetailsWithAttachments(tenantId, email3.Id);

            // Assert
            await stack.AuthorisationService.ThrowIfUserCannotViewEmail(claimsPrincipal, email2Record);
            await stack.AuthorisationService.ThrowIfUserCannotViewEmail(claimsPrincipal, email3Record);
            await stack.AuthorisationService.ThrowIfUserCannotViewEmail(claimsPrincipal, emailRecord);
        }

        private Email CreateEmail(Guid tenantId, string subject, Guid organisationId, Guid productId, DeploymentEnvironment environment, Instant timestamp)
        {
            // create email 1
            var email = new Email(
                tenantId,
                organisationId,
                productId,
                environment,
                Guid.NewGuid(),
                new List<string> { "jeo" },
                "test@gmail.com",
                new List<string>(),
                new List<string>(),
                new List<string>(),
                "test" + subject,
                "test" + subject,
                "test" + subject,
                new List<EmailAttachment>(),
                timestamp);

            return email;
        }
    }
}
