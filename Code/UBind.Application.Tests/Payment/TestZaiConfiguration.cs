// <copyright file="TestZaiConfiguration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Tests.Payment
{
    using System.Collections.Generic;
    using UBind.Application.Payment.Zai;
    using UBind.Application.Payment.Zai.ZaiEntities;

    public class TestZaiConfiguration : ZaiConfiguration
    {
        public TestZaiConfiguration()
            : base(
                  "4mgmdg1fden1a509i6edbf704t",
                  "1t1i8kb46cpulgavorpnkvfr8rjpubmeie3fmmkomodvt8ouf9r",
                  "im-au-04/93a7a470-8881-013a-39c2-0a58a9feac03:264df775-c193-4785-86a7-73abec598298:3",
                  "https://test.api.promisepay.com/items/{id}/make_payment",
                  "https://au-0000.sandbox.auth.assemblypay.com/tokens",
                  "https://test.api.promisepay.com/users",
                  "https://test.api.promisepay.com/users/{id}",
                  "https://test.api.promisepay.com/users/{id}/items",
                  "https://test.api.promisepay.com/items",
                  "https://test.api.promisepay.com/items/{id}",
                  "https://test.api.promisepay.com/card_accounts",
                  "https://test.api.promisepay.com/card_accounts/{id}/users",
                  "https://test.api.promisepay.com/fees",
                  new List<OrganisationSellerAccount>())
        {
        }

        private TestZaiConfiguration(
            string clientId,
            string clientSecret,
            string scope,
            string paymentUrl,
            string authorizationUrl,
            string userCreationUrl,
            string userRetrievalUrl,
            string userItemRetrievalUrl,
            string itemCreationUrl,
            string itemUpdateUrl,
            string cardCaptureUrl,
            string cardRetrievalUrl,
            string feeCreationUrl,
            List<OrganisationSellerAccount> organisationSellerAccounts)
            : base(
                  clientId,
                  clientSecret,
                  scope,
                  paymentUrl,
                  authorizationUrl,
                  userCreationUrl,
                  userRetrievalUrl,
                  itemCreationUrl,
                  itemUpdateUrl,
                  userItemRetrievalUrl,
                  cardCaptureUrl,
                  cardRetrievalUrl,
                  feeCreationUrl,
                  organisationSellerAccounts)
        {
        }

        public static TestZaiConfiguration GetIncorrectZaiConfiguration()
        {
            return new TestZaiConfiguration(
                  "4mgmdg1fden1a509i6edbf7000",
                  "1t1i8kb46cpulgavorpnkvfr8rjpubmeie3fmmkomodvt8ouf9r",
                  "im-au-04/93a7a470-8881-013a-39c2-0a58a9feac03:264df775-c193-4785-86a7-73abec598298:3",
                  "https://test.api.promisepay.com/items/{id}/make_payment",
                  "https://au-0000.sandbox.auth.assemblypay.com/tokens",
                  "https://test.api.promisepay.com/users",
                  "https://test.api.promisepay.com/users/{id}",
                  "https://test.api.promisepay.com/users/{id}/items",
                  "https://test.api.promisepay.com/items",
                  "https://test.api.promisepay.com/items/{id}",
                  "https://test.api.promisepay.com/card_accounts",
                  "https://test.api.promisepay.com/users/{id}/card_accounts",
                  "https://test.api.promisepay.com/fees",
                  new List<OrganisationSellerAccount>());
        }
    }
}
