// <copyright file="QuoteProductOrganisationPolicyCustomerReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.ReadModels
{
    using UBind.Domain;
    using UBind.Domain.ReadModel.Customer;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Persistence.ReadModels.Quote;

    internal class QuoteProductOrganisationPolicyCustomerReadModel : QuoteProductOrganisationReadModel
    {
        public CustomerReadModel Customer { get; set; }

        public PolicyReadModel Policy { get; set; }

        public Release? Release { get; set; }
    }
}
