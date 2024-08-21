// <copyright file="ReportAggregate.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

// IDE0060: REmoved unused parameter.
// Disable IDE0060 because there are unused parameters, e.g., the sequence number parameter, that we cannot remove.
#pragma warning disable IDE0060

namespace UBind.Domain.Aggregates.Report
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.Json.Serialization;
    using NodaTime;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Report;

    /// <summary>
    /// Aggregate for Report.
    /// </summary>
    public partial class ReportAggregate : AggregateRootEntity<ReportAggregate, Guid>
    {
        /// <summary>
        /// It should be needed during de/serialization of the aggregate.
        /// </summary>
        [JsonConstructor]
        public ReportAggregate()
        {
        }

        private ReportAggregate(Guid tenantId, Guid reportId, Guid organisationId, ReportAddModel report, Guid? performingUserId, Instant createdTimestamp)
        {
            var @event = new ReportInitializedEvent(
                tenantId,
                reportId,
                organisationId,
                performingUserId,
                createdTimestamp);
            this.ApplyNewEvent(@event);

            this.Update(tenantId, report, performingUserId, createdTimestamp);
        }

        private ReportAggregate(IEnumerable<IEvent<ReportAggregate, Guid>> events)
            : base(events)
        {
        }

        public override AggregateType AggregateType => AggregateType.Report;

        /// <summary>
        /// Gets the tenant id of the report.
        /// </summary>
        public Guid TenantId { get; private set; }

        /// <summary>
        /// Gets the organisation id of the report.
        /// </summary>
        public Guid OrganisationId { get; private set; }

        /// <summary>
        /// Gets the name of the report.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description of the report.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Gets the products of the report.
        /// </summary>
        public ICollection<Guid> ProductIds { get; private set; }
            = new Collection<Guid>();

        /// <summary>
        /// Gets the source data of the report.
        /// </summary>
        public string SourceData { get; private set; }

        /// <summary>
        /// Gets the mime-type of the report.
        /// </summary>
        public string MimeType { get; private set; }

        /// <summary>
        /// Gets the filename of the report.
        /// </summary>
        public string Filename { get; private set; }

        /// <summary>
        /// Gets the body of the report.
        /// </summary>
        public string Body { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the report is deleted.
        /// </summary>
        public bool IsDeleted { get; private set; }

        /// <summary>
        /// Gets the generated files of the report.
        /// </summary>
        public ICollection<Guid> ReportFileIds { get; private set; }
            = new Collection<Guid>();

        /// <summary>
        /// Factory method for creating reports.
        /// </summary>
        /// <param name="report">The report add model.</param>
        /// <param name="performingUserId">The performing user ID.</param>
        /// <param name="createdTimestamp">The time event was created.</param>
        /// <returns>A new instance of <see cref="ReportAggregate"/>, loaded from the event store.</returns>
        public static ReportAggregate CreateReport(Guid tenantId, Guid organisationId, ReportAddModel report, Guid? performingUserId, Instant createdTimestamp)
        {
            return new ReportAggregate(tenantId, Guid.NewGuid(), organisationId, report, performingUserId, createdTimestamp);
        }

        /// <summary>
        /// Loads a report from the event store.
        /// calculating state by re-applying existing events.
        /// </summary>
        /// <param name="events">Existing events.</param>
        /// <returns>A new instance of <see cref="ReportAggregate"/>, loaded from the event store.</returns>
        public static ReportAggregate LoadFromEvents(IEnumerable<IEvent<ReportAggregate, Guid>> events)
        {
            return new ReportAggregate(events);
        }

        /// <summary>
        /// Update report's name.
        /// </summary>
        /// <param name="report">The report.</param>
        /// <param name="performingUserId">The userId who updates the report.</param>
        /// <param name="timestamp">The timestamp.</param>
        public void Update(Guid tenantId, ReportAddModel report, Guid? performingUserId, Instant timestamp)
        {
            this.UpdateReportName(report.Name, performingUserId, timestamp);
            this.UpdateReportDescription(report.Description, performingUserId, timestamp);
            this.UpdateReportProducts(tenantId, report.ProductIds, performingUserId, timestamp);
            this.UpdateReportSourceData(report.SourceData, performingUserId, timestamp);
            this.UpdateReportMimeType(report.MimeType, performingUserId, timestamp);
            this.UpdateReportFilename(report.Filename, performingUserId, timestamp);
            this.UpdateReportBody(report.Body, performingUserId, timestamp);

            if (report.IsDeleted == true)
            {
                this.DeleteReport(performingUserId, timestamp);
            }
        }

        /// <summary>
        /// Update report's name.
        /// </summary>
        /// <param name="name">The name of the report.</param>
        /// <param name="performingUserId">The userId who updates the report name.</param>
        /// <param name="timestamp">The timestamp.</param>
        public void UpdateReportName(string name, Guid? performingUserId, Instant timestamp)
        {
            if (this.Name != name)
            {
                var @event = new ReportNameUpdatedEvent(this.TenantId, this.Id, name, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Update report's description.
        /// </summary>
        /// <param name="description">The description of the report.</param>
        /// <param name="performingUserId">The userId who update the report description.</param>
        /// <param name="timestamp">The timestamp.</param>
        public void UpdateReportDescription(string description, Guid? performingUserId, Instant timestamp)
        {
            if (this.Description != description)
            {
                var @event = new ReportDescriptionUpdatedEvent(this.TenantId, this.Id, description, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Update report's products.
        /// </summary>
        /// <param name="tenantId">The tenant Id.</param>
        /// <param name="productIds">The product ids of the report.</param>
        /// <param name="performingUserId">The userId who updates the report product.</param>
        /// <param name="timestamp">The timestamp.</param>
        public void UpdateReportProducts(
            Guid tenantId, IEnumerable<Guid> productIds, Guid? performingUserId, Instant timestamp)
        {
            if (!productIds.OrderBy(id => id).SequenceEqual((this.ProductIds ?? new List<Guid>()).OrderBy(id => id)))
            {
                var @event
                    = new ReportProductsUpdatedEvent(tenantId, this.Id, productIds, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Update report's sourceData.
        /// </summary>
        /// <param name="sourceData">The source data of the report.</param>
        /// <param name="performingUserId">The userId who update the report source.</param>
        /// <param name="timestamp">The timestamp.</param>
        public void UpdateReportSourceData(string sourceData, Guid? performingUserId, Instant timestamp)
        {
            if (this.SourceData != sourceData)
            {
                var @event = new ReportSourceDataUpdatedEvent(this.TenantId, this.Id, sourceData, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Update report's mime-type.
        /// </summary>
        /// <param name="mimeType">The mime-type of the report.</param>
        /// <param name="performingUserId">The userId who updates mine type.</param>
        /// <param name="timestamp">The timestamp.</param>
        public void UpdateReportMimeType(string mimeType, Guid? performingUserId, Instant timestamp)
        {
            if (this.MimeType != mimeType)
            {
                var @event = new ReportMimeTypeUpdatedEvent(this.TenantId, this.Id, mimeType, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Update report's filename.
        /// </summary>
        /// <param name="filename">The filename of the report.</param>
        /// <param name="performingUserId">The userId who update the report file name.</param>
        /// <param name="timestamp">The timestamp.</param>
        public void UpdateReportFilename(string filename, Guid? performingUserId, Instant timestamp)
        {
            if (this.Filename != filename)
            {
                var @event = new ReportFilenameUpdatedEvent(this.TenantId, this.Id, filename, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Update report's body.
        /// </summary>
        /// <param name="body">The body of the report.</param>
        /// <param name="performingUserId">The userId who updated the report body.</param>
        /// <param name="timestamp">The timestamp.</param>
        public void UpdateReportBody(string body, Guid? performingUserId, Instant timestamp)
        {
            if (this.Body != body)
            {
                var @event = new ReportBodyUpdatedEvent(this.TenantId, this.Id, body, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Add generated report file.
        /// </summary>
        /// <param name="reportFileId">The report file id.</param>
        /// <param name="performingUserId">The userId who generate report file.</param>
        /// <param name="timestamp">The timestamp.</param>
        public void AddGeneratedReportFile(Guid reportFileId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ReportGeneratedFileAddedEvent(this.TenantId, this.Id, reportFileId, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Delete report.
        /// </summary>
        /// <param name="performingUserId">The userId who deletes the report.</param>
        /// <param name="timestamp">The timestamp.</param>
        public void DeleteReport(Guid? performingUserId, Instant timestamp)
        {
            if (this.IsDeleted == false)
            {
                var @event = new ReportDeletedEvent(this.TenantId, this.Id, performingUserId, timestamp);
                this.ApplyNewEvent(@event);
            }
        }

        /// <summary>
        /// Trigger the ApplyNewIdEvent that applies new id to this aggregate.
        /// </summary>
        /// <param name="tenantNewId">The tenants new Id.</param>
        /// <param name="performingUserId">The userId who did the action.</param>
        /// <param name="timestamp">A timestamp.</param>
        public void TriggerApplyNewIdEvent(Guid tenantNewId, Guid? performingUserId, Instant timestamp)
        {
            var @event = new ApplyNewIdEvent(tenantNewId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        /// <summary>
        /// Record organisation migration and only applicable for an aggregate with an empty organisation Id.
        /// </summary>
        /// <param name="organisationId">The Id of the organisation to persist in this aggregate.</param>
        /// <param name="performingUserId">The user Id who updates the aggregate.</param>
        /// <param name="timestamp">The time the update was recorded.</param>
        public void RecordOrganisationMigration(Guid organisationId, Guid? performingUserId, Instant timestamp)
        {
            if (this.OrganisationId != Guid.Empty)
            {
                throw new ErrorException(
                    Errors.Organisation.FailedToMigrateForOrganisation(this.Id, this.OrganisationId));
            }

            var @event = new ReportOrganisationMigratedEvent(this.TenantId, organisationId, this.Id, performingUserId, timestamp);
            this.ApplyNewEvent(@event);
        }

        public override ReportAggregate ApplyEventsAfterSnapshot(IEnumerable<IEvent<ReportAggregate, Guid>> events, int latestSnapshotVersion)
        {
            this.ApplyEvents(events, latestSnapshotVersion);
            return this;
        }

        protected override void ApplyDerivedEvent(dynamic @event, int sequenceNumber)
        {
            this.Apply(@event, sequenceNumber);
        }

        private void Apply(ReportInitializedEvent @event, int sequenceNumber)
        {
            this.Id = @event.AggregateId;
            this.TenantId = @event.TenantId;
            this.OrganisationId = @event.OrganisationId;
        }

        private void Apply(ReportNameUpdatedEvent @event, int sequenceNumber)
        {
            this.Name = @event.Name;
        }

        private void Apply(ReportDescriptionUpdatedEvent @event, int sequenceNumber)
        {
            this.Description = @event.Description;
        }

        private void Apply(ReportProductsUpdatedEvent @event, int sequenceNumber)
        {
            this.ProductIds = @event.ProductNewIds;
        }

        private void Apply(ReportSourceDataUpdatedEvent @event, int sequenceNumber)
        {
            this.SourceData = @event.SourceData;
        }

        private void Apply(ReportMimeTypeUpdatedEvent @event, int sequenceNumber)
        {
            this.MimeType = @event.MimeType;
        }

        private void Apply(ReportFilenameUpdatedEvent @event, int sequenceNumber)
        {
            this.Filename = @event.Filename;
        }

        private void Apply(ReportBodyUpdatedEvent @event, int sequenceNumber)
        {
            this.Body = @event.Body;
        }

        private void Apply(ReportDeletedEvent @event, int sequenceNumber)
        {
            this.IsDeleted = @event.IsDeleted;
        }

        private void Apply(ReportGeneratedFileAddedEvent @event, int sequenceNumber)
        {
            this.ReportFileIds.Add(@event.ReportFileId);
        }

        private void Apply(ApplyNewIdEvent applyNewIdEvent, int sequenceNumber)
        {
            this.TenantId = applyNewIdEvent.TenantId;
        }

        private void Apply(ReportOrganisationMigratedEvent @event, int sequenceNumber)
        {
            this.OrganisationId = @event.OrganisationId;
        }
    }
}
