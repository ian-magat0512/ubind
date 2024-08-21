// <copyright file="OrganisationReadModelRepositoryTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace UBind.Persistence.Tests.ReadModels
{
    using System.Diagnostics;
    using FluentAssertions;
    using UBind.Application.Commands.Organisation;
    using UBind.Domain.ReadModel;
    using UBind.Domain.Tests.Fakes;
    using UBind.Persistence.Tests.Fakes;
    using Xunit;

    [Collection(DatabaseCollection.Name)]
    public class OrganisationReadModelRepositoryTests
    {
        private Func<ApplicationStack> stackFactory = () => new ApplicationStack(
            DatabaseFixture.TestConnectionStringName, ApplicationStackConfiguration.Default);

        [Fact]
        public async Task GetIdsOfDescendantOrganisationsOfOrganisation_ExecutionTimeDoesNotExceedTheThreshold()
        {
            // The threshold for the maximum acceptable execution time
            // As per John, the execution time for this scenario should not exceed 500ms
            double maxExecutionTimeMilliseconds = 500;

            // Arrange
            var tenantId = Guid.NewGuid();
            using (var stack = this.stackFactory())
            {
                stack.CreateTenant(TenantFactory.Create(tenantId, "test"));

                // Create a default organisation
                var defaultOrganisation = await stack.OrganisationService.CreateDefaultAsync(tenantId, "default", "Default");
                var managingOrganisationId = defaultOrganisation.Id;

                // Create 1000 organisations
                for (int i = 1; i <= 1000; i++)
                {
                    var organisation = await this.CreateOrganisation(stack, tenantId, $"org{i}", $"Org{i}", managingOrganisationId);

                    // Change the managing organisation every 150 organisations, in this way, the hierarchy of the organisations
                    // will have a depth of 6
                    if (i % 150 == 0)
                    {
                        managingOrganisationId = organisation.Id;
                    }
                }

                // Create a Stopwatch
                Stopwatch stopwatch = new Stopwatch();

                // Act
                stopwatch.Start(); // Start the stopwatch
                var descendants = await stack.OrganisationReadModelRepository
                    .GetIdsOfDescendantOrganisationsOfOrganisation(tenantId, defaultOrganisation.Id); // Execute the method
                stopwatch.Stop(); // Stop the stopwatch

                // Assert
                TimeSpan elapsed = stopwatch.Elapsed;
                double executionTimeMilliseconds = elapsed.TotalMilliseconds;

                executionTimeMilliseconds.Should().BeLessThan(maxExecutionTimeMilliseconds);
                descendants.Should().HaveCount(1000);
            }
        }

        [Fact]
        public async Task GetIdsOfDescendantOrganisationsOfOrganisation_ReturnsTheListOfDescendatsOrganisationOfAnOrganisation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            using (var stack = this.stackFactory())
            {
                stack.CreateTenant(TenantFactory.Create(tenantId, "test2"));

                // Create a default organisation
                var defaultOrganisation = await stack.OrganisationService.CreateDefaultAsync(tenantId, "default", "Default");

                // Create org0 with defaultOrganisation as managing organisation
                var org0 = await this.CreateOrganisation(stack, tenantId, $"org0", $"Org0", defaultOrganisation.Id);

                // Create org1 with org0 as managing organisation
                var org1 = await this.CreateOrganisation(stack, tenantId, $"org1", $"Org1", defaultOrganisation.Id);

                // Create descendants of org1
                var org2 = await this.CreateOrganisation(stack, tenantId, $"org2", $"Org2", org1.Id);
                var org3 = await this.CreateOrganisation(stack, tenantId, $"org3", $"Org3", org2.Id);

                // Act
                // Get descendants of org1
                var descendantsOfOrg1 = await stack.OrganisationReadModelRepository
                    .GetIdsOfDescendantOrganisationsOfOrganisation(tenantId, org1.Id);

                // Assert
                // Should not contain default organisation as it is its current managing organisation
                descendantsOfOrg1.Should().NotContain(defaultOrganisation.Id);

                // Should not contain itself
                descendantsOfOrg1.Should().NotContain(org1.Id);

                // Should contain its descendants (org2 and org3)
                descendantsOfOrg1.Should().Equal(new Guid[] { org2.Id, org3.Id });
            }
        }

        [Fact]
        public async Task GetIdsOfDescendantOrganisationsOfOrganisation_ReturnsAnEmptyList_WhenTheOrganisationIsNotManagingAnyOrganisation()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            using (var stack = this.stackFactory())
            {
                stack.CreateTenant(TenantFactory.Create(tenantId, "test3"));

                // Create a default organisation
                var defaultOrganisation = await stack.OrganisationService.CreateDefaultAsync(tenantId, "default", "Default");

                // Create an organisation that is not managing any organisation
                var org = await this.CreateOrganisation(stack, tenantId, $"org", $"Org", defaultOrganisation.Id);

                // Act
                var descendants = await stack.OrganisationReadModelRepository
                    .GetIdsOfDescendantOrganisationsOfOrganisation(tenantId, org.Id);

                // Assert
                descendants.Should().BeEmpty();
            }
        }

        private async Task<IOrganisationReadModelSummary> CreateOrganisation(
            ApplicationStack stack,
            Guid tenantId,
            string alias,
            string name,
            Guid? managingOrganisationId = null)
        {
            var organisation = await stack.OrganisationService.CreateActiveNonDefaultAsync(tenantId, alias, name, null);
            if (managingOrganisationId.HasValue)
            {
                await stack.Mediator.Send(new SetManagingOrganisationCommand(tenantId, organisation.Id, managingOrganisationId));
            }

            return organisation;
        }
    }
}
