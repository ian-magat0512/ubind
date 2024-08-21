// <copyright file="UserResourceModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.ResourceModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using UBind.Domain.Dto;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadModel.User;

    /// <summary>
    /// Resource model for users.
    /// </summary>
    public class UserResourceModel : UserPersonDetailsModel, IAdditionalPropertyValues
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserResourceModel"/> class form a read model.
        /// </summary>
        /// <param name="userReadModel">The user read model.</param>
        /// <param name="person">The person read model summary (optional).</param>
        public UserResourceModel(IUserReadModelSummary userReadModel, IPersonReadModelSummary person = null)
            : base(person)
        {
            this.PersonId = userReadModel.PersonId;
            this.OrganisationId = userReadModel.OrganisationId;
            this.FullName = userReadModel.FullName;
            this.PreferredName = userReadModel.PreferredName;
            this.NamePrefix = userReadModel.NamePrefix;
            this.FirstName = userReadModel.FirstName;
            this.MiddleNames = userReadModel.MiddleNames;
            this.LastName = userReadModel.LastName;
            this.NameSuffix = userReadModel.NameSuffix;

            var personCommonProperties = new PersonCommonProperties();
            personCommonProperties.FullName = this.FullName;
            personCommonProperties.PreferredName = this.PreferredName;
            personCommonProperties.NamePrefix = this.NamePrefix;
            personCommonProperties.FirstName = this.FirstName;
            personCommonProperties.MiddleNames = this.MiddleNames;
            personCommonProperties.LastName = this.LastName;
            personCommonProperties.NameSuffix = this.NameSuffix;

            personCommonProperties.SetNameComponentsFromFullNameIfNoneAlreadySet();
            personCommonProperties.SetBasicFullName();

            this.FullName = personCommonProperties.FullName;
            this.PreferredName = personCommonProperties.PreferredName;
            this.NamePrefix = personCommonProperties.NamePrefix;
            this.FirstName = personCommonProperties.FirstName;
            this.MiddleNames = personCommonProperties.MiddleNames;
            this.LastName = personCommonProperties.LastName;
            this.NameSuffix = personCommonProperties.NameSuffix;

            this.Id = userReadModel.Id;
            this.Email = userReadModel.Email;
            this.CreatedDateTime = userReadModel.CreatedTimestamp.ToExtendedIso8601String();
            this.Blocked = userReadModel.IsDisabled;
            this.Company = userReadModel.Company;
            this.Title = userReadModel.Title;
            this.AlternativeEmail = userReadModel.AlternativeEmail;
            this.MobilePhoneNumber = userReadModel.MobilePhoneNumber;
            this.HomePhoneNumber = userReadModel.HomePhoneNumber;
            this.WorkPhoneNumber = userReadModel.WorkPhoneNumber;
            this.UserType = userReadModel.UserType;
            this.Status = userReadModel.UserStatus.ToString();
            this.TenantId = userReadModel.TenantId;
            this.SetRepeatingFieldsValues(person);
            this.CheckCreateRepeatingFieldsForBackwardCompatibility();
            this.CheckAndFixRepeatingFieldsDefaults();

            // TODO: picture ID?
            this.LastModifiedDateTime = userReadModel.LastModifiedTimestamp.ToExtendedIso8601String();
            this.ProfilePictureId = userReadModel.ProfilePictureId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserResourceModel"/> class form a read model.
        /// </summary>
        /// <param name="userReadModel">THe user read model.</param>
        /// <param name="additionalPropertyValueDtos">Collection of <see cref="AdditionalPropertyValueDto"/>.</param>
        /// <param name="person">The person read model summary (optional).</param>
        public UserResourceModel(
            IUserReadModelSummary userReadModel,
            List<AdditionalPropertyValueDto> additionalPropertyValueDtos,
            IPersonReadModelSummary person = null)
            : this(userReadModel, person)
        {
            this.AdditionalPropertyValues = additionalPropertyValueDtos != null
                && additionalPropertyValueDtos.Any() ?
                additionalPropertyValueDtos.Select(apvm => new AdditionalPropertyValueModel(apvm)).ToList() :
                new List<AdditionalPropertyValueModel>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserResourceModel"/> class form a read model.
        /// </summary>
        /// <param name="userReadModelDetail">The user read model detail.</param>
        /// <param name="person">The person read model summary (optional).</param>
        public UserResourceModel(
            UserReadModelDetail userReadModelDetail,
            IPersonReadModelSummary person = null)
            : this((IUserReadModelSummary)userReadModelDetail, person)
        {
            this.OrganisationAlias = userReadModelDetail.OrganisationAlias;
        }

        /// <summary>
        /// Gets the tenant Id of the tenant where the user belongs.
        /// </summary>
        public string Environment { get; private set; }

        /// <summary>
        /// Gets the Id reference of the user picture.
        /// </summary>
        public Guid? ProfilePictureId { get; private set; }

        /// <summary>
        /// Gets or sets the person's Id.
        /// </summary>
        [JsonProperty]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets the role type of the user.
        /// </summary>
        public string UserType { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is blocked or not.
        /// </summary>
        public bool Blocked { get; set; }

        /// <inheritdoc/>
        public List<AdditionalPropertyValueModel> AdditionalPropertyValues { get; private set; }
    }
}
