// <copyright file="AbnRegistration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.AbnLookup
{
    /// <summary>
    /// This class is needed because we need to represents the details of the ABN registration.
    /// </summary>
    public class AbnRegistration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AbnRegistration"/> class.
        /// </summary>
        /// <param name="abn">The ABN.</param>
        /// <param name="abnStatus">The ABN status.</param>
        /// <param name="name">The name.</param>
        /// <param name="nameType">The name type.</param>
        /// <param name="addressPostcode">The address post code.</param>
        /// <param name="addressState">The address state.</param>
        /// <param name="score">The relative search score.</param>
        public AbnRegistration(
            string abn,
            string abnStatus,
            string name,
            string nameType,
            int? addressPostcode,
            string addressState,
            int score)
        {
            this.Abn = abn.Insert(2, " ").Insert(6, " ").Insert(10, " ");
            this.AbnStatus = abnStatus;
            this.Name = name;
            this.NameType = nameType;
            this.AddressPostcode = addressPostcode;
            this.AddressState = string.IsNullOrWhiteSpace(addressState) ? null : addressState;
            this.Score = score;
        }

        /// <summary>
        /// Gets the ABN registration number.
        /// </summary>
        public string Abn { get; private set; }

        /// <summary>
        /// Gets the ABN status whether active or cancelled.
        /// </summary>
        public string AbnStatus { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the name type.
        /// </summary>
        public string NameType { get; private set; }

        /// <summary>
        /// Gets address post code.
        /// </summary>
        public int? AddressPostcode { get; private set; }

        /// <summary>
        /// Gets the address state.
        /// </summary>
        public string AddressState { get; private set; }

        /// <summary>
        /// Gets the score which represents the relevance of the result to the given search query.
        /// </summary>
        public int Score { get; private set; }
    }
}
