// <copyright file="RolePermissionMigration.cs" company="uBind">
// Copyright (c) uBind. All rights reserved.
// </copyright>

// If you edit this file, you must remove this line and then do proper null checking
#pragma warning disable CS8600, CS8625, CS8629, CS8618, CS8605, CS8604, CS8601, CS8602, CS8603, CS8622, CS8619, CS8767, CS8620, CS8765

namespace UBind.Persistence
{
    using System.Linq;
    using System.Text;
    using UBind.Domain.Permissions;
    using UBind.Domain.Repositories;
    using UBind.Domain.Services.Migration;

    public class RolePermissionMigration : IRolePermissionMigration
    {
        private readonly IUBindDbContext dbContext;

        public RolePermissionMigration(
            IUBindDbContext dbContext,
            IConnectionConfiguration connection)
        {
            this.dbContext = dbContext;
        }

        /// <inheritdoc />
        public void ChangeSerializedPermissionsForMessages()
        {
            var roles = this.dbContext.Roles.ToList();
            var command = new StringBuilder();

            foreach (var role in roles)
            {
                if (role.Permissions.Any(p => p == Permission.ViewMessages || p == Permission.ManageMessages
                    || p == Permission.ViewAllMessages || p == Permission.ManageAllMessages))
                {
                    var serializedPermission = string.Join(",", role.Permissions);
                    command.AppendLine($"UPDATE Roles SET SerializedPermissions = '{serializedPermission}' WHERE Id = '{role.Id}';");
                }
            }

            if (command.Length > 0)
            {
                this.dbContext.Database.ExecuteSqlCommand(command.ToString());
            }
        }
    }
}
