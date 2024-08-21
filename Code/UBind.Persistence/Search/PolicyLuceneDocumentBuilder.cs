// <copyright file="PolicyLuceneDocumentBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search
{
    using System;
    using Lucene.Net.Documents;
    using UBind.Domain;
    using UBind.Domain.Extensions;
    using UBind.Domain.ReadModel.Policy;
    using UBind.Domain.Search;

    public class PolicyLuceneDocumentBuilder : ILuceneDocumentBuilder<IPolicySearchIndexWriteModel>
    {
        public Document BuildLuceneDocument(
            IPolicySearchIndexWriteModel policy,
            long currentDateTime)
        {
            var document = new Document();

            this.SetPolicyIdDocumentField(policy.Id, document);
            this.SetQuoteIdDocumentField(policy.QuoteId, document);
            this.SetOrganisationIdDocumentField(policy.OrganisationId, document);
            this.SetTenantIdDocumentField(policy.TenantId, document);
            this.SetProductIdDocumentField(policy.ProductId, document);
            this.SetCreatedDateDocumentField(policy.CreatedTicksSinceEpoch, document);
            this.SetProductNameDocumentField(policy.ProductName, document);
            this.SetPolicyNumberDocumentField(policy.PolicyNumber, document);
            this.SetPolicyStateDocumentField(policy.PolicyState, document);
            this.SetPolicyTitleDocumentField(policy.PolicyTitle, document);
            this.SetIsDiscardedDocumentField(policy.IsDiscarded, document);
            this.SetIsTestDataDocumentField(policy.IsTestData, document);
            this.SetOwnerUserIdDocumentField(policy.OwnerUserId, document);
            this.SetOwnerPersonIdDocumentField(policy.OwnerPersonId, document);
            this.SetCustomerIdDocumentField(policy.CustomerId, document);
            this.SetCustomerPersonIdDocumentField(policy.CustomerPersonId, document);
            this.SetLastModifiedTimestampDocumentField(policy.LastModifiedTicksSinceEpoch, document);
            this.SetLastModifiedByUserTimestampDocumentField(policy.LastModifiedByUserTicksSinceEpoch, document);
            this.SetInceptionTimestampDocumentField(policy.InceptionTicksSinceEpoch, document);
            this.SetIssuedTimestampDocumentField(policy.IssuedTicksSinceEpoch, document);
            this.SetExpiryTimestampDocumentField(policy.ExpiryTicksSinceEpoch, document);
            this.SetCancellationEffectiveTimestampDocumentField(policy.CancellationEffectiveTicksSinceEpoch, document);
            this.SetLatestRenewalEffectiveTimestampDocumentField(policy.LatestRenewalEffectiveTicksSinceEpoch, document);
            this.SetRetroactiveTimestampDocumentField(policy.RetroactiveTicksSinceEpoch, document);
            this.SetOwnerFullNameDocumentField(policy.OwnerFullName, document);
            this.SetCustomerFullNameDocumentField(policy.CustomerFullName, document);
            this.SetCustomerPreferredNameDocumentField(policy.CustomerPreferredName, document);
            this.SetCustomerEmailDocumentField(policy.CustomerEmail, document);
            this.SetCustomerAlternativeEmailDocumentField(policy.CustomerAlternativeEmail, document);
            this.SetCustomerHomePhoneDocumentField(policy.CustomerHomePhone, document);
            this.SetCustomerWorkPhoneDocumentField(policy.CustomerWorkPhone, document);
            this.SetCustomerMobilePhoneDocumentField(policy.CustomerMobilePhone, document);
            this.SetSerializedCalculationResultDocumentField(policy.SerializedCalculationResult, document);

            if (policy.PolicyTransactionModel != null)
            {
                foreach (var policyTransaction in policy.PolicyTransactionModel)
                {
                    this.SetPolicyTransactionFields(policyTransaction, document);
                }
            }

            return document;
        }

        private void SetPolicyIdDocumentField(Guid policyId, Document document)
        {
            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldPolicyId, policyId.ToString("N"), Field.Store.YES));
        }

        private void SetQuoteIdDocumentField(Guid quoteId, Document document)
        {
            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldQuoteId, quoteId.ToString("N"), Field.Store.YES));
        }

        private void SetOrganisationIdDocumentField(Guid organisationId, Document document)
        {
            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldOrganisationId, organisationId.ToString("N"), Field.Store.YES));
        }

        private void SetTenantIdDocumentField(Guid tenantId, Document document)
        {
            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldTenantId, tenantId.ToString("N"), Field.Store.YES));
        }

        private void SetOwnerUserIdDocumentField(Guid? ownerUserId, Document document)
        {
            if (!ownerUserId.HasValue)
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldOwnerUserId, ownerUserId.Value.ToString("N"), Field.Store.YES));
        }

        private void SetOwnerPersonIdDocumentField(Guid? ownerPersonId, Document document)
        {
            if (!ownerPersonId.HasValue)
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldOwnerPersonId, ownerPersonId.Value.ToString("N"), Field.Store.YES));
        }

        private void SetCustomerIdDocumentField(Guid? customerId, Document document)
        {
            if (!customerId.HasValue)
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldCustomerId, customerId.Value.ToString("N"), Field.Store.YES));
        }

        private void SetCustomerPersonIdDocumentField(Guid? customerPersonId, Document document)
        {
            if (!customerPersonId.HasValue)
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldCustomerPersonId, customerPersonId.Value.ToString("N"), Field.Store.YES));
        }

        private void SetOwnerFullNameDocumentField(string ownerFullName, Document document)
        {
            if (ownerFullName.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldOwnerFullName, ownerFullName, Field.Store.YES));
        }

        private void SetCustomerFullNameDocumentField(string customerFullName, Document document)
        {
            if (customerFullName.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldCustomerFullName, customerFullName, Field.Store.YES));
        }

        private void SetCustomerPreferredNameDocumentField(string customerPreferredName, Document document)
        {
            if (customerPreferredName.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldCustomerPreferredName, customerPreferredName, Field.Store.YES));
        }

        private void SetCustomerEmailDocumentField(string customerEmail, Document document)
        {
            if (customerEmail.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldCustomerEmail, customerEmail, Field.Store.YES));
        }

        private void SetCustomerAlternativeEmailDocumentField(string customerAlternativeEmail, Document document)
        {
            if (customerAlternativeEmail.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldCustomerAlternativeEmail, customerAlternativeEmail, Field.Store.YES));
        }

        private void SetCustomerHomePhoneDocumentField(string customerHomePhone, Document document)
        {
            if (customerHomePhone.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldCustomerHomePhone, customerHomePhone, Field.Store.YES));
        }

        private void SetCustomerWorkPhoneDocumentField(string customerWorkPhone, Document document)
        {
            if (customerWorkPhone.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldCustomerWorkPhone, customerWorkPhone, Field.Store.YES));
        }

        private void SetCustomerMobilePhoneDocumentField(string customerMobilePhone, Document document)
        {
            if (customerMobilePhone.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldCustomerMobilePhone, customerMobilePhone, Field.Store.YES));
        }

        private void SetSerializedCalculationResultDocumentField(string serializedCalculationResult, Document document)
        {
            if (serializedCalculationResult.IsNullOrWhitespace())
            {
                return;
            }

            var flattenedSerializedCalculationResult = JsonHelper.FlattenJsonToString(serializedCalculationResult);
            this.SetDocumentHaveLargeValue(document, PolicyLuceneFieldNames.FieldSerializedCalculationResult, flattenedSerializedCalculationResult);
        }

        private void SetLastModifiedTimestampDocumentField(long lastModifiedTicksSinceEpoch, Document document)
        {
            var numField = this.GetStoredNumericFieldWithIndexing(PolicyLuceneFieldNames.FieldLastModifiedTicksSinceEpoch, lastModifiedTicksSinceEpoch);
            document.Add(numField);
        }

        private void SetLastModifiedByUserTimestampDocumentField(long? lastModifiedByUserTicksSinceEpoch, Document document)
        {
            if (!lastModifiedByUserTicksSinceEpoch.HasValue)
            {
                return;
            }

            var numField = this.GetStoredNumericFieldWithIndexing(PolicyLuceneFieldNames.FieldLastModifiedByUserTicksSinceEpoch, lastModifiedByUserTicksSinceEpoch.Value);
            document.Add(numField);
        }

        private void SetIssuedTimestampDocumentField(long issuedTicksSinceEpoch, Document document)
        {
            var numField = this.GetStoredNumericFieldWithIndexing(PolicyLuceneFieldNames.FieldIssuedTicksSinceEpoch, issuedTicksSinceEpoch);
            document.Add(numField);
        }

        private void SetExpiryTimestampDocumentField(long expiryTicksSinceEpoch, Document document)
        {
            var numField = this.GetStoredNumericFieldWithIndexing(PolicyLuceneFieldNames.FieldExpiryTicksSinceEpoch, expiryTicksSinceEpoch);
            document.Add(numField);
        }

        private void SetInceptionTimestampDocumentField(long inceptionTicksSinceEpoch, Document document)
        {
            var numField = this.GetStoredNumericFieldWithIndexing(PolicyLuceneFieldNames.FieldInceptionTicksSinceEpoch, inceptionTicksSinceEpoch);
            document.Add(numField);
        }

        private void SetCancellationEffectiveTimestampDocumentField(long cancellationEffectiveTicksSinceEpoch, Document document)
        {
            var numField = this.GetStoredNumericFieldWithIndexing(PolicyLuceneFieldNames.FieldCancellationEffectiveTicksSinceEpoch, cancellationEffectiveTicksSinceEpoch);
            document.Add(numField);
        }

        private void SetLatestRenewalEffectiveTimestampDocumentField(long? latestRenewalEffectiveTimestampInTicksSinceEpoch, Document document)
        {
            if (!latestRenewalEffectiveTimestampInTicksSinceEpoch.HasValue)
            {
                return;
            }

            var numField = this.GetStoredNumericFieldWithIndexing(PolicyLuceneFieldNames.FieldLatestRenewalEffectiveTicksSinceEpoch, latestRenewalEffectiveTimestampInTicksSinceEpoch.Value);
            document.Add(numField);
        }

        private void SetRetroactiveTimestampDocumentField(long? retroactiveTimestampInTicksSinceEpoch, Document document)
        {
            if (!retroactiveTimestampInTicksSinceEpoch.HasValue)
            {
                return;
            }

            var numField = this.GetStoredNumericFieldWithIndexing(PolicyLuceneFieldNames.FieldRetroactiveTicksSinceEpoch, retroactiveTimestampInTicksSinceEpoch.Value);
            document.Add(numField);
        }

        private void SetProductIdDocumentField(Guid productId, Document document)
        {
            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldProductId, productId.ToString(), Field.Store.YES));
        }

        private void SetProductNameDocumentField(string productName, Document document)
        {
            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldProductName, productName, Field.Store.YES));
        }

        private void SetPolicyNumberDocumentField(string policyNumber, Document document)
        {
            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldPolicyNumber, policyNumber, Field.Store.YES));
        }

        private void SetCreatedDateDocumentField(long createdDate, Document document)
        {
            var numField = this.GetStoredNumericFieldWithIndexing(PolicyLuceneFieldNames.FieldCreatedTicksSinceEpoch, createdDate);
            document.Add(numField);
        }

        private void SetPolicyStateDocumentField(string policyState, Document document)
        {
            if (policyState.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldPolicyState, policyState.ToLower(), Field.Store.YES));
        }

        private void SetPolicyTitleDocumentField(string policyTitle, Document document)
        {
            if (policyTitle.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyLuceneFieldNames.FieldPolicyTitle, policyTitle, Field.Store.YES));
        }

        private void SetIsDiscardedDocumentField(bool isDiscarded, Document document)
        {
            document.Add(
              new TextField(PolicyLuceneFieldNames.IsDiscarded, isDiscarded.ToString().ToLower(), Field.Store.YES));
        }

        private void SetIsTestDataDocumentField(bool isTestData, Document document)
        {
            document.Add(
              new TextField(PolicyLuceneFieldNames.IsTestData, isTestData.ToString().ToLower(), Field.Store.YES));
        }

        private void SetPolicyTransactionIdDocumentField(Guid policyTransactionId, Document document)
        {
            document.Add(
              new TextField(PolicyTransactionLuceneFieldNames.FieldPolicyTransactionId, policyTransactionId.ToString("N"), Field.Store.YES));
        }

        private void SetPolicyTransactionPolicyIdDocumentField(Guid policyId, Document document)
        {
            document.Add(
              new TextField(PolicyTransactionLuceneFieldNames.FieldPolicyId, policyId.ToString("N"), Field.Store.YES));
        }

        private void SetPolicyTransactionQuoteIdDocumentField(Guid quoteId, Document document)
        {
            document.Add(
              new TextField(PolicyTransactionLuceneFieldNames.FieldQuoteId, quoteId.ToString("N"), Field.Store.YES));
        }

        private void SetPolicyTransactionCustomerIdDocumentField(Guid? customerId, Document document)
        {
            if (!customerId.HasValue)
            {
                return;
            }

            document.Add(
              new TextField(PolicyTransactionLuceneFieldNames.FieldCustomerId, customerId.Value.ToString("N"), Field.Store.YES));
        }

        private void SetPolicyTransactionOwnerUserIdDocumentField(Guid? ownerUserId, Document document)
        {
            if (!ownerUserId.HasValue)
            {
                return;
            }

            document.Add(
              new TextField(PolicyTransactionLuceneFieldNames.FieldOwnerUserId, ownerUserId.Value.ToString("N"), Field.Store.YES));
        }

        private void SetPolicyTransactionOrganisationIdDocumentField(Guid organisationId, Document document)
        {
            document.Add(
              new TextField(PolicyTransactionLuceneFieldNames.FieldOrganisationId, organisationId.ToString("N"), Field.Store.YES));
        }

        private void SetPolicyTransactionTenantIdDocumentField(Guid tenantId, Document document)
        {
            document.Add(
              new TextField(PolicyTransactionLuceneFieldNames.FieldTenantId, tenantId.ToString("N"), Field.Store.YES));
        }

        private void SetPolicyTransactionProductIdDocumentField(Guid productId, Document document)
        {
            document.Add(
              new TextField(PolicyTransactionLuceneFieldNames.FieldProductId, productId.ToString("N"), Field.Store.YES));
        }

        private void SetPolicyTransactionQuoteNumberDocumentField(string quoteNumber, Document document)
        {
            if (quoteNumber.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyTransactionLuceneFieldNames.FieldQuoteNumber, quoteNumber, Field.Store.YES));
        }

        private void SetPolicyTransactionDiscriminatorDocumentField(string discriminator, Document document)
        {
            if (discriminator.IsNullOrWhitespace())
            {
                return;
            }

            document.Add(
              new TextField(PolicyTransactionLuceneFieldNames.FieldDiscriminator, discriminator, Field.Store.YES));
        }

        private void SetPolicyTransactionEffectiveTimestampDocumentField(long effectiveTimeTick, Document document)
        {
            var numField = this.GetStoredNumericFieldWithIndexing(PolicyTransactionLuceneFieldNames.FieldEffectiveTimeInTicksSinceEpoch, effectiveTimeTick);
            document.Add(numField);
        }

        private void SetPolicyTransactionCreatedTimestampDocumentField(long createdTicksSinceEpoch, Document document)
        {
            var numField = this.GetStoredNumericFieldWithIndexing(PolicyTransactionLuceneFieldNames.FieldCreatedTicksSinceEpoch, createdTicksSinceEpoch);
            document.Add(numField);
        }

        private void SetPolicyTransactionLastModifiedTimestampDocumentField(long lastModifiedTicksSinceEpoch, Document document)
        {
            var numField = this.GetStoredNumericFieldWithIndexing(PolicyTransactionLuceneFieldNames.FieldLastModifiedTicksSinceEpoch, lastModifiedTicksSinceEpoch);
            document.Add(numField);
        }

        private void SetPolicyTransactionCancellationEffectiveTimestampDocumentField(long? cancellationTimeTick, Document document)
        {
            if (!cancellationTimeTick.HasValue)
            {
                return;
            }

            var numField = this.GetStoredNumericFieldWithIndexing(PolicyTransactionLuceneFieldNames.FieldCancellationTicksSinceEpoch, cancellationTimeTick.Value);
            document.Add(numField);
        }

        private void SetPolicyTransactionFormDataDocumentField(string formDataJson, Document document)
        {
            if (formDataJson.IsNullOrWhitespace())
            {
                return;
            }

            var flattenedFormDataJson = JsonHelper.FlattenJsonToString(formDataJson);
            this.SetDocumentHaveLargeValue(document, PolicyTransactionLuceneFieldNames.FieldPolicyDataFormData, flattenedFormDataJson);
        }

        private void SetPolicyTransactionSerializedCalculationResultDocumentField(string serializedCalculationResult, Document document)
        {
            if (serializedCalculationResult.IsNullOrWhitespace())
            {
                return;
            }

            var flattenedSerializedCalculationResult = JsonHelper.FlattenJsonToString(serializedCalculationResult);
            this.SetDocumentHaveLargeValue(document, PolicyTransactionLuceneFieldNames.FieldPolicyDataSerializedCalculationResult, flattenedSerializedCalculationResult);
        }

        private void SetPolicyTransactionInceptionTimestampDocumentField(long inceptionTicksSinceEpoch, Document document)
        {
            var numField = this.GetStoredNumericFieldWithIndexing(PolicyTransactionLuceneFieldNames.FieldPolicyDataInceptionTicksSinceEpoch, inceptionTicksSinceEpoch);
            document.Add(numField);
        }

        private void SetPolicyTransactionExpiryTimestampDocumentField(long expiryTicksSinceEpoch, Document document)
        {
            var numField = this.GetStoredNumericFieldWithIndexing(PolicyTransactionLuceneFieldNames.FieldPolicyDataExpiryTicksSinceEpoch, expiryTicksSinceEpoch);
            document.Add(numField);
        }

        private void SetPolicyTransactionIsTestDataDocumentField(bool isTestData, Document document)
        {
            document.Add(
              new TextField(PolicyTransactionLuceneFieldNames.FieldIsTestData, isTestData.ToString().ToLower(), Field.Store.YES));
        }

        private void SetPolicyTransactionFields(IPolicyTransactionSearchIndexWriteModel policyTransaction, Document document)
        {
            this.SetPolicyTransactionIdDocumentField(policyTransaction.Id, document);
            this.SetPolicyTransactionPolicyIdDocumentField(policyTransaction.PolicyId, document);
            this.SetPolicyTransactionQuoteIdDocumentField(policyTransaction.QuoteId, document);
            this.SetPolicyTransactionCustomerIdDocumentField(policyTransaction.CustomerId, document);
            this.SetPolicyTransactionOwnerUserIdDocumentField(policyTransaction.OwnerUserId, document);
            this.SetPolicyTransactionOrganisationIdDocumentField(policyTransaction.OrganisationId, document);
            this.SetPolicyTransactionTenantIdDocumentField(policyTransaction.TenantId, document);
            this.SetPolicyTransactionProductIdDocumentField(policyTransaction.ProductId, document);
            this.SetPolicyTransactionCreatedTimestampDocumentField(policyTransaction.CreatedTicksSinceEpoch, document);
            this.SetPolicyTransactionLastModifiedTimestampDocumentField(policyTransaction.LastModifiedTicksSinceEpoch, document);

            if (policyTransaction.Discriminator == typeof(NewBusinessTransaction).Name.ToString())
            {
                this.SetPolicyTransactionInceptionTimestampDocumentField(policyTransaction.EffectiveTicksSinceEpoch, document);
                this.SetPolicyTransactionEffectiveTimestampDocumentField(policyTransaction.EffectiveTicksSinceEpoch, document);
            }
            else if (policyTransaction.Discriminator == typeof(RenewalTransaction).Name.ToString())
            {
                this.SetPolicyTransactionEffectiveTimestampDocumentField(policyTransaction.EffectiveTicksSinceEpoch, document);
            }
            else if (policyTransaction.Discriminator == typeof(CancellationTransaction).Name.ToString())
            {
                this.SetPolicyTransactionCancellationEffectiveTimestampDocumentField(policyTransaction.EffectiveTicksSinceEpoch, document);
            }

            this.SetPolicyTransactionExpiryTimestampDocumentField(policyTransaction.ExpiryTicksSinceEpoch, document);
            this.SetPolicyTransactionIsTestDataDocumentField(policyTransaction.IsTestData, document);
            this.SetPolicyTransactionQuoteNumberDocumentField(policyTransaction.QuoteNumber, document);
            this.SetPolicyTransactionDiscriminatorDocumentField(policyTransaction.Discriminator, document);
            this.SetPolicyTransactionFormDataDocumentField(policyTransaction.PolicyData_FormData, document);
            this.SetPolicyTransactionSerializedCalculationResultDocumentField(policyTransaction.PolicyData_SerializedCalculationResult, document);
        }

        private Int64Field GetStoredNumericFieldWithIndexing(string name, long value)
        {
            var fieldType = new FieldType() { IsStored = true, IsIndexed = true, NumericType = NumericType.INT64 };
            return new Int64Field(name, value, fieldType);
        }

        private void SetDocumentHaveLargeValue(Document document, string fieldName, string value)
        {
            var listData = this.SplitStringIntoChunks(value);
            foreach (var data in listData)
            {
                document.Add(new TextField(fieldName, data, Field.Store.YES));
            }
        }

        private List<string> SplitStringIntoChunks(string value)
        {
            int maxChunkSize = 30000;
            List<string> chunks = new List<string>();
            int strLength = value.Length;
            int start = 0;

            while (start < strLength)
            {
                int end = Math.Min(start + maxChunkSize, strLength);
                if (end < strLength && char.IsLetterOrDigit(value[end]) && char.IsLetterOrDigit(value[end - 1]))
                {
                    while (end > start && char.IsLetterOrDigit(value[end - 1]))
                    {
                        end--;
                    }
                }

                chunks.Add(value.Substring(start, end - start));
                start = end;
            }

            return chunks;
        }
    }
}
