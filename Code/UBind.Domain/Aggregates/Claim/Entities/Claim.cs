// <copyright file="Claim.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

#pragma warning disable IDE0060 // Remove unused parameter

namespace UBind.Domain.Aggregates.Claim.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CSharpFunctionalExtensions;
    using Humanizer;
    using Newtonsoft.Json.Linq;
    using NodaTime;
    using UBind.Domain;
    using UBind.Domain.Aggregates.Claim.Workflow;
    using UBind.Domain.Entities;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Extensions;
    using UBind.Domain.Services;

    /// <summary>
    /// Represents a Claim within a ClaimAggregate.
    /// </summary>
    public class Claim
    {
        private readonly Guid aggregateId;
        private readonly IList<ClaimDataUpdate<ClaimCalculationResult>> calculationResults = new List<ClaimDataUpdate<ClaimCalculationResult>>();
        private readonly IList<ClaimDataUpdate<ClaimFormData>> formDataUpdates = new List<ClaimDataUpdate<ClaimFormData>>();
        private readonly IList<ClaimVersion> claimVersions = new List<ClaimVersion>();
        private readonly IList<ClaimStateChange> claimStateChangeUpdates = new List<ClaimStateChange>();
        private readonly IList<ClaimFileAttachment> claimFileAttachments = new List<ClaimFileAttachment>();

        private Claim(ClaimAggregate aggregate, ClaimAggregate.ClaimInitializedEvent @event)
        {
            this.TenantId = aggregate.TenantId;
            this.ProductId = aggregate.ProductId;
            this.aggregateId = aggregate.Id;
            this.ClaimId = aggregate.Id; // Claims so not have their own Id at present, but they may do in future.
            this.CreatedTimestamp = @event.Timestamp;
            this.ClaimReference = @event.ReferenceNumber;
            this.ClaimStatus = Domain.ClaimState.Nascent;
            this.PolicyId = @event.PolicyId;
            this.TimeZone = Timezones.GetTimeZoneByIdOrDefault(@event.TimeZoneId);
        }

        private Claim(ClaimAggregate aggregate, ClaimAggregate.ClaimImportedEvent @event)
        {
            this.TenantId = aggregate.TenantId;
            this.ProductId = aggregate.ProductId;
            this.aggregateId = aggregate.Id;
            this.ClaimId = aggregate.Id; // Claims so not have their own Id at present, but they may do in future.
            this.CreatedTimestamp = @event.Timestamp;
            this.ClaimReference = @event.ReferenceNumber;
            this.ClaimNumber = @event.ClaimNumber;
            this.ClaimStatus = @event.Status ?? ClaimState.Incomplete;
            this.CommonData.Amount = @event.Amount;
            this.CommonData.Description = @event.Description;
            this.PolicyId = @event.PolicyId;
            this.TimeZone = Timezones.GetTimeZoneByIdOrDefault(@event.TimeZoneId);
            this.CommonData.IncidentTimestamp = @event.IncidentDate?.At(new LocalTime(12, 0))
                .InZoneLeniently(this.TimeZone).ToInstant();
        }

        /// <summary>
        /// Gets the ID of the claim.
        /// </summary>
        /// <remarks>
        /// At the moment this is just the aggregate Id, but in future there may be multiple claim entities in an aggregate.
        /// </remarks>
        public Guid ClaimId { get; private set; }

        /// <summary>
        /// Gets the reference number of the claim.
        /// </summary>
        public string ClaimNumber { get; private set; }

        /// <summary>
        /// Gets the reference number of the claim.
        /// </summary>
        public string ClaimReference { get; private set; }

        /// <summary>
        /// Gets well-known claim data.
        /// </summary>
        public ClaimData CommonData { get; private set; } = new ClaimData();

        /// <summary>
        /// Gets the latest claim state change, or null, if no quote state change operation exists.
        /// </summary>
        public ClaimStateChange LatestClaimStateChange => this.claimStateChangeUpdates.LastOrDefault();

        /// <summary>
        /// Gets the claim status.
        /// </summary>
        public string ClaimStatus { get; private set; }

        /// <summary>
        /// Gets the claim previousStatus.
        /// </summary>
        public string PreviousStatus { get; private set; }

        /// <summary>
        /// Gets the time the claim was created.
        /// </summary>
        public Instant CreatedTimestamp { get; private set; }

        /// <summary>
        /// Gets the ID of the Tenant the claim is for.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the ID of the product the claim is for.
        /// </summary>
        public Guid ProductId { get; private set; }

        /// <summary>
        /// Gets the ID of the quote the claim is for.
        /// </summary>
        public Guid? PolicyId { get; private set; }

        /// <summary>
        /// Gets the latest form data, or null if no form data exists.
        /// </summary>
        public ClaimDataUpdate<ClaimCalculationResult> LatestCalculationResult => this.calculationResults.LastOrDefault();

        /// <summary>
        /// Gets the latest version number if one exists, otherwise zero.
        /// </summary>
        public int VersionNumber => this.claimVersions.Select(v => v.Number).LastOrDefault();

        /// <summary>
        /// Gets the claim version id.
        /// </summary>
        public Guid ClaimVersionId => this.claimVersions.Select(v => v.VersionId).LastOrDefault();

        /// <summary>
        /// Gets the quote workflow step.
        /// </summary>
        public string WorkflowStep { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to return if the claim is already actualised.
        /// </summary>
        public bool IsActualised { get; private set; }

        public DateTimeZone TimeZone { get; private set; }

        /// <summary>
        /// Create a new instance of <see cref="Claim"/> for a newly created claim.
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="event">The intialized event.</param>
        /// <returns>A new instance of <see cref="Claim"/> for a new claim.</returns>
        public static Claim CreateClaim(
            ClaimAggregate aggregate, ClaimAggregate.ClaimInitializedEvent @event)
        {
            return new Claim(aggregate, @event);
        }

        /// <summary>
        /// Create a new instance of <see cref="Claim"/> for an imported claim.
        /// </summary>
        /// <param name="aggregate">The aggregate.</param>
        /// <param name="event">The claim imported event.</param>
        /// <returns>A new instance of <see cref="Claim"/> for the imported claim.</returns>
        public static Claim CreateImportedClaim(
            ClaimAggregate aggregate, ClaimAggregate.ClaimImportedEvent @event)
        {
            return new Claim(aggregate, @event);
        }

        /// <summary>
        /// Apply an enquiry made event.
        /// </summary>
        /// <param name="event">The claim enquiry made event.</param>
        public void Apply(ClaimAggregate.ClaimEnquiryMadeEvent @event, int sequenceNumber)
        {
            // NOP.
        }

        /// <summary>
        /// Assign a claim number to this claim.
        /// </summary>
        /// <param name="claimNumber">The quote number.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <returns>Quote number assigned event and state chaned event.</returns>
        public ClaimAggregate.ClaimNumberUpdatedEvent AssignClaimNumber(string claimNumber, Guid? performingUserId, Instant timestamp)
        {
            return new ClaimAggregate.ClaimNumberUpdatedEvent(this.TenantId, this.ClaimId, claimNumber, performingUserId, timestamp);
        }

        /// <summary>
        /// Unassign a claim number to from claim.
        /// </summary>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <returns>Quote number assigned event and state chaned event.</returns>
        public ClaimAggregate.ClaimNumberUpdatedEvent UnassignClaimNumber(Guid? performingUserId, Instant timestamp)
        {
            return new ClaimAggregate.ClaimNumberUpdatedEvent(this.TenantId, this.ClaimId, null, performingUserId, timestamp);
        }

        /// <summary>
        /// Record the creation of a new calculation result.
        /// </summary>
        /// <param name="formDataJson">The form data used in the calculation.</param>
        /// <param name="calculationResultJson">The calculation result json.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">A timestamp.</param>
        /// <returns>Events for form data update and calculation result creation.</returns>
        public IEnumerable<IEvent<ClaimAggregate, Guid>> RecordCalculationResult(string formDataJson, string calculationResultJson, Guid? performingUserId, Instant timestamp)
        {
            var events = new List<IEvent<ClaimAggregate, Guid>>();

            var formUpdateEvent = new ClaimAggregate.ClaimFormDataUpdatedEvent(
                this.TenantId, this.aggregateId, this.ClaimId, formDataJson, performingUserId, timestamp);
            events.Add(formUpdateEvent);

            ClaimCalculationResult newCalculationResult = null;
            newCalculationResult = ClaimCalculationResult.Create(
                    formUpdateEvent.FormUpdateId, calculationResultJson);
            var calculationResultCreatedEvent = new ClaimAggregate.ClaimCalculationResultCreatedEvent(
                this.TenantId, this.ClaimId, newCalculationResult, performingUserId, timestamp);
            events.Add(calculationResultCreatedEvent);

            var newWellKnownData = new DefaultClaimDataMapper(new DefaultPolicyTransactionTimeOfDayScheme())
                .ExtractClaimData(formDataJson, calculationResultJson, this.TimeZone);
            if (this.CommonData.Amount != newWellKnownData.Amount)
            {
                events.Add(new ClaimAggregate.ClaimAmountUpdatedEvent(
                    this.TenantId, this.aggregateId, newWellKnownData.Amount, performingUserId, timestamp));
            }

            if (this.CommonData.Description != newWellKnownData.Description)
            {
                events.Add(new ClaimAggregate.ClaimDescriptionUpdatedEvent(
                    this.TenantId, this.aggregateId, newWellKnownData.Description, performingUserId, timestamp));
            }

            if (this.CommonData.IncidentTimestamp != newWellKnownData.IncidentTimestamp)
            {
                events.Add(new ClaimAggregate.ClaimIncidentDateUpdatedEvent(
                    this.TenantId, this.aggregateId, newWellKnownData.IncidentTimestamp, performingUserId, timestamp));
            }

            return events;
        }

        /// <summary>
        /// Attach a file to the claim.
        /// </summary>
        /// <param name="fileAttachment">The file attachment.</param>
        public void Attachfile(ClaimFileAttachment fileAttachment)
        {
            this.claimFileAttachments.Add(fileAttachment);
        }

        /// <summary>
        /// Get list of file attachments.
        /// </summary>
        /// <returns>list of file attachments.</returns>
        public IList<ClaimFileAttachment> GetClaimFileAttachments()
        {
            return this.claimFileAttachments;
        }

        /// <summary>
        /// Apply an update imported event.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Apply(ClaimAggregate.ClaimUpdateImportedEvent @event, int sequenceNumber)
        {
            this.ClaimNumber = @event.ClaimNumber;
            if (@event.ReferenceNumber != null)
            {
                this.ClaimReference = @event.ReferenceNumber;
            }

            this.CommonData.Amount = @event.Amount;
            this.CommonData.Description = @event.Description;
            this.CommonData.IncidentTimestamp
                = @event.IncidentDate?.At(new LocalTime(12, 0)).InZoneLeniently(this.TimeZone).ToInstant();
            if (@event.Status != null)
            {
                this.ClaimStatus = @event.Status;
            }
        }

        /// <summary>
        /// Handle claim number updated event.
        /// </summary>
        /// <param name="event">The claim number updated event.</param>
        public void Apply(ClaimAggregate.ClaimNumberUpdatedEvent @event, int sequenceNumber)
        {
            this.ClaimNumber = @event.ClaimNumber;
        }

        /// <summary>
        /// Gets a form model for editing this claim.
        /// </summary>
        /// <returns>A form model.</returns>
        public string GetFormModel()
        {
            var formModel = this.formDataUpdates.LastOrDefault()?.Data?.FormModel ?? new JObject();

            // Old or imported claims may have data that is not in the form model, so it is added here.
            var syncedFormModel = new DefaultClaimDataMapper(new DefaultPolicyTransactionTimeOfDayScheme())
                .SyncFormdata(formModel, this.CommonData);
            return syncedFormModel.ToString();
        }

        /// <summary>
        /// Update status of the current claim.
        /// </summary>
        /// <param name="event">The state changed event.</param>
        /// <param name="sequenceNumber">The event sequence number.</param>
        public void Apply(ClaimAggregate.ClaimStateChangedEvent @event, int sequenceNumber)
        {
            this.ClaimStatus = @event.ResultingState;
            this.PreviousStatus = @event.OriginalState;
        }

        /// <summary>
        /// Update status of the current claim.
        /// </summary>
        /// <param name="event">The state changed event.</param>
        /// <param name="sequenceNumber">The event sequence number.</param>
        public void Apply(ClaimAggregate.ClaimStatusUpdatedEvent @event, int sequenceNumber)
        {
            var newState = ClaimState.FromClaimStatus(@event.ClaimStatus);
            this.ClaimStatus = newState;
        }

        /// <summary>
        /// Updates the form data for the quote.
        /// </summary>
        /// <param name="formData">The latest form data for the application.</param>
        /// <param name="performingUserId">The performing userId.</param>
        /// <param name="timestamp">The time the update was recorded.</param>
        /// <returns>Event for form data update.</returns>
        public IEnumerable<IEvent<ClaimAggregate, Guid>> UpdateFormData(string formData, Guid? performingUserId, Instant timestamp)
        {
            var events = new List<IEvent<ClaimAggregate, Guid>>();
            var formUpdateEvent = new ClaimAggregate.ClaimFormDataUpdatedEvent(
                this.TenantId, this.aggregateId, this.ClaimId, formData, performingUserId, timestamp);
            events.Add(formUpdateEvent);
            var updatedClaimData = new DefaultClaimDataMapper(new DefaultPolicyTransactionTimeOfDayScheme())
                .UpdateClaimData(this.CommonData.Clone(), formData);
            if (updatedClaimData.Amount != this.CommonData.Amount)
            {
                events.Add(new ClaimAggregate.ClaimAmountUpdatedEvent(
                    this.TenantId, this.aggregateId, updatedClaimData.Amount, performingUserId, timestamp));
            }

            if (updatedClaimData.Description != this.CommonData.Description)
            {
                events.Add(new ClaimAggregate.ClaimDescriptionUpdatedEvent(
                    this.TenantId, this.aggregateId, updatedClaimData.Description, performingUserId, timestamp));
            }

            if (updatedClaimData.IncidentTimestamp != this.CommonData.IncidentTimestamp)
            {
                events.Add(new ClaimAggregate.ClaimIncidentDateUpdatedEvent(this.TenantId, this.aggregateId, updatedClaimData.IncidentTimestamp, performingUserId, timestamp));
            }

            return events;
        }

        /// <summary>
        /// Record the creation of a new quote version.
        /// </summary>
        /// <param name="event">The versioned event.</param>
        public void RecordVersioning(ClaimAggregate.ClaimVersionCreatedEvent @event)
        {
            var newVersion = new ClaimVersion(@event.VersionId, @event.VersionNumber, this.GetDataSnapshot(@event.DataSnapshotIds), @event.Timestamp);
            this.claimVersions.Add(newVersion);
        }

        /// <summary>
        /// Claim change state per action.
        /// </summary>
        /// <returns>The resulting state change event.</returns>
        public ClaimAggregate.ClaimStateChangedEvent ChangeClaimState(
            ClaimActions claimAction, Guid? performingUserId, Instant timestamp, IClaimWorkflow workflow)
        {
            if (!workflow.IsActionPermittedByState(claimAction, this.ClaimStatus.ToString()))
            {
                var operation = workflow.GetOperation(claimAction);
                throw new ErrorException(Errors.Claim.OperationNotPermittedForState(
                    claimAction.Humanize(),
                    this.ClaimStatus.ToString(),
                    operation.ResultingState,
                    operation.RequiredStates));
            }

            this.CheckWorkflowOperationRequirements(claimAction);

            var resultingState = workflow.GetResultingState(claimAction, this.ClaimStatus.ToString());
            return new ClaimAggregate.ClaimStateChangedEvent(
                this.TenantId, this.aggregateId, claimAction, performingUserId, this.ClaimStatus, resultingState, timestamp);
        }

        /// <summary>
        /// Apply a form update event.
        /// </summary>
        /// <param name="event">The form update event.</param>
        public void Apply(ClaimAggregate.ClaimFormDataUpdatedEvent @event, int sequenceNumber)
        {
            var formDataUpdate = new ClaimDataUpdate<ClaimFormData>(@event.FormUpdateId, new ClaimFormData(@event.FormData), @event.Timestamp);
            this.formDataUpdates.Add(formDataUpdate);
        }

        /// <summary>
        /// Apply a calculation result creation event.
        /// </summary>
        /// <param name="event">The calculation result creation event.</param>
        public void Apply(ClaimAggregate.ClaimCalculationResultCreatedEvent @event, int sequenceNumber)
        {
            var calculationResultUpdate = new ClaimDataUpdate<ClaimCalculationResult>(
                @event.CalculationResultId, @event.CalculationResult, @event.Timestamp);
            this.calculationResults.Add(calculationResultUpdate);
        }

        /// <summary>
        /// Apply a claim amount update event.
        /// </summary>
        /// <param name="event">The claim amount updated event.</param>
        public void Apply(ClaimAggregate.ClaimAmountUpdatedEvent @event, int sequenceNumber)
        {
            this.CommonData.Amount = @event.Amount;
        }

        /// <summary>
        /// Apply associate claim with policy event.
        /// </summary>
        /// <param name="event">The associate claim with policy event.</param>
        public void Apply(ClaimAggregate.AssociateClaimWithPolicyEvent @event, int sequenceNumber)
        {
            this.PolicyId = @event.PolicyId;
        }

        /// <summary>
        /// Apply disassociate claim with policy event.
        /// </summary>
        /// <param name="event">The disassociate claim with policy event.</param>
        public void Apply(ClaimAggregate.DisassociateClaimWithPolicyEvent @event, int sequenceNumber)
        {
            this.PolicyId = null;
        }

        /// <summary>
        /// Apply delete claim event.
        /// </summary>
        /// <param name="event">The delete claim event.</param>
        public void Apply(ClaimAggregate.ClaimDeletedEvent @event, int sequenceNumber)
        {
            // NOP.
        }

        /// <summary>
        /// Apply a claim description updated event.
        /// </summary>
        /// <param name="event">The claim description updated event.</param>
        public void Apply(ClaimAggregate.ClaimDescriptionUpdatedEvent @event, int sequenceNumber)
        {
            this.CommonData.Description = @event.Description;
        }

        /// <summary>
        /// Apply an incident date updated event.
        /// </summary>
        /// <param name="event">The claim amount update event.</param>
        public void Apply(ClaimAggregate.ClaimIncidentDateUpdatedEvent @event, int sequenceNumber)
        {
            this.CommonData.IncidentTimestamp = @event.IncidentTimestamp;
        }

        /// <summary>
        /// Apply to determine that the claim is actualised.
        /// </summary>
        /// <param name="event">The claim amount update event.</param>
        public void Apply(ClaimAggregate.ClaimActualisedEvent @event, int sequenceNumber)
        {
            this.IsActualised = true;
        }

        /// <summary>
        /// Gets the IDs of the latest quote data updates.
        /// </summary>
        /// <returns>The IDs of the latest quote data updates.</returns>
        public ClaimDataSnapshotIds GetLatestDataSnapshotIds()
        {
            return new ClaimDataSnapshotIds(
                this.formDataUpdates.Select(u => u.Id).LastOrDefault(),
                this.calculationResults.Select(r => r.Id).LastOrDefault());
        }

        /// <summary>
        /// Update the claim's workflow step.
        /// </summary>
        /// <param name="newStep">The new step.</param>
        public void UpdateWorkflowStep(string newStep)
        {
            this.WorkflowStep = newStep;
        }

        private ClaimDataSnapshot GetDataSnapshot(ClaimDataSnapshotIds ids)
        {
            return new ClaimDataSnapshot(
                this.formDataUpdates.SingleOrDefault(u => u.Id == ids.FormDataId),
                this.calculationResults.SingleOrDefault(r => r.Id == ids.CalculationResultId));
        }

        private void CheckWorkflowOperationRequirements(ClaimActions operationName)
        {
            var latestCalculationResult = this.LatestCalculationResult?.Data;

            if (latestCalculationResult == null)
            {
                return;
            }

            var additionalInformation = new List<string>
            {
                $"Related Claim Id: {this.ClaimId}",
                $"Claim Action: ClaimActions.{operationName}",
            };
            additionalInformation.AddRange(this.CreateAndGetLightweightInformationDataForClaimErrorDetails());
            var lightweightInformationData = this.CreateAndGetLightweightClaimInformationObject(operationName);
            if (operationName == ClaimActions.AutoApproval)
            {
                if (latestCalculationResult.Triggers.Any())
                {
                    throw new ErrorException(
                        Errors.Calculation.Claim.
                        ShouldNotHaveActiveTriggersForAutoApproval(
                        lightweightInformationData,
                        additionalInformation));
                }
            }

            if (operationName == ClaimActions.ReviewReferral)
            {
                if (!latestCalculationResult.HasReviewCalculationTriggers)
                {
                    throw new ErrorException(
                        Errors.Calculation.Claim.
                        ShouldHaveReviewCalculationTriggers(
                        lightweightInformationData,
                        additionalInformation));
                }
            }

            if (operationName == ClaimActions.ReviewApproval)
            {
                if (latestCalculationResult.HasAssessmentCalculationTriggers)
                {
                    throw new ErrorException(
                        Errors.Calculation.Claim.
                        ShouldNotHaveAssessmentCalculationTriggers(
                        lightweightInformationData,
                        additionalInformation));
                }

                if (latestCalculationResult.HasDeclinedCalculationTriggers)
                {
                    throw new ErrorException(
                        Errors.Calculation.Claim.
                        ShouldNotHaveDeclinedCalculationTriggers(
                        lightweightInformationData,
                        additionalInformation));
                }
            }

            if (operationName == ClaimActions.AssessmentReferral)
            {
                if (!latestCalculationResult.HasAssessmentCalculationTriggers)
                {
                    throw new ErrorException(
                        Errors.Calculation.Claim.
                        DoesNotHaveAssessmentCalculationTriggers(
                        lightweightInformationData,
                        additionalInformation));
                }
            }
        }
    }
}
