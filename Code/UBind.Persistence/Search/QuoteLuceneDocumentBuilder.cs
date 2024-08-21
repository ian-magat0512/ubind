// <copyright file="QuoteLuceneDocumentBuilder.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence.Search
{
    using System;
    using Lucene.Net.Documents;
    using UBind.Domain;
    using UBind.Domain.Search;

    /// <inheritdoc/>
    public class QuoteLuceneDocumentBuilder : ILuceneDocumentBuilder<IQuoteSearchIndexWriteModel>
    {
        /// <inheritdoc/>
        public Document BuildLuceneDocument(
            IQuoteSearchIndexWriteModel quote,
            long currentDateTime)
        {
            var document = new Document();

            this.SetQuoteIdDocumentField(quote.Id, document);
            this.SetProductIdDocumentField(quote.ProductId, document);
            this.SetOrganisationIdDocumentField(quote.OrganisationId, document);
            this.SetProductNameDocumentField(quote.ProductName, document);
            this.SetQuoteTypeDocumentField(quote.QuoteType, document);
            this.SetCreatedDateDocumentField(quote.CreatedTicksSinceEpoch, document);
            this.SetLastUpdatedDocumentField(quote.LastModifiedTicksSinceEpoch, document);
            this.SetOwnerUserIdDocumentField(quote.OwnerUserId, document);
            this.SetOwnerPersonIdDocumentField(quote.OwnerPersonId, document);
            this.SetIsDiscarded(quote.IsDiscarded, document);
            this.SetIsTestData(quote.IsTestData, document);

            if (quote.LastModifiedByUserTicksSinceEpoch.HasValue)
            {
                this.SetLastUpdatedByUserDocumentField(quote.LastModifiedByUserTicksSinceEpoch.Value, document);
            }

            if (!string.IsNullOrEmpty(quote.CustomerFullname))
            {
                this.SetCustomerFullnameDocumentField(quote.CustomerFullname, document);
            }

            if (!string.IsNullOrEmpty(quote.OwnerFullname))
            {
                this.SetOwnerFullnameDocumentField(quote.OwnerFullname, document);
            }

            if (!string.IsNullOrEmpty(quote.QuoteState))
            {
                this.SetQuoteStateDocumentField(quote.QuoteState, document);
            }

            if (quote.ExpiryTicksSinceEpoch.HasValue)
            {
                this.SetExpiryDateTimeDocumentField(quote.ExpiryTicksSinceEpoch.Value, document);
            }

            if (!string.IsNullOrEmpty(quote.QuoteTitle))
            {
                this.SetQuoteTitleDocumentField(quote.QuoteTitle, document);
            }

            if (!string.IsNullOrEmpty(quote.QuoteNumber))
            {
                this.SetQuoteNumberDocumentField(quote.QuoteNumber, document);
            }

            this.SetCustomerIdDocumentField(quote.CustomerId, document);

            if (!string.IsNullOrEmpty(quote.CustomerPreferredName))
            {
                this.SetCustomerPreferredNameDocumentField(quote.CustomerPreferredName, document);
            }

            if (!string.IsNullOrEmpty(quote.FormDataJson))
            {
                this.SetFormDataJsonDocumentField(quote.FormDataJson, document);
            }

            if (!string.IsNullOrEmpty(quote.CustomerEmail))
            {
                this.SetEmailDocumentField(quote.CustomerEmail, document);
            }

            if (!string.IsNullOrEmpty(quote.CustomerAlternativeEmail))
            {
                this.SetAlternativeEmailDocumentField(quote.CustomerAlternativeEmail, document);
            }

            if (!string.IsNullOrEmpty(quote.CustomerHomePhone))
            {
                this.SetHomePhoneDocumentField(quote.CustomerHomePhone, document);
            }

            if (!string.IsNullOrEmpty(quote.CustomerWorkPhone))
            {
                this.SetWorkPhoneDocumentField(quote.CustomerWorkPhone, document);
            }

            if (!string.IsNullOrEmpty(quote.CustomerMobilePhone))
            {
                this.SetMobilePhoneDocumentField(quote.CustomerMobilePhone, document);
            }

            if (!string.IsNullOrEmpty(quote.PolicyNumber))
            {
                this.SetPolicyNumberDocumentField(quote.PolicyNumber, document);
            }

            return document;
        }

        private void SetQuoteIdDocumentField(Guid quoteId, Document document)
        {
            var quoteIdString = quoteId.ToString("N");
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldQuoteId, quoteIdString, Field.Store.YES));
        }

        private void SetOrganisationIdDocumentField(Guid organisationId, Document document)
        {
            var organisationIdString = organisationId.ToString("N");
            var organisationField = new TextField(
                QuoteLuceneFieldsNames.FieldOrganisationId, organisationIdString, Field.Store.YES);
            document.Add(organisationField);
        }

        private void SetFormDataJsonDocumentField(string formDataJson, Document document)
        {
            var flattenedFormData = JsonHelper.FlattenJsonToString(formDataJson);
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldFormDataJsonValues, flattenedFormData, Field.Store.YES));
        }

        private void SetQuoteTitleDocumentField(string quoteTitle, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldQuoteTitle, quoteTitle, Field.Store.YES));
        }

        private void SetQuoteNumberDocumentField(string quoteNumber, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldQuoteNumber, quoteNumber, Field.Store.YES));
        }

        private void SetCustomerIdDocumentField(Guid? customerId, Document document)
        {
            if (!customerId.HasValue)
            {
                return;
            }

            var customerIdString = customerId.Value.ToString("N");
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldCustomerId, customerIdString, Field.Store.YES));
        }

        private void SetQuoteStateDocumentField(string quoteState, Document document)
        {
            document.Add(
                    new TextField(QuoteLuceneFieldsNames.FieldQuoteState, quoteState.ToLower(), Field.Store.YES));
        }

        private void SetCustomerFullnameDocumentField(string customerFullName, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldCustomerFullName, customerFullName, Field.Store.YES));
        }

        private void SetCustomerPreferredNameDocumentField(string customerPreferredName, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldCustomerPreferredName, customerPreferredName, Field.Store.YES));
        }

        private void SetEmailDocumentField(string email, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldCustomerEmail, email, Field.Store.YES));
        }

        private void SetAlternativeEmailDocumentField(string customerAlternativeEmail, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldCustomerAlternativeEmail, customerAlternativeEmail, Field.Store.YES));
        }

        private void SetMobilePhoneDocumentField(string mobile, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldCustomerMobilePhone, mobile, Field.Store.YES));
        }

        private void SetHomePhoneDocumentField(string customerHomePhone, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldCustomerHomePhone, customerHomePhone, Field.Store.YES));
        }

        private void SetWorkPhoneDocumentField(string customerWorkPhone, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldWorkPhone, customerWorkPhone, Field.Store.YES));
        }

        private void SetLastUpdatedDocumentField(long lastUpdated, Document document)
        {
            var numField = this.GetStoredNumericFieldWithNumericTokenStream(QuoteLuceneFieldsNames.FieldLastUpdatedTimeStamp, lastUpdated);
            document.Add(numField);
        }

        private void SetLastUpdatedByUserDocumentField(long lastUpdatedByUser, Document document)
        {
            var numField = this.GetStoredNumericFieldWithNumericTokenStream(QuoteLuceneFieldsNames.FieldLastUpdatedByUserTimestamp, lastUpdatedByUser);
            document.Add(numField);
        }

        private void SetOwnerUserIdDocumentField(Guid? ownerUserId, Document document)
        {
            if (!ownerUserId.HasValue)
            {
                return;
            }

            var ownerUserIdString = ownerUserId.Value.ToString("N");
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldOwnerUserId, ownerUserIdString, Field.Store.YES));
        }

        private void SetOwnerPersonIdDocumentField(Guid? ownerPersonId, Document document)
        {
            if (!ownerPersonId.HasValue)
            {
                return;
            }

            var ownerPersonIdString = ownerPersonId.Value.ToString("N");
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldOwnerPersonId, ownerPersonIdString, Field.Store.YES));
        }

        private void SetIsDiscarded(bool isDiscarded, Document document)
        {
            var isDiscardedString = isDiscarded.ToString().ToLower();
            document.Add(
            new TextField(QuoteLuceneFieldsNames.IsDiscarded, isDiscardedString, Field.Store.YES));
        }

        private void SetOwnerFullnameDocumentField(string ownerFullname, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldOwnerFullname, ownerFullname, Field.Store.YES));
        }

        private void SetExpiryDateTimeDocumentField(long expiryDateTime, Document document)
        {
            var numField = this.GetStoredNumericFieldWithNumericTokenStream(QuoteLuceneFieldsNames.FieldExpiryTimestamp, expiryDateTime);
            document.Add(numField);
        }

        private void SetCreatedDateDocumentField(long createdDate, Document document)
        {
            var numField = this.GetStoredNumericFieldWithNumericTokenStream(QuoteLuceneFieldsNames.FieldCreatedTimestamp, createdDate);
            document.Add(numField);
        }

        private void SetProductNameDocumentField(string productName, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldProductName, productName, Field.Store.YES));
        }

        private void SetQuoteTypeDocumentField(QuoteType quoteType, Document document)
        {
            document.Add(
                 new TextField(QuoteLuceneFieldsNames.FieldQuoteType, ((int)quoteType).ToString(), Field.Store.YES));
        }

        private void SetProductIdDocumentField(Guid productId, Document document)
        {
            document.Add(
              new TextField(QuoteLuceneFieldsNames.FieldProductId, productId.ToString(), Field.Store.YES));
        }

        private void SetIsTestData(bool isTestData, Document document)
        {
            document.Add(
            new TextField(QuoteLuceneFieldsNames.IsTestData, isTestData.ToString().ToLower(), Field.Store.YES));
        }

        private void SetPolicyNumberDocumentField(string policyNumber, Document document)
        {
            document.Add(
            new TextField(QuoteLuceneFieldsNames.FieldPolicyNumber, policyNumber, Field.Store.YES));
        }

        private Int64Field GetStoredNumericFieldWithNumericTokenStream(string name, long value)
        {
            var fieldType = new FieldType() { IsStored = true, IsIndexed = true, NumericType = NumericType.INT64 };
            return new Int64Field(name, value, fieldType);
        }
    }
}
