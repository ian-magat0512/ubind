// <copyright file="ClientDataRetriever.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.Aggregates.Quote.DataLocator.IQumulateQuoteDataRetriever
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Exceptions;
    using UBind.Domain.Json;

    public class ClientDataRetriever : BaseDataRetriever
    {
        /// <inheritdoc/>
        public override object Retrieve(IIQumulateQuoteDatumLocations config, CachingJObjectWrapper formData, CachingJObjectWrapper calculationData)
        {
            return new ClientData
            {
                LegalName = this.GetDataValue(config.LegalName, formData, calculationData),
                TradingName = this.GetDataValue(config.TradingName, formData, calculationData),
                EntityType = this.GetDataValue(config.EntityType, formData, calculationData),
                Abn = this.GetDataValue(config.Abn, formData, calculationData),
                Industry = this.GetDataValue(config.Industry, formData, calculationData),
                StreetAddress = this.ResolveStreetAddress(config, formData, calculationData),
                PostalAddress = this.ResolvePostalAddress(config, formData, calculationData),
                MobileNumber = this.GetDataValue(config.MobileNumber, formData, calculationData),
                TelephoneNumber = this.GetDataValue(config.TelephoneNumber, formData, calculationData),
                FaxNumber = this.GetDataValue(config.FaxNumber, formData, calculationData),
                Email = this.GetDataValue(config.Email, formData, calculationData),
                Title = this.GetDataValue(config.Title, formData, calculationData),
                FirstName = this.GetDataValue(config.FirstName, formData, calculationData),
                LastName = this.GetDataValue(config.LastName, formData, calculationData),
                IntroducerClientReference = this.GetDataValue(config.IntroducerClientReference, formData, calculationData),
                Borrowers = this.ResolveBorrowers(config, formData, calculationData),
            };
        }

        private List<Borrower> ResolveBorrowers(IIQumulateQuoteDatumLocations config, CachingJObjectWrapper fd, CachingJObjectWrapper crd)
        {
            List<Borrower> borrowers = new List<Borrower>();

            string borrower1FirstName = this.GetDataValue(config.Borrower1FirstName, fd, crd);
            if (borrower1FirstName != null)
            {
                borrowers.Add(new Borrower
                {
                    FirstName = borrower1FirstName,
                    LastName = this.GetDataValue(config.Borrower1LastName, fd, crd),
                    DateOfBirth = this.GetDataValue(config.Borrower1DateOfBirth, fd, crd),
                    DriverLicense = this.GetDataValue(config.Borrower1DriversLicense, fd, crd),
                });

                string borrower2FirstName = this.GetDataValue(config.Borrower2FirstName, fd, crd);
                if (borrower2FirstName != null)
                {
                    borrowers.Add(new Borrower
                    {
                        FirstName = borrower2FirstName,
                        LastName = this.GetDataValue(config.Borrower2LastName, fd, crd),
                        DateOfBirth = this.GetDataValue(config.Borrower2DateOfBirth, fd, crd),
                        DriverLicense = this.GetDataValue(config.Borrower2DriversLicense, fd, crd),
                    });

                    string borrower3FirstName = this.GetDataValue(config.Borrower3FirstName, fd, crd);
                    if (borrower3FirstName != null)
                    {
                        borrowers.Add(new Borrower
                        {
                            FirstName = borrower3FirstName,
                            LastName = this.GetDataValue(config.Borrower3LastName, fd, crd),
                            DateOfBirth = this.GetDataValue(config.Borrower3DateOfBirth, fd, crd),
                            DriverLicense = this.GetDataValue(config.Borrower3DriversLicense, fd, crd),
                        });

                        string borrower4FirstName = this.GetDataValue(config.Borrower4FirstName, fd, crd);
                        if (borrower4FirstName != null)
                        {
                            borrowers.Add(new Borrower
                            {
                                FirstName = borrower4FirstName,
                                LastName = this.GetDataValue(config.Borrower4LastName, fd, crd),
                                DateOfBirth = this.GetDataValue(config.Borrower4DateOfBirth, fd, crd),
                                DriverLicense = this.GetDataValue(config.Borrower4DriversLicense, fd, crd),
                            });
                        }
                    }
                }
            }

            return borrowers;
        }

        private Address ResolveStreetAddress(IIQumulateQuoteDatumLocations config, CachingJObjectWrapper fd, CachingJObjectWrapper crd)
        {
            string streetAddressStateString = this.GetDataValue(config.StreetAddressState, fd, crd);
            ValueTypes.State streetAddressState = default;
            if (!string.IsNullOrEmpty(streetAddressStateString))
            {
                if (!Enum.TryParse<ValueTypes.State>(streetAddressStateString, out streetAddressState))
                {
                    string reason = "None of the enum values for state matched. Please check the documentation for "
                        + "the list of accepted states.";
                    throw new ErrorException(
                        Errors.DataLocators.ParseFailure(config.StreetAddressState, typeof(ValueTypes.State), reason));
                }
            }

            return new Address
            {
                StreetLine1 = this.GetDataValue(config.StreetAddressStreetLine1, fd, crd),
                StreetLine2 = this.GetDataValue(config.StreetAddressStreetLine2, fd, crd),
                Suburb = this.GetDataValue(config.StreetAddressSuburb, fd, crd),
                Postcode = this.GetDataValue(config.StreetAddressPostcode, fd, crd),
                State = streetAddressState,
            };
        }

        private Address ResolvePostalAddress(IIQumulateQuoteDatumLocations config, CachingJObjectWrapper fd, CachingJObjectWrapper crd)
        {
            string postalAddressStateString = this.GetDataValue(config.PostalAddressState, fd, crd);
            ValueTypes.State postalAddressState = default;
            if (postalAddressStateString != null)
            {
                Enum.TryParse<ValueTypes.State>(postalAddressStateString, out postalAddressState);
            }

            var streetLine1 = this.GetDataValue(config.PostalAddressStreetLine1, fd, crd);
            if (string.IsNullOrEmpty(streetLine1))
            {
                return null;
            }

            return new Address
            {
                StreetLine1 = streetLine1,
                StreetLine2 = this.GetDataValue(config.PostalAddressStreetLine2, fd, crd),
                Suburb = this.GetDataValue(config.PostalAddressSuburb, fd, crd),
                Postcode = this.GetDataValue(config.PostalAddressPostcode, fd, crd),
                State = postalAddressState,
            };
        }
    }
}
