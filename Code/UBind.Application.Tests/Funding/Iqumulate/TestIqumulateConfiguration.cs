// <copyright file="TestIqumulateConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Tests.Funding.Iqumulate
{
    using UBind.Application.Funding.Iqumulate;
    using UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever;
    using UBind.Domain.Tests.Fakes;

    public class TestIqumulateConfiguration : IIqumulateConfiguration
    {
        public string BaseUrl => "TODO: Put test URL here!";

        public string ActionUrl => "TODO: Put test URL here!";

        public string MessageOriginUrl => "TODO: Put test URL here!";

        public string AffinitySchemeCode => "TODO: put affinity scheme code for tests here!";

        public string IntroducerContactEmail => null;

        public string PolicyClassCode => "TODO: put policy class code for tests here!";

        public string PolicyUnderwriterCode => "TODO: put PolicyUnderwriterCode for tests here!";

        public string PaymentMethod => "TODO: put PaymentMethod for tests here!";

        public string AcceptanceConfirmationField => "TODO: put AcceptanceConfirmationField for tests here!";

        public ConfiguredIQumulateQuoteDatumLocations QuoteDataLocations => QuoteFactory.IQumulateQuoteDataLocations;
    }
}
