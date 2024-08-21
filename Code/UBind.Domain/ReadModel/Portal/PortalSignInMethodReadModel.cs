// <copyright file="PortalSignInMethodReadModel.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Domain.ReadModel.Portal
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("PortalSignInMethods")]
    public class PortalSignInMethodReadModel : IReadModel<Guid>
    {
        public Guid TenantId { get; set; }

        public Guid PortalId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the AuthenticationMethod.
        /// </summary>
        [Column("AuthenticationMethodId")]
        public Guid Id { get; set; }

        [NotMapped]
        public Guid AuthenticationMethodId
        {
            get => this.Id;
            set => this.Id = value;
        }

        public int SortOrder { get; set; }

        public bool IsEnabled { get; set; }

        public string Name { get; set; }

        public string TypeName { get; set; }
    }
}
