// <copyright file="PolicyLuceneFieldNames.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search
{
    /// <summary>
    /// Policy Lucene Document field names.
    /// </summary>
    public class PolicyLuceneFieldNames
    {
        /// <summary>
        /// The Document field for Policy Id.
        /// </summary>
        public const string FieldPolicyId = "id";

        /// <summary>
        /// The Document field for Quote Id.
        /// </summary>
        public const string FieldQuoteId = "quoteid";

        /// <summary>
        /// The Document field for Organisation Id.
        /// </summary>
        public const string FieldOrganisationId = "organisationid";

        /// <summary>
        /// The Document field for Tenant Id.
        /// </summary>
        public const string FieldTenantId = "tenantid";

        /// <summary>
        /// The Document field for Product Id.
        /// </summary>
        public const string FieldProductId = "productid";

        /// <summary>
        /// The Document field for Product Name.
        /// </summary>
        public const string FieldProductName = "productname";

        /// <summary>
        /// The Document field for Owner User Id.
        /// </summary>
        public const string FieldOwnerUserId = "owneruserid";

        /// <summary>
        /// The Document field for Owner Person Id.
        /// </summary>
        public const string FieldOwnerPersonId = "ownerpersonid";

        /// <summary>
        /// The Document field for Owner FullName.
        /// </summary>
        public const string FieldOwnerFullName = "ownerfullname";

        /// <summary>
        /// The Document field for customer Id.
        /// </summary>
        public const string FieldCustomerId = "customerid";

        /// <summary>
        /// The Document field for customer person Id.
        /// </summary>
        public const string FieldCustomerPersonId = "customerpersonid";

        /// <summary>
        /// The Document field for customer fullname.
        /// </summary>
        public const string FieldCustomerFullName = "customerfullname";

        /// <summary>
        /// The Document field for customer preferred name.
        /// </summary>
        public const string FieldCustomerPreferredName = "customerpreferredname";

        /// <summary>
        /// The Document field for customer email.
        /// </summary>
        public const string FieldCustomerEmail = "customeremail";

        /// <summary>
        /// The Document field for customer alternative email.
        /// </summary>
        public const string FieldCustomerAlternativeEmail = "customeralternativeemail";

        /// <summary>
        /// The Document field for customer home phone.
        /// </summary>
        public const string FieldCustomerHomePhone = "customerhomephone";

        /// <summary>
        /// The Document field for customer work phone.
        /// </summary>
        public const string FieldCustomerWorkPhone = "customerworkphone";

        /// <summary>
        /// The Document field for customer mobile phone.
        /// </summary>
        public const string FieldCustomerMobilePhone = "customermobilephone";

        /// <summary>
        /// The Document field for policy issued timestamp in tick.
        /// </summary>
        public const string FieldIssuedTicksSinceEpoch = "policyissuetimestampintickssinceepoch";

        /// <summary>
        /// The Document field for policy number.
        /// </summary>
        public const string FieldPolicyNumber = "policynumber";

        /// <summary>
        /// The Document field for policy state.
        /// </summary>
        public const string FieldPolicyState = "policystate";

        /// <summary>
        /// The Document field for policy title.
        /// </summary>
        public const string FieldPolicyTitle = "policytitle";

        /// <summary>
        /// The Document field for policy inception date in tick.
        /// </summary>
        public const string FieldInceptionTicksSinceEpoch = "inceptiondatetimestampintickssinceepoch";

        /// <summary>
        /// The Document field for policy expiry date in tick.
        /// </summary>
        public const string FieldExpiryTicksSinceEpoch = "expirydatetimestampintickssinceepoch";

        /// <summary>
        /// The Document field for policy cancellation date timestamp.
        /// </summary>
        public const string FieldCancellationEffectiveTicksSinceEpoch = "cancellationdatetimestampintickssinceepoch";

        /// <summary>
        /// The Document field for policy last modified timestamp.
        /// </summary>
        public const string FieldLastModifiedTicksSinceEpoch = "lastmodifiedtimestamp";

        /// <summary>
        /// The Document field for Last Modified Timestamp By User.
        /// </summary>
        public const string FieldLastModifiedByUserTicksSinceEpoch = "lastmodifiedbyusertimestamp";

        /// <summary>
        /// The Document field for policy created date timestamp.
        /// </summary>
        public const string FieldCreatedTicksSinceEpoch = "createddatetimestamp";

        /// <summary>
        /// The Document field for policy latest renewal effective date in tick.
        /// </summary>
        public const string FieldLatestRenewalEffectiveTicksSinceEpoch = "latestrenewaleffectivetickssinceepoch";

        /// <summary>
        /// The Document field for policy retroactive date in tick.
        /// </summary>
        public const string FieldRetroactiveTicksSinceEpoch = "retroactivetickssinceepoch";

        /// <summary>
        /// The Document field for serialized calculation result.
        /// </summary>
        public const string FieldSerializedCalculationResult = "serializedCalculationResult";

        /// <summary>
        /// The Document field for is discarded.
        /// </summary>
        public const string IsDiscarded = "isdiscarded";

        /// <summary>
        /// The Document field for is test data identifier in the lucene indexes.
        /// </summary>
        public const string IsTestData = "istestdata";
    }
}
