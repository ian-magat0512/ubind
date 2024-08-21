// <copyright file="ClaimAggregate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// Suppress IDE0060 because there are Apply Event method in which parameter are not in used.
// And we cannot remove the apply method otherwise it will throw an exception.
#pragma warning disable IDE0060 // Remove unused parameter

namespace UBind.Domain.Aggregates.Claim
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json.Serialization;
    using CSharpFunctionalExtensions;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.AdditionalPropertyValue;
    using UBind.Domain.Aggregates.Claim.Entities;
    using UBind.Domain.Aggregates.Claim.Workflow;
    using UBind.Domain.Aggregates.Customer;
    using UBind.Domain.Aggregates.Person;
    using UBind.Domain.Aggregates.Quote;
    using UBind.Domain.Entities;
    using UBind.Domain.Enums;
    using UBind.Domain.Helpers;
    using UBind.Domain.Imports;
    using UBind.Domain.Product;
    using UBind.Domain.ReadModel.Claim;
    using UBind.Domain.ReadModel.Policy;

    /// <summary>
    /// For representing an insurance claim.
    /// </summary>
    public partial class ClaimAggregate
        : AggregateRootEntity<ClaimAggregate, Guid>,
          IProductContext,
          IAdditionalProperties,
          IAdditionalPropertyValueEntityAggregate,
          IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueInitializedEvent<ClaimAggregate, IClaimEventObserver>>,
          IApplyAdditionalPropertyValueEvent<
            AdditionalPropertyValueUpdatedEvent<ClaimAggregate, IClaimEventObserver>>
    {
        /// <summary>
        /// It should be needed during de/serialization of the aggregate.
        /// </summary>
        [JsonConstructor]
        public ClaimAggregate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimAggregate"/> class.
        /// </summary>
        /// <param name="referenceNumber">A unique reference number for the claim.</param>
        /// <param name="quoteAggregate">The quote the claim pertains to.</param>
        /// <param name="personId">The ID of the person the claim is for.</param>
        /// <param name="customerFullName">The full name of the customer the claim is for.</param>
        /// <param name="customerPreferredName">The preferred name of the customer the claim is for.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="createdTimestamp"> The time the claim aggregate was created.</param>
        private ClaimAggregate(
            string referenceNumber,
            QuoteAggregate quoteAggregate,
            Guid? personId,
            string customerFullName,
            string customerPreferredName,
            Guid? performingUserId,
            Instant createdTimestamp,
            DateTimeZone timeZone)
        {
            var @event = new ClaimInitializedEvent(
                quoteAggregate.TenantId,
                quoteAggregate.OrganisationId,
                quoteAggregate.ProductId,
                quoteAggregate.Environment,
                quoteAggregate.IsTestData,
                Guid.NewGuid(),
                quoteAggregate.Id,
                referenceNumber,
                quoteAggregate.Policy.PolicyNumber,
                quoteAggregate.CustomerId,
                personId,
                customerFullName,
                customerPreferredName,
                performingUserId,
                createdTimestamp,
                timeZone);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Creates a claim aggregate which is not associated with a policy.
        /// </summary>
        private ClaimAggregate(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            string referenceNumber,
            bool isTestData,
            Guid? customerId,
            Guid? personId,
            string customerFullName,
            string customerPreferredName,
            Guid? performingUserId,
            Instant createdTimestamp,
            DateTimeZone timeZone)
        {
            var @event = new ClaimInitializedEvent(
                tenantId,
                organisationId,
                productId,
                environment,
                isTestData,
                Guid.NewGuid(),
                null,
                referenceNumber,
                null,
                customerId,
                personId,
                customerFullName,
                customerPreferredName,
                performingUserId,
                createdTimestamp,
                timeZone);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimAggregate"/> class,
        /// calculating state by re-applying existing events.
        /// </summary>
        /// <param name="events">Existing events.</param>
        private ClaimAggregate(
            IEnumerable<IEvent<ClaimAggregate, Guid>> events)
            : base(events)
        {
        }

        private ClaimAggregate(
            string referenceNumber,
            PolicyReadModel policy,
            PersonAggregate person,
            ClaimImportData data,
            Guid? performingUserId,
            Instant timestamp)
        {
            var @event = new ClaimImportedEvent(referenceNumber, policy, person, data, performingUserId, timestamp, policy.TimeZone);
            this.ApplyNewEvent(@event);
        }

        public override AggregateType AggregateType => AggregateType.Claim;

        /// <summary>
        /// Gets the claim entity.
        /// </summary>
        public Claim Claim { get; private set; }

        /// <summary>
        /// Gets the ID of the customer the claim relates to.
        /// </summary>
        public Guid? CustomerId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the quote is assigned to a customer.
        /// </summary>
        public bool HasCustomer => this.CustomerId.HasValue && this.CustomerId.Value != Guid.Empty;

        /// <summary>
        /// Gets the ID of the policy the claim relates to.
        /// </summary>
        public Guid? PolicyId { get; private set; }

        /// <summary>
        /// Gets the ID of the product the claim is for.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the ID of the tenant the quote is for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the organisation the quote is for.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to return a test data.
        /// </summary>
        public bool IsTestData { get; private set; }

        /// <summary>
        /// Gets the ID of the environment the quote belongs to.
        /// </summary>
        public DeploymentEnvironment Environment { get; private set; }

        public Guid? OwnerUserId { get; private set; }

        /// <summary>
        /// Gets the product context that the claim belongs to.
        /// </summary>
        public ProductContext ProductContext =>
            new ProductContext(this.TenantId, this.ProductId, this.Environment);

        /// <summary>
        /// Gets the current collection of additional property values.
        /// </summary>
        public List<AdditionalPropertyValue> AdditionalPropertyValues { get; private set; }
            = new List<AdditionalPropertyValue>();

        /// <summary>
        /// Gets or sets the latest projected read model.
        /// </summary>
        public ClaimReadModel? ReadModel { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimAggregate"/> class.
        /// </summary>
        /// <param name="referenceNumber">A unique reference number for the claim.</param>
        /// <param name="quoteAggregate">The quote aggregate for the policy the claim pertains to.</param>
        /// <param name="personId">The ID of the person the claim is for.</param>
        /// <param name="customerFullName">The full name of the customer the claim is for.</param>
        /// <param name="customerPreferredName">The preferred name of the customer the claim is for.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="createdTimestamp"> The time the claim aggregate was created.</param>
        /// <returns>A new instance of <see cref="ClaimAggregate"/>.</returns>
        public static ClaimAggregate CreateForPolicy(
            string referenceNumber,
            QuoteAggregate quoteAggregate,
            Guid? personId,
            string customerFullName,
            string customerPreferredName,
            Guid? performingUserId,
            Instant createdTimestamp)
        {
            return new ClaimAggregate(
                referenceNumber,
                quoteAggregate,
                personId,
                customerFullName,
                customerPreferredName,
                performingUserId,
                createdTimestamp,
                quoteAggregate.Policy.TimeZone);
        }

        public static ClaimAggregate CreateWithoutPolicy(
            Guid tenantId,
            Guid organisationId,
            Guid productId,
            DeploymentEnvironment environment,
            string referenceNumber,
            bool isTestData,
            Guid? customerId,
            Guid? personId,
            string customerFullName,
            string customerPreferredName,
            Guid? performingUserId,
            Instant createdTimestamp,
            DateTimeZone timeZone)
        {
            return new ClaimAggregate(
                tenantId,
                organisationId,
                productId,
                environment,
                referenceNumber,
                isTestData,
                customerId,
                personId,
                customerFullName,
                customerPreferredName,
                performingUserId,
                createdTimestamp,
                timeZone);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimAggregate"/> class,
        /// calculating state by re-applying existing events.
        /// </summary>
        /// <param name="events">Existing events.</param>
        /// <returns>A new instance of <see cref="ClaimAggregate"/>, with state updated from existing events.</returns>
        public static ClaimAggregate LoadFromEvents(IEnumerable<IEvent<ClaimAggregate, Guid>> events)
        {
            return new ClaimAggregate(events);
        }

        /// <summary>
        /// Factory method for creating claims.
        /// </summary>
        /// <param name="referenceNumber">A pseudo-random unique reference number for the claim.</param>
        /// <param name="policy">The policy read model of the claim relates to.</param>
        /// <param name="person">The person aggregate the claim is for.</param>
        /// <param name="data">The claim import data object.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">The time the claim aggregate was notified.</param>
        /// <returns>A new instance of <see cref="ClaimAggregate"/>.</returns>
        public static ClaimAggregate CreateImportedClaim(
            string referenceNumber,
            PolicyReadModel policy,
            PersonAggregate person,
            ClaimImportData data,
            Guid? performingUserId,
            Instant timestamp)
        {
            return new ClaimAggregate(referenceNumber, policy, person, data, performingUserId, timestamp);
        }

        /// <summary>
        /// Update the claim aggregate using the imported event.
        /// </summary>
        /// <param name="data">The claim import data object.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">The time the claim aggregate was updated.</param>
        public void UpdateClaimFromImport(ClaimImportData data, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ClaimUpdateImportedEvent(this.TenantId, this.Id, data, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Raise a claim actualised event.
        /// </summary>
        /// <param name="performingUserId">The identifier of the performing user.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void Actualise(Guid? performingUserId, Instant timestamp)
        {
            if (!this.Claim.IsActualised)
            {
                var @event = new ClaimActualisedEvent(this.TenantId, this.Claim.ClaimId, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Claim change state per action.
        /// </summary>
        public void ChangeClaimState(
            ClaimActions operationName, Guid? performingUserId, Instant timestamp, IClaimWorkflow workflow)
        {
            var @event = this.Claim.ChangeClaimState(operationName, performingUserId, timestamp, workflow);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Assign a claim number to this claim.
        /// </summary>
        /// <param name="claimNumber">The claim number.</param>
        /// <param name="performingUserId">The user's ID.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AssignClaimNumber(string claimNumber, Guid? performingUserId, Instant timestamp)
        {
            var @event = this.Claim.AssignClaimNumber(claimNumber, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Unassign a claim number from this claim.
        /// </summary>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void UnassignClaimNumber(Guid? performingUserId, Instant timestamp)
        {
            var @event = this.Claim.UnassignClaimNumber(performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Updates the form data for the claim.
        /// </summary>
        /// <param name="formData">The latest form data for the claim.</param>
        /// <param name="performingUserId">The userId who updates form data.</param>
        /// <param name="timestamp">The time form data was updated.</param>'
        public void UpdateFormData(string formData, Guid? performingUserId, Instant timestamp)
        {
            var events = this.Claim.UpdateFormData(formData, performingUserId, timestamp);
            foreach (var @event in events)
            {
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Record the creation of a new calculation result.
        /// </summary>
        /// <param name="formDataJson">The form data used in the calculation.</param>
        /// <param name="calculationResultJson">The calculation result json.</param>
        /// <param name="performingUserId">The userId who records the calculation.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void RecordCalculationResult(
            string formDataJson, string calculationResultJson, Guid? performingUserId, Instant timestamp)
        {
            var events = this.Claim.RecordCalculationResult(
                formDataJson, calculationResultJson, performingUserId, timestamp);
            foreach (var @event in events)
            {
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Attach a file.
        /// </summary>
        /// <param name="fileContentId">The file Content Id.</param>
        /// <param name="name">The file name.</param>
        /// <param name="type">The file type.</param>
        /// <param name="fileSize">The file size.</param>
        /// <param name="performingUserId">The userId who attached the file.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AttachFile(
            Guid fileContentId,
            string name,
            string type,
            long fileSize,
            Guid? performingUserId,
            Instant timestamp)
        {
            var document = new ClaimFileAttachment(name, type, fileSize, fileContentId, timestamp);
            var @event = new ClaimFileAttachedEvent(this.TenantId, this.Id, document, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record the current workflow step.
        /// </summary>
        /// <param name="workflowStep">The workflow step.</param>
        /// <param name="performingUserId">The userId who record workflow step.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void RecordWorkflowStep(
            string workflowStep,
            Guid? performingUserId,
            Instant timestamp)
        {
            var @event = new ClaimWorkflowStepAssignedEvent(
                this.TenantId, this.Id, this.Claim.ClaimId, workflowStep, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Creates a version of the claim.
        /// </summary>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void CreateVersion(Guid? performingUserId, Instant timestamp)
        {
            var currentAttachments = this.Claim.GetClaimFileAttachments();
            var attachments = new List<ClaimFileAttachment>();
            attachments.AddRange(currentAttachments
               .Select(a => new ClaimFileAttachment(a.Name, a.Type, a.FileSize, a.FileContentId, a.CreatedTimestamp)));

            var @event = new ClaimVersionCreatedEvent(this.TenantId, this.Id, this.Claim, performingUserId, timestamp, attachments);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record an enquiry made for the claim.
        /// </summary>
        /// <param name="performingUserId">The Id who makes the inquiry.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void MakeEnquiry(Guid? performingUserId, Instant timestamp)
        {
            var @event = new ClaimEnquiryMadeEvent(this.TenantId, this.Id, this.Claim, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Associate Claim With Policy.
        /// </summary>
        /// <param name="claimId">The claim Id.</param>
        /// <param name="quoteAggregate">The quote aggregate.</param>
        /// <param name="personAggregate">The person aggregate.</param>
        /// <param name="performingUserId">The user Id.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AssociateClaimWithPolicy(
            Guid claimId,
            QuoteAggregate quoteAggregate,
            PersonAggregate personAggregate,
            Guid? performingUserId,
            Instant timestamp)
        {
            var @event = new AssociateClaimWithPolicyEvent(
                this.TenantId,
                claimId,
                quoteAggregate.Policy.QuoteId,
                quoteAggregate.Id,
                quoteAggregate.Policy.PolicyNumber,
                personAggregate?.Id,
                quoteAggregate.CustomerId,
                personAggregate?.PreferredName,
                personAggregate?.FullName,
                performingUserId,
                timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Disassociate Claim With Policy.
        /// </summary>
        /// <param name="claimId">The claim Id.</param>
        /// <param name="policyId">The policy Id.</param>
        /// <param name="performingUserId">The user Id.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void DisassociateClaimWithPolicy(
            Guid claimId,
            Guid policyId,
            Guid? performingUserId,
            Instant timestamp)
        {
            var @event = new DisassociateClaimWithPolicyEvent(
                this.TenantId,
                claimId,
                policyId,
                performingUserId,
                timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Deletes the claim and all its associated records.
        /// </summary>
        /// <param name="claimId">The ID of the claim to delete.</param>
        /// <param name="performingUserId">The ID of the user who deleted the policy.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void DeleteClaimRecords(Guid claimId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ClaimDeletedEvent(this.TenantId, claimId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Assign ownership of the claim to a given user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="performingUserId">The userId who assign to new owner.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AssignToOwner(Guid userId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new OwnershipAssignedEvent(
                this.TenantId, this.Id, userId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public void UnassignOwnership(Guid? performingUserId, Instant timestamp)
        {
            var @event = new OwnershipUnassignedEvent(
               this.TenantId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Assign the claim to a given customer.
        /// </summary>
        /// <param name="customer">The customer.</param>
        /// <param name="customerDetails">The customer details.</param>
        /// <param name="performingUserId">The userId who assigned quote to customer.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AssignToCustomer(
            CustomerAggregate customer, IPersonalDetails customerDetails, Guid? performingUserId, Instant timestamp)
        {
            this.AssignToCustomer(customer.Id, customer.PrimaryPersonId, customerDetails, performingUserId, timestamp);
        }

        /// <summary>
        /// Assign the claim to a given customer.
        /// </summary>
        /// <param name="customerId">The customers Id.</param>
        /// <param name="customerPrimaryPersonId">The customers primary person Id.</param>
        /// <param name="customerDetails">The customer details.</param>
        /// <param name="performingUserId">The userId who assigned quote to customer.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void AssignToCustomer(
            Guid customerId, Guid customerPrimaryPersonId, IPersonalDetails customerDetails, Guid? performingUserId, Instant timestamp)
        {
            var @event = new CustomerAssignedEvent(
                this.TenantId, this.Id, customerId, customerPrimaryPersonId, customerDetails, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record organisation migration and only applicable for an aggregate with an empty organisation Id.
        /// </summary>
        /// <param name="organisationId">The Id of the organisation to persist in this aggregate.</param>
        /// <param name="performingUserId">The user Id who updates the aggregate.</param>
        /// <param name="timestamp">The time the update was recorded.</param>
        /// <returns>A result indicating success or any error.</returns>
        public Result<Guid, Error> RecordOrganisationMigration(
            Guid organisationId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ClaimOrganisationMigratedEvent(this.TenantId, organisationId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);

            return Result.Success<Guid, Error>(@event.AggregateId);
        }

        /// <summary>
        /// Trigger the ApplyNewIdEvent that applies new id to this aggregate.
        /// </summary>
        /// <param name="tenantNewId">The tenants new Id.</param>
        /// <param name="productNewId">The products new Id.</param>
        /// <param name="performingUserId">The userId who did the action.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void TriggerApplyNewIdEvent(
            Guid tenantNewId, Guid productNewId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ApplyNewIdEvent(tenantNewId, this.Id, productNewId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <inheritdoc/>
        public void AddAdditionalPropertyValue(
            Guid tenantId,
            Guid entityId,
            Guid additionalPropertyDefinitionId,
            string value,
            AdditionalPropertyDefinitionType propertyType,
            Guid? performingUserId,
            Instant createdTimestamp)
        {
            var initializedEvent = new AdditionalPropertyValueInitializedEvent<ClaimAggregate, IClaimEventObserver>(
                tenantId,
                this.Id,
                Guid.NewGuid(),
                additionalPropertyDefinitionId,
                entityId,
                value,
                propertyType,
                performingUserId,
                createdTimestamp);
            this.ApplyNewEvent(initializedEvent);
        }

        /// <inheritdoc/>
        public void UpdateAdditionalPropertyValue(
            Guid tenantId,
            Guid entityId,
            AdditionalPropertyDefinitionType type,
            Guid additionalPropertyDefinitionId,
            Guid additionalPropertyValueId,
            string value,
            Guid? performingUserId,
            Instant createdTimestamp)
        {
            var updateEvent = new AdditionalPropertyValueUpdatedEvent<ClaimAggregate, IClaimEventObserver>(
                tenantId,
                this.Id,
                value,
                performingUserId,
                createdTimestamp,
                type,
                additionalPropertyDefinitionId,
                additionalPropertyValueId,
                entityId);
            this.ApplyNewEvent(updateEvent);
        }

        /// <inheritdoc/>
        public void Apply(
            AdditionalPropertyValueUpdatedEvent<ClaimAggregate, IClaimEventObserver> aggregateEvent, int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.AddOrUpdate(this.AdditionalPropertyValues, aggregateEvent);
        }

        /// <inheritdoc/>
        public void Apply(
            AdditionalPropertyValueInitializedEvent<ClaimAggregate, IClaimEventObserver> aggregateEvent,
            int sequenceNumber)
        {
            AdditionalPropertyValueCollectionHelper.Add(this.AdditionalPropertyValues, aggregateEvent);
        }

        public void TransferToAnotherOrganisation(
            Guid organisationId, Guid personId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ClaimTransferredToAnotherOrganisationEvent(
                this.TenantId, organisationId, this.Id, personId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public override ClaimAggregate ApplyEventsAfterSnapshot(IEnumerable<IEvent<ClaimAggregate, Guid>> events, int latestSnapshotVersion)
        {
            this.ApplyEvents(events, latestSnapshotVersion);
            return this;
        }

        protected override void ApplyDerivedEvent(dynamic @event, int sequenceNumber)
        {
            this.Apply(@event, sequenceNumber);
        }

        private void Apply(ClaimFileAttachedEvent @event, int sequenceNumber)
        {
            this.Claim.Attachfile(@event.Attachment);
        }

        private void Apply(ClaimVersionFileAttachedEvent @event, int sequenceNumber)
        {
            // NOP.
        }

        private void Apply(ClaimInitializedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.CreatedTimestamp = @event.Timestamp;
            this.TenantId = @event.TenantId;
            this.OrganisationId = @event.OrganisationId;
            this.ProductId = @event.ProductId;
            this.Environment = @event.Environment;
            this.PolicyId = @event.PolicyId;
            this.CustomerId = @event.CustomerId;
            this.IsTestData = @event.IsTestData;
            this.Claim = Entities.Claim.CreateClaim(this, @event);
        }

        private void Apply(ClaimImportedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.CreatedTimestamp = @event.Timestamp;
            this.TenantId = @event.TenantId;
            this.OrganisationId = @event.OrganisationId;
            this.ProductId = @event.ProductId;
            this.Environment = @event.Environment;
            this.PolicyId = @event.PolicyId;
            this.CustomerId = @event.CustomerId;
            this.Claim = Entities.Claim.CreateImportedClaim(this, @event);
        }

        private void Apply(ClaimOrganisationMigratedEvent @event, int sequenceNumber)
        {
            this.OrganisationId = @event.OrganisationId;
        }

        private void Apply(ClaimUpdateImportedEvent @event, int sequenceNumber)
        {
            this.Claim.Apply(@event, sequenceNumber);
        }

        private void Apply(ClaimStatusUpdatedEvent @event, int sequenceNumber)
        {
            this.Claim.Apply(@event, sequenceNumber);
        }

        private void Apply(ClaimNumberUpdatedEvent @event, int sequenceNumber)
        {
            this.Claim.Apply(@event, sequenceNumber);
        }

        private void Apply(ClaimStateChangedEvent @event, int sequenceNumber)
        {
            this.Claim.Apply(@event, sequenceNumber);
        }

        private void Apply(ClaimVersionCreatedEvent @event, int sequenceNumber)
        {
            this.Claim.RecordVersioning(@event);
        }

        private void Apply(ClaimFormDataUpdatedEvent @event, int sequenceNumber)
        {
            this.Claim.Apply(@event, sequenceNumber);
        }

        private void Apply(ClaimCalculationResultCreatedEvent @event, int sequenceNumber)
        {
            this.Claim.Apply(@event, sequenceNumber);
        }

        private void Apply(ClaimWorkflowStepAssignedEvent @event, int sequenceNumber)
        {
            this.Claim.UpdateWorkflowStep(@event.WorkflowStep);
        }

        private void Apply(ClaimEnquiryMadeEvent @event, int sequenceNumber)
        {
            this.Claim.Apply(@event, sequenceNumber);
        }

        private void Apply(AssociateClaimWithPolicyEvent @event, int sequenceNumber)
        {
            this.Claim.Apply(@event, sequenceNumber);
            this.PolicyId = @event.PolicyId;
        }

        private void Apply(DisassociateClaimWithPolicyEvent @event, int sequenceNumber)
        {
            this.Claim.Apply(@event, sequenceNumber);
            this.PolicyId = null;
        }

        private void Apply(ClaimDeletedEvent @event, int sequenceNumber)
        {
            // NOP.
        }

        private void Apply(ClaimActualisedEvent @event, int sequenceNumber)
        {
            this.Claim.Apply(@event, sequenceNumber);
        }

        private void Apply(ApplyNewIdEvent applyNewIdEvent, int sequenceNumber)
        {
            this.TenantId = applyNewIdEvent.TenantId;
            this.ProductId = applyNewIdEvent.ProductNewId;
        }

        private void Apply(OwnershipAssignedEvent @event, int sequenceNumber)
        {
            this.OwnerUserId = @event.UserId;
        }

        private void Apply(OwnershipUnassignedEvent @event, int sequenceNumber)
        {
            this.OwnerUserId = null;
        }

        private void Apply(CustomerAssignedEvent @event, int sequenceNumber)
        {
            this.CustomerId = @event.CustomerId;
        }

        private void Apply(ClaimTransferredToAnotherOrganisationEvent @event, int sequenceNumber)
        {
            this.OrganisationId = @event.OrganisationId;
        }
    }
}
