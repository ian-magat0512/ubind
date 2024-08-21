// <copyright file="PersonConstructorData.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Application.Automation.ContactDetail
{
    using System.Collections.Generic;
    using System.Linq;
    using UBind.Domain.Aggregates.Person.Fields;

    public class PersonConstructorData
    {
        public PersonConstructorData(
            string? namePrefix,
            string firstName,
            string? lastName,
            string? middleNames,
            string? nameSuffix,
            string? preferredName,
            string? company,
            string? title,
            IEnumerable<PhoneNumberConstructorData> phoneNumbers,
            IEnumerable<EmailAddressConstructorData> emailAddresses,
            IEnumerable<StreetAddressConstructorData> streetAddresses,
            IEnumerable<WebsiteAddressConstructorData> websiteAddresses,
            IEnumerable<MessengerIdConstructorData> messengerIds,
            IEnumerable<SocialMediaIdConstructorData> socialMediaIds)
        {
            this.NamePrefix = namePrefix;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.MiddleNames = middleNames;
            this.NameSuffix = nameSuffix;
            this.PreferredName = preferredName;
            this.Company = company;
            this.Title = title;
            this.PhoneNumbers = phoneNumbers?.ToList();
            this.EmailAddresses = emailAddresses?.ToList();
            this.StreetAddresses = streetAddresses?.ToList();
            this.WebsiteAddresses = websiteAddresses?.ToList();
            this.MessengerIds = messengerIds?.ToList();
            this.SocialMediaIds = socialMediaIds?.ToList();
        }

        public string? NamePrefix { get; }

        public string FirstName { get; }

        public string? LastName { get; }

        public string? MiddleNames { get; }

        public string? NameSuffix { get; }

        public string? PreferredName { get; }

        public string? Company { get; }

        public string? Title { get; }

        public List<PhoneNumberConstructorData>? PhoneNumbers { get; }

        public List<EmailAddressConstructorData>? EmailAddresses { get; }

        public List<StreetAddressConstructorData>? StreetAddresses { get; }

        public List<WebsiteAddressConstructorData>? WebsiteAddresses { get; }

        public List<MessengerIdConstructorData>? MessengerIds { get; }

        public List<SocialMediaIdConstructorData>? SocialMediaIds { get; }

        public IEnumerable<PhoneNumberField> GetPhoneNumbers()
        {
            var list = new List<PhoneNumberField>();
            if (this?.PhoneNumbers == null || !this.PhoneNumbers.Any())
            {
                return list;
            }

            int sequence = 0;
            foreach (var x in this.PhoneNumbers)
            {
                var phoneNumber = new PhoneNumberField
                {
                    Label = x.Label,
                    PhoneNumber = x.Value,
                    SequenceNo = sequence++,
                    IsDefault = x.IsDefault,
                };
                list.Add(phoneNumber);
            }

            return list;
        }

        public IEnumerable<EmailAddressField> GetEmailAddresses()
        {
            var list = new List<EmailAddressField>();
            if (this?.EmailAddresses == null || !this.EmailAddresses.Any())
            {
                return list;
            }

            int sequence = 0;
            foreach (var x in this.EmailAddresses)
            {
                var model = new EmailAddressField
                {
                    EmailAddress = x.Value,
                    Label = x.Label,
                    SequenceNo = sequence++,
                    IsDefault = x.IsDefault,
                };

                list.Add(model);
            }

            return list;
        }

        public IEnumerable<StreetAddressField> GetStreetAddresses()
        {
            var list = new List<StreetAddressField>();
            if (this?.StreetAddresses == null || !this.StreetAddresses.Any())
            {
                return list;
            }

            int sequence = 0;
            foreach (var x in this.StreetAddresses)
            {
                list.Add(new StreetAddressField(x.Address, x.Suburb, x.State, x.Postcode, sequence++, x.Label, x.IsDefault));
            }

            return list;
        }

        public IEnumerable<WebsiteAddressField> GetWebsiteAddresses()
        {
            var list = new List<WebsiteAddressField>();
            if (this?.WebsiteAddresses == null || !this.WebsiteAddresses.Any())
            {
                return list;
            }

            int sequence = 0;
            foreach (var websiteAddress in this.WebsiteAddresses)
            {
                list.Add(new WebsiteAddressField
                {
                    WebsiteAddress = websiteAddress.Value,
                    Label = websiteAddress.Label,
                    SequenceNo = sequence++,
                    IsDefault = websiteAddress.IsDefault,
                });
            }

            return list;
        }

        public IEnumerable<MessengerIdField> GetMessengerIds()
        {
            var list = new List<MessengerIdField>();
            if (this?.MessengerIds == null || !this.MessengerIds.Any())
            {
                return list;
            }

            int sequence = 0;
            foreach (var x in this.MessengerIds)
            {
                if (!string.IsNullOrEmpty(x.Value))
                {
                    list.Add(new MessengerIdField
                    {
                        MessengerId = x.Value,
                        Label = x.Label,
                        SequenceNo = sequence++,
                        IsDefault = x.IsDefault,
                    });
                }
            }

            return list;
        }

        public IEnumerable<SocialMediaIdField> GetSocialmediaIds()
        {
            var list = new List<SocialMediaIdField>();
            if (this?.SocialMediaIds == null || !this.SocialMediaIds.Any())
            {
                return list;
            }

            int sequence = 0;
            foreach (var x in this.SocialMediaIds)
            {
                if (!string.IsNullOrEmpty(x.Value))
                {
                    list.Add(new SocialMediaIdField
                    {
                        SocialMediaId = x.Value,
                        Label = x.Label,
                        SequenceNo = sequence++,
                        IsDefault = x.IsDefault,
                    });
                }
            }

            return list;
        }
    }
}
