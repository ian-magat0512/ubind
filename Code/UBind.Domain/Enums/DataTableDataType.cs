// <copyright file="DataTableDataType.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Domain.Enums;

using UBind.Domain.Attributes;

/// <summary>
/// This data types are used for mapping uBind Data Types into an SQL Server data type.
/// </summary>
public enum DataTableDataType
{
    [DataTypeSqlSettings(SqlDataType = "bit", DataTableDataType = typeof(bool))]
    Boolean,

    [DataTypeSqlSettings(SqlDataType = "nvarchar", MaxLength = int.MaxValue)]
    Text,

    [DataTypeSqlSettings(SqlDataType = "nvarchar", MaxLength = 250)]
    Name,

    [DataTypeSqlSettings(SqlDataType = "nvarchar", MaxLength = 400)]
    FullName,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 35)]
    PhoneNumber,

    [DataTypeSqlSettings(SqlDataType = "nvarchar", MaxLength = 320)]
    EmailAddress,

    [DataTypeSqlSettings(SqlDataType = "nvarchar", MaxLength = 400)]
    WebsiteAddress,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 37)]
    PaymentCardNumber,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 35)]
    PaymentCardType,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 7)]
    PaymentCardExpiryDate,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 4)]
    PaymentCardVerificationCode,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 30)]
    BankAccountNumber,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 11)]
    BankStateBranchNumber,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 17)]
    AustralianCompanyNumber,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 21)]
    AustralianBusinessNumber,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 7)]
    VehicleRegistrationNumber,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 15)]
    PostalCode,

    [DataTypeSqlSettings(SqlDataType = "varchar", MaxLength = 3)]
    CurrencyCode,

    [DataTypeSqlSettings(SqlDataType = "uniqueidentifier", DataTableDataType = typeof(Guid))]
    UniversallyUniqueIdentifier,

    [DataTypeSqlSettings(SqlDataType = "decimal", MaxLength = 25, DecimalsLength = 10, DataTableDataType = typeof(decimal))]
    Number,

    [DataTypeSqlSettings(SqlDataType = "bigint", DataTableDataType = typeof(long))]
    WholeNumber,

    [DataTypeSqlSettings(SqlDataType = "decimal", MaxLength = 25, DecimalsLength = 10, DataTableDataType = typeof(decimal))]
    DecimalNumber,

    [DataTypeSqlSettings(SqlDataType = "decimal", MaxLength = 25, DecimalsLength = 10, DataTableDataType = typeof(decimal))]
    Percentage,

    [DataTypeSqlSettings(SqlDataType = "decimal", MaxLength = 25, DecimalsLength = 10, DataTableDataType = typeof(decimal))]
    MonetaryAmount,

    [DataTypeSqlSettings(SqlDataType = "date")]
    Date,

    [DataTypeSqlSettings(SqlDataType = "time")]
    Time,

    [DataTypeSqlSettings(SqlDataType = "datetime2")]
    DateTime,
}
