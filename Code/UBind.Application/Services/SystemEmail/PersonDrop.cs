// <copyright file="PersonDrop.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Services.SystemEmail
{
    using global::DotLiquid;

    /// <summary>
    /// A drop model for person.
    /// </summary>
    public class PersonDrop : Drop
    {
        public PersonDrop(
            string email,
            string alternativeEmail,
            string preferredName,
            string fullName,
            string namePrefix,
            string firstName,
            string middleNames,
            string lastName,
            string nameSuffix,
            string greetingName,
            string company,
            string title,
            string mobilePhone,
            string workPhone,
            string homePhone)
        {
            this.Email = email;
            this.AlternativeEmail = alternativeEmail;
            this.PreferredName = preferredName ?? greetingName;
            this.FullName = fullName;
            this.NamePrefix = namePrefix;
            this.FirstName = firstName;
            this.MiddleNames = middleNames;
            this.LastName = lastName;
            this.NameSuffix = nameSuffix;
            this.GreetingName = greetingName;
            this.Company = company;
            this.Title = title;
            this.MobilePhone = mobilePhone;
            this.WorkPhone = workPhone;
            this.HomePhone = homePhone;
        }

        /// <summary>
        /// Gets or sets gets the email of the person.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the alternative email of the person.
        /// </summary>
        public string AlternativeEmail { get; set; }

        /// <summary>
        /// Gets or sets the preferred name of the person.
        /// </summary>
        public string PreferredName { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the person's name prefix.
        /// </summary>
        public string NamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the person's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the person's middle names.
        /// </summary>
        public string MiddleNames { get; set; }

        /// <summary>
        /// Gets or sets the person's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the person's name suffix.
        /// </summary>
        public string NameSuffix { get; set; }

        /// <summary>
        /// Gets or sets the person's greeting name.
        /// </summary>
        public string GreetingName { get; set; }

        /// <summary>
        /// Gets or sets the person's company.
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// Gets or sets the person's title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the mobile phone number of the person.
        /// </summary>
        public string MobilePhone { get; set; }

        /// <summary>
        /// Gets or sets gets the home phone number of the person.
        /// </summary>
        public string HomePhone { get; set; }

        /// <summary>
        /// Gets or sets the work phone number of the person.
        /// </summary>
        public string WorkPhone { get; set; }
    }
}
