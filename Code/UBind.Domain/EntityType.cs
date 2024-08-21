// <copyright file="EntityType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain;

/// <summary>
/// The entity types.
/// Warning: Do not change the number here as these are mapped through to existing database records.
/// You can add new entity types only at the end.
/// </summary>
public enum EntityType
{
    Quote = 0,

    Policy = 1,

    User = 2,

    Customer = 3,

    PolicyTransaction = 4,

    QuoteVersion = 5,

    Claim = 6,

    ClaimVersion = 7,

    Product = 8,

    Message = 9,

    Tenant = 10,

    Document = 11,

    Organisation = 12,

    Event = 13,

    Portal = 14,

    CreditNote = 15,

    Invoice = 16,

    Payment = 17,

    Refund = 18,

    Role = 19,

    Report = 20,

    Release = 21,

    Person = 22,

    SmsMessage = 23,

    EmailMessage = 24,

    Relationship = 25,

    ProductRelease = 26,
}
