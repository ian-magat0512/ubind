// <copyright file="PortalSignInMethodMapper.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Web.Mappers
{
    using Riok.Mapperly.Abstractions;
    using UBind.Domain.ReadModel.Portal;
    using UBind.Web.ResourceModels.Portal;

    /// <summary>
    /// Auto maps the properties from a PortalSignInMethodReadModel to a PortalSignInMethodModel (resource model).
    /// </summary>
    [Mapper]
    public partial class PortalSignInMethodMapper
    {
        /// <summary>
        /// Auto maps the properties from a PortalSignInMethodReadModel to a PortalSignInMethodModel (resource model).
        /// </summary>
        public partial PortalSignInMethodModel ReadModelToResourceModel(PortalSignInMethodReadModel readModel);
    }
}
