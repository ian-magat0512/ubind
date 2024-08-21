// <copyright file="LocalDateHandler.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

namespace UBind.Persistence.Helpers
{
    using System;
    using System.Data;
    using Dapper;
    using NodaTime;

    /// <summary>
    /// This class defines a custom mapping for NodaTime's LocalDate type.
    /// So that Dapper can map this type properly.
    /// </summary>
    public class LocalDateHandler : SqlMapper.TypeHandler<LocalDateTime>
    {
        public static readonly LocalDateHandler Default = new LocalDateHandler();

        public override LocalDateTime Parse(object value)
        {
            if (value is DateTime dateTime)
            {
                return LocalDateTime.FromDateTime(dateTime);
            }

            throw new DataException("Cannot convert database value to LocalDate.");
        }

        public override void SetValue(IDbDataParameter parameter, LocalDateTime value)
        {
            parameter.Value = value.ToDateTimeUnspecified();
        }
    }
}
