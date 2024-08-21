﻿// <copyright file="GetAllSmsByFilterQuery.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Application.Queries.Sms
{
    using System;
    using System.Collections.Generic;
    using UBind.Domain.Patterns.Cqrs;
    using UBind.Domain.ReadModel;
    using UBind.Domain.ReadWriteModel;

    public class GetAllSmsByFilterQuery : IQuery<IEnumerable<Sms>>
    {
        public GetAllSmsByFilterQuery(Guid tenantId, EntityListFilters filters)
        {
            this.TenantId = tenantId;
            this.Filters = filters;
        }

        public Guid TenantId { get; }

        public EntityListFilters Filters { get; }
    }
}
