// <copyright file="PortalDetails.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain
{
    using System;
    using NodaTime;

    /// <summary>
    /// Portal Details.
    /// </summary>
    public class PortalDetails : Entity<Guid>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalDetails"/> class.
        /// </summary>
        /// <param name="name">The portal name.</param>
        /// <param name="alias">The portal name alias.</param>
        /// <param name="title">The portal title.</param>
        /// <param name="stylesheetUrl">The portal stylesheet ( optional ).</param>
        /// <param name="disabled">If portal is disabled.</param>
        /// <param name="deleted">If portal is deleted.</param>
        /// <param name="createdTimestamp">The current time.</param>
        private PortalDetails(
            string name,
            string alias,
            string title,
            string stylesheetUrl,
            bool disabled,
            bool deleted,
            Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Name = name;
            this.Alias = alias;
            this.Title = title;
            this.Disabled = disabled;
            this.Deleted = deleted;
            this.StylesheetUrl = stylesheetUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalDetails"/> class as a copy of existin portal details.
        /// </summary>
        /// <param name="other">The details to copy.</param>
        /// <param name="createdTimestamp">The current time.</param>
        private PortalDetails(PortalDetails other, Instant createdTimestamp)
            : base(Guid.NewGuid(), createdTimestamp)
        {
            this.Name = other.Name;
            this.Alias = other.Alias;
            this.Title = other.Title;
            this.Disabled = other.Disabled;
            this.Deleted = other.Deleted;
            this.StylesheetUrl = other.StylesheetUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalDetails"/> class.
        /// </summary>
        /// <remarks>
        /// Parameterless constructor for EF.
        /// .</remarks>
        private PortalDetails()
            : base(default(Guid), default(Instant))
        {
        }

        /// <summary>
        /// Gets the portal name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the portal name alias.
        /// </summary>
        public string Alias { get; private set; }

        /// <summary>
        /// Gets the portal title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Gets the URL to a stylesheet which is loaded when rendering this portal.
        /// The stylesheet or css file as an external link.
        /// </summary>
        public string StylesheetUrl { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the portal was disabled.
        /// </summary>
        public bool Disabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the portal was deleted.
        /// </summary>
        public bool Deleted { get; private set; }

        /// <summary>
        /// Create portal details.
        /// </summary>
        /// <param name="name">The portal name.</param>
        /// <param name="alias">The portal name alias.</param>
        /// <param name="title">The portal title.</param>
        /// <param name="stylesheetUrl">The portal stylesheet ( optional ).</param>
        /// <param name="disabled">If portal is disabled.</param>
        /// <param name="deleted">If portal is deleted.</param>
        /// <param name="createdTimestamp">The current time.</param>
        /// <returns>The new instance of portalDetails.</returns>
        public static PortalDetails Create(string name, string alias, string title, string stylesheetUrl, bool disabled, bool deleted, Instant createdTimestamp)
        {
            return new PortalDetails(name, alias, title, stylesheetUrl, disabled, deleted, createdTimestamp);
        }

        /// <summary>
        /// Create portal details coming from another portalDetails.
        /// </summary>
        /// <param name="portalDetails">The details to copy.</param>
        /// <param name="createdTimestamp">The current time.</param>
        /// <returns>The new instance of portalDetails.</returns>
        public static PortalDetails CreateFromPortalDetails(PortalDetails portalDetails, Instant createdTimestamp)
        {
            return new PortalDetails(portalDetails, createdTimestamp);
        }
    }
}
